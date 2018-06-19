using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class IgnoreNode : IExpression
  {
    private IgnoreNode() { }

    public void Visit(IExpressionVisitor visitor)
    {
      throw new NotSupportedException();
    }

    public static IgnoreNode Instance { get; } = new IgnoreNode();
  }
}
