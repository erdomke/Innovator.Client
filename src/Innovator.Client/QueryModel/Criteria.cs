using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class Criteria
  {
    private readonly SimpleSearchParser _parser;

    public string Property { get; }
    public Condition Condition { get; }
    public object Value { get; }

    public Criteria(string property, Condition condition, object value, SimpleSearchParser parser)
    {
      Property = property;
      Condition = condition;
      Value = value;
      _parser = parser;
    }

    public string ToSimpleSearch()
    {
      switch (Condition)
      {
        case Condition.Equal:
        case Condition.Like:
        case Condition.In:
        case Condition.NotIn:
          return ToString();
        case Condition.LessThan:
          if (_parser.Number.AllowOperators)
            return "<" + ToString();
          break;
        case Condition.LessThanEqual:
          if (_parser.Number.AllowOperators)
            return "<=" + ToString();
          break;
        case Condition.GreaterThan:
          if (_parser.Number.AllowOperators)
            return ">" + ToString();
          break;
        case Condition.GreaterThanEqual:
          if (_parser.Number.AllowOperators)
            return ">=" + ToString();
          break;
      }
      throw new NotSupportedException();
    }

    public override string ToString()
    {
      if (Value is IEnumerable enumerable && !(Value is string))
      {
        switch (Condition)
        {
          case Condition.Like:
          case Condition.NotLike:
          case Condition.Equal:
          case Condition.NotEqual:
          case Condition.In:
          case Condition.NotIn:
            break;
          default:
            throw new NotSupportedException();
        }

        var separator = _parser.OrDelimiters.First().ToString();
        if (separator == "\r" || separator == "\n")
          separator = Environment.NewLine;

        return enumerable.OfType<object>().GroupConcat(separator, Render);
      }
      else
      {
        return Render(Value);
      }
    }

    private string Render(object value)
    {
      if (Condition == Condition.Between && Value is Range<ZonedDateTime> range)
      {
        var max = range.Maximum.ZoneDateTime.AddSeconds(1);
        var min = range.Minimum.ZoneDateTime;
        if (max == min.AddMinutes(1) && min.Second == 0)
          return min.ToString("yyyy-MM-ddTHH:mm");
        else if (max == min.AddHours(1) && min.Minute == 0 && min.Second == 0)
          return min.ToString("yyyy-MM-ddTHH");
        else if (max == min.AddDays(1) && min.TimeOfDay == TimeSpan.Zero)
          return min.ToString("yyyy-MM-dd");
        else if (max == min.AddMonths(1) && min.Day == 1 && min.TimeOfDay == TimeSpan.Zero)
          return min.ToString("yyyy-MM");
        else if (max == min.AddYears(1) && min.Day == 1 && min.Month == 1 && min.TimeOfDay == TimeSpan.Zero)
          return min.ToString("yyyy");
        else
          throw new NotSupportedException();
      }
      else
      {
        return _parser.Context.Format(value);
      }
    }

    private string Escape(string value)
    {
      if (_parser.OrEscapeCharacter == '\0')
        return value;

      var builder = new StringBuilder();
      for (var i = 0; i < value.Length; i++)
      {
        if (_parser.OrDelimiters.Contains(value[i]))
        {
          builder.Append(_parser.OrEscapeCharacter);
          builder.Append(value[i]);
          if (value[i] == '\r'
            && (i + 1) < value.Length
            && value[i + 1] == '\n')
          {
            builder.Append(value[++i]);
          }
        }
        else
        {
          builder.Append(value[i]);
        }
      }
      return builder.ToString();
    }
  }
}
