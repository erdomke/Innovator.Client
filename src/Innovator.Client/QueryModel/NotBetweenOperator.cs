using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class NotBetweenOperator : BetweenOp
  {
    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
