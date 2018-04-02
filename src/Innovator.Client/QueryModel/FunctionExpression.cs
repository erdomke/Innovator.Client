using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class FunctionExpression : IExpression
  {
    private List<IExpression> _args = new List<IExpression>();

    public IList<IExpression> Args { get { return _args; } }
    public string Name { get; set; }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
