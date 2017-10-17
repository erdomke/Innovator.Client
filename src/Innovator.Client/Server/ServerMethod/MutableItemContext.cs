using Innovator.Client;

namespace Innovator.Server
{
  /// <summary>
  /// Context for a server method which is called with a single item
  /// that must be modified to alter Aras behavior
  /// </summary>
  public class MutableItemContext : IMutableItemContext
  {
    /// <summary>
    /// Connection to the database
    /// </summary>
    public IServerConnection Conn { get; }

    /// <summary>
    /// Item that method should act on
    /// </summary>
    public IItem Item { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MutableItemContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="item">The item.</param>
    public MutableItemContext(IServerConnection conn, IItem item)
    {
      Conn = conn;
      Item = item;
    }
  }
}
