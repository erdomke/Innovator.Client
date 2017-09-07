using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Innovator.Client;
using Innovator.Client.Model;

namespace Innovator.Server
{
  /// <summary>
  /// Base interface for a server method context argument
  /// </summary>
  public interface IContext
  {
    /// <summary>
    /// Connection to the database
    /// </summary>
    IServerConnection Conn { get; }
  }

  /// <summary>
  /// Context for a server method which is called with a single item
  /// </summary>
  public interface ISingleItemContext : IContext
  {
    /// <summary>
    /// Item that method should act on
    /// </summary>
    IReadOnlyItem Item { get; }
  }

  /// <summary>
  /// Context for a server method which is called with a single item
  /// that must be modified to alter Aras behavior
  /// </summary>
  public interface IMutableItemContext : IContext
  {
    /// <summary>
    /// Item that method should act on
    /// </summary>
    IItem Item { get; }
  }

  /// <summary>
  /// Context for a server method which is called with multiple
  /// items that must be modified to alter Aras behavior
  /// </summary>
  public interface IMultipleItemContext : IContext
  {
    /// <summary>
    /// Items that the method should act on
    /// </summary>
    IEnumerable<IItem> Items { get; }
  }

  /// <summary>
  /// Context for a server method which is used on before add
  /// or update of an item.
  /// </summary>
  public interface IValidationContext : IContext
  {
    /// <summary>
    /// Indicates if the argument is new (not in the database)
    /// </summary>
    /// <value><c>true</c> if the item is not in the database, <c>false</c> otherwise</value>
    bool IsNew { get; }

    /// <summary>
    /// Error builder which captures any errors which are encountered
    /// </summary>
    IErrorBuilder ErrorBuilder { get; }

    /// <summary>
    /// Get the exception object created for any errors that have happened so far.
    /// </summary>
    Exception Exception { get; }

    /// <summary>
    /// Get the existing item in the database
    /// </summary>
    IReadOnlyItem Existing { get; }

    /// <summary>
    /// The changes given to the database.  This object should be modified to make any additional
    /// changes
    /// </summary>
    IItem Item { get; }

    /// <summary>
    /// Gets an item which represents the new item after the changes are applied
    /// </summary>
    IReadOnlyItem Merged { get; }

    /// <summary>
    /// Indicates if a property is being set null.  Note that this does not detect if the property
    /// already is null.
    /// </summary>
    /// <param name="name">The name of the property to check</param>
    /// <returns><c>true</c> if a non-null property is being set null with this query, 
    /// <c>false</c> otherwise</returns>
    bool IsBeingSetNull(string name);

    /// <summary>
    /// Indicates if one or more properties in the list are changing
    /// </summary>
    /// <param name="names">Property name(s)</param>
    /// <returns><c>true</c> if at least one of the properties is being changed with
    /// this query, <c>false</c> otherwise</returns>
    bool IsChanging(params string[] names);

    /// <summary>
    /// Gets a property from the <see cref="Item"/> item (if it exists).  Otherwise, the property
    /// from <see cref="Existing"/> is returned
    /// </summary>
    /// <param name="name">Name of the property to retrieve</param>
    /// <returns>The <see cref="IReadOnlyProperty"/> from the incoming query (if defined).  Otherwise, 
    /// the <see cref="IReadOnlyProperty"/> from the database data</returns>
    IReadOnlyProperty NewOrExisting(string name);

    /// <summary>
    /// Method for modifying the query to get existing items
    /// </summary>
    Action<IItem> QueryDefaults { get; set; }
  }

  /// <summary>
  /// Context for a server method which runs as part of an item being versioned
  /// </summary>
  public interface IVersionContext : IContext
  {
    /// <summary>
    /// Metadata about the previous generation
    /// </summary>
    /// <value>
    /// The previous generation.
    /// </value>
    IReadOnlyItem OldVersion { get; }

    /// <summary>
    /// Metadata about the nex generation
    /// </summary>
    /// <value>
    /// The new generation.
    /// </value>
    IReadOnlyItem NewVersion { get; }

    /// <summary>
    /// Method for modifying the query to get the new revision
    /// </summary>
    Action<IItem> QueryDefaults { get; set; }
  }

  /// <summary>
  /// Context for a server method which runs on a life cycle promotion
  /// </summary>
  public interface IPromotionContext : ISingleItemContext
  {
    /// <summary>
    /// The Life Cycle transition which is taking place
    /// </summary>
    LifeCycleTransition Transition { get; }
  }

  /// <summary>
  /// Context for a server method which runs on a Workflow event
  /// </summary>
  public interface IWorkflowContext : IContext
  {
    /// <summary>
    /// The activity during which the method is being called
    /// </summary>
    Activity Activity { get; }
    /// <summary>
    /// The item which is the context of the workflow
    /// </summary>
    IReadOnlyItem Context { get; }
    /// <summary>
    /// The event for which the server method is being called
    /// </summary>
    WorkflowEvent WorkflowEvent { get; }
    /// <summary>
    /// Error builder which captures any errors which are encountered
    /// </summary>
    IErrorBuilder ErrorBuilder { get; }
    /// <summary>
    /// Get the exception object created for any errors that have happened so far.
    /// </summary>
    Exception Exception { get; }
    /// <summary>
    /// Method for modifying the query to get the context item
    /// </summary>
    Action<IItem> QueryDefaults { get; set; }
  }

  /// <summary>
  /// Context for a server method which runs on a Workflow vote
  /// </summary>
  public interface IVoteContext : IWorkflowContext
  {
    /// <summary>
    /// Which assignment the event pertains to
    /// </summary>
    IReadOnlyItem Assignment { get; }

    /// <summary>
    /// The workflow path being voted for
    /// </summary>
    string Path { get; }
  }

  /// <summary>
  /// Context for a server method which runs on a Workflow delegation
  /// </summary>
  public interface IDelegateContext : IWorkflowContext
  {
    /// <summary>
    /// Which assignment the event pertains to
    /// </summary>
    IReadOnlyItem Assignment { get; }
    /// <summary>
    /// The identity which is being delegated to
    /// </summary>
    IReadOnlyItem DelegateTo { get; }
  }

  /// <summary>
  /// Interface for a generic server method (e.g. called directly with AML such as
  /// <c>&lt;Item action='{Method name}' /&gt;</c>)
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, consult the following table
  /// <list type="table">
  ///   <listheader><term>Event</term><description>Preferred interface</description></listheader>  
  ///   <item><term>Item-&gt;GetKeyedName</term><description><see cref="IGetKeyedName"/></description></item>
  ///   <item><term>Item-&gt;onAdd</term><description><see cref="IOnAction"/></description></item>
  ///   <item><term>Item-&gt;onAfterAdd</term><description><see cref="IAfterAddUpdate"/></description></item>
  ///   <item><term>Item-&gt;onAfterCopy</term><description><see cref="IMethod"/></description></item>
  ///   <item><term>Item-&gt;onAfterDelete</term><description><see cref="IMethod"/></description></item>
  ///   <item><term>Item-&gt;onAfterGet</term><description><see cref="IAfterGet"/></description></item>
  ///   <item><term>Item-&gt;onAfterLock</term><description><see cref="IMethod"/></description></item>
  ///   <item><term>Item-&gt;onAfterMethod</term><description><see cref="IMethod"/></description></item>
  ///   <item><term>Item-&gt;onAfterUnlock</term><description><see cref="IMethod"/></description></item>
  ///   <item><term>Item-&gt;onAfterUpdate</term><description><see cref="IAfterAddUpdate"/></description></item>
  ///   <item><term>Item-&gt;onAfterVersion</term><description><see cref="IAfterVersion"/></description></item>
  ///   <item><term>Item-&gt;onBeforeAdd</term><description><see cref="IBeforeAddUpdate"/></description></item>
  ///   <item><term>Item-&gt;onBeforeCopy</term><description><see cref="IMethod"/></description></item>
  ///   <item><term>Item-&gt;onBeforeDelete</term><description><see cref="IMethod"/></description></item>
  ///   <item><term>Item-&gt;onBeforeGet</term><description><see cref="IBeforeGet"/></description></item>
  ///   <item><term>Item-&gt;onBeforeLock</term><description><see cref="IMethod"/></description></item>
  ///   <item><term>Item-&gt;onBeforeMethod</term><description><see cref="IMethod"/></description></item>
  ///   <item><term>Item-&gt;onBeforeUnlock</term><description><see cref="IMethod"/></description></item>
  ///   <item><term>Item-&gt;onBeforeUpdate</term><description><see cref="IBeforeAddUpdate"/></description></item>
  ///   <item><term>Item-&gt;onBeforeVersion</term><description><see cref="IBeforeVersion"/></description></item>
  ///   <item><term>Item-&gt;onDelete</term><description><see cref="IOnAction"/></description></item>
  ///   <item><term>Item-&gt;onGet</term><description><see cref="IOnGet"/></description></item>
  ///   <item><term>Item-&gt;onUpdate</term><description><see cref="IOnAction"/></description></item>
  ///   <item><term>Life Cycle-&gt;Transition-&gt;Pre</term><description><see cref="IPromotion"/></description></item>
  ///   <item><term>Life Cycle-&gt;Transition-&gt;Post</term><description><see cref="IPromotion"/></description></item>
  ///   <item><term>Workflow-&gt;onActivate</term><description><see cref="IWorkflow"/></description></item>
  ///   <item><term>Workflow-&gt;onAssign</term><description><see cref="IWorkflow"/></description></item>
  ///   <item><term>Workflow-&gt;onClose</term><description><see cref="IWorkflow"/></description></item>
  ///   <item><term>Workflow-&gt;onDelegate</term><description><see cref="IDelegate"/></description></item>
  ///   <item><term>Workflow-&gt;onDue</term><description><see cref="IWorkflow"/></description></item>
  ///   <item><term>Workflow-&gt;onEscalate</term><description><see cref="IWorkflow"/></description></item>
  ///   <item><term>Workflow-&gt;onRefuse</term><description><see cref="IRefuse"/></description></item>
  ///   <item><term>Workflow-&gt;onRemind</term><description><see cref="IWorkflow"/></description></item>
  ///   <item><term>Workflow-&gt;onVote</term><description><see cref="IVote"/></description></item>
  ///   <item><term>Workflow-&gt;Path-&gt;Pre</term><description><see cref="IWorkflow"/></description></item>
  ///   <item><term>Workflow-&gt;Path-&gt;Post</term><description><see cref="IWorkflow"/></description></item>
  /// </list>
  /// </remarks>
  public interface IMethod
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <returns>A result to be returned to the caller (either another method or client)</returns>
    IReadOnlyResult Execute(ISingleItemContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Item-&gt;onBeforeGet event
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IBeforeGet
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <remarks>To respond to this method, either modify the <see cref="IMutableItemContext.Item"/>
    /// or throw an exception</remarks>
    void Execute(IMutableItemContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Item-&gt;onAfterGet event
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IAfterGet
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <remarks>To respond to this method, either modify the <see cref="IMultipleItemContext.Items"/>
    /// or throw an exception</remarks>
    void Execute(IMultipleItemContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Item-&gt;onAdd, Item-&gt;onDelete,
  /// or Item-&gt;onUpdate events
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IOnAction
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <returns>One or more items to client</returns>
    IEnumerable<IReadOnlyItem> Execute(ISingleItemContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Item-&gt;onGet event
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IOnGet : IOnAction { }

  /// <summary>
  /// Interface for a server method which runs on the Item-&gt;onBeforeAdd or 
  /// Item-&gt;onBeforeUpdate events
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IBeforeAddUpdate
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <remarks>To respond to this method, either modify the <see cref="IValidationContext.Item"/>
    /// or use the <see cref="IValidationContext.ErrorBuilder"/> to create an exception which will be
    /// returned</remarks>
    void Execute(IValidationContext arg);
  }

  /// <summary>
  /// Base interface for a server method which is called for a single item
  /// and where the return is ignored
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface ISingleItemSubroutine
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <remarks>To respond to this method, throw an exception</remarks>
    void Execute(ISingleItemContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Item-&gt;onAfterAdd or 
  /// Item-&gt;onAfterUpdate events
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IAfterAddUpdate : ISingleItemSubroutine { }

  /// <summary>
  /// Interface for a server method which runs on the Item-&gt;onBeforeVersion event
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IBeforeVersion : ISingleItemSubroutine { }

  /// <summary>
  /// Interface for a server method which runs on the Item-&gt;onAfterVersion event
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IAfterVersion
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <remarks>To respond to this method, throw an exception</remarks>
    void Execute(IVersionContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Item-&gt;GetKeyedName event
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IGetKeyedName
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <returns>The new keyed name</returns>
    string Execute(ISingleItemContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Life Cycle-&gt;Transition-&gt;* event
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IPromotion
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <remarks>To respond to this method, throw an exception.  An exception will only roll back the
    /// transaction when this is run on the Life Cycle-&gt;Transition-&gt;Pre event</remarks>
    void Execute(IPromotionContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Workflow-&gt;* or 
  /// Workflow-&gt;Path-&gt;* events
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IWorkflow
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <remarks>To respond to this method, use the <see cref="IWorkflowContext.ErrorBuilder"/> to 
    /// create an exception which will be returned</remarks>
    void Execute(IWorkflowContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Workflow-&gt;onVote event
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IVote
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <remarks>To respond to this method, use the <see cref="IWorkflowContext.ErrorBuilder"/> to 
    /// create an exception which will be returned</remarks>
    void Execute(IVoteContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Workflow-&gt;onDelegate event
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IDelegate
  {
    /// <summary>Executes the server event.</summary>
    /// <param name="arg">The server context (server connection, context item, and other helper methods).</param>
    /// <remarks>To respond to this method, use the <see cref="IWorkflowContext.ErrorBuilder"/> to 
    /// create an exception which will be returned</remarks>
    void Execute(IDelegateContext arg);
  }

  /// <summary>
  /// Interface for a server method which runs on the Workflow-&gt;onRefuse event
  /// </summary>
  /// <remarks>
  /// For help deciding which interface to use for which type of method, see the 
  /// remarks for <see cref="IMethod"/>
  /// </remarks>
  public interface IRefuse : IDelegate { }
}
