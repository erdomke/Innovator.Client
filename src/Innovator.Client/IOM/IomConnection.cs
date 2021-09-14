using Innovator.Client.Connection;
using Innovator.Server;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

#if XMLLEGACY
namespace Innovator.Client.IOM
{
  /// <summary>
  /// Wraps an IOM connection instance with the <see cref="IServerConnection"/> interface
  /// </summary>
  /// <seealso cref="Innovator.Server.IServerConnection" />
  /// <seealso cref="Innovator.Client.Connection.IArasConnection" />
  public class IomConnection : IServerConnection, IArasConnection
  {
    private readonly Action<string, XmlDocument, XmlDocument> _callAction;
    private readonly List<Action<IHttpRequest>> _defaults = new List<Action<IHttpRequest>>();
    protected string _httpDatabase;
    protected string _httpPassword;
    protected string _httpUsername;
    private readonly ArasVaultConnection _vaultConn;
    protected Uri _innovatorClientBin;
    protected Uri _requestUrl;
    protected object _iomConnection;

    private NameValueCollection _headers;
    private object _cco;
    private IServerCache _appCache;
    private IServerCache _requestCache;
    private IServerCache _sessionCache;
    private IServerPermissions _perm;

    /// <summary>
    /// Initializes a new instance of the <see cref="IomConnection"/> class.
    /// </summary>
    /// <param name="iomConnection">The IOM connection instance which minimally implements 
    /// <c>Aras.IOM.IServerConnection</c>.</param>
    public IomConnection(object iomConnection) : this(iomConnection, Factory.DefaultItemFactory, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IomConnection"/> class.
    /// </summary>
    /// <param name="iomConnection">The IOM connection instance which minimally implements 
    /// <c>Aras.IOM.IServerConnection</c>.</param>
    /// <param name="itemFactory">The item factory.</param>
    public IomConnection(object iomConnection, IItemFactory itemFactory) : this(iomConnection, itemFactory, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IomConnection"/> class.
    /// </summary>
    /// <param name="iomConnection">The IOM connection instance which minimally implements 
    /// <c>Aras.IOM.IServerConnection</c>.</param>
    /// <param name="itemFactory">The item factory.</param>
    /// <param name="CCO">The call context object.</param>
    public IomConnection(object iomConnection, IItemFactory itemFactory, object CCO)
    {
      _iomConnection = iomConnection;
      _cco = CCO;
      var type = iomConnection.GetType();
      var noArgs = new object[] { };

      Version = type.Assembly.GetName().Version;
      _httpDatabase = (string)type.GetMethod("GetDatabaseName").Invoke(iomConnection, noArgs);
      _callAction = (Action<string, XmlDocument, XmlDocument>)Delegate.CreateDelegate(typeof(Action<string, XmlDocument, XmlDocument>), iomConnection, type.GetMethod("CallAction"));

      var validateXml = (string)type.GetMethod("GetValidateUserXmlResult").Invoke(iomConnection, noArgs);
      var context = new ServerContext(false);
      if (!string.IsNullOrEmpty(validateXml))
      {
        var data = XElement.Parse(validateXml).DescendantsAndSelf("Result").FirstOrDefault();
        foreach (var elem in data.Elements())
        {
          switch (elem.Name.LocalName)
          {
            case "id":
              UserId = elem.Value;
              break;
            case "i18nsessioncontext":
              context.DefaultLanguageCode = elem.Element("default_language_code").Value;
              context.DefaultLanguageSuffix = elem.Element("default_language_suffix").Value;
              context.LanguageCode = elem.Element("language_code").Value;
              context.LanguageSuffix = elem.Element("language_suffix").Value;
              context.Locale = elem.Element("locale").Value;
              context.TimeZone = elem.Element("time_zone").Value;
              break;
            case "ServerInfo":
              foreach (var info in elem.Elements())
              {
                if (info.Name.LocalName == "Version")
                  Version = new Version(info.Value);
              }
              break;
          }
        }
      }

      AmlContext = new ElementFactory(context, itemFactory);
      _vaultConn = new ArasVaultConnection(this);
    }

    /// <summary>
    /// AML context used for creating AML objects and formatting AML statements
    /// </summary>
    public ElementFactory AmlContext { get; }

    /// <summary>
    /// Gets the compression setting to use when sending requests to the server.
    /// </summary>
    /// <value>
    /// The compression setting to use when sending requests to the server.
    /// </value>
    public CompressionType Compression { get { return CompressionType.none; } }

    /// <summary>
    /// Name of the connected database
    /// </summary>
    public string Database { get { return _httpDatabase; } }

    /// <summary>
    /// Gets the default settings for configuring the HTTP request.
    /// </summary>
    /// <value>
    /// The default settings for configuring the HTTP request.
    /// </value>
    List<Action<IHttpRequest>> IArasConnection.DefaultSettings { get { return _defaults; } }

    /// <summary>
    /// ID of the authenticated user
    /// </summary>
    public string UserId { get; }

    /// <summary>
    /// Gets the major version of the Aras installation
    /// </summary>
    /// <value>
    /// The major version of the Aras installation.
    /// </value>
    public Version Version { get; }

    /// <summary>
    /// Creates an upload request used for uploading files to the server
    /// </summary>
    /// <returns>
    /// A new upload request used for uploading files to the server
    /// </returns>
    public UploadCommand CreateUploadCommand()
    {
      var version = Version ?? new Version(255, 0);
      if (version.Major > 9)
        return new TransactionalUploadCommand(this, _vaultConn.VaultStrategy.WritePriority(false).Value.First());
      return new NontransactionalUploadCommand(this, _vaultConn.VaultStrategy.WritePriority(false).Value.First());
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
      LazyLoadCreds();
      return new Uri(this._innovatorClientBin, relativeUrl).ToString();
    }

    /// <summary>
    /// Calls a SOAP action asynchronously
    /// </summary>
    /// <param name="request">Request AML and possibly files <see cref="UploadCommand" /></param>
    /// <returns>
    /// An XML SOAP response as a string
    /// </returns>
    public Stream Process(Command request)
    {
      return Process(request, false).Value;
    }

    /// <summary>
    /// Calls a SOAP action asynchronously
    /// </summary>
    /// <param name="request">Request AML and possibly files <see cref="UploadCommand" /></param>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>
    /// A promise to return an XML SOAP response as a string
    /// </returns>
    public IPromise<Stream> Process(Command request, bool async)
    {
      var upload = request as UploadCommand;
      if (upload == null)
      {
        if (request.Action == CommandAction.DownloadFile)
          return _vaultConn.Download(request, async);

        var input = new XmlDocument();
        input.LoadXml(request.ToNormalizedAml(AmlContext.LocalizationContext));
        var output = new XmlDocument();

        using (new LogData(4
          , "Innovator: Execute query"
          , request.LogListener ?? Factory.LogListener
          , request.Parameters)
        {
          { "database", _httpDatabase },
          { "query", request.Aml },
          { "soap_action", request.ActionString },
          { "url", _requestUrl },
          { "user_name", _httpUsername },
          { "user_id", UserId },
          { "version", Version }
        })
        {
          _callAction(request.ActionString, input, output);
        }

        return Promises.Resolved<Stream>(new XmlStream(() => new XmlNodeReader(output)));
      }

      // Files need to be uploaded, so build the vault request
      return _vaultConn.Upload(upload, async);
    }

    /// <summary>
    /// Sets the default headers for an AML request. (e.g. AUTHUSER, AUTHPASSWORD,
    /// DATABASE, LOCALE, TIMEZONE_NAME);
    /// </summary>
    /// <param name="writer">The method to call with each header.</param>
    void IArasConnection.SetDefaultHeaders(Action<string, string> writer)
    {
      LazyLoadCreds();
      writer.Invoke("AUTHUSER", this._httpUsername);
      writer.Invoke("AUTHPASSWORD", this._httpPassword);
      writer.Invoke("DATABASE", this._httpDatabase);
      writer.Invoke("LOCALE", this.AmlContext.LocalizationContext.Locale);
      writer.Invoke("TIMEZONE_NAME", this.AmlContext.LocalizationContext.TimeZone);
    }

    public IPromise<ExplicitHashCredentials> HashCredentials(ICredentials credentials, bool async)
    {
      var explicitCred = credentials as ExplicitCredentials;
      var hashCred = credentials as ExplicitHashCredentials;

      if (explicitCred != null)
      {
        return Promises.Resolved(new ExplicitHashCredentials(explicitCred.Database, explicitCred.Username, ElementFactory.Local.CalcMd5(explicitCred.Password)));
      }
      else if (hashCred != null)
      {
        return Promises.Resolved(hashCred);
      }
      else
      {
        throw new NotSupportedException("This connection implementation does not support the specified credential type");
      }
    }

    public ExplicitHashCredentials HashCredentials(ICredentials credentials)
    {
      return HashCredentials(credentials, false).Value;
    }

    public IPromise<Version> FetchVersion(bool async)
    {
      return Promises.Resolved(this.Version);
    }

#region "Server Connection"    
    /// <summary>
    /// Gets the in-memory application-wide cache.
    /// </summary>
    /// <value>
    /// The application cache.
    /// </value>
    public IServerCache ApplicationCache
    {
      get
      {
        if (_cco == null)
          return null;
        if (_appCache != null)
          return _appCache;

        LazyLoadCreds();
        var context = _cco.GetType().GetProperty("Context").GetValue(_cco, null);
        var cache = context.GetType().GetProperty("Cache").GetValue(context, null);
        var indexProp = cache.GetType()
          .GetProperties()
          .FirstOrDefault(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string));

        var getter = (Func<string, object>)Delegate.CreateDelegate(typeof(Func<string, object>), cache, indexProp.GetGetMethod());
        var setter = (Action<string, object>)Delegate.CreateDelegate(typeof(Action<string, object>), cache, indexProp.GetSetMethod());
        _appCache = new Cache(getter, setter);
        return _appCache;
      }
    }

    /// <summary>
    /// Gets the information about the current user's permissions.
    /// </summary>
    /// <value>
    /// The permissions.
    /// </value>
    public IServerPermissions Permissions
    {
      get
      {
        if (_perm != null)
          return _perm;

        LazyLoadCreds();
        _perm = new ServerPermissions(_cco);
        return _perm;
      }
    }

    /// <summary>
    /// Gets the in-memory request-specific cache.
    /// </summary>
    /// <value>
    /// The request cache.
    /// </value>
    public IServerCache RequestState
    {
      get
      {
        if (_cco == null)
          return null;
        if (_requestCache != null)
          return _requestCache;

        LazyLoadCreds();
        var session = _cco.GetType().GetProperty("RequestState").GetValue(_cco, null);
        var indexProp = session.GetType()
          .GetProperties()
          .FirstOrDefault(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string));

        var getter = (Func<string, object>)Delegate.CreateDelegate(typeof(Func<string, object>), session, indexProp.GetGetMethod());
        var setter = (Action<string, object>)Delegate.CreateDelegate(typeof(Action<string, object>), session, indexProp.GetSetMethod());
        _requestCache = new Cache(getter, setter);
        return _requestCache;
      }
    }

    /// <summary>
    /// Gets the requested URL.
    /// </summary>
    /// <value>
    /// The requested URL.
    /// </value>
    public string RequestUrl
    {
      get
      {
        LazyLoadCreds();
        return _requestUrl.ToString();
      }
    }

    /// <summary>
    /// Gets the in-memory session-specific cache.
    /// </summary>
    /// <value>
    /// The session cache.
    /// </value>
    public IServerCache SessionCache
    {
      get
      {
        if (_cco == null)
          return null;
        if (_sessionCache != null)
          return _sessionCache;


        LazyLoadCreds();
        var session = _cco.GetType().GetProperty("Session").GetValue(_cco, null);
        var indexProp = session.GetType()
          .GetProperties()
          .FirstOrDefault(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(string));

        var getter = (Func<string, object>)Delegate.CreateDelegate(typeof(Func<string, object>), session, indexProp.GetGetMethod());
        var setter = (Action<string, object>)Delegate.CreateDelegate(typeof(Action<string, object>), session, indexProp.GetSetMethod());
        _sessionCache = new Cache(getter, setter);
        return _sessionCache;
      }
    }

    /// <summary>
    /// Gets the HTTP header by name.
    /// </summary>
    /// <param name="name">The HTTP name.</param>
    /// <returns>
    /// The value of the HTTP header
    /// </returns>
    public string GetHeader(string name)
    {
      LazyLoadCreds();
      return _headers[name];
    }


    private class Cache : IServerCache
    {
      private readonly Func<string, object> _getter;
      private readonly Action<string, object> _setter;

      public Cache(Func<string, object> getter, Action<string, object> setter)
      {
        _getter = getter;
        _setter = setter;
      }

      public object this[string key]
      {
        get { return _getter(key); }
        set { _setter(key, value); }
      }

      public T Get<T>(string key)
      {
        return (T)_getter(key);
      }
    }

    private class ServerPermissions : IServerPermissions
    {
      private bool? _isRootOrAdmin;
      private readonly object _cco;

      public ServerPermissions(object cco)
      {
        _cco = cco;
      }

      public bool IsRootOrAdmin
      {
        get
        {
          if (_isRootOrAdmin.HasValue)
            return _isRootOrAdmin.Value;

          var permissions = _cco.GetType().GetProperty("Permissions").GetValue(_cco, null);
          _isRootOrAdmin = (bool)permissions.GetType().GetMethod("UserHasRootOrAdminIdentity").Invoke(permissions, new object[] { });
          return _isRootOrAdmin.Value;
        }
      }

      public IDisposable Escalate(params string[] identNames)
      {
        return new Escalate(identNames);
      }

      public IEnumerable<string> Identities()
      {
        var perm = Type.GetType("Aras.Server.Security.Permissions").GetProperty("Current").GetValue(null, null);
        return ((string)perm.GetType().GetProperty("IdentitiesList").GetValue(null, null)).Split(',');
      }

      public IEnumerable<string> Identities(string userId)
      {
        var var = _cco.GetType().GetProperty("Variables").GetValue(_cco, null);
        var db = var.GetType().GetProperty("InnDatabase").GetValue(var, null);
        var idents = Type.GetType("Aras.Server.Security.Permissions").GetMethod("GetIdentitiesList");
        return ((string)idents.Invoke(null, new object[] { db, userId })).Split(',');
      }
    }

    private class Escalate : IDisposable
    {
      private List<object> _grantedIdents = new List<object>();

      public Escalate(params string[] identNames)
      {
        var grantIdent = Type.GetType("Aras.Server.Security.Permissions").GetMethod("GrantIdentity");
        var getIdent = Type.GetType("Aras.Server.Security.Identity").GetMethod("GetByName");

        foreach (var name in identNames)
        {
          var ident = getIdent.Invoke(null, new object[] { name });
          grantIdent.Invoke(null, new object[] { ident });
          _grantedIdents.Add(ident);
        }
      }

      public void Dispose()
      {
        var revokeIdent = Type.GetType("Aras.Server.Security.Permissions").GetMethod("RevokeIdentity");

        foreach (var ident in _grantedIdents)
        {
          revokeIdent.Invoke(null, new object[] { ident });
        }
        _grantedIdents.Clear();
      }
    }

#endregion

    protected virtual void LazyLoadCreds()
    {
      if (!string.IsNullOrEmpty(_httpUsername))
        return;

      var type = _iomConnection.GetType();
      var ccoProp = type.GetProperty("CCO");
      if (ccoProp == null)
      {
        var prop = type.GetProperty("UserName", BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null)
        {
          _httpUsername = (string)prop.GetValue(_iomConnection, null);
          prop = type.GetProperty("UserPassword", BindingFlags.NonPublic | BindingFlags.Instance);
          _httpPassword = (string)prop.GetValue(_iomConnection, null);
        }

        var field = type.GetField("innovator_server_base_url", BindingFlags.NonPublic | BindingFlags.Instance)
          ?? type.GetField("InnovatorServerBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
          _innovatorClientBin = new Uri(new Uri((string)field.GetValue(_iomConnection)), "../Client/cbin/");
        }

      }
      else
      {
        var noArgs = new object[] { };
        _cco = ccoProp.GetValue(_iomConnection, null);
        var vars = _cco.GetType().GetProperty("Variables").GetValue(_cco, null);
        _httpPassword = (string)vars.GetType().GetMethod("GetUserPassword").Invoke(vars, noArgs);
        _httpUsername = (string)vars.GetType().GetMethod("GetLoginName").Invoke(vars, noArgs);

        var request = _cco.GetType().GetProperty("Request").GetValue(_cco, null);
        _requestUrl = (Uri)request.GetType().GetProperty("Url").GetValue(request, null);
        _innovatorClientBin = new Uri(_requestUrl, "../Client/cbin/");
        _headers = (NameValueCollection)request.GetType().GetProperty("Headers").GetValue(request, null);
      }
    }
  }
}
#endif
