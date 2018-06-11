using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class ODataVisitor : IQueryVisitor
  {
    private readonly TextWriter _writer;
    private readonly Stack<IOperator> _operators = new Stack<IOperator>();
    private readonly IQueryWriterSettings _settings;
    private readonly IServerContext _context;
    private readonly ODataVersion _version;
    private bool _firstParam = true;

    public bool SupportsUtcDates { get; set; }

    public ODataVisitor(TextWriter writer, IQueryWriterSettings settings, IServerContext context, ODataVersion version = ODataVersion.All)
    {
      _writer = writer;
      _settings = settings;
      _context = context;
      _version = version;
    }

    public void Visit(QueryItem query)
    {
      WriteEncoded(query.Type);

      if (query.Where != null)
      {
        WriteParamStart("$filter");
        query.Where.Visit(this);
      }

      if (query.Select.Count > 0)
      {
        WriteParamStart("$select");
        for (var i = 0; i < query.Select.Count; i++)
        {
          if (i > 0)
            _writer.Write(',');
          query.Select[i].Expression.Visit(this);
        }
      }

      if (query.OrderBy.Count > 0)
      {
        WriteParamStart("$orderby");
        for (var i = 0; i < query.OrderBy.Count; i++)
        {
          if (i > 0)
            _writer.Write(',');
          query.OrderBy[i].Expression.Visit(this);
          if (!query.OrderBy[i].Ascending)
            WriteEncoded(" desc");
        }
      }

      if (query.Fetch.HasValue)
      {
        WriteParamStart("$top");
        _writer.Write(query.Fetch);
      }

      if (query.Offset.HasValue)
      {
        WriteParamStart("$skip");
        _writer.Write(query.Fetch);
      }
    }

    private void WriteParamStart(string name)
    {
      _writer.Write(_firstParam ? '?' : '&');
      _firstParam = false;
      _writer.Write(name);
      _writer.Write('=');
    }

    private void WriteEncoded(string value)
    {
      foreach (var b in Encoding.UTF8.GetBytes(value))
      {
        if ((b >= 'a' && b <= 'z')
          || (b >= 'A' && b <= 'Z')
          || (b >= '0' && b <= '9')
          || b == '/'
          || b == '?'
          || b == '-'
          || b == '.'
          || b == '_'
          || b == '~'
          || b == '!'
          || b == '$'
          || b == '\''
          || b == '('
          || b == ')'
          || b == '*'
          || b == ','
          || b == ';'
          || b == ':'
          || b == '@')
        {
          _writer.Write((char)b);
        }
        else
        {
          _writer.Write('%');
          _writer.Write(b.ToString("X2"));
        }
      }
    }

    public void Visit(AndOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" and ");
        op.Right.Visit(this);
      });
    }

    public void Visit(BetweenOperator op)
    {
      op.ToConditional().Visit(this);
    }

    public void Visit(BooleanLiteral op)
    {
      if (op.Value)
        WriteEncoded("true");
      else
        WriteEncoded("false");
    }

    public void Visit(DateTimeLiteral op)
    {
      if (_version.OnlySupportsV2OrV3())
      {
        WriteEncoded("datetime'");
        WriteEncoded(_context.AsZonedDateTime(op.Value).Value.ZoneDateTime.ToString("s"));
        WriteEncoded("'");
      }
      else
      {
        if (op.Value.TimeOfDay.Equals(TimeSpan.Zero))
          WriteEncoded(_context.AsZonedDateTime(op.Value).Value.ZoneDateTime.ToString("yyyy-MM-dd"));
        else if (SupportsUtcDates)
          WriteEncoded(_context.AsZonedDateTime(op.Value).Value.UtcDateTime.ToString("s") + "Z");
        else
          WriteEncoded(_context.AsZonedDateTime(op.Value).Value.ZoneDateTime.ToString("s"));
      }
    }

    public void Visit(EqualsOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" eq ");
        op.Right.Visit(this);
      });
    }

    public void Visit(FloatLiteral op)
    {
      WriteEncoded(_context.Format(op.Value));
      if (_version.OnlySupportsV2OrV3())
        WriteEncoded("d");
    }

    public void Visit(FunctionExpression op)
    {
      var name = op.Name.ToLowerInvariant();
      switch (name)
      {
        case "length":
        case "tolower":
        case "toupper":
        case "trim":
        case "day":
        case "hour":
        case "minute":
        case "month":
        case "second":
        case "year":
        case "ceiling":
        case "floor":
        case "startswith":
        case "endswith":
        case "contains":
          // Do nothing
          break;
        case "round":
          if (!(op.Args.Skip(1).First() is IntegerLiteral iLit && iLit.Value == 0))
            throw new NotSupportedException();
          break;
        case "indexof_zero":
          name = "indexof";
          break;
        case "substring_zero":
          name = "substring";
          break;
        case "truncatetime":
          name = "date";
          break;
        case "currentdatetime":
        case "currentutcdatetime":
          name = "now";
          break;
        default:
          op.Evaluate().Visit(this);
          return;
      }

      WriteEncoded(name);
      WriteEncoded("(");
      var first = true;
      foreach (var arg in op.Args)
      {
        if (!first)
          WriteEncoded(",");
        first = false;
        arg.Visit(this);
      }
      WriteEncoded(")");
    }

    public void Visit(GreaterThanOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" gt ");
        op.Right.Visit(this);
      });
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" ge ");
        op.Right.Visit(this);
      });
    }

    public void Visit(InOperator op)
    {
      op.ToConditional().Visit(this);
    }

    public void Visit(IntegerLiteral op)
    {
      if (op.Value <= int.MaxValue && op.Value >= int.MinValue)
      {
        var i = (int)op.Value;
        WriteEncoded(_context.Format(i));
      }
      else if (_version.OnlySupportsV2OrV3())
      {
        WriteEncoded(_context.Format(op.Value) + "L");
      }
      else
      {
        WriteEncoded(_context.Format(op.Value));
      }
    }

    public void Visit(IsOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        switch (op.Right)
        {
          case IsOperand.Defined:
          case IsOperand.NotNull:
            WriteEncoded(" ne ");
            break;
          default:
            WriteEncoded(" eq ");
            break;
        }
        WriteEncoded("null");
      });
    }

    public void Visit(LessThanOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" lt ");
        op.Right.Visit(this);
      });
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" le ");
        op.Right.Visit(this);
      });
    }

    public void Visit(LikeOperator op)
    {
      if (!(op.Right is PatternList pattern && pattern.Patterns.Count == 1))
        throw new NotSupportedException();

      var matches = pattern.Patterns[0].Matches;
      if (matches.Count == 1 && matches[0] is StringMatch s1)
      {
        if (_version.OnlySupportsV2OrV3())
        {
          WriteEncoded("substringof('");
          WriteEncoded(s1.Match.ToString().Replace("'", "''"));
          WriteEncoded("',");
          op.Left.Visit(this);
          WriteEncoded(")");
        }
        else
        {
          WriteEncoded("contains(");
          op.Left.Visit(this);
          WriteEncoded(",'");
          WriteEncoded(s1.Match.ToString().Replace("'", "''"));
          WriteEncoded("')");
        }
      }
      else if (matches.Count == 2
        && matches[0] is Anchor a2
        && a2.Type == AnchorType.Start_Absolute
        && matches[1] is StringMatch s2)
      {
        WriteEncoded("startswith(");
        op.Left.Visit(this);
        WriteEncoded(",'");
        WriteEncoded(s2.Match.ToString().Replace("'", "''"));
        WriteEncoded("')");
      }
      else if (matches.Count == 2
        && matches[0] is StringMatch s3
        && matches[1] is Anchor a3
        && a3.Type == AnchorType.End_Absolute)
      {
        WriteEncoded("endswith(");
        op.Left.Visit(this);
        WriteEncoded(",'");
        WriteEncoded(s3.Match.ToString().Replace("'", "''"));
        WriteEncoded("')");
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    public void Visit(ListExpression op)
    {
      throw new NotSupportedException();
    }

    public void Visit(NotEqualsOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" ne ");
        op.Right.Visit(this);
      });
    }

    public void Visit(NotOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        WriteEncoded(" not ");
        op.Arg.Visit(this);
      });
    }

    public void Visit(ObjectLiteral op)
    {
      op.Normalize(_settings).Visit(this);
    }

    public void Visit(OrOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" or ");
        op.Right.Visit(this);
      });
    }

    public void Visit(PropertyReference op)
    {
      var first = true;
      foreach (var prop in GetPropTree(op).Reverse())
      {
        if (!first)
          WriteEncoded("/");
        first = false;
        WriteEncoded(prop.Name);
      }
    }

    public void Visit(StringLiteral op)
    {
      WriteEncoded("'");
      WriteEncoded(op.Value.Replace("'", "''"));
      WriteEncoded("'");
    }

    public void Visit(MultiplicationOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" mul ");
        op.Right.Visit(this);
      });
    }

    public void Visit(DivisionOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" div ");
        op.Right.Visit(this);
      });
    }

    public void Visit(ModulusOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" mod ");
        op.Right.Visit(this);
      });
    }

    public void Visit(AdditionOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" add ");
        op.Right.Visit(this);
      });
    }

    public void Visit(SubtractionOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        WriteEncoded(" sub ");
        op.Right.Visit(this);
      });
    }

    public void Visit(NegationOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        WriteEncoded(" -");
        op.Arg.Visit(this);
      });
    }

    public void Visit(ConcatenationOperator op)
    {
      WriteEncoded("concat(");
      op.Left.Visit(this);
      WriteEncoded(",");
      op.Right.Visit(this);
      WriteEncoded(")");
    }

    public void Visit(ParameterReference op)
    {
      WriteEncoded("@");
      WriteEncoded(op.Name);
    }

    public void Visit(AllProperties op)
    {
      WriteEncoded("*");
    }

    public void Visit(PatternList op)
    {
      throw new NotSupportedException();
    }

    private IEnumerable<PropertyReference> GetPropTree(PropertyReference curr)
    {
      while (curr != null)
      {
        yield return curr;
        curr = curr.Table?.TypeProvider;
      }
    }

    protected void AddParenthesesIfNeeded(IOperator op, Action render)
    {
      var paren = _operators.Count > 0 && GetPrecedence(_operators.Peek()) > GetPrecedence(op);

      if (paren)
        WriteEncoded("(");
      _operators.Push(op);

      render();

      _operators.Pop();
      if (paren)
        WriteEncoded(")");
    }

    private int GetPrecedence(IOperator op)
    {
      if (op is NotOperator)
        return (int)PrecedenceLevel.Negation;
      return op.Precedence;
    }
  }
}
