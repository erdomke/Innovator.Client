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
    private CompressionType _compression;
    private readonly HttpClient _service;
    private int _arasVersion;
    private ServerContext _context = new ServerContext(false);
    private ElementFactory _factory;
    private string _httpDatabase;
    private string _httpPassword;
    private string _httpUsername;
    private ICredentials _lastCredentials;
    private readonly Uri _innovatorServerBaseUrl;
    private readonly Uri _innovatorServerUrl;
    private readonly Uri _innovatorClientBin;
    private string _userId;
    private List<Action<IHttpRequest>> _defaults = new List<Action<IHttpRequest>>();
    private readonly ArasVaultConnection _vaultConn;
    private readonly List<KeyValuePair<string, string>> _serverInfo = new List<KeyValuePair<string, string>>();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
      get
      {
        return string.Format("[Connection] {0} | {1} | {2}", _httpUsername, _httpDatabase, _innovatorServerBaseUrl);
      }
    }

    /// <summary>
    /// AML context used for creating AML objects and formatting AML statements
    /// </summary>
    public ElementFactory AmlContext
    {
      get { return _factory; }
    }

    /// <summary>
    /// Gets the compression setting to use when sending requests to the server.
    /// </summary>
    /// <value>
    /// The compression setting to use when sending requests to the server.
    /// </value>
    public CompressionType Compression
    {
      get { return _compression; }
      set { _compression = value; }
    }

    /// <summary>
    /// Name of the connected database
    /// </summary>
    public string Database
    {
      get { return _httpDatabase; }
    }

    /// <summary>
    /// Gets the server information returned when logging in.
    /// </summary>
    /// <value>
    /// The server information.
    /// </value>
    public IEnumerable<KeyValuePair<string, string>> ServerInfo
    {
      get { return _serverInfo; }
    }

    /// <summary>
    /// URL that the instance resides at
    /// </summary>
    public Uri Url
    {
      get { return _innovatorServerBaseUrl; }
    }

    /// <summary>
    /// ID of the authenticated user
    /// </summary>
    public string UserId
    {
      get { return _userId; }
    }

    /// <summary>
    /// Gets the major version of the Aras installation
    /// </summary>
    /// <value>
    /// The major version of the Aras installation.
    /// </value>
    public int Version
    {
      get { return _arasVersion; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArasHttpConnection"/> class.
    /// </summary>
    /// <param name="service">The service.</param>
    /// <param name="innovatorServerUrl">The innovator server URL.</param>
    /// <param name="itemFactory">The item factory.</param>
    public ArasHttpConnection(HttpClient service, string innovatorServerUrl, IItemFactory itemFactory)
    {
      _service = service;
      this.Compression = CompressionType.none;
      _factory = new ElementFactory(_context, itemFactory);

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

      this._innovatorServerBaseUrl = new Uri(innovatorServerUrl);
      this._innovatorServerUrl = new Uri(this._innovatorServerBaseUrl, "InnovatorServer.aspx");
      this._innovatorClientBin = new Uri(this._innovatorServerBaseUrl, "../Client/cbin/");

      _vaultConn = new ArasVaultConnection(this);
    }

    /// <summary>
    /// Process a command by crafting the appropriate HTTP request and returning the HTTP response stream
    /// </summary>
    public Stream Process(Command request)
    {
      if (string.IsNullOrEmpty(_httpDatabase))
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
      return new UploadCommand(_vaultConn.VaultStrategy.WritePriority(false).Value.First());
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
        { "url", this._innovatorServerBaseUrl },
        { "version", _arasVersion }
      })
      {
        resp = _service.GetPromise(new Uri(this._innovatorServerBaseUrl, "DBList.aspx"), false, trace).Wait();
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
      var explicitCred = credentials as ExplicitCredentials;
      var hashCred = credentials as ExplicitHashCredentials;
      var winCred = credentials as WindowsCredentials;
      var unHashedPassword = default(SecureToken);

      IPromise<bool> authProcess;
      if (explicitCred != null)
      {
        _httpDatabase = explicitCred.Database;
        _httpUsername = explicitCred.Username;
        unHashedPassword = explicitCred.Password;
        _httpPassword = ElementFactory.Local.CalcMd5(explicitCred.Password);
        authProcess = Promises.Resolved(true);
      }
      else if (hashCred != null)
      {
        _httpDatabase = hashCred.Database;
        _httpUsername = hashCred.Username;
        _httpPassword = hashCred.PasswordHash;
        authProcess = Promises.Resolved(true);
      }
      else if (winCred != null)
      {
        var waLoginUrl = new Uri(this._innovatorClientBin, "../scripts/IOMLogin.aspx");
        _httpDatabase = winCred.Database;
        authProcess = UploadAml(waLoginUrl, "", "<Item />", async)
          .Convert(r =>
          {
            var res = r.AsXml().DescendantsAndSelf("Result").FirstOrDefault();
            _httpUsername = res.Element("user").Value;
            var pwd = res.Element("password").Value;
            if (pwd.IsNullOrWhiteSpace())
              throw new ArgumentException("Failed to authenticate with Innovator server '" + _innovatorServerUrl + "'. Original error: " + _httpUsername, "credentials");
            var needHash = res.Element("hash").Value;
            if (string.Equals(needHash.Trim(), "false", StringComparison.OrdinalIgnoreCase))
            {
              _httpPassword = pwd;
            }
            else
            {
              unHashedPassword = pwd;
              _httpPassword = ElementFactory.Local.CalcMd5(pwd);
            }
            return true;
          });
      }
      else
      {
        throw new NotSupportedException("This connection implementation does not support the specified credential type");
      }

      _lastCredentials = credentials;

      var result = new Promise<string>();
      result.CancelTarget(
        authProcess.Continue(_ =>
          Process(new Command("<Item/>").WithAction(CommandAction.ValidateUser), async)
        )
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
            if (authNode != null && unHashedPassword != null
              && authNode.Element(afNs + "schema")?.Attribute("mode")?.Value == "SHA256"
              && _httpPassword?.Length == 32)
            {
              // Switch from MD5 hashing to SHA256
              Login(new ExplicitHashCredentials(_httpDatabase, _httpUsername, ElementFactory.Local.CalcSha256(unHashedPassword)), true)
                .Done(result.Resolve)
                .Fail(result.Reject);
            }
            else if (data == null)
            {
              var res = ElementFactory.Local.FromXml(xml);
              var ex = res.Exception ?? ElementFactory.Local.ServerException("Failed to login");
              ex.SetDetails(_httpDatabase, "<Item/>");

              _httpDatabase = null;
              _httpUsername = null;
              _httpPassword = null;
              result.Reject(ex);
            }
            else
            {
              foreach (var elem in data.Elements())
              {
                switch (elem.Name.LocalName)
                {
                  case "id":
                    _userId = elem.Value;
                    break;
                  case "i18nsessioncontext":
                    _context.DefaultLanguageCode = elem.Element("default_language_code").Value;
                    _context.DefaultLanguageSuffix = elem.Element("default_language_suffix").Value;
                    _context.LanguageCode = elem.Element("language_code").Value;
                    _context.LanguageSuffix = elem.Element("language_suffix").Value;
                    _context.Locale = elem.Element("locale").Value;
                    _context.TimeZone = elem.Element("time_zone").Value;
                    break;
                  case "ServerInfo":
                    foreach (var info in elem.Elements())
                    {
                      if (info.Name.LocalName == "Version")
                        _arasVersion = int.Parse(info.Value.Substring(0, info.Value.IndexOf('.')));

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
              result.Resolve(_userId);
            }
          }).Fail(ex =>
          {
            _httpDatabase = null;
            _httpUsername = null;
            _httpPassword = null;
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
      Process(new Command("<logoff skip_unlock=\"" + (unlockOnLogout ? 0 : 1) + "\"/>").WithAction(CommandAction.LogOff), async)
        .Done(r =>
        {
          _context = null;
          _factory = null;
          _httpDatabase = null;
          _httpPassword = null;
          _httpUsername = null;
          _userId = null;
        });
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
      if (!string.IsNullOrEmpty(_userId)) _vaultConn.InitializeStrategy();
    }

    private IPromise<IHttpResponse> UploadAml(Uri uri, string action, Command request, bool async)
    {
      var req = new HttpRequest()
      {
        Content = new SimpleContent("<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + request.ToNormalizedAml(_factory.LocalizationContext), "text/xml")
        {
          Compression = _compression
        }
      };
      ((IArasConnection)this).SetDefaultHeaders((k, v) => { req.SetHeader(k, v); });
      foreach (var a in _defaults)
      {
        a.Invoke(req);
      }
      if (request.Settings != null) request.Settings.Invoke(req);
      if (!string.IsNullOrEmpty(action)) req.SetHeader("SOAPACTION", action);

      var trace = new LogData(4
        , "Innovator: Execute query"
        , request.LogListener ?? Factory.LogListener
        , request.Parameters)
      {
        { "database", _httpDatabase },
        { "query", request.Aml },
        { "soap_action", action },
        { "url", uri },
        { "user_name", _httpUsername },
        { "user_id", _userId },
        { "version", _arasVersion }
      };
      return _service.PostPromise(uri, async, req, trace).Always(trace.Dispose);
    }

    internal IPromise<string> GetResult(string action, string request, bool async)
    {
      var result = new Promise<string>();
      result.CancelTarget(
        UploadAml(_innovatorServerUrl, "ApplyItem", request, async)
          .Progress((p, m) => result.Notify(p, m))
          .Done(r =>
          {
            var res = _factory.FromXml(r.AsString(), request, this);
            if (res.Exception == null)
            {
              result.Resolve(res.Value);
            }
            else
            {
              result.Reject(res.Exception);
            }
          }).Fail(ex => { result.Reject(ex); }));
      return result;
    }

    void IArasConnection.SetDefaultHeaders(Action<string, string> writer)
    {
      writer.Invoke("AUTHUSER", this._httpUsername);
      writer.Invoke("AUTHPASSWORD", this._httpPassword);
      writer.Invoke("DATABASE", this._httpDatabase);
      writer.Invoke("LOCALE", this._context.Locale);
      writer.Invoke("TIMEZONE_NAME", this._context.TimeZone);
    }

    /// <summary>
    /// Causes the connection to logout.
    /// </summary>
    /// <see cref="Logout(bool)"/>
    public void Dispose()
    {
      if (!string.IsNullOrEmpty(_userId)) Logout(true);
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
      return conn._innovatorServerBaseUrl.Equals(this._innovatorServerBaseUrl)
        && String.Equals(conn._httpDatabase, this._httpDatabase)
        && String.Equals(conn._userId, this._userId);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      return this._innovatorServerBaseUrl.GetHashCode()
        ^ (_httpDatabase ?? "").GetHashCode()
        ^ (_userId ?? "").GetHashCode();
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
      var newConn = new ArasHttpConnection(Factory.DefaultService.Invoke(), _innovatorServerUrl.ToString(), _factory.ItemFactory);
      newConn._defaults = this._defaults;
      return newConn.Login(_lastCredentials, async)
        .Convert(u => (IRemoteConnection)newConn);
    }
  }
}
