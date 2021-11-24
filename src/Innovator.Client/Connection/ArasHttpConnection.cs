using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;

namespace Innovator.Client.Connection
{
  /// <summary>
  /// The main implementation of a connection to an Aras instance over HTTP
  /// </summary>
  /// <seealso cref="Innovator.Client.IRemoteConnection" />
  /// <seealso cref="Innovator.Client.Connection.IArasConnection" />
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public class ArasHttpConnection : IRemoteConnection, IArasConnection
  {
    private ServerContext _context = new ServerContext(false);
    private readonly Uri _innovatorServerUrl;
    private readonly Uri _innovatorClientBin;
    private List<Action<IHttpRequest>> _defaults = new List<Action<IHttpRequest>>();
    private readonly ArasVaultConnection _vaultConn;
    private readonly List<KeyValuePair<string, string>> _serverInfo = new List<KeyValuePair<string, string>>();

    private IAuthenticator _authenticator;
    private string _httpUsername;
    private ICredentials _lastCredentials;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
      get { return string.Format("[Connection] {0} | {1} | {2}", _httpUsername, Database, Url); }
    }

    internal HttpClient Service { get; }

    /// <summary>
    /// AML context used for creating AML objects and formatting AML statements
    /// </summary>
    public ElementFactory AmlContext { get; private set; }

    /// <summary>
    /// Gets the compression setting to use when sending requests to the server.
    /// </summary>
    /// <value>
    /// The compression setting to use when sending requests to the server.
    /// </value>
    public CompressionType Compression { get; set; }

    /// <summary>
    /// Name of the connected database
    /// </summary>
    public string Database { get; private set; }

    /// <summary>
    /// Gets the server information returned when logging in.
    /// </summary>
    /// <value>
    /// The server information.
    /// </value>
    public IEnumerable<KeyValuePair<string, string>> ServerInfo { get { return _serverInfo; } }

    /// <summary>
    /// URL that the instance resides at
    /// </summary>
    public Uri Url { get; }

    /// <summary>
    /// ID of the authenticated user
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Gets the major version of the Aras installation
    /// </summary>
    /// <value>
    /// The major version of the Aras installation.
    /// </value>
    public Version Version { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArasHttpConnection"/> class.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="innovatorServerUrl">The innovator server URL.</param>
    /// <param name="itemFactory">The item factory.</param>
    public ArasHttpConnection(HttpClient service, string innovatorServerUrl, IItemFactory itemFactory)
    {
      Service = service;
      this.Compression = CompressionType.none;
      AmlContext = new ElementFactory(_context, itemFactory);

      if (innovatorServerUrl.EndsWith("Server/InnovatorServer.aspx", StringComparison.OrdinalIgnoreCase))
      {
        innovatorServerUrl = innovatorServerUrl.Substring(0, innovatorServerUrl.Length - 20);
      }
      else if (innovatorServerUrl.EndsWith("/Server", StringComparison.OrdinalIgnoreCase)
        || innovatorServerUrl.EndsWith("/Server/", StringComparison.OrdinalIgnoreCase))
      {
        innovatorServerUrl += (innovatorServerUrl.EndsWith("/") ? "" : "/");
      }
      else
      {
        innovatorServerUrl += (innovatorServerUrl.EndsWith("/") ? "" : "/") + "Server/";
      }

      this.Url = new Uri(innovatorServerUrl);
      this._innovatorServerUrl = new Uri(this.Url, "InnovatorServer.aspx");
      this._innovatorClientBin = new Uri(this.Url, "../Client/cbin/");

      _vaultConn = new ArasVaultConnection(this, Service);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArasHttpConnection"/> class using an existing <see cref="IConnection"/>.
    /// The existing credential headers should be transferred to the new service in the calling code.
    /// Optionally also copy the session cookies to re-use the existing session and prevent a login.
    /// </summary>
    /// <param name="existingConnection">The existing connection</param>
    /// <param name="service">The service.</param>
    /// <param name="innovatorServerUrl">The innovator server URL.</param>
    /// <param name="itemFactory">The item factory.</param>
    /// <remarks>Primarily used in the server environment to get a new connection that won't be rolled back when the transaction encounters an error.</remarks>
    public ArasHttpConnection(IConnection existingConnection, HttpClient service, string innovatorServerUrl, IItemFactory itemFactory) : this(service, innovatorServerUrl, itemFactory)
    {
      Database = existingConnection.Database;
      UserId = existingConnection.UserId;
    }

    /// <summary>
    /// Process a command by crafting the appropriate HTTP request and returning the HTTP response stream
    /// </summary>
    public Stream Process(Command request)
    {
      if (string.IsNullOrEmpty(Database))
        throw new Exception("You are no longer connected to Aras. Please log in again.");

      var upload = request as UploadCommand;
      if (upload == null)
      {
        if (request.Action == CommandAction.DownloadFile)
          return _vaultConn.Download(request, false).Value;

        return UploadAml(_innovatorServerUrl, request.Action.ToString(), request, false).Value.AsStream;
      }
      return Process(request, false).Value;
    }

    /// <summary>
    /// Process a command asynchronously by crafting the appropriate HTTP request and returning the HTTP response stream
    /// </summary>
    /// <param name="request">The query to execute</param>
    /// <param name="async">Whether the query should be executed asynchronously</param>
    public IPromise<Stream> Process(Command request, bool async)
    {
      var upload = request as UploadCommand;
      if (upload == null)
      {
        if (request.Action == CommandAction.DownloadFile)
          return _vaultConn.Download(request, async);

        return UploadAml(_innovatorServerUrl, request.Action.ToString(), request, async)
          .Convert(r => r.AsStream);
      }
      else if (request.Action == CommandAction.DownloadFile)
      {
        throw new ArgumentException("Cannot download a file with an upload request.");
      }

      // Files need to be uploaded, so build the vault request
      return _vaultConn.Upload(upload, async);
    }

    /// <summary>
    /// Creates an upload request used for uploading files to the server
    /// </summary>
    /// <returns>
    /// A new upload request used for uploading files to the server
    /// </returns>
    public UploadCommand CreateUploadCommand()
    {
      return _vaultConn.CreateUploadCommand();
    }

    /// <summary>
    /// Returns a set of databases which can be connected to using this URL
    /// </summary>
    /// <returns>
    /// A set of databases which can be connected to using this URL
    /// </returns>
    public IEnumerable<string> GetDatabases()
    {
      IHttpResponse resp;
      using (var trace = new LogData(4, "Innovator: Get database list", Factory.LogListener)
      {
        { "url", this.Url },
        { "version", Version }
      })
      {
        resp = Service.GetPromise(new Uri(this.Url, "DBList.aspx"), false, trace).Wait();
      }
      using (var reader = XmlReader.Create(resp.AsStream))
      {
        while (reader.Read())
        {
          if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "DB"
            && !string.IsNullOrEmpty(reader.GetAttribute("id")))
          {
            yield return reader.GetAttribute("id");
          }
        }
      }
    }

    /// <summary>
    /// Log in to the database
    /// </summary>
    /// <param name="credentials">Credentials used for authenticating to the instance</param>
    public void Login(ICredentials credentials)
    {
      // Access the value property to force throwing any appropriate exception
      var result = Login(credentials, false).Value;
    }

    private IPromise<IAuthenticator> GetAuthenticator(ICredentials credentials, bool async)
    {
      if (_authenticator != null)
        return Promises.Resolved(_authenticator);
      return AuthenticationFactory.GetAuthenticator(Url, Service, credentials, async)
        .Done(a => _authenticator = a);
    }

    /// <summary>
    /// Log in to the database asynchronosuly
    /// </summary>
    /// <param name="credentials">Credentials used for authenticating to the instance</param>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>
    /// A promise to return the user ID as a string
    /// </returns>
    /// <exception cref="NotSupportedException">This connection implementation does not support the specified credential type</exception>
    public IPromise<string> Login(ICredentials credentials, bool async)
    {
      _lastCredentials = credentials;
      return GetAuthenticator(credentials, async)
        .Continue(a => a.GetAuthHeaders(async)) // Cache the credential information
        .Continue((IEnumerable<KeyValuePair<string, string>> a) => LoginAmlCall(async))
        .Done(a =>
        {
          Database = credentials.Database;
          if (string.IsNullOrEmpty(_httpUsername))
          {
            if (credentials is IUserCredentials uCred && !string.IsNullOrEmpty(uCred.Username))
              _httpUsername = uCred.Username;
          }
        });
    }

    private IPromise<string> LoginAmlCall(bool async)
    {
      var result = new Promise<string>();
      result.CancelTarget(
        Process(new Command("<Item/>").WithAction(CommandAction.ValidateUser), async)
          .Progress(result.Notify)
          .Done(r =>
          {
            string xml;
            using (var reader = new StreamReader(r))
            {
              xml = reader.ReadToEnd();
            }

            var root = XElement.Parse(xml);
            var data = root.DescendantsAndSelf("Result").FirstOrDefault();
            var afNs = (XNamespace)"http://www.aras.com/InnovatorFault";
            var authNode = root.DescendantsAndSelf(afNs + "supported_authentication_schema").FirstOrDefault();
            if (authNode != null
              && authNode.Element(afNs + "schema")?.Attribute("mode")?.Value == "SHA256"
              && _authenticator is LegacyAuthenticator legacy)
            {
              legacy.HashFunction = ElementFactory.Local.CalcSha256;
              // Switch from MD5 hashing to SHA256
              LoginAmlCall(async)
                .Done(result.Resolve)
                .Fail(result.Reject);
            }
            else if (data == null)
            {
              var res = ElementFactory.Local.FromXml(xml);
              var ex = res.Exception ?? ElementFactory.Local.ServerException("Failed to login");
              ex.SetDetails(Database, "<Item/>");

              Database = null;
              _httpUsername = null;
              result.Reject(ex);
            }
            else
            {
              foreach (var elem in data.Elements())
              {
                switch (elem.Name.LocalName)
                {
                  case "id":
                    UserId = elem.Value;
                    break;
                  case "login_name":
                    _httpUsername = elem.Value;
                    break;
                  case "database":
                    Database = elem.Value;
                    break;
                  case "i18nsessioncontext":
                    _context.DefaultLanguageCode = elem.Element("default_language_code").Value;
                    _context.DefaultLanguageSuffix = elem.Element("default_language_suffix").Value;
                    _context.LanguageCode = elem.Element("language_code").Value;
                    _context.LanguageSuffix = elem.Element("language_suffix").Value;
                    _context.Locale = elem.Element("locale").Value;
                    _context.TimeZone = elem.Element("time_zone").Value;
                    break;
                  // Since some version in v11, ServerInfo is not returned with ValidateUser
                  // This leaves Version as null
                  case "ServerInfo":
                    foreach (var info in elem.Elements())
                    {
                      if (info.Name.LocalName == "Version")
                        Version = new Version(info.Value);

                      if (!string.IsNullOrEmpty(elem.Value))
                        _serverInfo.Add(new KeyValuePair<string, string>("ServerInfo/" + elem.Name.LocalName, elem.Value));
                    }
                    break;
                  default:
                    if (!string.IsNullOrEmpty(elem.Value))
                      _serverInfo.Add(new KeyValuePair<string, string>(elem.Name.LocalName, elem.Value));
                    break;
                }
              }

              _vaultConn.InitializeStrategy();
              result.Resolve(UserId);
            }
          }).Fail(ex =>
          {
            Database = null;
            _httpUsername = null;
            result.Reject(ex);
          }));
      return result;
    }

    /// <summary>
    /// Log out of the database
    /// </summary>
    /// <param name="unlockOnLogout">Whether to unlock locked items while logging out</param>
    public void Logout(bool unlockOnLogout)
    {
      Logout(unlockOnLogout, false);
    }

    /// <summary>
    /// Log out of the database
    /// </summary>
    /// <param name="unlockOnLogout">Whether to unlock locked items while logging out</param>
    /// <param name="async">Whether to perform this action asynchronously</param>
    public void Logout(bool unlockOnLogout, bool async)
    {
      if (!string.IsNullOrEmpty(Database) && !string.IsNullOrEmpty(_httpUsername))
      {
        Process(new Command("<logoff skip_unlock=\"" + (unlockOnLogout ? 0 : 1) + "\"/>").WithAction(CommandAction.LogOff), async)
          .Done(r =>
          {
            _context = null;
            AmlContext = null;
            Database = null;
            _httpUsername = null;
            UserId = null;
          });
      }
    }

    /// <summary>
    /// Use a method to configure each outgoing HTTP request
    /// </summary>
    /// <param name="settings">Action used to configure the request</param>
    public void DefaultSettings(Action<IHttpRequest> settings)
    {
      _defaults.Add(settings);
    }

    /// <summary>
    /// Sets the vault strategy.
    /// </summary>
    /// <param name="strategy">The strategy.</param>
    public void SetVaultStrategy(IVaultStrategy strategy)
    {
      _vaultConn.VaultStrategy = strategy;
      if (!string.IsNullOrEmpty(UserId)) _vaultConn.InitializeStrategy();
    }

    private IPromise<IHttpResponse> UploadAml(Uri uri, string action, Command request, bool async, HttpClient service = null)
    {
      var req = new HttpRequest()
      {
        Content = new SimpleContent("<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + request.ToNormalizedAml(AmlContext.LocalizationContext), "text/xml")
        {
          Compression = Compression
        }
      };
      ((IArasConnection)this).SetDefaultHeaders(req.SetHeader);
      foreach (var a in _defaults)
      {
        a.Invoke(req);
      }
      request.Settings?.Invoke(req);
      if (!string.IsNullOrEmpty(action)) req.SetHeader("SOAPACTION", action);

      var trace = new LogData(4
        , "Innovator: Execute query"
        , request.LogListener ?? Factory.LogListener
        , request.Parameters)
      {
        { "database", Database },
        { "query", request.Aml },
        { "soap_action", action },
        { "url", uri },
        { "user_name", _httpUsername },
        { "user_id", UserId },
        { "version", Version }
      };
      return (service ?? Service).PostPromise(uri, async, req, trace).Always(trace.Dispose);
    }

    void IArasConnection.SetDefaultHeaders(Action<string, string> writer)
    {
      // Support a missing authenticator when a connection has been cloned and authentication is handled outside of Innovator.Client
      if (_authenticator != null)
      {
        foreach (var kvp in _authenticator.GetAuthHeaders(false).Value)
          writer(kvp.Key, kvp.Value);
      }
      writer.Invoke("LOCALE", this._context.Locale);
      writer.Invoke("TIMEZONE_NAME", this._context.TimeZone);
    }

    /// <summary>
    /// Causes the connection to logout.
    /// </summary>
    /// <see cref="Logout(bool)"/>
    public void Dispose()
    {
      if (!string.IsNullOrEmpty(UserId)) Logout(true);
    }

    /// <summary>
    /// Expands a relative URL to a full URL
    /// </summary>
    /// <param name="relativeUrl">The relative URL</param>
    /// <returns>
    /// A full URL relative to the connection
    /// </returns>
    public string MapClientUrl(string relativeUrl)
    {
      return new Uri(this._innovatorClientBin, relativeUrl).ToString();
    }

    List<Action<IHttpRequest>> IArasConnection.DefaultSettings
    {
      get { return _defaults; }
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
      var conn = obj as ArasHttpConnection;
      if (conn == null) return false;
      return Equals(conn);
    }

    /// <summary>
    /// Determines whether the specified <see cref="ArasHttpConnection" />, is equal to this instance.
    /// </summary>
    /// <param name="conn">The <see cref="ArasHttpConnection" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="ArasHttpConnection" /> is equal to this instance 
    ///   (same URL, database, and user ID); otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(ArasHttpConnection conn)
    {
      return conn.Url.Equals(this.Url)
        && string.Equals(conn.Database, this.Database)
        && string.Equals(conn.UserId, this.UserId);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      return this.Url.GetHashCode()
        ^ (Database ?? "").GetHashCode()
        ^ (UserId ?? "").GetHashCode();
    }

    /// <summary>
    /// Gets a new connection logged in with the same credentials
    /// </summary>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>
    /// A promise to return a new connection
    /// </returns>
    public IPromise<IRemoteConnection> Clone(bool async)
    {
      var newConn = new ArasHttpConnection(Service, _innovatorServerUrl.ToString(), AmlContext.ItemFactory)
      {
        _defaults = this._defaults
      };
      return newConn.Login(_lastCredentials, async)
        .Convert(u => (IRemoteConnection)newConn);
    }

    /// <summary>
    /// Hashes the credentials for use with logging in or workflow voting
    /// </summary>
    /// <param name="credentials">The credentials.</param>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>
    /// A promise to return hashed credentials
    /// </returns>
    public IPromise<ExplicitHashCredentials> HashCredentials(ICredentials credentials, bool async)
    {
      return _authenticator.HashCredentials(credentials, async);
    }

    /// <summary>
    /// Hashes the credentials for use with logging in or workflow voting
    /// </summary>
    /// <param name="credentials">The credentials.</param>
    /// <returns>
    /// Hashed credentials
    /// </returns>
    public ExplicitHashCredentials HashCredentials(ICredentials credentials)
    {
      return _authenticator.HashCredentials(credentials, false).Value;
    }
  }
}
