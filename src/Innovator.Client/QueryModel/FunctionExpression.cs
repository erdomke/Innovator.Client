using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public abstract class FunctionExpression : IExpression
  {
    protected readonly IExpression[] _args;

    public IEnumerable<IExpression> Args { get { return _args; } }
    public virtual string Name { get { return GetType().Name; } }

    protected FunctionExpression(int capacity)
    {
      _args = new IExpression[capacity];
    }

    internal virtual FunctionExpression Clone(Func<IExpression, IExpression> clone)
    {
      var result = (FunctionExpression)Activator.CreateInstance(this.GetType());
      for (var i = 0; i < _args.Length; i++)
        result._args[i] = clone(_args[i]);
      return result;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public virtual IExpression Evaluate()
    {
      throw new NotSupportedException();
    }

    public override string ToString()
    {
      return this.ToSqlString();
    }
  }
}
