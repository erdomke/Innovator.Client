using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class ModulusOperator : BinaryOperator, INormalize
  {
    public override int Precedence => (int)PrecedenceLevel.Multiplicative;

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }


    public IExpression Normalize()
    {
      if (Expressions.TryGetLong(Left, out var i1)
        && Expressions.TryGetLong(Right, out var i2))
      {
        return new IntegerLiteral(i1 % i2);
      }

      return this;
    }
  }
}
