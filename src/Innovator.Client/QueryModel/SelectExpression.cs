using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class SelectExpression
  {
    public IExpression Expression { get; set; }
    public string Alias { get; set; }
  }
}
