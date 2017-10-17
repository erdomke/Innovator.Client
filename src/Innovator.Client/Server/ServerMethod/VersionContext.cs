using Innovator.Client;
using System;

namespace Innovator.Server
{
  /// <summary>
  /// Context for a server method which runs as part of an item being versioned
  /// </summary>
  public class VersionContext : IVersionContext
  {
    private bool _newLoaded;
    private IReadOnlyItem _newVersion;

    /// <summary>
    /// Metadata about the previous generation
    /// </summary>
    /// <value>
    /// The previous generation.
    /// </value>
    public IReadOnlyItem OldVersion { get; }

    /// <summary>
    /// Metadata about the nex generation
    /// </summary>
    /// <value>
    /// The new generation.
    /// </value>
    public IReadOnlyItem NewVersion
    {
      get
      {
        EnsureNewVersion();
        return _newVersion;
      }
    }

    /// <summary>
    /// Method for modifying the query to get the new revision
    /// </summary>
    public Action<IItem> QueryDefaults { get; set; }

    /// <summary>
    /// Connection to the database
    /// </summary>
    public IServerConnection Conn { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="item">The item.</param>
    public VersionContext(IServerConnection conn, IReadOnlyItem item)
    {
      Conn = conn;
      OldVersion = item;
    }

    private void EnsureNewVersion()
    {
      if (!_newLoaded)
      {
        _newLoaded = true;
        var props = OldVersion.LazyMap(Conn, i => new
        {
          ConfigId = i.ConfigId().Value,
          Generation = i.Generation().AsInt()
        });

        var aml = Conn.AmlContext;
        var query = aml.Item(OldVersion.Type(), aml.Action("get"),
          aml.ConfigId(props.ConfigId),
          aml.Generation(props.Generation + 1)
        );
        if (QueryDefaults != null) QueryDefaults.Invoke(query);
        _newVersion = query.Apply(Conn).AssertItem();
      }
    }
  }
}
