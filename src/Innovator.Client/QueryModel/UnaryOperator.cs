using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public abstract class UnaryOperator : IOperator
  {
    public IExpression Arg { get; set; }

    public abstract int Precedence { get; }

    public abstract void Visit(IExpressionVisitor visitor);

    public override string ToString()
    {
      return this.ToSqlString();
    }
  }
}
