using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// An operator which takes to arguments
  /// </summary>
  /// <seealso cref="Innovator.Client.QueryModel.IOperator" />
  public abstract class BinaryOperator : IOperator
  {
    /// <summary>
    /// Gets or sets the left argument
    /// </summary>
    public IExpression Left { get; set; }

    /// <summary>
    /// Gets or sets the right argument
    /// </summary>
    public IExpression Right { get; set; }

    /// <summary>
    /// Gets the precedence used when evaluating the order of operations
    /// </summary>
    /// <remarks>
    /// Higher precedence operators are evaluated first in the absence of parentheses.  For example
    /// the precedence of multiplication is greater than addition.
    /// </remarks>
    public abstract int Precedence { get; }

    /// <summary>
    /// Tell the specified visitor to process this expression component.
    /// </summary>
    /// <param name="visitor">The visitor.</param>
    public abstract void Visit(IExpressionVisitor visitor);

    /// <summary>
    /// Returns a SQL formatted string useful for debugging.
    /// </summary>
    public override string ToString()
    {
      return this.ToSqlString();
    }

    /// <summary>
    /// Sets the table being processed by this expression
    /// </summary>
    protected void SetTable()
    {
      var boolOp = this as IBooleanOperator;
      if (boolOp == null)
        return;

      var tables = new[] { Left, Right }
        .OfType<ITableProvider>()
        .Select(p => p.Table)
        .Distinct()
        .ToArray();
      if (tables.Length == 1)
        boolOp.Table = tables[0];
    }

    /// <summary>
    /// Flattens a tree of binary operators (e.g. AND or OR) into a list of expressions at the same
    /// level
    /// </summary>
    /// <param name="op">The operator to flatten.</param>
    /// <returns>A list of argument expressions</returns>
    public static IEnumerable<IExpression> Flatten<T>(T op) where T : BinaryOperator
    {
      var parts = new List<IExpression>()
      {
        op.Left,
        op.Right
      };

      var i = 0;
      while (i < parts.Count)
      {
        var same = parts[i] as T;
        if (same == null)
        {
          i++;
        }
        else
        {
          parts.RemoveAt(i);
          parts.Add(same.Left);
          parts.Add(same.Right);
        }
      }

      return parts;
    }
  }
}
