using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Innovator.Client
{
  /// <summary>
  /// Headers to send with each request to Aras
  /// </summary>
  public class ArasHeaders : IDictionary<string, string>
  {
    private readonly Dictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the header with the specified name.
    /// </summary>
    /// <value>
    /// The header value.
    /// </value>
    /// <param name="key">The name of the header.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets the number of headers contained in the <see cref="ArasHeaders" />.
    /// </summary>
    public int Count { get { return _headers.Count; } }
    /// <summary>
    /// Returns <c>false</c> to indicate that the collection is editable
    /// </summary>
    public bool IsReadOnly { get { return false; } }
    /// <summary>
    /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="ArasHeaders" />.
    /// </summary>
    public ICollection<string> Keys { get { return _headers.Keys; } }
    /// <summary>
    /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values of the <see cref="ArasHeaders" />.
    /// </summary>
    public ICollection<string> Values { get { return _headers.Values; } }

    /// <summary>
    /// Adds a header with the provided name and value to the <see cref="ArasHeaders" />.
    /// </summary>
    /// <param name="key">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <exception cref="ArgumentException"></exception>
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
    /// <summary>
    /// Removes all headers from the <see cref="ArasHeaders" />.
    /// </summary>
    public void Clear()
    {
      _headers.Clear();
    }

    /// <summary>
    /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
    /// <returns>
    /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
    /// </returns>
    public bool ContainsKey(string key)
    {
      return _headers.ContainsKey(key);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An enumerator that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
      return _headers.GetEnumerator();
    }

    /// <summary>
    /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>
    /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
    /// </returns>
    public bool Remove(string key)
    {
      return _headers.Remove(key);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
    /// <returns>
    /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
    /// </returns>
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
