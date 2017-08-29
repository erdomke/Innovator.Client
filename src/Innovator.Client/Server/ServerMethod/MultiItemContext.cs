using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Innovator.Client;

namespace Innovator.Server
{
  /// <summary>
  /// Context for a server method which is called with multiple
  /// items that must be modified to alter Aras behavior
  /// </summary>
  public class MultiItemContext : IMultipleItemContext
  {
    private readonly IServerConnection _conn;
    private readonly IEnumerable<IItem> _items;

    /// <summary>
    /// Connection to the database
    /// </summary>
    public IServerConnection Conn
    {
      get { return _conn; }
    }
    /// <summary>
    /// Items that the method should act on
    /// </summary>
    public IEnumerable<IItem> Items
    {
      get { return _items; }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiItemContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="items">The items.</param>
    public MultiItemContext(IServerConnection conn, IEnumerable<IItem> items)
    {
      _conn = conn;
      _items = items;
    }
  }
}
