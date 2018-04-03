using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public abstract class BetweenOp : IOperator
  {
    public IExpression Left { get; set; }
    public IExpression Min { get; set; }
    public IExpression Max { get; set; }

    public void SetMinMaxFromSql(string value)
    {
      var tokens = new SqlTokenizer(value).ToArray();
      if (tokens.Length != 3)
        throw new InvalidOperationException();

      if (!string.Equals(tokens[1].Text, "and", StringComparison.OrdinalIgnoreCase)
        || !Expression.TryGetExpression(tokens[0], out var min)
        || !Expression.TryGetExpression(tokens[2], out var max))
        throw new InvalidOperationException();

      Min = min;
      Max = max;
    }

    public abstract void Visit(IExpressionVisitor visitor);
  }
}
