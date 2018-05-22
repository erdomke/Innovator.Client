using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class ConcatenationOperator : BinaryOperator, INormalize
  {
    public override int Precedence => (int)PrecedenceLevel.Additive;

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public IExpression Normalize()
    {
      if (Left is StringLiteral str1 && Right is StringLiteral str2)
        return new StringLiteral(str1.Value + str2.Value);

      return this;
    }
  }
}
