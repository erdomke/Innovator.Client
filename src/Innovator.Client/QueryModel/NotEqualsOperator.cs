using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class NotEqualsOperator : BinaryOperator, IBooleanOperator
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
        return new NotEqualsOperator()
        {
          Left = Right,
          Right = Left
        };
      }
      else if (Left is IBooleanOperator && Right is BooleanLiteral boolean)
      {
        if (boolean.Value)
          return new NotOperator() { Arg = Left };
        else
          return Left;
      }
      else if (Right is IBooleanOperator && Left is BooleanLiteral boolean2)
      {
        if (boolean2.Value)
          return new NotOperator() { Arg = Right };
        else
          return Right;
      }
      return this;
    }
  }
}
