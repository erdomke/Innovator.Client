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

    public static ListExpression FromSqlInClause(string clause, PropertyReference prop = null)
    {
      var result = new ListExpression();
      if (clause.IndexOf('\'') < 0)
      {
        var parts = clause.Split(',');
        if (parts.All(p => double.TryParse(p, out var dbl)))
        {
          foreach (var part in parts)
          {
            if (long.TryParse(part, out var lng))
              result.Values.Add(new IntegerLiteral(lng));
            else
              result.Values.Add(new FloatLiteral(double.Parse(part)));
          }
        }
        else
        {
          foreach (var part in parts)
          {
            result.Values.Add(new StringLiteral(part));
          }
        }
      }
      else
      {
        var tokenizer = new SqlTokenizer(clause);
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
      }

      return result;
    }

    public override string ToString()
    {
      return this.ToSqlString();
    }
  }
}
