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
    Day = 0,
    BusinessDay = 5,
    Week = 1,
    Month = 2,
    Quarter = 3,
    Year = 4
  }
}
