using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// Unit of time used with relative dates, <see cref="DateOffset"/>
  /// </summary>
  public enum DateMagnitude
  {
    /// <summary>
    /// A calendar day
    /// </summary>
    Day = 0,
    /// <summary>
    /// A business day (Monday-Friday)
    /// </summary>
    BusinessDay = 5,
    /// <summary>
    /// A week
    /// </summary>
    Week = 1,
    /// <summary>
    /// A month
    /// </summary>
    Month = 2,
    /// <summary>
    /// A quarter (three months, e.g. January - March)
    /// </summary>
    Quarter = 3,
    /// <summary>
    /// A year
    /// </summary>
    Year = 4
  }
}
