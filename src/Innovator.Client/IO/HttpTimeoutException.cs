using System;
using System.Collections.Generic;
using System.Linq;
#if SERIALIZATION
using System.Runtime.Serialization;
#endif
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  /// <summary>
  /// Exception that occurs when an HTTP request times out.
  /// </summary>
#if SERIALIZATION
  [Serializable]
#endif
  public class HttpTimeoutException : Exception
  {
    private IHttpResponse _resp;

    /// <summary>
    /// Gets the HTTP server response (if any).
    /// </summary>
    /// <value>
    /// The HTTP server response.
    /// </value>
    public IHttpResponse Response
    {
      get { return _resp; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpTimeoutException"/> class.
    /// </summary>
    public HttpTimeoutException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public HttpTimeoutException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public HttpTimeoutException(string message, Exception innerException) : base(message, innerException) { }
#if SERIALIZATION
    public HttpTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    internal HttpTimeoutException(IHttpResponse resp) : base(resp.StatusCode.ToString())
    {
      _resp = resp;
    }

  }
}
