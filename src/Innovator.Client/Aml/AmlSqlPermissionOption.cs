namespace Innovator.Client
{
  /// <summary>
  /// What SQL permission check to include in the SQL query
  /// </summary>
  public enum AmlSqlPermissionOption
  {
    /// <summary>
    /// Don't include permissions
    /// </summary>
    None,
    /// <summary>
    /// Use the secured ItemType functions generated in 11sp5 to 11sp11
    /// </summary>
    SecuredFunction,
    /// <summary>
    /// Use the secured ItemType functions generated in 11sp12 and after
    /// </summary>
    SecuredFunctionEnviron,
    /// <summary>
    /// Use the legacy functions for verions prior to 11sp5
    /// </summary>
    LegacyFunction
  }
}
