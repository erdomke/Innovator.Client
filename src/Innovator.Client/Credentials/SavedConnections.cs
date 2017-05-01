using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
#if SECURESTRING
using System.Security.Cryptography;
#endif

namespace Innovator.Client
{
  public class SavedConnections : IEnumerable<ConnectionPreferences>
  {
    private const string DefaultConnection = "__DEFAULT__";
    private const string ConnectionData = "ConnectionData";
    private const string ConnectionName = "ConnectionName";

    private XDocument _doc;
    private XElement _root;

    public ConnectionPreferences this[string name]
    {
      get
      {
        var elem = ByName(name);
        if (elem == null)
          return null;
        return new ConnectionPreferences(elem.WriteTo);
      }
      set
      {
        var elem = ByName(name);
        if (elem == null)
        {
          elem = new XElement(ConnectionData);
          _root.Add(elem);
        }
        Serialize(name, value, elem);
      }
    }

    public ConnectionPreferences Default
    {
      get { return this[DefaultConnection]; }
      set { this[DefaultConnection] = value; }
    }
    public IEnumerable<string> Keys
    {
      get { return Applicable().Select(e => (string)e.Element(ConnectionName)); }
    }

    public SavedConnections()
      : this(XDocument.Parse("<ConnectionLibrary><Connections></Connections></ConnectionLibrary>")) { }

    private SavedConnections(XDocument doc)
    {
      _doc = doc;
      _root = doc.Root.EnsureElement("Connections");
    }

    public void Add(ConnectionPreferences value)
    {
      this[value.Name ?? DefaultConnection] = value;
    }

    public XmlReader CreateReader()
    {
      return _doc.CreateReader();
    }

    public IEnumerator<ConnectionPreferences> GetEnumerator()
    {
      return Applicable()
        .Select(elem => new ConnectionPreferences(elem.WriteTo))
        .GetEnumerator();
    }

    public void Save(Stream stream)
    {
#if NET35
      using (var textWriter = new StreamWriter(stream))
      {
        _doc.Save(textWriter);
      }
#else
      _doc.Save(stream);
#endif
    }
    public void Save(TextWriter textWriter)
    {
      _doc.Save(textWriter);
    }
    public void Save(XmlWriter writer)
    {
      _doc.Save(writer);
    }

    public override string ToString()
    {
      return _doc.ToString();
    }

    public bool TryGetValue(string name, out ConnectionPreferences value)
    {
      value = this[name];
      return value != null;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private XElement ByName(string name)
    {
      return Applicable()
        .FirstOrDefault(e => string.Equals((string)e.Element(ConnectionName), name, StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<XElement> Applicable()
    {
      return _root
        .Elements(ConnectionData)
        .Where(e => (string)e.Element("Type") == null || (string)e.Element("Type") == "Innovator");
    }

    private void Serialize(string name, ConnectionPreferences prefs, XElement elem)
    {
      elem.EnsureElement(ConnectionName).Value = name ?? prefs.Name;

      var buffer = elem.EnsureElement("Database");
      if (prefs.Credentials != null)
        buffer.Value = prefs.Credentials.Database;

      var explicitCred = prefs.Credentials as ExplicitCredentials;
      buffer = elem.EnsureElement("Password");
      if (explicitCred != null)
        buffer.Value = EncryptWindows(explicitCred.Password) ?? "";

      elem.EnsureElement("Url").Value = prefs.Url;
      buffer = elem.EnsureElement("UserName");
      if (explicitCred != null)
        buffer.Value = explicitCred.Username;

      buffer = elem.EnsureElement("Color");
      if (string.IsNullOrEmpty(buffer.Value))
        buffer.Value = "#0000FF";

      elem.EnsureElement("Type").Value = "Innovator";
      elem.EnsureElement("Authentication").Value =
        prefs.Credentials is AnonymousCredentials
        ? "Anonymous"
        : (prefs.Credentials is WindowsCredentials
          ? "Windows" : "Explicit");
      elem.EnsureElement("Confirm").Value = "False";
      buffer = elem.EnsureElement("Timeout");
      if (prefs.DefaultTimeout.HasValue)
        buffer.Value = prefs.DefaultTimeout.ToString();

      buffer = elem.EnsureElement("Params");
      foreach (var header in prefs.Headers.NonUserAgentHeaders())
      {
        var headerElem = buffer.EnsureElement("Param");
        headerElem.SetAttributeValue("name", header.Key);
        headerElem.Value = header.Value;
      }
    }

    private string EncryptWindows(SecureToken data)
    {
      if (data == null)
        return null;

#if SECURESTRING
      return Convert.ToBase64String(ProtectedData.Protect(data.UseString((ref string s) => Encoding.UTF8.GetBytes(s))
        , ConnectionPreferences.Salt, DataProtectionScope.CurrentUser));
#else
      return null;
#endif
    }

#if ENVIRONMENT
    public static SavedConnections Load()
    {
      var path = DefaultPath();
      if (File.Exists(path))
        return Load(path);
      return new SavedConnections();
    }
    public void Save()
    {
      var path = DefaultPath();
      var dir = Path.GetDirectoryName(path);
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);

      using (var file = new FileStream(path, FileMode.Create))
      {
        Save(file);
      }
    }

    private static string DefaultPath()
    {
      var path = @"{0}\{1}\connections.xml";
      return string.Format(path, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Innovator Admin");
    }
#endif

    public static SavedConnections Load(Stream stream)
    {
#if NET35
      using (var textReader = new StreamReader(stream))
      {
        return new SavedConnections(XDocument.Load(textReader));
      }
#else
      return new SavedConnections(XDocument.Load(stream));
#endif
    }
    public static SavedConnections Load(TextReader textReader)
    {
      return new SavedConnections(XDocument.Load(textReader));
    }
    public static SavedConnections Load(string uri)
    {
      return new SavedConnections(XDocument.Load(uri));
    }
    public static SavedConnections Parse(string uri)
    {
      return new SavedConnections(XDocument.Parse(uri));
    }
  }
}
