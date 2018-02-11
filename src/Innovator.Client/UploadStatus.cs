using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  /// <summary>
  /// Status of an upload command
  /// </summary>
  public enum UploadStatus
  {
    /// <summary>
    /// Files are still being uploaded
    /// </summary>
    Pending,
    /// <summary>
    /// The transaction is being committed
    /// </summary>
    Committed,
    /// <summary>
    /// The transaction is being rolled back
    /// </summary>
    RolledBack,
  }
}
