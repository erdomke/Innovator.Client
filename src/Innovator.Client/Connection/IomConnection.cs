using Innovator.Client.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

#if XMLLEGACY
namespace Innovator.Client
{
  public class IomConnection : IConnection, IArasConnection
  {
    private int _arasVersion;
    private Action<string, XmlDocument, XmlDocument> _callAction;
    private ServerContext _context = new ServerContext(false);
    private List<Action<IHttpRequest>> _defaults = new List<Action<IHttpRequest>>();
    private ElementFactory _factory;
    protected string _httpDatabase;
    protected string _httpPassword;
    protected string _httpUsername;
    private string _userId;
    private ArasVaultConnection _vaultConn;
    protected Uri _innovatorClientBin;
    protected Uri _requestUrl;
    protected object _iomConnection;

    public IomConnection(object iomConnection) : this (iomConnection, Factory.DefaultItemFactory) { }
    public IomConnection(object iomConnection, IItemFactory itemFactory)
    {
      _iomConnection = iomConnection;
      var type = iomConnection.GetType();
      var noArgs = new object[] { };

      _arasVersion = type.Assembly.GetName().Version.Major;
      _httpDatabase = (string)type.GetMethod("GetDatabaseName").Invoke(iomConnection, noArgs);
      _callAction = (Action<string, XmlDocument, XmlDocument>)Delegate.CreateDelegate(typeof(Action<string, XmlDocument, XmlDocument>), iomConnection, type.GetMethod("CallAction"));

      var validateXml = (string)type.GetMethod("GetValidateUserXmlResult").Invoke(iomConnection, noArgs);
      if (!string.IsNullOrEmpty(validateXml))
      {
        var data = XElement.Parse(validateXml).DescendantsAndSelf("Result").FirstOrDefault();
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
              }
              break;
          }
        }
      }

      _factory = new ElementFactory(_context, itemFactory);
      _vaultConn = new ArasVaultConnection(this);
    }

    public ElementFactory AmlContext { get { return _factory; } }
    public CompressionType Compression { get { return CompressionType.none; } }
    public string Database { get { return _httpDatabase; } }
    List<Action<IHttpRequest>> IArasConnection.DefaultSettings { get { return _defaults; } }
    public string UserId { get { return _userId; } }
    public int Version { get { return _arasVersion; } }

    public UploadCommand CreateUploadCommand()
    {
      return new UploadCommand(_vaultConn.VaultStrategy.WritePriority(false).Value.First());
    }

    public string MapClientUrl(string relativeUrl)
    {
      LazyLoadCreds();
      return new Uri(this._innovatorClientBin, relativeUrl).ToString();
    }

    public Stream Process(Command request)
    {
      var upload = request as UploadCommand;
      if (upload == null)
      {
        if (request.Action == CommandAction.DownloadFile)
          return _vaultConn.Download(request, false).Value;

        var input = new XmlDocument();
        input.LoadXml(request.ToNormalizedAml(_factory.LocalizationContext));
        var output = new XmlDocument();

        _callAction(request.ActionString, input, output);

        var ms = new MemoryTributary();
        using (var writer = XmlWriter.Create(ms, new XmlWriterSettings() { CloseOutput = false }))
        {
          output.WriteTo(writer);
        }
        ms.Position = 0;
        return ms;
      }

      // Files need to be uploaded, so build the vault request
      return _vaultConn.Upload(upload, false).Value;
    }

    public IPromise<Stream> Process(Command request, bool async)
    {
      return Promises.Resolved(Process(request));
    }

    void IArasConnection.SetDefaultHeaders(Action<string, string> writer)
    {
      LazyLoadCreds();
      writer.Invoke("AUTHUSER", this._httpUsername);
      writer.Invoke("AUTHPASSWORD", this._httpPassword);
      writer.Invoke("DATABASE", this._httpDatabase);
      writer.Invoke("LOCALE", this._context.Locale);
      writer.Invoke("TIMEZONE_NAME", this._context.TimeZone);
    }

    protected virtual void LazyLoadCreds()
    {
      if (string.IsNullOrEmpty(_httpUsername))
      {
        var type = _iomConnection.GetType();

        try
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
        catch (Exception) { }
      }
    }
  }
}
#endif
