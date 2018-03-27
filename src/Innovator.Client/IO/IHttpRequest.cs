using System;

namespace Innovator.Client
{
  /// <summary>
  /// HTTP request being sent to a server (Aras, Proxy, etc.)
  /// </summary>
  public interface IHttpRequest
  {
    /// <summary>
    /// HTTP request timeout
    /// </summary>
    TimeSpan Timeout { get; set; }

    /// <summary>
    /// User-Agent string to send with the request
    /// </summary>
    string UserAgent { get; set; }

    /// <summary>
    /// Set a request header value
    /// </summary>
    void SetHeader(string name, string value);
  }
}
