using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class NotOperator : ILogical
  {
    public IExpression Right { get; set; }

    public void Add(IExpression child)
    {
      if (Right != null)
        throw new NotSupportedException("The `not` operator cannot have multiple children");

      Right = child;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
