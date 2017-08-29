using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Innovator.Client;

namespace Innovator.Server
{
  public static class Extensions
  {
    /// <summary>
    /// Adds "Administrators" to the identity list of the current connection
    /// </summary>
    /// <param name="conn">The connection to escalate.</param>
    /// <returns>A <see cref="IDisposable"/> object.  Calling <see cref="IDisposable.Dispose"/> will 
    /// return the identity list to its original state</returns>
    public static IDisposable AsAdmin(this IServerConnection conn)
    {
      return conn.Permissions.Escalate("Administrators");
    }
    /// <summary>
    /// Adds "ArasPlm" to the identity list of the current connection
    /// </summary>
    /// <param name="conn">The connection to escalate.</param>
    /// <returns>A <see cref="IDisposable"/> object.  Calling <see cref="IDisposable.Dispose"/> will 
    /// return the identity list to its original state</returns>
    public static IDisposable AsArasPlm(this IServerConnection conn)
    {
      return conn.Permissions.Escalate("ArasPlm");
    }
  }
}
