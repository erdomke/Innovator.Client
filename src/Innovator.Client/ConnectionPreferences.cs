using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Xml;
#if SECURESTRING
using System.Security.Cryptography;
#endif

namespace Innovator.Client
{
  /// <summary>
  /// Preferences for connection behavior
  /// </summary>
  public class ConnectionPreferences
  {
    private ArasHeaders _headers;

    /// <summary>
    /// Allow a connection to a URL with a mapping file to send login requests to an authentication
    /// service
    /// </summary>
    public bool AllowAuthPreCheck { get; set; }
    /// <summary>
    /// Default credentials used for immediate log in
    /// </summary>
    public ICredentials Credentials { get; set; }
    /// <summary>
    /// Default timeout in milliseconds
    /// </summary>
    public int? DefaultTimeout { get; set; }
    public ArasHeaders Headers { get { return _headers; } }
    public HttpClient HttpService { get; set; }
    public IItemFactory ItemFactory { get; set; }
    public string Name { get; set; }
    /// <summary>
    /// The URL to use if not otherwise specified
    /// </summary>
    public string Url { get; set; }

    public ConnectionPreferences()
    {
      _headers = new ArasHeaders();
      this.AllowAuthPreCheck = true;
    }
    public ConnectionPreferences(Action<XmlWriter> xml) : this()
    {
      var writer = new PreferencesWriter(this);
      xml.Invoke(writer);
      writer.Flush();
    }

    private class PreferencesWriter : XmlWriter
    {
      private Dictionary<string, string> _attrs = new Dictionary<string, string>();
      private Dictionary<string, string> _elemBuffer = new Dictionary<string, string>();
      private Stack<string> _names = new Stack<string>();
      private StringBuilder _buffer = new StringBuilder();
      private ConnectionPreferences _prefs;

      public PreferencesWriter(ConnectionPreferences prefs)
      {
        _prefs = prefs;
      }

      public override WriteState WriteState
      {
        get { return WriteState.Start; }
      }

      public override void Flush()
      {
        if (_elemBuffer.Count > 0)
        {
          string authString;
          string db;
          string username;
          string password;

          if (!_elemBuffer.TryGetValue("Authentication", out authString))
            authString = "Explicit";
          var auth = (Authentication)Enum.Parse(typeof(Authentication), authString);
          _elemBuffer.TryGetValue("Database", out db);
          _elemBuffer.TryGetValue("UserName", out username);
          _elemBuffer.TryGetValue("Password", out password);

          if (!string.IsNullOrEmpty(db))
          {
            switch (auth)
            {
              case Authentication.Anonymous:
                _prefs.Credentials = new AnonymousCredentials(db);
                break;
              case Authentication.Windows:
                _prefs.Credentials = new WindowsCredentials(db);
                break;
              default:
                SecureToken token;
                if (TryDecryptWindows(password, out token))
                {
                  _prefs.Credentials = new ExplicitCredentials(db, username, token);
                }
                else
                {
                  _prefs.Credentials = new ExplicitCredentials(db, username, "");
                }
                break;
            }
          }

          _elemBuffer.Clear();
        }
      }

      public override string LookupPrefix(string ns)
      {
        return string.Empty;
      }

      public override void WriteBase64(byte[] buffer, int index, int count)
      {
        // Do nothing
      }

      public override void WriteCData(string text)
      {
        _buffer.Append(text);
      }

      public override void WriteCharEntity(char ch)
      {
        _buffer.Append(ch);
      }

      public override void WriteChars(char[] buffer, int index, int count)
      {
        _buffer.Append(buffer, index, count);
      }

      public override void WriteComment(string text)
      {
        // do nothing
      }

      public override void WriteDocType(string name, string pubid, string sysid, string subset)
      {
        // do nothing
      }

      public override void WriteEndAttribute()
      {
        _attrs[_names.Pop()] = _buffer.ToString();
        _buffer.Length = 0;
      }

      public override void WriteEndDocument()
      {
        // do nothing
      }

      public override void WriteEndElement()
      {
        var name = _names.Pop();
        switch (name)
        {
          case "ConnectionName":
            _prefs.Name = _buffer.ToString();
            break;
          case "Url":
            _prefs.Url = _buffer.ToString();
            break;
          case "Timeout":
            int timeout;
            if (_buffer.Length > 0 && int.TryParse(_buffer.ToString(), out timeout))
              _prefs.DefaultTimeout = timeout;
            break;
          case "Database":
          case "Authentication":
          case "UserName":
          case "Password":
            _elemBuffer[name] = _buffer.ToString();
            break;
          case "Param":
            _prefs.Headers[_attrs["name"]] = _buffer.ToString();
            break;
        }
        _attrs.Clear();
      }

      public override void WriteEntityRef(string name)
      {
        // do nothing
      }

      public override void WriteFullEndElement()
      {
        WriteEndElement();
      }

      public override void WriteProcessingInstruction(string name, string text)
      {
        // do nothing
      }

      public override void WriteRaw(string data)
      {
        _buffer.Append(data);
      }

      public override void WriteRaw(char[] buffer, int index, int count)
      {
        _buffer.Append(buffer, index, count);
      }

      public override void WriteStartAttribute(string prefix, string localName, string ns)
      {
        _names.Push(localName);
        _buffer.Length = 0;
      }

      public override void WriteStartDocument()
      {
        // Do nothing
      }

      public override void WriteStartDocument(bool standalone)
      {
        // Do nothing
      }

      public override void WriteStartElement(string prefix, string localName, string ns)
      {
        _names.Push(localName);
        _buffer.Length = 0;
      }

      public override void WriteString(string text)
      {
        _buffer.Append(text);
      }

      public override void WriteSurrogateCharEntity(char lowChar, char highChar)
      {
        // Do nothing
      }

      public override void WriteWhitespace(string ws)
      {
        _buffer.Append(ws);
      }

#if XMLLEGACY
    public override void Close()
    {
      // Do nothing
    }
#endif

      private bool TryDecryptWindows(string encrypted, out SecureToken decrypted)
      {
        decrypted = string.Empty;
#if SECURESTRING
      try
      {
        var data = Convert.FromBase64String(encrypted);
        var decryptedData = ProtectedData.Unprotect(data, Salt, DataProtectionScope.CurrentUser);
        var chars = Encoding.UTF8.GetChars(decryptedData);
        decrypted = new SecureToken(ref chars);
        return true;
      }
      catch (System.Security.Cryptography.CryptographicException)
      {
        return false;
      }
      catch (FormatException)
      {
        return false;
      }
#else
        return false;
#endif
      }
    }

    internal static readonly byte[] Salt = new byte[] { 0x4f, 0xbe, 0x6e, 0x2e, 0x27, 0x5e, 0xdf, 0x7a, 0xec, 0x62, 0x76, 0xfa, 0xa4, 0xee, 0xd8, 0xd3, 0xdf, 0x12, 0x33, 0xb7, 0xfb, 0xf4, 0x81, 0xe6 };
  }
}
