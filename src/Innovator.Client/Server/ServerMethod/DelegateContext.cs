using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Innovator.Client;

namespace Innovator.Server
{
  /// <summary>
  /// Context for a server method which runs on a Workflow delegation
  /// </summary>
  public class DelegateContext : WorkflowContext, IDelegateContext
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="item">The item.</param>
    public DelegateContext(IServerConnection conn, IReadOnlyItem item)
      : base(conn, item)
    {
      var aml = conn.AmlContext;
      Assignment = aml.Item(aml.Type("Activity Assignment"), aml.Id(item.Property("AssignmentId").Value),
        aml.SourceId(aml.KeyedName(item.KeyedName()), aml.Type(item.Type().Value), item.Id()),
        aml.Property("id", item.Property("AssignmentId").Value)
      );
      DelegateTo = aml.Item(aml.Type("Activity Assignment"), aml.Id(item.Property("ToAssignmentId").Value),
        aml.SourceId(aml.KeyedName(item.KeyedName()), aml.Type(item.Type().Value), item.Id()),
        aml.Property("id", item.Property("ToAssignmentId").Value)
      );
    }

    /// <summary>
    /// Which assignment the event pertains to
    /// </summary>
    public IReadOnlyItem Assignment { get; }

    /// <summary>
    /// The identity which is being delegated to
    /// </summary>
    public IReadOnlyItem DelegateTo { get; }
  }
}
