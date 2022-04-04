using Innovator.Client;
using System;

namespace Innovator.Server
{
  /// <summary>
  /// Represents a connection to Aras Innovator.  It is used for code running
  /// within the Innovator server process that needs access to extra server 
  /// information
  /// </summary>
  public interface IServerConnection : IConnection
  {
    /// <summary>
    /// Gets the in-memory application-wide cache.
    /// </summary>
    /// <value>
    /// The application cache.
    /// </value>
    IServerCache ApplicationCache { get; }
    /// <summary>
    /// Gets the string representing original AML request which eventually
    /// resulted in the execution of the current code
    /// </summary>
    /// <value>
    /// The original AML request.
    /// </value>
    [Obsolete("The original request can not be retrieved starting between v11sp12 and v12sp09. The source InputStream is emptied after Innovator reads the incoming AML.")]
    string OriginalRequest { get; }
    /// <summary>
    /// Gets the information about the current user's permissions.
    /// </summary>
    /// <value>
    /// The permissions.
    /// </value>
    IServerPermissions Permissions { get; }
    /// <summary>
    /// Gets the in-memory request-specific cache.
    /// </summary>
    /// <value>
    /// The request cache.
    /// </value>
    IServerCache RequestState { get; }
    /// <summary>
    /// Gets the requested URL.
    /// </summary>
    /// <value>
    /// The requested URL.
    /// </value>
    string RequestUrl { get; }
    /// <summary>
    /// Gets the in-memory session-specific cache.
    /// </summary>
    /// <value>
    /// The session cache.
    /// </value>
    IServerCache SessionCache { get; }

    /// <summary>
    /// Gets the HTTP header by name.
    /// </summary>
    /// <param name="name">The HTTP name.</param>
    /// <returns>The value of the HTTP header</returns>
    string GetHeader(string name);
  }
}
