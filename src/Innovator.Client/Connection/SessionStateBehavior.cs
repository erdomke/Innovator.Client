namespace Innovator.Client
{
  /// <summary>
  /// How session state should be handled with the request
  /// </summary>
  /// <remarks>
  /// Requiring a writeable session state means that multiple requests from the same session will
  /// be handled serially by ASP.Net even if they are sent in parallel.
  /// </remarks>
  public enum SessionStateBehavior
  {
    /// <summary>
    /// Let the server define the behavior (defaults to read-only in 11sp12).
    /// </summary>
    Default,
    /// <summary>
    /// The session state is read-only
    /// </summary>
    ReadOnly,
    /// <summary>
    /// The session state i writeable
    /// </summary>
    Writeable,
    /// <summary>
    /// Switch back to the initial behavior
    /// </summary>
    SwitchToInitial
  }
}
