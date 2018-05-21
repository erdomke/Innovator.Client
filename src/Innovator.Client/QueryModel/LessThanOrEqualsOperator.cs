using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class LessThanOrEqualsOperator : BinaryOperator, IBooleanOperator
  {
    public override int Precedence => (int)PrecedenceLevel.Comparison;

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public IExpression Normalize()
    {
      if (Right is PropertyReference && !(Left is PropertyReference))
      {
        return new GreaterThanOrEqualsOperator()
        {
          Left = Right,
          Right = Left
        };
      }
      return this;
    }
  }
}
