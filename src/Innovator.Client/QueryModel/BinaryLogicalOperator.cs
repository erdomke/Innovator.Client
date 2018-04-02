using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public abstract class BinaryLogicalOperator : ILogical
  {
    private List<IExpression> _args = new List<IExpression>();

    public IList<IExpression> Args { get { return _args; } }

    public void Add(IExpression child)
    {
      _args.Add(child);
    }

    public abstract void Visit(IExpressionVisitor visitor);
  }
}
