using Innovator.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Server
{
  /// <summary>
  /// Context for a server method which is called with a single item
  /// that must be modified to alter Aras behavior
  /// </summary>
  public class MutableItemContext : IMutableItemContext
  {
    private readonly IServerConnection _conn;
    private readonly IItem _item;

    /// <summary>
    /// Connection to the database
    /// </summary>
    public IServerConnection Conn
    {
      get { return _conn; }
    }

    /// <summary>
    /// Item that method should act on
    /// </summary>
    public IItem Item
    {
      get { return _item; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MutableItemContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="item">The item.</param>
    public MutableItemContext(IServerConnection conn, IItem item)
    {
      _conn = conn;
      _item = item;
    }
  }
}
