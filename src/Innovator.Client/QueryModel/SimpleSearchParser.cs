using Innovator.Client.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public class SimpleSearchParser
  {
    private IServerContext _context;

    public HashSet<char> OrDelimiters { get; } = new HashSet<char>() { '|' };
    public char OrEscapeCharacter { get; set; } = '\\';

    public IServerContext Context
    {
      get { return _context; }
      set
      {
        _context = value;
        Date.SetContext(value);
      }
    }

    public BooleanParser Boolean { get; } = new BooleanParser();
    public DateParser Date { get; } = new DateParser();
    public NumberParser Number { get; } = new NumberParser();
    public StringParser String { get; } = new StringParser();

    internal IExpression Parse(QueryItem table, IPropertyDefinition prop, string value, Condition condition = Condition.Undefined)
    {
      var propRef = new PropertyReference(prop.NameProp().Value
        ?? prop.KeyedName().Value
        ?? prop.IdProp().KeyedName().Value, table);
      var meta = prop.Metadata();
      if (meta.DataType().Value == "item")
        propRef = new PropertyReference("keyed_name", propRef.GetOrAddTable(table.Context));

      var expressions = new List<IExpression>();
      var values = default(IEnumerable<string>);
      switch (condition)
      {
        case Condition.Undefined:
        case Condition.Equal:
        case Condition.NotEqual:
        case Condition.Like:
        case Condition.NotLike:
          values = SplitOr(value);
          break;
        default:
          values = new[] { value };
          break;
      }

      foreach (var val in values)
      {
        if ((val == "*" || (val == "%" && String.IsPercentWildcard)) && condition == Condition.Undefined)
        {
          expressions.Add(new LikeOperator()
          {
            Left = propRef,
            Right = AmlLikeParser.Instance.Parse("%")
          }.Normalize());
        }
        else if (condition == Condition.IsNotNull)
        {
          return new IsOperator()
          {
            Left = propRef,
            Right = IsOperand.NotNull
          }.Normalize();
        }
        else if (condition == Condition.IsNull)
        {
          return new IsOperator()
          {
            Left = propRef,
            Right = IsOperand.Null
          }.Normalize();
        }
        else
        {
          switch (meta.DataType().AsString("").ToLowerInvariant())
          {
            case "boolean":
              expressions.Add(Boolean.Parse(propRef, val, condition));
              break;
            case "date":
              expressions.Add(Date.Parse(propRef, val, condition));
              break;
            case "decimal":
            case "float":
            case "integer":
              expressions.Add(Number.Parse(propRef, val, condition));
              break;
            default:
              expressions.Add(String.Parse(propRef, val, condition));
              break;
          }
        }
      }

      var addNotOperator = expressions.Count > 0 && expressions.All(e => e is NotOperator);
      if (addNotOperator)
        expressions = expressions.Cast<NotOperator>().Select(n => n.Arg).ToList();

      var result = default(IExpression);
      if (expressions.Count > 0 && expressions.All(e => e is EqualsOperator eq && eq.Right is IOperand))
      {
        var list = new ListExpression();
        foreach (var op in expressions.Cast<EqualsOperator>().Select(e => (IOperand)e.Right))
          list.Values.Add(op);
        result = new InOperator()
        {
          Left = propRef,
          Right = list
        }.Normalize();
      }
      else if (expressions.Count > 0 && expressions.All(e => e is NotEqualsOperator eq && eq.Right is IOperand))
      {
        var list = new ListExpression();
        foreach (var op in expressions.Cast<NotEqualsOperator>().Select(e => (IOperand)e.Right))
          list.Values.Add(op);
        result = new NotInOperator()
        {
          Left = propRef,
          Right = list
        }.Normalize();
      }
      else
      {
        result = expressions[0];
        foreach (var expr in expressions.Skip(1))
        {
          result = new OrOperator()
          {
            Left = result,
            Right = expr
          }.Normalize();
        }
      }

      if (addNotOperator)
        result = new NotOperator() { Arg = result }.Normalize();
      return result;
    }

    private IEnumerable<string> SplitOr(string value)
    {
      var builder = new StringBuilder();
      for (var i = 0; i < value.Length; i++)
      {
        if (value[i] == OrEscapeCharacter)
        {
          i++;
          builder.Append(value[i]);
          if (value[i] == '\r' && (i + 1) < value.Length && value[i + 1] == '\n')
          {
            i++;
            builder.Append(value[i]);
          }
        }
        else if (OrDelimiters.Contains(value[i]))
        {
          yield return builder.ToString();
          builder.Length = 0;
        }
        else
        {
          builder.Append(value[i]);
        }
      }

      yield return builder.ToString();
    }

    private interface ISimpleParser
    {
      IExpression Parse(PropertyReference prop, string value, Condition condition = Condition.Undefined);
    }

    public class StringParser : ISimpleParser
    {
      private bool _allowCharRanges = true;
      private bool _isPercentWildcard = true;
      private PatternParser _parser = AmlLikeParser.Instance;

      public bool AllowCharacterRanges
      {
        get { return _allowCharRanges; }
        set
        {
          _allowCharRanges = value;
          SetParser();
        }
      }

      public bool DefaultSearchIsContains { get; set; } = false;

      public bool IsPercentWildcard
      {
        get { return _isPercentWildcard; }
        set
        {
          _isPercentWildcard = value;
          SetParser();
        }
      }

      public IExpression Parse(PropertyReference prop, string value, Condition condition = Condition.Undefined)
      {
        if (condition == Condition.Undefined && (value.IndexOf('*') >= 0
          || (value.IndexOf('%') >= 0 && IsPercentWildcard)
          || (value.IndexOf('[') >= 0 && AllowCharacterRanges)))
        {
          condition = Condition.Like;
        }

        if (DefaultSearchIsContains && condition == Condition.Undefined)
        {
          condition = Condition.Like;
          value = "*" + value + "*";
        }

        if (condition == Condition.Like || condition == Condition.NotLike)
        {
          if (condition == Condition.NotLike)
          {
            return new NotLikeOperator()
            {
              Left = prop,
              Right = _parser.Parse(value)
            }.Normalize();
          }
          else
          {
            return new LikeOperator()
            {
              Left = prop,
              Right = _parser.Parse(value)
            }.Normalize();
          }
        }
        else
        {
          switch (condition)
          {
            case Condition.Equal:
              return new EqualsOperator()
              {
                Left = prop,
                Right = new StringLiteral(value)
              }.Normalize();
            case Condition.NotEqual:
              return new NotEqualsOperator()
              {
                Left = prop,
                Right = new StringLiteral(value)
              }.Normalize();
            default:
              throw new InvalidOperationException();
          }
        }
      }

      public string Render(PatternList pattern)
      {
        if (DefaultSearchIsContains
          && pattern.Patterns.Count == 1
          && pattern.Patterns[0].Matches.Count == 1
          && pattern.Patterns[0].Matches[0] is StringMatch str)
        {
          return str.Match.ToString();
        }
        return _parser.Render(pattern);
      }

      private void SetParser()
      {
        if (_isPercentWildcard)
        {
          _parser = _allowCharRanges ? AmlLikeParser.Instance : AmlLikeParser.NoCharSet;
        }
        else
        {
          _parser = _allowCharRanges
            ? new AmlLikeParser('*', '\0', '\0', '\0', '^', '-')
            : new AmlLikeParser('*', '\0', '\0', '\0', '\0', '\0');
        }
      }
    }

    public class NumberParser : ISimpleParser
    {
      public bool AllowOperators { get; set; } = true;
      public bool AllowSiPrefixes { get; set; } = true;

      public IExpression Parse(PropertyReference prop, string value, Condition condition = Condition.Undefined)
      {
        if (value.StartsWith("<=") && AllowOperators && condition == Condition.Undefined)
        {
          return new LessThanOrEqualsOperator()
          {
            Left = prop,
            Right = NumberLiteral(value.Substring(2))
          }.Normalize();
        }
        else if (value.StartsWith(">=") && AllowOperators && condition == Condition.Undefined)
        {
          return new GreaterThanOrEqualsOperator()
          {
            Left = prop,
            Right = NumberLiteral(value.Substring(2))
          }.Normalize();
        }
        else if (value.StartsWith("<") && AllowOperators && condition == Condition.Undefined)
        {
          return new LessThanOperator()
          {
            Left = prop,
            Right = NumberLiteral(value.Substring(1))
          }.Normalize();
        }
        else if (value.StartsWith(">") && AllowOperators && condition == Condition.Undefined)
        {
          return new GreaterThanOperator()
          {
            Left = prop,
            Right = NumberLiteral(value.Substring(1))
          }.Normalize();
        }
        else if (value.IndexOf("...") > 0 && AllowOperators && condition == Condition.Undefined)
        {
          var parts = value.Split(new[] { "..." }, StringSplitOptions.None);
          if (parts.Length != 2)
            throw new InvalidOperationException();
          return new BetweenOperator()
          {
            Left = prop,
            Min = NumberLiteral(parts[0]),
            Max = NumberLiteral(parts[1])
          }.Normalize();
        }
        else
        {
          switch (condition)
          {
            case Condition.Equal:
              return new EqualsOperator()
              {
                Left = prop,
                Right = NumberLiteral(value)
              }.Normalize();
            case Condition.GreaterThan:
              return new GreaterThanOperator()
              {
                Left = prop,
                Right = NumberLiteral(value)
              }.Normalize();
            case Condition.GreaterThanEqual:
              return new GreaterThanOrEqualsOperator()
              {
                Left = prop,
                Right = NumberLiteral(value)
              }.Normalize();
            case Condition.LessThan:
              return new LessThanOperator()
              {
                Left = prop,
                Right = NumberLiteral(value)
              }.Normalize();
            case Condition.LessThanEqual:
              return new LessThanOrEqualsOperator()
              {
                Left = prop,
                Right = NumberLiteral(value)
              }.Normalize();
            case Condition.NotEqual:
              return new NotEqualsOperator()
              {
                Left = prop,
                Right = NumberLiteral(value)
              }.Normalize();
            default:
              throw new InvalidOperationException();
          }
        }
      }

      private ILiteral NumberLiteral(string value)
      {
        var exponent = 0;

        if (AllowSiPrefixes)
        {
          switch (value[value.Length - 1])
          {
            case 'P': exponent = 15; break;
            case 'T': exponent = 12; break;
            case 'G': exponent = 9; break;
            case 'M': exponent = 6; break;
            case 'K':
            case 'k': exponent = 3; break;
            case 'm': exponent = -3; break;
            case '\u00B5':
            case '\u03BC':
            case 'u': exponent = -6; break;
            case 'n': exponent = -9; break;
            case 'p': exponent = -12; break;
            case 'f': exponent = -15; break;
          }
        }

        if (exponent != 0)
          value = value.Substring(0, value.Length - 1);

        if (long.TryParse(value, out var lng) && exponent == 0)
          return new IntegerLiteral(lng);

        if (!double.TryParse(value, out var dbl))
          throw new InvalidOperationException();

        return new FloatLiteral(dbl * Math.Pow(10, exponent));
      }
    }

    public class DateParser : ISimpleParser
    {
      private readonly HashSet<string> _dateFormats = new HashSet<string>();
      private CultureInfo _culture;
      private DateTimeZone _tz;

      public void SetContext(IServerContext context)
      {
        _tz = DateTimeZone.ById(context.TimeZoneCorporate);
        _culture = new CultureInfo(context.Locale);
        var dateFormatting = _culture.DateTimeFormat;
        _dateFormats.Clear();
        _dateFormats.Add(dateFormatting.SortableDateTimePattern);
        _dateFormats.Add(dateFormatting.UniversalSortableDateTimePattern);
        _dateFormats.Add("yyyy");
        _dateFormats.Add("yyyy-MM");
        _dateFormats.Add("yyyy-MM-dd");
#if DATEFORMATLIST
        _dateFormats.UnionWith(dateFormatting.GetAllDateTimePatterns().Where(f => f.IndexOf('y') >= 0));
#endif
        _dateFormats.Add(dateFormatting.FullDateTimePattern);
        _dateFormats.Add(dateFormatting.LongDatePattern);
        _dateFormats.Add(dateFormatting.ShortDatePattern);
        _dateFormats.Add(dateFormatting.ShortDatePattern + " " + dateFormatting.LongTimePattern);
        _dateFormats.Add(dateFormatting.ShortDatePattern + " " + dateFormatting.ShortTimePattern);
        _dateFormats.Add(dateFormatting.YearMonthPattern);
      }

      public IExpression Parse(PropertyReference prop, string value, Condition condition = Condition.Undefined)
      {
        var parses = _dateFormats
          .Select(f => new DateTimeParse(f, value, _tz, _culture))
          .Where(p => p.Value.HasValue)
          .OrderBy(p => p.Part)
          .ToArray();

        if (parses.Select(p => p.Value).Distinct().Count() != 1)
          throw new InvalidOperationException();

        var start = parses[0].Value.Value;

        switch (condition)
        {
          case Condition.Undefined:
          case Condition.Equal:
          case Condition.NotEqual:
            if (parses[0].Part == DatePart.Second)
            {
              if (condition == Condition.NotEqual)
              {
                return new NotEqualsOperator()
                {
                  Left = prop,
                  Right = new DateTimeLiteral(start.LocalDateTime)
                }.Normalize();
              }
              else
              {
                return new EqualsOperator()
                {
                  Left = prop,
                  Right = new DateTimeLiteral(start.LocalDateTime)
                }.Normalize();
              }
            }
            else
            {
              var end = start;
              switch (parses[0].Part)
              {
                case DatePart.Minute:
                  end = start.AddMinutes(1).AddSeconds(-1);
                  break;
                case DatePart.Hour:
                  end = start.AddHours(1).AddSeconds(-1);
                  break;
                case DatePart.Day:
                  end = start.AddDays(1).AddSeconds(-1);
                  break;
                case DatePart.Month:
                  end = start.AddMonths(1).AddSeconds(-1);
                  break;
                default:
                  end = start.AddYears(1).AddSeconds(-1);
                  break;
              }

              if (condition == Condition.NotEqual)
              {
                return new NotBetweenOperator()
                {
                  Left = prop,
                  Min = new DateTimeLiteral(start.LocalDateTime),
                  Max = new DateTimeLiteral(end.LocalDateTime)
                }.Normalize();
              }
              else
              {
                return new BetweenOperator()
                {
                  Left = prop,
                  Min = new DateTimeLiteral(start.LocalDateTime),
                  Max = new DateTimeLiteral(end.LocalDateTime)
                }.Normalize();
              }
            }
          case Condition.GreaterThan:
            return new GreaterThanOperator()
            {
              Left = prop,
              Right = new DateTimeLiteral(start.LocalDateTime)
            }.Normalize();
          case Condition.GreaterThanEqual:
            return new GreaterThanOrEqualsOperator()
            {
              Left = prop,
              Right = new DateTimeLiteral(start.LocalDateTime)
            }.Normalize();
          case Condition.LessThan:
            return new LessThanOperator()
            {
              Left = prop,
              Right = new DateTimeLiteral(start.LocalDateTime)
            }.Normalize();
          case Condition.LessThanEqual:
            return new LessThanOrEqualsOperator()
            {
              Left = prop,
              Right = new DateTimeLiteral(start.LocalDateTime)
            }.Normalize();
          default:
            throw new InvalidOperationException();
        }
      }

      private class DateTimeParse
      {
        public ZonedDateTime? Value { get; }
        public DatePart Part { get; }

        public DateTimeParse(string format, string value, DateTimeZone tz, CultureInfo culture)
        {
          Value = ZonedDateTime.TryParseExact(value, format, culture, DateTimeStyles.AssumeLocal, tz, out var instant)
            ? instant
            : default(ZonedDateTime?);
          if (format.IndexOf('s') >= 0)
            Part = DatePart.Second;
          else if (format.IndexOf('m') >= 0)
            Part = DatePart.Minute;
          else if (format.IndexOf('h') >= 0 || format.IndexOf('H') >= 0)
            Part = DatePart.Hour;
          else if (format.IndexOf('d') >= 0)
            Part = DatePart.Day;
          else if (format.IndexOf('M') >= 0)
            Part = DatePart.Month;
          else
            Part = DatePart.Year;
        }
      }

      private enum DatePart
      {
        Year,
        Month,
        Day,
        Hour,
        Minute,
        Second
      }
    }

    public class BooleanParser : ISimpleParser
    {
      public IExpression Parse(PropertyReference prop, string value, Condition condition = Condition.Undefined)
      {
        if (value == "0" || value == "1")
        {
          switch (condition)
          {
            case Condition.Equal:
            case Condition.Undefined:
              return new EqualsOperator()
              {
                Left = prop,
                Right = new BooleanLiteral(value == "1")
              }.Normalize();
            case Condition.NotEqual:
              return new NotEqualsOperator()
              {
                Left = prop,
                Right = new BooleanLiteral(value == "1")
              }.Normalize();
            default:
              throw new InvalidOperationException();
          }
        }
        else
        {
          throw new InvalidOperationException();
        }
      }
    }
  }
}
