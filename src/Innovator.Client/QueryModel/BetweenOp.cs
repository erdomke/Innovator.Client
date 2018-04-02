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
      if (Utils.IsNullOrWhiteSpace(value))
        return;

      var i = 0;
      while (char.IsWhiteSpace(value[i]))
        i++;

      if (value[i] == '\'')
      {
        var start = i + 1;
        var end = value.IndexOf('\'', start);
        while (end > 0 && (end + 1) < value.Length && value[end + 1] == '\'')
          end = value.IndexOf('\'', end + 2);
        if (end < 0)
          throw new InvalidOperationException();

        Min = new StringLiteral(value.Substring(start, end - start).Replace("''", "'"));

        start = value.IndexOf('\'', end + 1);
        end = value.IndexOf('\'', start);
        while (end > 0 && (end + 1) < value.Length && value[end + 1] == '\'')
          end = value.IndexOf('\'', end + 2);
        if (end < 0)
          throw new InvalidOperationException();

        Max = new StringLiteral(value.Substring(start, end - start).Replace("''", "'"));
      }
      else
      {
        var idx = value.IndexOf("and", StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
          throw new InvalidOperationException();

        var num = value.Substring(0, idx).Trim();
        if (FloatLiteral.TryGetNumberLiteral(num, out ILiteral min))
          Min = min;
        else
          throw new InvalidOperationException();

        num = value.Substring(idx + 3).Trim();
        if (FloatLiteral.TryGetNumberLiteral(num, out ILiteral max))
          Max = max;
        else
          throw new InvalidOperationException();
      }
    }

    public abstract void Visit(IExpressionVisitor visitor);
  }
}
