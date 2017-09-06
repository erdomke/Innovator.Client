using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Connection
{
  /// <summary>
  /// A class which creates <see cref="Vault"/> objects from AML data
  /// </summary>
  /// <remarks>The default implementation caches <see cref="Vault"/> objects so
  /// that there won't be two instances in memory for the same Aras vault</remarks>
  public interface IVaultFactory
  {
    /// <summary>
    /// Gets the <see cref="Vault"/> object from AML data.
    /// </summary>
    /// <param name="item">The AML item.</param>
    /// <returns>The <see cref="Vault"/> object from AML data.</returns>
    Vault GetVault(IReadOnlyItem item);
  }
}
