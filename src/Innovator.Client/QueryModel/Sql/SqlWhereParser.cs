using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal static class SqlWhereParser
  {
    public static IExpression Parse(string whereClause, QueryItem table, IServerContext context = null)
    {
      context = context ?? ElementFactory.Utc.LocalizationContext;
      var tokens = new SqlTokenizer(whereClause).Where(t => t.Type != SqlType.Comment).ToArray();
      var expressions = GetExpressions(tokens, table).ToArray();

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
          if (expr is NotOperator && last is IsOperator)
          {
            expr = new NotIsOperator();
          }
          else if (expr is AndOperator)
          {
            for (var i = ops.Count - 1; i >= 0; i--)
            {
              if (ops[i] is AndOperator)
              {
                break;
              }
              else if (ops[i] is BetweenOperator)
              {
                expr = new AndBetweenOperator();
                break;
              }
            }
          }
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
              else if (parenExpr is SqlFunction funcExpr)
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
            ops.Push(new SqlFunction(prop.Name));
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
      if (op is LikeOperator like)
      {
        like.Right = output.Pop();
        like.Left = output.Pop();
        if (like.Right is StringLiteral str)
          like.Right = PatternParser.SqlServer.Parse(str.Value);
      }
      else if (op is BinaryOperator binOp)
      {
        binOp.Right = output.Pop();
        binOp.Left = output.Pop();
        if (binOp.Left is PropertyReference prop && binOp.Right is StringLiteral str)
          binOp.Right = AmlToModelWriter.NormalizeLiteral(prop, str.Value, context, AmlToModelWriter.AllowedTypes.SqlStrings);
      }
      else if (op is UnaryOperator unaryOp)
      {
        unaryOp.Arg = output.Pop();
      }
      else if (op is BetweenOperator between)
      {
        if (!(output.Pop() is AndOperator andOp))
          throw new NotSupportedException();
        between.Min = andOp.Left;
        between.Max = andOp.Right;
        between.Left = output.Pop();

        if (between.Left is PropertyReference prop)
        {
          if (between.Min is StringLiteral minStr)
            between.Min = AmlToModelWriter.NormalizeLiteral(prop, minStr.Value, context, AmlToModelWriter.AllowedTypes.SqlStrings);
          if (between.Max is StringLiteral maxStr)
            between.Max = AmlToModelWriter.NormalizeLiteral(prop, maxStr.Value, context, AmlToModelWriter.AllowedTypes.SqlStrings);
        }
      }
      else if (op is IsOperator isOp)
      {
        var right = output.Pop();
        isOp.Left = output.Pop();
        if (right is NotOperator notNullOp && notNullOp.Arg is NullLiteral)
          isOp.Right = IsOperand.NotNull;
        else if (right is NullLiteral)
          isOp.Right = IsOperand.Null;
        else
          throw new NotSupportedException();
      }
      else if (op is InOperator inOp)
      {
        if (!(output.Pop() is ListExpression list))
          throw new NotSupportedException();
        inOp.Right = list;
        inOp.Left = output.Pop();
      }
      else
      {
        throw new NotSupportedException();
      }

      if (op is INormalize norm)
        return norm.Normalize();

      return op;
    }

    private static IEnumerable<IExpression> GetExpressions(SqlToken[] tokens, QueryItem table)
    {
      var index = 0;
      while (index < tokens.Length)
      {
        switch (tokens[index].Type)
        {
          case SqlType.Keyword:
            switch (tokens[index++].Text.ToLowerInvariant())
            {
              case "is":
                yield return new IsOperator();
                break;
              case "null":
                yield return new NullLiteral();
                break;
              case "in":
                yield return new InOperator();
                break;
              case "like":
                yield return new LikeOperator();
                break;
              case "between":
                yield return new BetweenOperator();
                break;
              case "not":
                switch (tokens[index++].Text.ToLowerInvariant())
                {
                  case "in":
                    yield return new NotInOperator();
                    break;
                  case "like":
                    yield return new NotLikeOperator();
                    break;
                  case "between":
                    yield return new NotBetweenOperator();
                    break;
                  default:
                    index--;
                    yield return new NotOperator();
                    break;
                }
                break;
              case "and":
                yield return new AndOperator();
                break;
              case "or":
                yield return new OrOperator();
                break;
              default:
                throw new NotSupportedException();
            }
            break;
          case SqlType.Number:
          case SqlType.String:
            if (Expressions.TryGetExpression(tokens[index++], out var expr))
              yield return expr;
            else
              throw new NotSupportedException();
            break;
          case SqlType.Operator:
            switch (tokens[index++].Text.ToLowerInvariant())
            {
              case "=":
                yield return new EqualsOperator();
                break;
              case "<>":
              case "!=":
                yield return new NotEqualsOperator();
                break;
              case "<":
                yield return new LessThanOperator();
                break;
              case "<=":
              case "!>":
                yield return new LessThanOrEqualsOperator();
                break;
              case ">":
                yield return new GreaterThanOperator();
                break;
              case ">=":
              case "!<":
                yield return new GreaterThanOrEqualsOperator();
                break;
              case "(":
                yield return new ListExpression();
                break;
              case ")":
                yield return new EndParen();
                break;
              case ",":
                yield return new Comma();
                break;
              case "*":
                yield return new MultiplicationOperator();
                break;
              case "/":
                yield return new DivisionOperator();
                break;
              case "%":
                yield return new ModulusOperator();
                break;
              case "+":
                if ((index - 2 >= 0 && tokens[index - 2].Type == SqlType.String)
                  || (index < tokens.Length && tokens[index].Type == SqlType.String))
                {
                  yield return new ConcatenationOperator();
                }
                else
                {
                  yield return new AdditionOperator();
                }
                break;
              case "-":
                if (index - 2 >= 0 && (tokens[index - 2].Type == SqlType.Operator || tokens[index - 2].Type == SqlType.Keyword))
                  yield return new NegationOperator();
                else
                  yield return new SubtractionOperator();
                break;
              default:
                throw new NotSupportedException();
            }
            break;
          case SqlType.Identifier:
            if (tokens[index].Text[0] == '@')
              yield return new ParameterReference(tokens[index].Text.Substring(1), false);
            else
              yield return TryGetProperty(tokens, ref index, table);
            break;
          default:
            throw new NotSupportedException();
        }
      }
    }

    private static PropertyReference TryGetProperty(SqlToken[] tokens, ref int index, QueryItem table)
    {
      if (tokens[index].Type != SqlType.Identifier)
        throw new InvalidOperationException();

      var start = index;
      index++;
      while (index + 1 < tokens.Length
        && tokens[index].Type == SqlType.Operator
        && tokens[index].Text == "."
        && tokens[index + 1].Type == SqlType.Identifier)
      {
        index += 2;
      }

      var length = index - start;
      if (length > 5)
        throw new InvalidOperationException();
      if (length == 5 && !string.Equals(tokens[start].Text.TrimStart('[').TrimEnd(']'), "innovator", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException();
      if (length > 1 && !string.Equals(tokens[index - 3].Text.TrimStart('[').TrimEnd(']'), table.Type.Replace(' ', '_'), StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException();

      return new PropertyReference(tokens[index - 1].Text.TrimStart('[').TrimEnd(']'), table);
    }

    private class NotIsOperator : NotOperator
    {
      public override int Precedence => (int)PrecedenceLevel.SubComparison;
    }

    private class AndBetweenOperator : AndOperator
    {
      public override int Precedence => (int)PrecedenceLevel.SubComparison;
    }

    private class SqlFunction : FunctionExpression, INormalize
    {
      private int _ptr;
      public override string Name { get; }

      public SqlFunction(string name) : base(16)
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
          case "abs":
            if (_ptr == 1)
              return new Functions.Abs() { Value = _args[0] };
            break;
          case "ceiling":
            if (_ptr == 1)
              return new Functions.Ceiling() { Value = _args[0] };
            break;
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
          case "dateadd":
            if (_ptr == 3 && _args[0] is PropertyReference prop)
            {
              switch (prop.Name.ToLowerInvariant())
              {
                case "year":
                case "yy":
                case "yyyy":
                  return new Functions.AddYears() { Expression = _args[2], Number = _args[1] };
                case "month":
                case "mm":
                case "m":
                  return new Functions.AddMonths() { Expression = _args[2], Number = _args[1] };
                case "day":
                case "dd":
                case "d":
                  return new Functions.AddDays() { Expression = _args[2], Number = _args[1] };
                case "hour":
                case "hh":
                  return new Functions.AddHours() { Expression = _args[2], Number = _args[1] };
                case "minute":
                case "mi":
                case "n":
                  return new Functions.AddMinutes() { Expression = _args[2], Number = _args[1] };
                case "second":
                case "ss":
                case "s":
                  return new Functions.AddSeconds() { Expression = _args[2], Number = _args[1] };
                case "millisecond":
                case "ms":
                  return new Functions.AddMilliseconds() { Expression = _args[2], Number = _args[1] };
                case "microsecond":
                case "mcs":
                  return new Functions.AddMicroseconds() { Expression = _args[2], Number = _args[1] };
                case "nanosecond":
                case "ns":
                  return new Functions.AddNanoseconds() { Expression = _args[2], Number = _args[1] };
              }
            }
            break;
          case "datediff":
            if (_ptr == 3 && _args[0] is PropertyReference prop2)
            {
              switch (prop2.Name.ToLowerInvariant())
              {
                case "year":
                case "yy":
                case "yyyy":
                  return new Functions.DiffYears() { StartExpression = _args[1], EndExpression = _args[2] };
                case "month":
                case "mm":
                case "m":
                  return new Functions.DiffMonths() { StartExpression = _args[1], EndExpression = _args[2] };
                case "day":
                case "dd":
                case "d":
                  return new Functions.DiffDays() { StartExpression = _args[1], EndExpression = _args[2] };
                case "hour":
                case "hh":
                  return new Functions.DiffHours() { StartExpression = _args[1], EndExpression = _args[2] };
                case "minute":
                case "mi":
                case "n":
                  return new Functions.DiffMinutes() { StartExpression = _args[1], EndExpression = _args[2] };
                case "second":
                case "ss":
                case "s":
                  return new Functions.DiffSeconds() { StartExpression = _args[1], EndExpression = _args[2] };
                case "millisecond":
                case "ms":
                  return new Functions.DiffMilliseconds() { StartExpression = _args[1], EndExpression = _args[2] };
                case "microsecond":
                case "mcs":
                  return new Functions.DiffMicroseconds() { StartExpression = _args[1], EndExpression = _args[2] };
                case "nanosecond":
                case "ns":
                  return new Functions.DiffNanoseconds() { StartExpression = _args[1], EndExpression = _args[2] };
              }
            }
            break;
          case "datepart":
            if (_ptr == 2 && _args[0] is PropertyReference prop3)
            {
              switch (prop3.Name.ToLowerInvariant())
              {
                case "year":
                case "yy":
                case "yyyy":
                  return new Functions.Year() { Expression = _args[1] };
                case "month":
                case "mm":
                case "m":
                  return new Functions.Month() { Expression = _args[1] };
                case "day":
                case "dd":
                case "d":
                  return new Functions.Day() { Expression = _args[1] };
                case "hour":
                case "hh":
                  return new Functions.Hour() { Expression = _args[1] };
                case "minute":
                case "mi":
                case "n":
                  return new Functions.Minute() { Expression = _args[1] };
                case "second":
                case "ss":
                case "s":
                  return new Functions.Second() { Expression = _args[1] };
                case "millisecond":
                case "ms":
                  return new Functions.Millisecond() { Expression = _args[1] };
              }
            }
            break;
          case "day":
            if (_ptr == 1)
              return new Functions.Day() { Expression = _args[0] };
            break;
          case "floor":
            if (_ptr == 1)
              return new Functions.Floor() { Value = _args[0] };
            break;
          case "getdate":
            if (_ptr == 0)
              return new Functions.CurrentDateTime();
            break;
          case "getutcdate":
            if (_ptr == 0)
              return new Functions.CurrentUtcDateTime();
            break;
          case "left":
            if (_ptr == 2)
              return new Functions.Left() { String = _args[0], Length = _args[1] };
            break;
          case "lower":
            if (_ptr == 1)
              return new Functions.ToLower() { String = _args[0] };
            break;
          case "ltrim":
            if (_ptr == 1)
              return new Functions.LTrim() { String = _args[0] };
            break;
          case "month":
            if (_ptr == 1)
              return new Functions.Month() { Expression = _args[0] };
            break;
          case "newid":
            if (_ptr == 0)
              return new Functions.NewGuid();
            break;
          case "power":
            if (_ptr == 2)
              return new Functions.Power() { Value = _args[0], Exponent = _args[1] };
            break;
          case "replace":
            if (_ptr == 3)
              return new Functions.Replace() { String = _args[0], Find = _args[1], Substitute = _args[2] };
            break;
          case "reverse":
            if (_ptr == 1)
              return new Functions.Reverse() { String = _args[0] };
            break;
          case "right":
            if (_ptr == 2)
              return new Functions.Right() { String = _args[0], Length = _args[1] };
            break;
          case "round":
            if (_ptr == 2 || (_ptr == 3 && _args[2] is IntegerLiteral iLit && iLit.Value == 0))
              return new Functions.Round() { Value = _args[0], Digits = _args[1] };
            if (_ptr == 3)
              return new Functions.Truncate() { Value = _args[0], Digits = _args[1] };
            break;
          case "rtrim":
            if (_ptr == 1)
              return new Functions.RTrim() { String = _args[0] };
            break;
          case "substring":
            if (_ptr == 3)
              return new Functions.Substring_One() { String = _args[0], Start = _args[1], Length = _args[2] };
            break;
          case "trim":
            if (_ptr == 1)
              return new Functions.Trim() { String = _args[0] };
            break;
          case "upper":
            if (_ptr == 1)
              return new Functions.ToUpper() { String = _args[0] };
            break;
          case "year":
            if (_ptr == 1)
              return new Functions.Year() { Expression = _args[0] };
            break;
        }
        throw new NotSupportedException();
      }
    }

    private static int GetPrecedence(IExpression expr)
    {
      if (expr is ListExpression || expr is FunctionExpression)
        return (int)PrecedenceLevel.Parentheses;
      else if (expr is EndParen)
        return -1;
      else if (expr is IOperator op)
        return op.Precedence;

      throw new NotSupportedException();
    }
  }
}
