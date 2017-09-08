using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// Data from an HTTP response (including the headers and body)
  /// </summary>
  public interface IHttpResponse
  {
    /// <summary>
    /// Gets the headers from the response
    /// </summary>
    /// <value>
    /// The headers from the response.
    /// </value>
    IDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the status code.
    /// </summary>
    /// <value>
    /// The status code.
    /// </value>
    HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the body as a stream.
    /// </summary>
    /// <value>
    /// The response body as a stream.
    /// </value>
    Stream AsStream { get; }
  }
}
