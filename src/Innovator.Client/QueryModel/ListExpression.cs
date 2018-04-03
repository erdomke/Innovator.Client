using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class ListExpression : IOperand
  {
    private List<IOperand> _values;
    public IList<IOperand> Values { get { return _values; } }

    public ListExpression()
    {
      _values = new List<IOperand>();
    }

    public ListExpression(IEnumerable<IOperand> values)
    {
      _values = values.ToList();
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public static ListExpression FromSqlInClause(string clause)
    {
      var tokenizer = new SqlTokenizer(clause);
      var result = new ListExpression();

      var tokens = tokenizer.Where(t => t.Type != SqlType.Comment).ToList();
      if (tokens[0].Text == "(")
      {
        tokens.RemoveAt(0);
        if (tokens[tokens.Count - 1].Text != ")")
          throw new InvalidOperationException();
        tokens.RemoveAt(tokens.Count - 1);
      }

      for (var i = 0; i < tokens.Count; i++)
      {
        if ((i % 2) == 1)
        {
          if (tokens[i].Text != ",")
            throw new InvalidOperationException();
        }
        else if (!Expression.TryGetExpression(tokens[i], out IExpression expr))
        {
          throw new InvalidOperationException();
        }
        else
        {
          result.Values.Add((IOperand)expr);
        }
      }

      return result;
    }
  }
}
