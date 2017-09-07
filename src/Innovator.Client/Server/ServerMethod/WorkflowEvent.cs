using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Innovator.Client;

namespace Innovator.Server
{
  /// <summary>
  /// Type of workflow event
  /// </summary>
  public enum WorkflowEvent
  {
    /// <summary>
    /// Unknown or undefined
    /// </summary>
    Other,
    /// <summary>
    /// On activate
    /// </summary>
    OnActivate,
    /// <summary>
    /// On assign
    /// </summary>
    OnAssign,
    /// <summary>
    /// On close
    /// </summary>
    OnClose,
    /// <summary>
    /// On delegate
    /// </summary>
    OnDelegate,
    /// <summary>
    /// On due
    /// </summary>
    OnDue,
    /// <summary>
    /// On escalate
    /// </summary>
    OnEscalate,
    /// <summary>
    /// On refuse
    /// </summary>
    OnRefuse,
    /// <summary>
    /// On remind
    /// </summary>
    OnRemind,
    /// <summary>
    /// On vote
    /// </summary>
    OnVote
  }
}
