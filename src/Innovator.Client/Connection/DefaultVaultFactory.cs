using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Connection
{
  public class DefaultVaultFactory : IVaultFactory
  {
    private object _lock = new object();
    private Vault _last;

    public Vault GetVault(IReadOnlyItem i)
    {
      if (i == null || !i.Exists) return null;

      var vault = LinkedListOps.Find(_last, i.Id());

      if (vault == null)
      {
        lock (_lock)
        {
          vault = LinkedListOps.Find(_last, i.Id());

          if (vault == null)
          {
            vault = new Vault(i);
            LinkedListOps.Add(ref _last, vault);
          }
        }
      }

      return vault;
    }
  }
}
