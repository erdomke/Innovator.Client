using System.Collections.Generic;
using System.Linq;

namespace Innovator.Client.Connection
{
  /// <summary>
  /// Contains logic for determining which vaults to read from and write
  /// to for a given <see cref="IAsyncConnection"/>
  /// </summary>
  public class DefaultVaultStrategy : IVaultStrategy
  {
    private IAsyncConnection _conn;
    private IVaultFactory _factory;
    private IPromise<User> _userInfo;

    /// <summary>
    /// Initializes the stategy object with the specified connection and vault factory
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="factory">The vault factory.</param>
    public void Initialize(IAsyncConnection conn, IVaultFactory factory)
    {
      _conn = conn;
      _factory = factory;
    }

    /// <summary>
    /// Asynchronously returns a list of vaults to write to in priority order
    /// </summary>
    /// <param name="async">if set to <c>true</c>, execute asyncronously.
    /// Otherwise, a resolved promise is returned</param>
    /// <returns>
    /// A list of vaults to write to in priority order
    /// </returns>
    public IPromise<IEnumerable<Vault>> WritePriority(bool async)
    {
      return GetUserInfo(async)
        .Convert(u => Enumerable.Repeat(u.DefaultVault, 1).Concat(u.ReadPriority));
    }

    /// <summary>
    /// Asynchronously returns a list of vaults to read from in priority order
    /// </summary>
    /// <param name="async">if set to <c>true</c>, execute asyncronously.
    /// Otherwise, a resolved promise is returned</param>
    /// <returns>
    /// A list of vaults to read from in priority order
    /// </returns>
    public IPromise<IEnumerable<Vault>> ReadPriority(bool async)
    {
      return GetUserInfo(async)
        .Convert(u => u.ReadPriority.Concat(Enumerable.Repeat(u.DefaultVault, 1)));
    }

    private IPromise<User> GetUserInfo(bool async)
    {
      if (_userInfo == null)
      {
        _userInfo = _conn.ItemByQuery(new Command("<Item type='User' action='get' select='default_vault' expand='1'><id>@0</id><Relationships><Item type='ReadPriority' action='get' select='priority, related_id' expand='1' orderBy='priority'/></Relationships></Item>", _conn.UserId), async)
          .FailOver(() => _conn.ItemByQuery(new Command("<Item type='User' action='get' select='default_vault' expand='1'><id>@0</id></Item>", _conn.UserId), async))
          .Convert<IReadOnlyItem, User>(i =>
          {
            var result = new User();
            result.Id = i.Id();
            var vault = i.Property("default_vault").AsItem();
            if (vault.Exists) result.DefaultVault = _factory.GetVault(vault);
            foreach (var rel in i.Relationships("ReadPriority"))
            {
              vault = rel.RelatedId().AsItem();
              if (vault.Exists)
              {
                result.ReadPriority.Add(_factory.GetVault(vault));
              }
            }
            return result;
          })
          .Fail(ex => { ex.Rethrow(); });
      }
      return _userInfo;
    }

    private class User
    {
      private readonly List<Vault> _vaults = new List<Vault>();

      public string Id { get; set; }
      public Vault DefaultVault { get; set; }
      public IList<Vault> ReadPriority { get { return _vaults; } }
    }
  }
}
