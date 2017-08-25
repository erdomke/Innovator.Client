using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// Use the secured ItemType functions generated in 11sp5 and after
    /// </summary>
    SecuredFunction,
    /// <summary>
    /// Use the legacy functions for verions prior to 11sp5
    /// </summary>
    LegacyFunction
  }
}
