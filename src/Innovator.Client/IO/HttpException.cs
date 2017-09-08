using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
#if SERIALIZATION
using System.Runtime.Serialization;
#endif
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// Exception that occurs when sending an HTTP request to the server.
  /// </summary>
#if SERIALIZATION
  [Serializable]
#endif
  public class HttpException : Exception
  {
    private IHttpResponse _resp;

    /// <summary>
    /// Gets the HTTP server response.
    /// </summary>
    /// <value>
    /// The HTTP server response.
    /// </value>
    public IHttpResponse Response
    {
      get { return _resp; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpException"/> class.
    /// </summary>
    public HttpException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public HttpException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public HttpException(string message, Exception innerException) : base(message, innerException) { }
#if SERIALIZATION
    public HttpException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    internal HttpException(IHttpResponse resp) : base(resp.StatusCode.ToString())
    {
      _resp = resp;
    }

  }
}
