namespace Innovator.Client
{
  /// <summary>
  /// Identifies which item an AML <c>get</c> query should return
  /// </summary>
  public enum QueryType
  {
    /// <summary>
    /// The item marked as <c>is_current</c> in the database
    /// </summary>
    Current,
    /// <summary>
    /// The item with the most recent <c>effective_date</c> which occurred prior to the
    /// <c>queryDate</c>
    /// </summary>
    Effective,
    /// <summary>
    /// The item with the most recent <c>modified_on</c> which occurred prior to the
    /// <c>queryDate</c>
    /// </summary>
    Latest,
    /// <summary>
    /// The item with the most recent <c>released_date</c> which occurred prior to the
    /// <c>queryDate</c>
    /// </summary>
    Released
  }
}
