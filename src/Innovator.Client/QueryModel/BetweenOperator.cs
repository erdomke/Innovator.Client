using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// Represents a comparison indicating if <see cref="Left"/> is within the range
  /// [<see cref="Min"/>, <see cref="Max"/>] inclusive.
  /// </summary>
  /// <seealso cref="Innovator.Client.QueryModel.IBooleanOperator" />
  /// <seealso cref="Innovator.Client.QueryModel.INormalize" />
  public class BetweenOperator : IBooleanOperator, INormalize
  {
    /// <summary>
    /// Gets or sets the expression to perform range checks on.
    /// </summary>
    public IExpression Left { get; set; }

    /// <summary>
    /// Gets or sets the minimum value in the range
    /// </summary>
    public IExpression Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value in the range.
    /// </summary>
    public IExpression Max { get; set; }

    /// <summary>
    /// Gets the precedence.
    /// </summary>
    /// <value>
    /// The precedence.
    /// </value>
    public int Precedence => (int)PrecedenceLevel.Comparison;

    QueryItem ITableProvider.Table { get; set; }

    /// <summary>
    /// Converts to operator to an expression using simpler conditional operators.
    /// </summary>
    public IExpression ToConditional()
    {
      return new AndOperator()
      {
        Left = new GreaterThanOrEqualsOperator()
        {
          Left = Left,
          Right = Min
        }.Normalize(),
        Right = new LessThanOrEqualsOperator()
        {
          Left = Left,
          Right = Max
        }.Normalize()
      };
    }

    internal void SetMinMaxFromSql(string value)
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
          Text = "'" + value.Substring(0, andToken.StartOffset).Trim().Replace("'", "''") + "'",
          Type = SqlType.String
        };
        maxText = new SqlToken()
        {
          Text = "'" + value.Substring(andToken.StartOffset + 3).Trim().Replace("'", "''") + "'",
          Type = SqlType.String
        };
      }

      if (!Expressions.TryGetExpression(minText, out var min)
        || !Expressions.TryGetExpression(maxText, out var max))
        throw new InvalidOperationException();

      Min = min;
      Max = max;
    }

    /// <summary>
    /// Tell the specified visitor to process this expression component.
    /// </summary>
    /// <param name="visitor">The visitor.</param>
    public virtual void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    /// <summary>
    /// Returns a SQL formatted string useful for debugging.
    /// </summary>
    public override string ToString()
    {
      return this.ToSqlString();
    }

    /// <summary>
    /// Return a normalized version of the expression
    /// </summary>
    /// <returns>
    /// If the expression can be normalized (e.g. by placing the property reference on the left
    /// of a binary operator), return a new, more normalized expression.  If the expression cannot
    /// be normalized, return the current expression.
    /// </returns>
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
