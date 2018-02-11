namespace Innovator.Client
{
  /// <summary>
  /// What type of SQL query to render
  /// </summary>
  public enum AmlSqlRenderOption
  {
    /// <summary>
    /// The default render option.  This is the same as <see cref="SelectQuery"/>
    /// </summary>
    Default = 0,
    /// <summary>
    /// The <c>SELECT</c> clause of a <c>SELECT</c> query
    /// </summary>
    SelectClause = 0x1,
    /// <summary>
    /// The <c>FROM</c> clause of a <c>SELECT</c> query
    /// </summary>
    FromClause = 0x2,
    /// <summary>
    /// The <c>WHERE</c> clause of a <c>SELECT</c> query
    /// </summary>
    WhereClause = 0x4,
    /// <summary>
    /// The <c>ORDER BY</c> clause of a <c>SELECT</c> query
    /// </summary>
    OrderByClause = 0x8,
    /// <summary>
    /// The <c>OFFSET</c> clause of a <c>SELECT</c> query
    /// </summary>
    OffsetClause = 0x10,
    /// <summary>
    /// A <c>SELECT</c> query
    /// </summary>
    SelectQuery = 0x1f,
    /// <summary>
    /// A query which returns the count of matching items
    /// </summary>
    CountQuery = 0x100,
    /// <summary>
    /// A query which returns the offset of an item in a result set
    /// </summary>
    OffsetQuery = 0x200,
    /// <summary>
    /// Whether to ignore the <c>queryType</c> of a get query
    /// </summary>
    IgnoreQueryType = 0x1000,
  }
}
