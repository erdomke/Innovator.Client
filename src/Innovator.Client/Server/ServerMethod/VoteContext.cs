using Innovator.Client;

namespace Innovator.Server
{
  /// <summary>
  /// Context for a server method which runs on a Workflow vote
  /// </summary>
  public class VoteContext : WorkflowContext, IVoteContext
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="VoteContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="item">The item.</param>
    public VoteContext(IServerConnection conn, IReadOnlyItem item) : base(conn, item)
    {
      var aml = conn.AmlContext;
      Assignment = aml.Item(aml.Type("Activity Assignment"), aml.Id(item.Property("AssignmentId").Value),
        aml.SourceId(aml.KeyedName(item.KeyedName()), aml.Type(item.Type().Value), item.Id()),
        aml.Property("id", item.Property("AssignmentId").Value)
      );
    }

    /// <summary>
    /// Which assignment the event pertains to
    /// </summary>
    public IReadOnlyItem Assignment { get; }

    /// <summary>
    /// The workflow path being voted for
    /// </summary>
    public string Path
    {
      get { return Activity.Property("Path").Value; }
    }
  }
}
