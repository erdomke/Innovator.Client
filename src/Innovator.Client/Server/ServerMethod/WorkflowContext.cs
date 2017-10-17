using Innovator.Client;
using Innovator.Client.Model;
using System;

namespace Innovator.Server
{
  /// <summary>
  /// Context for a server method which runs on a Workflow event
  /// </summary>
  public class WorkflowContext : IWorkflowContext
  {
    private IReadOnlyItem _context;
    private bool _contextLoaded = false;
    private readonly IResult _result;

    /// <summary>
    /// The activity during which the method is being called
    /// </summary>
    public Activity Activity { get; }

    /// <summary>
    /// Connection to the database
    /// </summary>
    public IServerConnection Conn { get; }

    /// <summary>
    /// The item which is the context of the workflow
    /// </summary>
    public IReadOnlyItem Context
    {
      get
      {
        EnsureContext();
        return _context;
      }
    }

    /// <summary>
    /// Error builder which captures any errors which are encountered
    /// </summary>
    public IErrorBuilder ErrorBuilder
    {
      get { return _result; }
    }

    /// <summary>
    /// Get the exception object created for any errors that have happened so far.
    /// </summary>
    public Exception Exception
    {
      get { return _result.Exception; }
    }

    /// <summary>
    /// The event for which the server method is being called
    /// </summary>
    public WorkflowEvent WorkflowEvent { get; }

    /// <summary>
    /// Method for modifying the query to get the context item
    /// </summary>
    public Action<IItem> QueryDefaults { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="item">The item.</param>
    public WorkflowContext(IServerConnection conn, IReadOnlyItem item)
    {
      Conn = conn;
      Activity = item as Activity;
      _result = conn.AmlContext.Result();
      switch (item.Property("WorkflowEvent").Value)
      {
        case "on_activate":
          WorkflowEvent = WorkflowEvent.OnActivate;
          break;
        case "on_assign":
          WorkflowEvent = WorkflowEvent.OnAssign;
          break;
        case "on_close":
          WorkflowEvent = WorkflowEvent.OnClose;
          break;
        case "on_delegate":
          WorkflowEvent = WorkflowEvent.OnDelegate;
          break;
        case "on_due":
          WorkflowEvent = WorkflowEvent.OnDue;
          break;
        case "on_escalate":
          WorkflowEvent = WorkflowEvent.OnEscalate;
          break;
        case "on_refuse":
          WorkflowEvent = WorkflowEvent.OnRefuse;
          break;
        case "on_remind":
          WorkflowEvent = WorkflowEvent.OnRemind;
          break;
        case "on_vote":
          WorkflowEvent = WorkflowEvent.OnVote;
          break;
        default:
          WorkflowEvent = WorkflowEvent.Other;
          break;
      }
    }

    private void EnsureContext()
    {
      if (!_contextLoaded)
      {
        _contextLoaded = true;
        var aml = Conn.AmlContext;
        var query = aml.Item(aml.Type("Workflow"), aml.Action("get"),
          aml.Select("source_type", "source_id"), aml.RelatedExpand(false),
          aml.RelatedId(
            aml.Item(aml.Type("Workflow Process"), aml.Action("get"),
              aml.Relationships(
                aml.Item(aml.Type("Workflow Process Activity"), aml.Action("get"),
                  aml.RelatedId(Activity.Id())
                )
              )
            )
          )
        );
        if (QueryDefaults != null) QueryDefaults(query);
        var workflow = Conn.ItemByQuery(query.ToAml());

        query = aml.Item(aml.TypeId(workflow.Property("source_type").Value),
          aml.Id(workflow.SourceId().Value), aml.Action("get"));
        if (QueryDefaults != null) QueryDefaults(query);
        _context = Conn.ItemByQuery(query.ToAml());
        _result.ErrorContext(_context);
      }
    }
  }
}
