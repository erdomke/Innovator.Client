using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
#if SECURESTRING
using System.Security.Cryptography;
#endif

namespace Innovator.Client
{
  /// <summary>
  /// Access information about connection preferences that has been persisted to disk
  /// </summary>
  /// <example>
  /// Create a new connection using the default stored connection
  /// <code lang="C#">
  /// var pref = SavedConnections.Load().Default;
  /// var conn = Factory.GetConnection(pref);
  /// </code>
  /// </example>
  public class SavedConnections : IEnumerable<ConnectionPreferences>
  {
    private const string DefaultConnection = "__DEFAULT__";
    private const string ConnectionData = "ConnectionData";
    private const string ConnectionName = "ConnectionName";

    private readonly XDocument _doc;
    private readonly XElement _root;

    /// <summary>
    /// Gets or sets the <see cref="ConnectionPreferences"/> with the specified name.
    /// </summary>
    /// <value>
    /// The <see cref="ConnectionPreferences"/>.
    /// </value>
    /// <param name="name">The name.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets or sets the default preference.
    /// </summary>
    /// <value>
    /// The default preference.
    /// </value>
    public ConnectionPreferences Default
    {
      get { return this[DefaultConnection]; }
      set { this[DefaultConnection] = value; }
    }
    /// <summary>
    /// Gets the names of all applicable preferences.
    /// </summary>
    /// <value>
    /// The names of all applicable preferences.
    /// </value>
    public IEnumerable<string> Keys
    {
      get { return Applicable().Select(e => (string)e.Element(ConnectionName)); }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SavedConnections"/> class.
    /// </summary>
    public SavedConnections()
      : this(XDocument.Parse("<ConnectionLibrary><Connections></Connections></ConnectionLibrary>")) { }

    private SavedConnections(XDocument doc)
    {
      _doc = doc;
      _root = doc.Root.EnsureElement("Connections");
    }

    /// <summary>
    /// Adds the specified <see cref="ConnectionPreferences"/>.
    /// </summary>
    /// <param name="value">The <see cref="ConnectionPreferences"/> to store.</param>
    public void Add(ConnectionPreferences value)
    {
      this[value.Name ?? DefaultConnection] = value;
    }

    /// <summary>
    /// Creates a <see cref="XmlReader"/> for reading the preference data.
    /// </summary>
    /// <returns>An <see cref="XmlReader"/> for reading the preference data</returns>
    public XmlReader CreateReader()
    {
      return _doc.CreateReader();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<ConnectionPreferences> GetEnumerator()
    {
      return Applicable()
        .Select(elem => new ConnectionPreferences(elem.WriteTo))
        .GetEnumerator();
    }

    /// <summary>
    /// Saves the preferences to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to save the preferences to.</param>
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
    /// <summary>
    /// Saves the preferences to the specified <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="textWriter">The <see cref="TextWriter"/> to save the preferences to.</param>
    public void Save(TextWriter textWriter)
    {
      _doc.Save(textWriter);
    }
    /// <summary>
    /// Saves the preferences to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to save the preferences to.</param>
    public void Save(XmlWriter writer)
    {
      _doc.Save(writer);
    }

    /// <summary>
    /// Returns an XML <see cref="System.String" /> that represents serialized data.
    /// </summary>
    /// <returns>
    /// A XML <see cref="System.String" /> that represents serialized data.
    /// </returns>
    public override string ToString()
    {
      return _doc.ToString();
    }

    /// <summary>
    /// Tries the get a <see cref="ConnectionPreferences"/> by name.
    /// </summary>
    /// <param name="name">The name of the <see cref="ConnectionPreferences"/>.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> if the <see cref="ConnectionPreferences"/> was found, <c>false</c> otherwise</returns>
    public bool TryGetValue(string name, out ConnectionPreferences value)
    {
      value = this[name];
      return value != null;
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
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
      if (prefs.Credentials is IUserCredentials userCred)
        buffer.Value = userCred.Username;

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

#if PROTECTEDDATA
      return Convert.ToBase64String(ProtectedData.Protect(data.UseString((ref string s) => Encoding.UTF8.GetBytes(s))
        , ConnectionPreferences.Salt, DataProtectionScope.CurrentUser));
#else
      return null;
#endif
    }

#if ENVIRONMENT    
    /// <summary>
    /// Loads connection data from the default file system path.
    /// </summary>
    /// <returns>A new <see cref="SavedConnections"/> instance</returns>
    public static SavedConnections Load()
    {
      var path = DefaultPath();
      if (File.Exists(path))
        return Load(path);
      return new SavedConnections();
    }
    /// <summary>
    /// Saves this connection data to the default file system path.
    /// </summary>
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
    /// <summary>
    /// Initializes a new <see cref="SavedConnections"/> instance from the <paramref name="stream"/> data.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to load data from.</param>
    /// <returns>A new <see cref="SavedConnections"/> instance</returns>
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
    /// <summary>
    /// Initializes a new <see cref="SavedConnections"/> instance from the <paramref name="textReader"/> data.
    /// </summary>
    /// <param name="textReader">The <see cref="TextReader"/> to load data from.</param>
    /// <returns>A new <see cref="SavedConnections"/> instance</returns>
    public static SavedConnections Load(TextReader textReader)
    {
      return new SavedConnections(XDocument.Load(textReader));
    }
    /// <summary>
    /// Initializes a new <see cref="SavedConnections"/> instance from the <paramref name="uri"/>.
    /// </summary>
    /// <param name="uri">The URI to load data from.</param>
    /// <returns>A new <see cref="SavedConnections"/> instance</returns>
    public static SavedConnections Load(string uri)
    {
      return new SavedConnections(XDocument.Load(uri));
    }
    /// <summary>
    /// Initializes a new <see cref="SavedConnections"/> instance from the <paramref name="xml"/>.
    /// </summary>
    /// <param name="xml">The XML data to parse.</param>
    /// <returns>A new <see cref="SavedConnections"/> instance</returns>
    public static SavedConnections Parse(string xml)
    {
      return new SavedConnections(XDocument.Parse(xml));
    }
  }
}
