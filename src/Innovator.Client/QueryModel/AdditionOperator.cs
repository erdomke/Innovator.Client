using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class AdditionOperator : BinaryOperator, INormalize
  {
    public override int Precedence => (int)PrecedenceLevel.Additive;

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public IExpression Normalize()
    {
      if (Expressions.TryGetLong(Left, out var i1)
        && Expressions.TryGetLong(Right, out var i2))
      {
        return new IntegerLiteral(i1 + i2);
      }
      else if (Expressions.TryGetDouble(Left, out var d1)
        && Expressions.TryGetDouble(Right, out var d2))
      {
        return new FloatLiteral(d1 + d2);
      }

      return this;
    }
  }
}
