using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class LatestMatch : IVersionCriteria
  {
    public DateTime AsOf { get; set; }
  }
}
