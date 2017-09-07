using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Innovator.Client;

namespace Innovator.Server
{
  /// <summary>
  /// Context for a server method which is called with a single item
  /// </summary>
  public class SingleItemContext : ISingleItemContext
  {
    /// <summary>
    /// Connection to the database
    /// </summary>
    public IServerConnection Conn { get; }

    /// <summary>
    /// Item that method should act on
    /// </summary>
    public IReadOnlyItem Item { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleItemContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="item">The item.</param>
    public SingleItemContext(IServerConnection conn, IReadOnlyItem item)
    {
      Conn = conn;
      Item = item;
    }
  }
}
