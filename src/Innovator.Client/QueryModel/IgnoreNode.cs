using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class IgnoreNode : IExpression
  {
    public void Visit(IExpressionVisitor visitor)
    {
      throw new NotSupportedException();
    }
  }
}
