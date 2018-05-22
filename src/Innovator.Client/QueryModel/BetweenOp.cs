using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public abstract class BetweenOp : IBooleanOperator, INormalize
  {
    public IExpression Left { get; set; }
    public IExpression Min { get; set; }
    public IExpression Max { get; set; }

    public int Precedence => (int)PrecedenceLevel.Comparison;
    QueryItem ITableProvider.Table { get; set; }

    public abstract IExpression ToConditional();

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

      if (!Expressions.TryGetExpression(minText, out var min)
        || !Expressions.TryGetExpression(maxText, out var max))
        throw new InvalidOperationException();

      Min = min;
      Max = max;
    }

    public abstract void Visit(IExpressionVisitor visitor);

    public override string ToString()
    {
      return this.ToSqlString();
    }

    public virtual IExpression Normalize()
    {
      var tables = new[] { Left, Min, Max }
        .OfType<ITableProvider>()
        .Select(t => t.Table)
        .Distinct()
        .ToArray();
      if (tables.Length == 1)
        ((ITableProvider)this).Table = tables[0];

      if (Min is EqualsOperator eq && eq.Right is BooleanLiteral)
        Min = eq.Left;
      if (Max is EqualsOperator eq2 && eq2.Right is BooleanLiteral)
        Max = eq2.Left;

      return this;
    }
  }
}
