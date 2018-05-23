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
        Arg = base.Normalize()
      }.Normalize();
    }

    public override void Visit(IExpressionVisitor visitor)
    {
      throw new NotSupportedException();
    }
  }
}
