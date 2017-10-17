namespace Innovator.Client
{
  /// <summary>
  /// What type of SQL query to render
  /// </summary>
  public enum AmlSqlRenderOption
  {
    /// <summary>
    /// A <c>SELECT</c> query
    /// </summary>
    SelectQuery,
    /// <summary>
    /// A query which returns the count of matching items
    /// </summary>
    CountQuery,
    /// <summary>
    /// A query which returns the offset of an item in a result set
    /// </summary>
    OffsetQuery,
    /// <summary>
    /// The <c>FROM</c> clause of a <c>SELECT</c> query
    /// </summary>
    FromClause,
    /// <summary>
    /// The <c>WHERE</c> clause of a <c>SELECT</c> query
    /// </summary>
    WhereClause
  }
}
