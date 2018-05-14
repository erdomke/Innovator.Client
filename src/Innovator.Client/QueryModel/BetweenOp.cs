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

    public int Precedence => (int)PrecedenceLevel.Comparison;

    public void SetMinMaxFromSql(string value)
    {
      var tokens = new SqlTokenizer(value).ToArray();

      var minText = default(SqlToken);
      var maxText = default(SqlToken);
      if (tokens.Length == 3)
      {
        if (!string.Equals(tokens[1].Text, "and", StringComparison.OrdinalIgnoreCase))
          throw new InvalidOperationException();
        minText = tokens[0];
        maxText = tokens[2];
      }
      else
      {
        var andToken = tokens.Single(t => string.Equals(t.Text, "and", StringComparison.OrdinalIgnoreCase));
        minText = new SqlToken()
        {
          Text = "'" + value.Substring(0, andToken.StartOffset).Trim() + "'",
          Type = SqlType.String
        };
        maxText = new SqlToken()
        {
          Text = "'" + value.Substring(andToken.StartOffset + 3).Trim() + "'",
          Type = SqlType.String
        };
      }

      if (!Expression.TryGetExpression(minText, out var min)
        || !Expression.TryGetExpression(maxText, out var max))
        throw new InvalidOperationException();

      Min = min;
      Max = max;
    }

    public abstract void Visit(IExpressionVisitor visitor);

    public override string ToString()
    {
      return this.ToSqlString();
    }
  }
}
