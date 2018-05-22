using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class NegationOperator : UnaryOperator, INormalize
  {
    public override int Precedence => (int)PrecedenceLevel.Negation;

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public IExpression Normalize()
    {
      if (Expressions.TryGetLong(Arg, out var i))
      {
        return new IntegerLiteral(-1 * i);
      }
      else if (Expressions.TryGetDouble(Arg, out var d))
      {
        return new FloatLiteral(-1 * d);
      }

      return this;
    }
  }
}
