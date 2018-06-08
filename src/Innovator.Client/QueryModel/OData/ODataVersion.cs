using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// Version(s) of the OData spec to support
  /// </summary>
  [Flags]
  public enum ODataVersion
  {
    All = -1,
    v2 = 1,
    v3 = 2,
    v4 = 4,
  }
}
