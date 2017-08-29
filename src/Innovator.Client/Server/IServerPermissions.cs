using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Innovator.Client;

namespace Innovator.Server
{
  /// <summary>
  /// Retreives information about a user's permissions
  /// </summary>
  public interface IServerPermissions
  {
    /// <summary>
    /// Gets a value indicating whether this user is the root user or a admin.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this user is the root user or a admin; otherwise, <c>false</c>.
    /// </value>
    bool IsRootOrAdmin { get; }
    /// <summary>
    /// Gets a list of identities that the current user is in.
    /// </summary>
    /// <returns>A list of identities that the current user is in.</returns>
    IEnumerable<string> Identities();
    /// <summary>
    /// Gets a list of identities that the user <paramref name="userId"/> is in.
    /// </summary>
    /// <param name="userId">ID of the user to check</param>
    /// <returns>A list of identities that the user <paramref name="userId"/> is in.</returns>
    IEnumerable<string> Identities(string userId);
    /// <summary>
    /// Adds the <paramref name="identName"/> identity to the identity list of the current connection
    /// </summary>
    /// <param name="identName">Name of the identity to add</param>
    /// <returns>A <see cref="IDisposable"/> object.  Calling <see cref="IDisposable.Dispose"/> will 
    /// return the identity list to its original state</returns>
    IDisposable Escalate(string identName);
  }
}
