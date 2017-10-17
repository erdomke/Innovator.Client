namespace Innovator.Client.Connection
{
  /// <summary>
  /// A class which creates <see cref="Vault"/> objects from AML data
  /// </summary>
  /// <remarks>This implementation caches <see cref="Vault"/> objects so
  /// that there won't be two instances in memory for the same Aras vault</remarks>
  /// <seealso cref="Innovator.Client.Connection.IVaultFactory" />
  public class DefaultVaultFactory : IVaultFactory
  {
    private readonly object _lock = new object();
    private Vault _last;

    /// <summary>
    /// Gets the <see cref="Vault" /> object from AML data.
    /// </summary>
    /// <param name="item">The AML item.</param>
    /// <returns>The <see cref="Vault" /> object from AML data.</returns>
    public Vault GetVault(IReadOnlyItem item)
    {
      if (item == null || !item.Exists) return null;

      var vault = LinkedListOps.Find(_last, item.Id());

      if (vault == null)
      {
        lock (_lock)
        {
          vault = LinkedListOps.Find(_last, item.Id());

          if (vault == null)
          {
            vault = new Vault(item);
            LinkedListOps.Add(ref _last, vault);
          }
        }
      }

      return vault;
    }
  }
}
