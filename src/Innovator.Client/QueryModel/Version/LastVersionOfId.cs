using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class LastVersionOfId : IVersionCriteria
  {
    public string Id { get; set; }
  }
}
