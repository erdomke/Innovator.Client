using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal class EndParen : IExpression
  {
    public void Visit(IExpressionVisitor visitor)
    {
      throw new NotSupportedException();
    }
  }
}
