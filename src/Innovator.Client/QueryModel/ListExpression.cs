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

    private enum State
    {
      Start,
      ValueStart,
      Comma,
      StringValue,
      NumberValue,
      End,
    }

    public static ListExpression FromSqlInClause(string clause)
    {
      var state = State.Start;
      var tokenStart = 0;
      var result = new ListExpression();
      for (var i = 0; i <= clause.Length; i++)
      {
        switch (state)
        {
          case State.Start:
            if (i == clause.Length || char.IsWhiteSpace(clause[i]))
            {
              // Do nothing
            }
            else if (clause[i] == '(')
            {
              state = State.ValueStart;
            }
            else if (clause[i] == '\'')
            {
              state = State.StringValue;
              tokenStart = i + 1;
            }
            else if (clause[i] == '-' || clause[i] == '+' || char.IsNumber(clause[i]))
            {
              state = State.NumberValue;
              tokenStart = i;
            }
            else
            {
              throw new InvalidOperationException($"Character `{clause[i]}` is invalid in state {state}");
            }
            break;
          case State.ValueStart:
            if (i == clause.Length || char.IsWhiteSpace(clause[i]) || clause[i] == ',')
            {
              // Do nothing
            }
            else if (clause[i] == '\'')
            {
              state = State.StringValue;
              tokenStart = i + 1;
            }
            else if (clause[i] == '-' || clause[i] == '+' || char.IsNumber(clause[i]))
            {
              state = State.NumberValue;
              tokenStart = i;
            }
            else if (clause[i] == ')')
            {
              state = State.End;
            }
            else
            {
              throw new InvalidOperationException($"Character `{clause[i]}` is invalid in state {state}");
            }
            break;
          case State.NumberValue:
            if (i == clause.Length || clause[i] == ',' || char.IsWhiteSpace(clause[i]))
            {
              var value = clause.Substring(tokenStart, i - tokenStart);
              if (FloatLiteral.TryGetNumberLiteral(value, out ILiteral literal))
                result.Values.Add(literal);
              else
                throw new InvalidOperationException($"`{value}` is not a valid number");
              state = i == clause.Length || char.IsWhiteSpace(clause[i]) ? State.Comma : State.ValueStart;
            }
            break;
          case State.StringValue:
            if (i == clause.Length)
              throw new InvalidOperationException("Unterminated string constant");

            if (clause[i] == '\'')
            {
              if ((i + 1) < clause.Length && clause[i + 1] == '\'')
              {
                i++;
              }
              else
              {
                result.Values.Add(new StringLiteral(clause.Substring(tokenStart, i - tokenStart).Replace("''", "'")));
                state = State.Comma;
              }
            }
            break;
          case State.Comma:
            if (i == clause.Length || char.IsWhiteSpace(clause[i]))
            {
              // Do nothing
            }
            else if (clause[i] == ',')
            {
              state = State.ValueStart;
            }
            else if (clause[i] == ')')
            {
              state = State.End;
            }
            else
            {
              throw new InvalidOperationException($"Character `{clause[i]}` is invalid in state {state}");
            }
            break;
          case State.End:
            if (i == clause.Length || char.IsWhiteSpace(clause[i]))
            {
              // Do nothing
            }
            else
            {
              throw new InvalidOperationException($"Character `{clause[i]}` is invalid in state {state}");
            }
            break;
        }
      }

      return result;
    }
  }
}
