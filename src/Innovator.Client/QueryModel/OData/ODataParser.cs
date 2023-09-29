using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
      var segments = default(List<SegmentInfo>);

      using (var tokenizer = new ODataTokenizer(uri, version))
      {
        var i = 0;
        while (tokenizer.MoveNext())
        {
          tokens.Add(tokenizer.Current);
          if (tokenizer.Current.Type == ODataTokenType.Question)
          {
            segments = SegmentInfo.Create(new QuerySpan(tokens, "", 0) { Length = tokens.Count - 1 });
          }
          else if (tokenizer.Current.Type == ODataTokenType.QueryName)
          {
            if (span != null)
              span.Length = i - span.Start - 1;
            span = new QuerySpan(tokens, tokenizer.Current.Text, i + 2);
            spans[span.Name] = span;
          }
          i++;
        }

        if (span != null)
          span.Length = i - span.Start;
      }

      if (segments == null)
        segments = SegmentInfo.Create(new QuerySpan(tokens, "", 0) { Length = tokens.Count });

      if (segments.Any(s => string.Equals(s.Name, "$entity", StringComparison.OrdinalIgnoreCase)))
      {
        if (!(spans.TryGetValue("$id", out var idInfo) || spans.TryGetValue("id", out idInfo)))
          throw new InvalidOperationException();
        var s = idInfo.Start;
        if (idInfo.List[s].Text.StartsWith("http", StringComparison.OrdinalIgnoreCase)
          && idInfo.List[s].Text.EndsWith(":")
          && idInfo.Length > 3
          && idInfo.List[s + 1].Type == ODataTokenType.Navigation
          && idInfo.List[s + 2].Type == ODataTokenType.Navigation)
        {
          s += 3;
          while (idInfo.List[s].Type != ODataTokenType.Navigation)
            s++;
          s++;
        }
        var idSpan = new QuerySpan(idInfo.List, "", s) { Length = idInfo.Length - (s - idInfo.Start) };
        segments = SegmentInfo.Create(idSpan);
      }

      var end = segments.Count - 1;
      if (string.Equals(segments.Last().Name, "$value", StringComparison.OrdinalIgnoreCase))
        end -= 2;
      else if (segments.Last().Name[0] == '$')
        end--;

      var start = end;
      for (var i = 0; i <= end; i++)
      {
        if (segments[i].Args.All(a => a.Type == ODataTokenType.String) && segments[i].Args.Count() == 1)
        {
          start = i;
          break;
        }
      }

      table.Type = segments[start].Name;
      ProcessSpans(tokens, table, spans, context);

      if (segments[start].Args.Any())
      {
        if (start == end)
        {
          table.Where = new EqualsOperator()
          {
            Left = new PropertyReference("id", table),
            Right = new StringLiteral((string)segments[start].Args.Single().AsPrimitive())
          }.Normalize();
        }
        else if (start + 1 == end)
        {
          table.Type = segments[end].Name;
          table.AddCondition(new EqualsOperator()
          {
            Left = new PropertyReference("source_id", table),
            Right = new StringLiteral((string)segments[start].Args.Single().AsPrimitive())
          }.Normalize());
        }
        else
        {
          throw new InvalidOperationException();
        }

        if (string.Equals(segments.Last().Name, "$value", StringComparison.OrdinalIgnoreCase))
        {
          table.Select.Clear();
          table.Select.Add(new SelectExpression()
          {
            Expression = new PropertyReference(segments[end + 1].Name, table)
          });
        }
      }

      if (string.Equals(segments.Last().Name, "$count", StringComparison.OrdinalIgnoreCase))
      {
        table.Select.Clear();
        table.Select.Add(new SelectExpression()
        {
          Expression = new CountAggregate()
          {
            TablePath = { table }
          }
        });
      }

      table.RebalanceCriteria();
      return table;
    }

    [DebuggerDisplay("{Name}")]
    private class SegmentInfo
    {
      private List<ODataToken> _args;

      public string Name { get; }
      public IEnumerable<ODataToken> Args { get { return _args ?? Enumerable.Empty<ODataToken>(); } }

      private SegmentInfo(string name)
      {
        Name = name;
      }

      public static List<SegmentInfo> Create(QuerySpan tokens)
      {
        var result = new List<SegmentInfo>();
        var last = tokens.Start + tokens.Length;
        for (var i = tokens.Start; i < last; i++)
        {
          if (tokens.List[i].Type == ODataTokenType.Identifier)
          {
            var segment = new SegmentInfo(tokens.List[i].Text);
            if ((i + 1) < last && tokens.List[i + 1].Type == ODataTokenType.OpenParen)
            {
              segment._args = new List<ODataToken>();
              i += 2;
              while (i < last && tokens.List[i].Type != ODataTokenType.CloseParen)
              {
                if (tokens.List[i].Type != ODataTokenType.Comma)
                  segment._args.Add(tokens.List[i]);
                i++;
              }
            }
            result.Add(segment);
          }
        }
        return result;
      }
    }

    private static void ProcessSpans(IList<ODataToken> tokens, QueryItem table, Dictionary<string, QuerySpan> spans, IServerContext context)
    {
      var span = default(QuerySpan);
      var parseContext = new ParseContext();

      if (spans.TryGetValue("$compute", out span) || spans.TryGetValue("compute", out span))
      {
        parseContext.Name = "$compute";
        var tree = ParseWhere(GetExpressions(table, span, parseContext), table.Context);
        var list = new List<IExpression>();
        FlattenArgs(list, tree);
        foreach (var expr in list.Cast<SelectExpression>())
        {
          parseContext.Computed[new PropertyReference(expr.Alias, table)] = expr.Expression;
        }
      }

      if (spans.TryGetValue("$filter", out span) || spans.TryGetValue("filter", out span))
      {
        parseContext.Name = "$filter";
        table.Where = ParseWhere(GetExpressions(table, span, parseContext), table.Context);

        if (table.Where is PropertyReference)
        {
          table.Where = new EqualsOperator()
          {
            Left = table.Where,
            Right = new BooleanLiteral(true)
          }.Normalize();
        }
      }

      if (spans.TryGetValue("$select", out span) || spans.TryGetValue("select", out span))
      {
        parseContext.Name = "$select";
        foreach (var select in GetProperties(table, span, context, parseContext))
        {
          var provider = (ITableProvider)select;
          while (provider != null)
          {
            var propTable = provider is PropertyReference prop && IsCollectionProperty(prop)
              ? PropToTable(prop)
              : provider.Table;
            if (propTable == provider.Table)
              AddPropToSelectIfNotThere(provider.Table, (IExpression)provider);
            if (provider.Table == table)
              break;
            provider = provider.Table.TypeProvider;
          }
        }
      }

      if (spans.TryGetValue("$orderby", out span) || spans.TryGetValue("orderby", out span))
      {
        parseContext.Name = "$orderby";
        var expressions = GetExpressions(table, span, parseContext).ToList();
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

      if (spans.TryGetValue("$expand", out span) || spans.TryGetValue("expand", out span))
      {
        parseContext.Name = "$expand";
        foreach (var expandTable in GetProperties(table, span, context, parseContext)
          .ToArray()
          .Cast<PropertyReference>()
          .Select(GetExpansionTable))
        {
          if (expandTable.Select.Count < 1)
          {
            expandTable.Select.Add(new SelectExpression()
            {
              Expression = new AllProperties(expandTable),
              OnlyReturnNonNull = true
            });
          }
        }
      }

      if (spans.TryGetValue("$top", out span) || spans.TryGetValue("top", out span))
      {
        if (span.Length != 1 || tokens[span.Start].Type != ODataTokenType.Integer)
          throw new InvalidOperationException();
        table.Fetch = (int)tokens[span.Start].AsPrimitive();
      }

      if (spans.TryGetValue("$skip", out span) || spans.TryGetValue("skip", out span))
      {
        if (span.Length != 1 || tokens[span.Start].Type != ODataTokenType.Integer)
          throw new InvalidOperationException();
        table.Offset = (int)tokens[span.Start].AsPrimitive();
      }
    }

    private static bool IsCollectionProperty(PropertyReference prop)
    {
      return char.IsUpper(prop.Name[0]);
    }

    private static QueryItem GetExpansionTable(PropertyReference prop)
    {
      if (!IsCollectionProperty(prop))
        return prop.GetOrAddTable(prop.Table.Context);

      return PropToTable(prop);
    }

    private static QueryItem PropToTable(PropertyReference prop)
    {
      var join = prop.Table.Joins
        .Select(j => AmlJoin.TryCreate(prop.Table, j, out var amlJoin) ? amlJoin : null)
        .FirstOrDefault(j => j?.IsRelationship() == true
          && string.Equals(j.Table.Type, prop.Name, StringComparison.OrdinalIgnoreCase));
      if (join != null)
        return join.Table;

      var right = new QueryItem(prop.Table.Context) { Type = prop.Name };
      prop.Table.Joins.Add(new Join()
      {
        Condition = new EqualsOperator()
        {
          Left = new PropertyReference("id", prop.Table),
          Right = new PropertyReference("source_id", right)
        },
        Type = JoinType.LeftOuter,
        Left = prop.Table,
        Right = right
      });
      return right;
    }

    private static void AddPropToSelectIfNotThere(QueryItem table, IExpression value)
    {
      if (value is PropertyReference prop
        && table.Select
          .Select(s => s.Expression)
          .OfType<PropertyReference>()
          .Any(p => p.Equals(prop)))
      {
        return;
      }
      table.Select.Add(new SelectExpression()
      {
        Expression = value
      });
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
      else if (expr is SelectExpression)
        return (int)PrecedenceLevel.Comma + 1;

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
      else if (op is InOperator inOp)
      {
        inOp.Right = (ListExpression)output.Pop();
        inOp.Left = output.Pop();
      }
      else if (op is SelectExpression select)
      {
        select.Alias = ((PropertyReference)output.Pop()).Name;
        select.Expression = output.Pop();
      }
      else
      {
        throw new NotSupportedException();
      }

      if (op is INormalize norm)
        return norm.Normalize();

      return op;
    }

    private class QuerySpan : IEnumerable<ODataToken>
    {
      public string Name { get; }
      public int Start { get; }
      public int Length { get; set; }
      public IList<ODataToken> List { get; }

      public QuerySpan(IList<ODataToken> list, string name, int start)
      {
        List = list;
        Name = name;
        Start = start;
      }

      public IEnumerator<ODataToken> GetEnumerator()
      {
        for (var i = Start; i < Start + Length; i++)
          yield return List[i];
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }
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

    private static IEnumerable<IExpression> GetProperties(QueryItem table, QuerySpan tokens, IServerContext context, ParseContext parseContext)
    {
      var depth = 0;
      var span = new QuerySpan(tokens.List, "", tokens.Start);
      var spans = new List<QuerySpan>() { span };
      for (var i = tokens.Start; i < tokens.Start + tokens.Length; i++)
      {
        if (tokens.List[i].Text == "(")
        {
          if (depth == 0)
          {
            span.Length = i - span.Start;
            span = new QuerySpan(tokens.List, "(", i + 1);
            spans.Add(span);
          }
          depth++;
        }
        else if (tokens.List[i].Text == ")")
        {
          depth--;
          if (depth == 0)
          {
            span.Length = i - span.Start;
            span = new QuerySpan(tokens.List, "", i + 1);
            spans.Add(span);
          }
        }
      }
      span.Length = tokens.Start + tokens.Length - span.Start;

      var last = default(IExpression);
      foreach (var s in spans)
      {
        if (s.Name == "(")
        {
          if (!(last is PropertyReference prop))
            throw new InvalidOperationException();
          var newTable = GetExpansionTable(prop);
          var dict = GetSpans(s);
          ProcessSpans(tokens.List, newTable, dict, context);
        }
        else
        {
          foreach (var expr in GetExpressions(table, s, parseContext))
          {
            if (expr is PropertyReference || expr is AllProperties)
              yield return expr;
            last = expr;
          }
        }
      }
    }

    private static Dictionary<string, QuerySpan> GetSpans(QuerySpan tokens)
    {
      if (tokens.First().Type != ODataTokenType.Identifier)
        throw new InvalidOperationException();
      var span = new QuerySpan(tokens.List, tokens.First().Text, tokens.Start + 2);
      var result = new Dictionary<string, QuerySpan>(StringComparer.OrdinalIgnoreCase)
      {
        [span.Name] = span
      };

      for (var i = tokens.Start + 1; i < tokens.Start + tokens.Length; i++)
      {
        if (tokens.List[i].Type == ODataTokenType.Semicolon)
        {
          span.Length = i - span.Start;
          if ((i + 1) < tokens.Start + tokens.Length)
          {
            i++;
            if (tokens.List[i].Type != ODataTokenType.Identifier)
              throw new InvalidOperationException();
            span = new QuerySpan(tokens.List, tokens.List[i].Text, i + 2);
            result[span.Name] = span;
          }
        }
      }
      span.Length = tokens.Start + tokens.Length - span.Start;
      return result;
    }

    private static IEnumerable<IExpression> GetExpressions(QueryItem table, QuerySpan tokens, ParseContext context)
    {
      for (var i = tokens.Start; i < tokens.Start + tokens.Length; i++)
      {
        var token = tokens.List[i];
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
            if (!ZonedDateTime.TryParse(dateText, table.Context.GetTimeZoneUser(), out var date))
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
            if (string.Equals(context.Name, "$compute", StringComparison.OrdinalIgnoreCase)
              && string.Equals(tokens.List[i].Text, "as", StringComparison.OrdinalIgnoreCase))
            {
              yield return new SelectExpression();
            }
            else
            {
              yield return TryGetProperty(tokens.List, ref i, table, context);
            }
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
          case ODataTokenType.In:
            yield return new InOperator();
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

    private static IExpression TryGetProperty(IList<ODataToken> tokens, ref int index, QueryItem table, ParseContext context)
    {
      if (tokens[index].Type != ODataTokenType.Identifier)
        throw new InvalidOperationException();

      var path = new List<string>() { GetIdentifier(tokens, ref index, context) };
      var prop = new PropertyReference(path[0], table);
      if (context.Computed.TryGetValue(prop, out var expr))
        return expr;

      while (index + 2 < tokens.Count
        && tokens[index + 1].Type == ODataTokenType.Navigation
        && tokens[index + 2].Type == ODataTokenType.Identifier)
      {
        index += 2;
        path.Add(GetIdentifier(tokens, ref index, context));
      }

      return table.GetProperty(path);
    }

    private static string GetIdentifier(IList<ODataToken> tokens, ref int index, ParseContext context)
    {
      if (tokens[index].Type != ODataTokenType.Identifier)
        throw new InvalidOperationException();
      var builder = new StringBuilder(tokens[index].Text);

      while (index + 2 < tokens.Count
        && tokens[index + 1].Type == ODataTokenType.Whitespace
        && tokens[index + 2].Type == ODataTokenType.Identifier
        && !context.IsOperator(tokens[index + 2].Text))
      {
        builder
          .Append(tokens[index + 1].Text)
          .Append(tokens[index + 2].Text);
        index += 2;
      }

      return builder.ToString();
    }

    private class ParseContext
    {
      private static readonly KeyValuePair<string, string>[] _dontCombine = new[]
      {
        new KeyValuePair<string, string>("$orderby", "asc"),
        new KeyValuePair<string, string>("$orderby", "desc"),
        new KeyValuePair<string, string>("$compute", "as"),
      };

      public Dictionary<PropertyReference, IExpression> Computed { get; } = new Dictionary<PropertyReference, IExpression>();
      public string Name { get; set; }

      public bool IsOperator(string identifier)
      {
        return _dontCombine.Any(k => string.Equals(Name, k.Key, StringComparison.OrdinalIgnoreCase)
          && string.Equals(identifier, k.Value, StringComparison.OrdinalIgnoreCase));
      }
    }
  }
}
