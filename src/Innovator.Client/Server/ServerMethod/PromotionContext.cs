using Innovator.Client;
using Innovator.Client.Model;

namespace Innovator.Server
{
  /// <summary>
  /// Context for a server method which runs on a life cycle promotion
  /// </summary>
  public class PromotionContext : IPromotionContext
  {
    /// <summary>
    /// The Life Cycle transition which is taking place
    /// </summary>
    public LifeCycleTransition Transition
    {
      get { return Item.Property("transition").AsItem() as LifeCycleTransition; }
    }

    /// <summary>
    /// Connection to the database
    /// </summary>
    public IServerConnection Conn { get; }

    /// <summary>
    /// Item that method should act on
    /// </summary>
    public IReadOnlyItem Item { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PromotionContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="item">The item.</param>
    public PromotionContext(IServerConnection conn, IReadOnlyItem item)
    {
      Conn = conn;
      Item = item;
    }
  }
}
