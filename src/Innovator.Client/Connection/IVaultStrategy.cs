using System.Collections.Generic;

namespace Innovator.Client.Connection
{
  /// <summary>
  /// Represents a class with logic for determining which vaults to read from and write
  /// to for a given <see cref="IAsyncConnection"/>
  /// </summary>
  public interface IVaultStrategy
  {
    /// <summary>
    /// Initializes the stategy object with the specified connection and vault factory
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="factory">The vault factory.</param>
    void Initialize(IAsyncConnection conn, IVaultFactory factory);
    /// <summary>
    /// Asynchronously returns a list of vaults to write to in priority order
    /// </summary>
    /// <param name="async">if set to <c>true</c>, execute asyncronously.  
    /// Otherwise, a resolved promise is returned</param>
    /// <returns>A list of vaults to write to in priority order</returns>
    IPromise<IEnumerable<Vault>> WritePriority(bool async);
    /// <summary>
    /// Asynchronously returns a list of vaults to read from in priority order
    /// </summary>
    /// <param name="async">if set to <c>true</c>, execute asyncronously.  
    /// Otherwise, a resolved promise is returned</param>
    /// <returns>A list of vaults to read from in priority order</returns>
    IPromise<IEnumerable<Vault>> ReadPriority(bool async);
  }
}
