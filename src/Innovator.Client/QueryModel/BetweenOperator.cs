using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class BetweenOperator : BetweenOp
  {
    public override IExpression ToConditional()
    {
      return new AndOperator()
      {
        Left = new GreaterThanOrEqualsOperator()
        {
          Left = Left,
          Right = Min
        }.Normalize(),
        Right = new LessThanOrEqualsOperator()
        {
          Left = Left,
          Right = Max
        }.Normalize()
      };
    }

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
