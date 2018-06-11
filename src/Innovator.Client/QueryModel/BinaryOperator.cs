using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public abstract class BinaryOperator : IOperator
  {
    public IExpression Left { get; set; }
    public IExpression Right { get; set; }

    public abstract int Precedence { get; }

    public abstract void Visit(IExpressionVisitor visitor);

    public override string ToString()
    {
      return this.ToSqlString();
    }

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
