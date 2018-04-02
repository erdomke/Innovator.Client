using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class OrderByExpression
  {
    public IExpression Expression { get; set; }
    public bool Ascending { get; set; } = true;
  }
}
