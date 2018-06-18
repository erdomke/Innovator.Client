using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal class NotInOperator : InOperator
  {
    public override IExpression Normalize()
    {
      return new NotOperator()
      {
        Arg = new InOperator()
        {
          Left = Left,
          Right = Right
        }.Normalize()
      }.Normalize();
    }

    public override void Visit(IExpressionVisitor visitor)
    {
      throw new NotSupportedException();
    }
  }
}
