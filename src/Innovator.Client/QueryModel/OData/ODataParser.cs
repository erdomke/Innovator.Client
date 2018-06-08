using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public static class ODataParser
  {
    public static QueryItem Parse(string uri, IServerContext context, ODataVersion version = ODataVersion.All)
    {
      var tokens = new List<ODataToken>();
      var spans = new Dictionary<string, QuerySpan>(StringComparer.OrdinalIgnoreCase);
      var span = default(QuerySpan);
      var table = new QueryItem(context);
      var funcStart = -1;

      using (var tokenizer = new ODataTokenizer(uri, version))
      {
        var i = 0;
        while (tokenizer.MoveNext())
        {
          tokens.Add(tokenizer.Current);
          if (tokenizer.Current.Type == ODataTokenType.Question)
          {
            table.Type = tokens.LastOrDefault(t => t.Type == ODataTokenType.Identifier)?.Text;
            funcStart = tokens.Count - 2;
            while (funcStart >= 0 && tokens[funcStart].Type != ODataTokenType.OpenParen)
              funcStart--;
          }
          else if (tokenizer.Current.Type == ODataTokenType.QueryName)
          {
            if (span != null)
              span.Length = i - span.Start - 1;
            span = new QuerySpan()
            {
              Name = tokenizer.Current.Text,
              Start = i + 2
            };
            spans[span.Name] = span;
          }
          i++;
        }

        if (span != null)
          span.Length = i - span.Start;
      }

      if (!tokens.Any(t => t.Type == ODataTokenType.Question))
      {
        table.Type = tokens.LastOrDefault(t => t.Type == ODataTokenType.Identifier)?.Text;
        funcStart = tokens.Count - 1;
        while (funcStart >= 0 && tokens[funcStart].Type != ODataTokenType.OpenParen)
          funcStart--;
      }

      if (spans.TryGetValue("$filter", out span))
      {
        table.Where = ParseWhere(GetExpressions(tokens, table, span.Start, span.Length), table.Context);

        if (table.Where is PropertyReference)
        {
          table.Where = new EqualsOperator()
          {
            Left = table.Where,
            Right = new BooleanLiteral(true)
          }.Normalize();
        }
      }

      if (spans.TryGetValue("$select", out span))
      {
        foreach (var select in GetExpressions(tokens, table, span.Start, span.Length)
          .OfType<PropertyReference>()
          .Select(p => new SelectExpression()
          {
            Expression = p
          }))
        {
          table.Select.Add(select);
        }
      }

      if (funcStart > 0 && tokens[funcStart - 1].Type == ODataTokenType.Identifier
        && funcStart + 1 < tokens.Count && tokens[funcStart + 1].Type == ODataTokenType.String)
      {
        if (tokens[funcStart - 1].Text == table.Type
          || string.Equals(table.Type, "$value", StringComparison.OrdinalIgnoreCase))
        {
          table.Where = new EqualsOperator()
          {
            Left = new PropertyReference("id", table),
            Right = new StringLiteral((string)tokens[funcStart + 1].AsPrimitive())
          }.Normalize();

          if (string.Equals(table.Type, "$value", StringComparison.OrdinalIgnoreCase))
          {
            table.Type = tokens[funcStart - 1].Text;
            table.Select.Clear();
            table.Select.Add(new SelectExpression()
            {
              Expression = new PropertyReference(tokens.Skip(funcStart + 1).First(t => t.Type == ODataTokenType.Identifier).Text, table)
            });
          }
        }
        else
        {
          table.AddCondition(new EqualsOperator()
          {
            Left = new PropertyReference("source_id", table),
            Right = new StringLiteral((string)tokens[funcStart + 1].AsPrimitive())
          }.Normalize());
        }
      }

      if (spans.TryGetValue("$orderby", out span))
      {
        var expressions = GetExpressions(tokens, table, span.Start, span.Length).ToList();
        expressions.Add(new Comma()); // Make sure we always get the last part of the order by

        var pos = 0;
        for (var i = 0; i < expressions.Count; i++)
        {
          if (expressions[i] is Comma)
          {
            if ((i - pos) == 1)
            {
              table.OrderBy.Add(new OrderByExpression()
              {
                Expression = expressions[pos],
                Ascending = true
              });
              pos = i + 1;
            }
            else if ((i - pos) == 2 && expressions[i - 1] is PropertyReference direction)
            {
              if (string.Equals(direction.Name, "asc", StringComparison.OrdinalIgnoreCase))
              {
                table.OrderBy.Add(new OrderByExpression()
                {
                  Expression = expressions[pos],
                  Ascending = true
                });
              }
              else if (string.Equals(direction.Name, "desc", StringComparison.OrdinalIgnoreCase))
              {
                table.OrderBy.Add(new OrderByExpression()
                {
                  Expression = expressions[pos],
                  Ascending = false
                });
              }
              else
              {
                throw new InvalidOperationException();
              }
              pos = i + 1;
            }
            else
            {
              throw new InvalidOperationException();
            }
          }
        }
      }

      if (spans.TryGetValue("$expand", out span))
      {
        foreach (var expandTable in GetExpressions(tokens, table, span.Start, span.Length)
          .OfType<PropertyReference>()
          .Select(p => p.GetOrAddTable(context)))
        {
          expandTable.Select.Clear();
          expandTable.Select.Add(new SelectExpression()
          {
            Expression = new AllProperties(expandTable),
            OnlyReturnNonNull = true
          });
        }
      }

      if (spans.TryGetValue("$top", out span))
      {
        if (span.Length != 1 || tokens[span.Start].Type != ODataTokenType.Integer)
          throw new InvalidOperationException();
        table.Fetch = (int)tokens[span.Start].AsPrimitive();
      }

      if (spans.TryGetValue("$skip", out span))
      {
        if (span.Length != 1 || tokens[span.Start].Type != ODataTokenType.Integer)
          throw new InvalidOperationException();
        table.Offset = (int)tokens[span.Start].AsPrimitive();
      }

      return table;
    }

    private static IExpression ParseWhere(IEnumerable<IExpression> expressions, IServerContext context)
    {
      var output = new List<IExpression>();
      var ops = new List<IExpression>();
      var last = default(IExpression);

      foreach (var expression in expressions)
      {
        var expr = expression;
        if (expr is ILiteral || expr is PropertyReference)
        {
          output.Push(expr);
        }
        else
        {
          var precedence = GetPrecedence(expr);

          if (expr is EndParen)
          {
            while (ops.Count > 0 && !IsOpenParen(ops.Peek()))
            {
              output.Push(HandleOperator(ops.Pop(), output, context));
            }
            var parenExpr = ops.Pop();
            if (!IsOpenParen(last))
            {
              var args = new List<IExpression>();
              FlattenArgs(args, output.Pop());
              if (ops.Count > 0
                && ops.Peek() is InOperator
                && parenExpr is ListExpression listExpr)
              {
                foreach (var arg in args.Cast<IOperand>())
                {
                  listExpr.Values.Add(arg);
                }
              }
              else if (parenExpr is ODataFunction funcExpr)
              {
                foreach (var arg in args)
                {
                  funcExpr.Add(arg);
                }
                parenExpr = funcExpr.Normalize();
              }
              else if (args.Count == 1)
              {
                parenExpr = args[0];
              }
              else
              {
                throw new NotSupportedException();
              }
            }
            output.Push(parenExpr);
          }
          else if (expr is ListExpression && last is PropertyReference prop)
          {
            output.Pop();
            ops.Push(new ODataFunction(prop.Name));
          }
          else if (ops.Count < 1 || precedence > GetPrecedence(ops.Peek()))
          {
            ops.Push(expr);
          }
          else
          {
            while (ops.Count > 0 && precedence <= GetPrecedence(ops.Peek()) && !IsOpenParen(ops.Peek()))
            {
              output.Push(HandleOperator(ops.Pop(), output, context));
            }
            ops.Push(expr);
          }
        }
        last = expr;
      }

      while (ops.Count > 0)
      {
        output.Push(HandleOperator(ops.Pop(), output, context));
      }

      if (output.Count == 1)
        return output.Pop();

      throw new NotSupportedException();
    }

    private static int GetPrecedence(IExpression expr)
    {
      if (expr is ListExpression || expr is FunctionExpression)
        return (int)PrecedenceLevel.Parentheses;
      else if (expr is NotOperator)
        return (int)PrecedenceLevel.Negation;
      else if (expr is EndParen)
        return -1;
      else if (expr is IOperator op)
        return op.Precedence;

      throw new NotSupportedException();
    }

    private static bool IsOpenParen(IExpression expr)
    {
      return expr is FunctionExpression || expr is ListExpression;
    }

    private static void FlattenArgs(List<IExpression> list, IExpression expr)
    {
      var comma = expr as Comma;
      if (comma == null)
      {
        list.Add(expr);
      }
      else
      {
        FlattenArgs(list, comma.Left);
        FlattenArgs(list, comma.Right);
      }
    }

    private static IExpression HandleOperator(IExpression op, List<IExpression> output, IServerContext context)
    {
      if (op is BinaryOperator binOp)
      {
        binOp.Right = output.Pop();
        binOp.Left = output.Pop();
      }
      else if (op is UnaryOperator unaryOp)
      {
        unaryOp.Arg = output.Pop();
      }
      else
      {
        throw new NotSupportedException();
      }

      if (op is INormalize norm)
        return norm.Normalize();

      return op;
    }

    private class QuerySpan
    {
      public string Name { get; set; }
      public int Start { get; set; }
      public int Length { get; set; }
    }

    private class ODataFunction : FunctionExpression, INormalize
    {
      private int _ptr;
      public override string Name { get; }

      public ODataFunction(string name) : base(16)
      {
        Name = name;
      }

      public void Add(IExpression expression)
      {
        _args[_ptr++] = expression;
      }

      public IExpression Normalize()
      {
        switch (Name.ToLowerInvariant())
        {
          case "concat":
            if (_ptr < 1)
              throw new NotSupportedException();
            if (_ptr == 1)
              return _args[0];
            var concat = (IExpression)new ConcatenationOperator()
            {
              Left = _args[0],
              Right = _args[1]
            }.Normalize();
            for (var i = 2; i < _ptr; i++)
            {
              concat = new ConcatenationOperator()
              {
                Left = concat,
                Right = _args[i]
              }.Normalize();
            }
            return concat;
          case "contains":
          case "endswith":
          case "startswith":
            if (_ptr == 2)
              return LikeOperator.FromMethod(Name, _args[0], _args[1]);
            break;
          case "substringof":
            if (_ptr == 2)
              return LikeOperator.FromMethod("contains", _args[1], _args[0]);
            break;
          case "indexof":
            if (_ptr == 2)
            {
              return new Functions.IndexOf_Zero() { String = _args[0], Target = _args[1] };
            }
            break;
          case "length":
            if (_ptr == 1)
              return new Functions.Length() { String = _args[0] };
            break;
          case "substring":
            if (_ptr == 2)
            {
              return new Functions.Substring_Zero()
              {
                String = _args[0],
                Start = _args[1],
                Length = new SubtractionOperator()
                {
                  Left = new Functions.Length()
                  {
                    String = _args[2]
                  },
                  Right = _args[1]
                }.Normalize()
              };
            }
            else if (_ptr == 3)
            {
              return new Functions.Substring_Zero()
              {
                String = _args[0],
                Start = _args[1],
                Length = _args[2]
              };
            }
            break;
          case "tolower":
            if (_ptr == 1)
              return new Functions.ToLower() { String = _args[0] };
            break;
          case "toupper":
            if (_ptr == 1)
              return new Functions.ToUpper() { String = _args[0] };
            break;
          case "trim":
            if (_ptr == 1)
              return new Functions.Trim() { String = _args[0] };
            break;
          case "date":
            if (_ptr == 1)
              return new Functions.TruncateTime() { Expression = _args[0] };
            break;
          case "day":
            if (_ptr == 1)
              return new Functions.Day() { Expression = _args[0] };
            break;
          case "fractionalseconds":
            if (_ptr == 1)
            {
              return new DivisionOperator()
              {
                Left = new Functions.Millisecond() { Expression = _args[0] },
                Right = new FloatLiteral(1000)
              }.Normalize();
            }
            break;
          case "hour":
            if (_ptr == 1)
              return new Functions.Hour() { Expression = _args[0] };
            break;
          case "minute":
            if (_ptr == 1)
              return new Functions.Minute() { Expression = _args[0] };
            break;
          case "month":
            if (_ptr == 1)
              return new Functions.Month() { Expression = _args[0] };
            break;
          case "now":
            if (_ptr == 0)
              return new Functions.CurrentDateTime();
            break;
          case "second":
            if (_ptr == 1)
              return new Functions.Second() { Expression = _args[0] };
            break;
          case "year":
            if (_ptr == 1)
              return new Functions.Year() { Expression = _args[0] };
            break;
          case "ceiling":
            if (_ptr == 1)
              return new Functions.Ceiling() { Value = _args[0] };
            break;
          case "floor":
            if (_ptr == 1)
              return new Functions.Floor() { Value = _args[0] };
            break;
          case "round":
            if (_ptr == 1)
              return new Functions.Round() { Value = _args[0], Digits = new IntegerLiteral(0) };
            break;
        }

        throw new NotSupportedException();
      }
    }

    private static IEnumerable<IExpression> GetExpressions(IList<ODataToken> tokens, QueryItem table, int start, int length)
    {
      for (var i = start; i < start + length; i++)
      {
        var token = tokens[i];
        switch (token.Type)
        {
          case ODataTokenType.Parameter:
            yield return new ParameterReference(token.Text.Substring(1), false);
            break;
          case ODataTokenType.Date:
          case ODataTokenType.TimeOfDay:
            var dateText = token.Text;
            if (dateText.StartsWith("datetime'"))
              dateText = dateText.Substring(9).TrimEnd('\'');
            if (!ZonedDateTime.TryParse(dateText, table.Context.GetTimeZone(), out var date))
              throw new InvalidOperationException();
            yield return new DateTimeLiteral(date.LocalDateTime);
            break;
          case ODataTokenType.Decimal:
            yield return new FloatLiteral((double)((decimal)token.AsPrimitive()));
            break;
          case ODataTokenType.Single:
            yield return new FloatLiteral((double)((float)token.AsPrimitive()));
            break;
          case ODataTokenType.Double:
          case ODataTokenType.NaN:
          case ODataTokenType.NegInfinity:
          case ODataTokenType.PosInfinity:
            yield return new FloatLiteral((double)token.AsPrimitive());
            break;
          case ODataTokenType.False:
            yield return new BooleanLiteral(false);
            break;
          case ODataTokenType.Guid:
            yield return new StringLiteral(((Guid)token.AsPrimitive()).ToArasId());
            break;
          case ODataTokenType.Integer:
            yield return new IntegerLiteral((int)token.AsPrimitive());
            break;
          case ODataTokenType.Long:
            yield return new IntegerLiteral((long)token.AsPrimitive());
            break;
          case ODataTokenType.String:
            yield return new StringLiteral((string)token.AsPrimitive());
            break;
          case ODataTokenType.True:
            yield return new BooleanLiteral(true);
            break;
          case ODataTokenType.Null:
            yield return new NullLiteral();
            break;
          case ODataTokenType.Identifier:
            yield return TryGetProperty(tokens, ref i, table);
            break;
          case ODataTokenType.And:
            yield return new AndOperator();
            break;
          case ODataTokenType.Or:
            yield return new OrOperator();
            break;
          case ODataTokenType.Equal:
            yield return new EqualsOperator();
            break;
          case ODataTokenType.NotEqual:
            yield return new NotEqualsOperator();
            break;
          case ODataTokenType.LessThan:
            yield return new LessThanOperator();
            break;
          case ODataTokenType.LessThanOrEqual:
            yield return new LessThanOrEqualsOperator();
            break;
          case ODataTokenType.GreaterThan:
            yield return new GreaterThanOperator();
            break;
          case ODataTokenType.GreaterThanOrEqual:
            yield return new GreaterThanOrEqualsOperator();
            break;
          case ODataTokenType.Add:
            yield return new AdditionOperator();
            break;
          case ODataTokenType.Subtract:
            yield return new SubtractionOperator();
            break;
          case ODataTokenType.Multiply:
            yield return new MultiplicationOperator();
            break;
          case ODataTokenType.Divide:
            yield return new DivisionOperator();
            break;
          case ODataTokenType.Modulo:
            yield return new ModulusOperator();
            break;
          case ODataTokenType.Not:
            yield return new NotOperator();
            break;
          case ODataTokenType.Negate:
            yield return new NegationOperator();
            break;
          case ODataTokenType.Star:
            yield return new AllProperties(table);
            break;
          case ODataTokenType.OpenParen:
            yield return new ListExpression();
            break;
          case ODataTokenType.CloseParen:
            yield return new EndParen();
            break;
          case ODataTokenType.Comma:
            yield return new Comma();
            break;
          case ODataTokenType.Base64:
          case ODataTokenType.Binary:
          case ODataTokenType.Duration:
          case ODataTokenType.Has:
          case ODataTokenType.Navigation:
            throw new NotSupportedException();

          default:
            //return new IgnoredNode(token);
            break;
        }
      }
    }

    private static PropertyReference TryGetProperty(IList<ODataToken> tokens, ref int index, QueryItem table)
    {
      if (tokens[index].Type != ODataTokenType.Identifier)
        throw new InvalidOperationException();

      var path = new List<string>() { tokens[index].Text };
      while (index + 2 < tokens.Count
        && tokens[index + 1].Type == ODataTokenType.Navigation
        && tokens[index + 2].Type == ODataTokenType.Identifier)
      {
        path.Add(tokens[index + 2].Text);
        index += 2;
      }

      return table.GetProperty(path);
    }
  }
}
