using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  public class ArasHeaders : IDictionary<string, string>
  {
    private Dictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string this[string key]
    {
      get
      {
        string result;
        if (_headers.TryGetValue(key, out result))
          return result;
        return null;
      }
      set { _headers[key] = value; }
    }

    /// <summary>
    /// Locale to use when logging in
    /// </summary>
    public string Locale
    {
      get { return this["LOCALE"]; }
      set { this["LOCALE"] = value; }
    }
    /// <summary>
    /// Time zone to use with each request
    /// </summary>
    public string TimeZone
    {
      get { return this["TIMEZONE_NAME"]; }
      set { this["TIMEZONE_NAME"] = value; }
    }
    /// <summary>
    /// User agent string to send with each request
    /// </summary>
    public string UserAgent
    {
      get { return this["User-Agent"]; }
      set { this["User-Agent"] = value; }
    }

    public int Count { get { return _headers.Count; } }
    public bool IsReadOnly { get { return false; } }
    public ICollection<string> Keys { get { return _headers.Keys; } }
    public ICollection<string> Values { get { return _headers.Values; } }

    public void Add(string key, string value)
    {
      try
      {
        _headers.Add(key, value);
      }
      catch (ArgumentException ex)
      {
        throw new ArgumentException(string.Format("An element with the key '{0}' already exists", key), ex);
      }
    }

    public void Clear()
    {
      _headers.Clear();
    }

    public bool ContainsKey(string key)
    {
      return _headers.ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
      return _headers.GetEnumerator();
    }

    public bool Remove(string key)
    {
      return _headers.Remove(key);
    }

    public bool TryGetValue(string key, out string value)
    {
      return _headers.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
    {
      Add(item.Key, item.Value);
    }
    bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
    {
      return _headers.ContainsKey(item.Key);
    }
    void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<string, string>>)_headers).CopyTo(array, arrayIndex);
    }
    bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
    {
      return _headers.Remove(item.Key);
    }

    internal IEnumerable<KeyValuePair<string, string>> NonUserAgentHeaders()
    {
      return this.Where(k => !string.Equals(k.Key, "User-Agent", StringComparison.OrdinalIgnoreCase));
    }
  }
}
