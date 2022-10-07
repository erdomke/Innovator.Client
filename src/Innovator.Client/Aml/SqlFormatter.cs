using System;
using System.Globalization;
using System.Text;

namespace Innovator.Client
{
  internal class SqlFormatter : IFormatProvider, ICustomFormatter
  {
    readonly IServerContext _serverContext;

    public SqlFormatter(IServerContext serverContext)
    {
      _serverContext = serverContext;
    }

    public object GetFormat(Type formatType)
    {
      if (formatType == typeof(ICustomFormatter))
        return this;
      else
        return null;
    }

    public string Format(string format, object arg, IFormatProvider formatProvider)
    {
      if (!this.Equals(formatProvider)) return null;
      if (arg == null) return "null";

      var parts = (format ?? "").Split(':');
      string formatString;
      var numberMode = false;
      IFormattable number = null;

      switch (parts[0])
      {
        case "str":
          numberMode = false;
          formatString = string.Join(":", parts, 1, parts.Length - 1);
          break;
        case "num":
          if (!ServerContext.TryCastNumber(arg, out number))
          {
            number = double.Parse(arg.ToString());
            numberMode = true;
          }
          formatString = string.Join(":", parts, 1, parts.Length - 1);
          break;
        default:
          numberMode = ServerContext.TryCastNumber(arg, out number);
          formatString = format;
          break;
      }

      if (numberMode)
      {
        return number.ToString(formatString, CultureInfo.InvariantCulture);
      }
      else
      {
        return Quote(Render(arg,
          n => n.ToString(formatString, CultureInfo.InvariantCulture),
          s => SqlEscape(arg, formatString)));
      }

    }

    internal static string Quote(string value)
    {
      if (value.IsGuid())
        return "'" + value + "'";
      return "N'" + value + "'";
    }

    private static string SqlEscape(object obj, string format)
    {
      if (obj == null) return null;

      var value = ServerContext.GetString(obj, format);
      var builder = new StringBuilder(value.Length + 10);
      for (var i = 0; i < value.Length; i++)
      {
        switch (value[i])
        {
          case '\'':
            builder.Append("''");
            break;
          default:
            builder.Append(value[i]);
            break;
        }
      }
      return builder.ToString();
    }

    internal string Format(object arg)
    {
      return Render(arg,
          n => n.ToString(null, CultureInfo.InvariantCulture),
          s => SqlEscape(s, null));
    }

    private string Render(object arg, Func<IFormattable, string> numberRenderer, Func<object, string> stringRenderer)
    {
      if (arg == null)
      {
        return "null";
      }
      else if (arg is Condition condition)
      {
        switch (condition)
        {
          case Condition.Between:
            return "between";
          case Condition.Equal:
            return "=";
          case Condition.GreaterThan:
            return ">";
          case Condition.GreaterThanEqual:
            return ">=";
          case Condition.In:
            return "in";
          case Condition.Is:
            return "is";
          case Condition.IsNotNull:
            return "is not null";
          case Condition.IsNull:
            return "is null";
          case Condition.LessThan:
            return "<";
          case Condition.LessThanEqual:
            return "<=";
          case Condition.Like:
            return "like";
          case Condition.NotBetween:
            return "not between";
          case Condition.NotEqual:
            return "<>";
          case Condition.NotIn:
            return "not in";
          case Condition.NotLike:
            return "not like";
          default:
            throw new NotSupportedException();
        }
      }
      else if (arg is QueryModel.PatternList pattern)
      {
        return stringRenderer(QueryModel.PatternParser.SqlServer.Render(pattern));
      }
      else if (ServerContext.TryCastNumber(arg, out IFormattable number))
      {
        return numberRenderer(number);
      }
      else if (arg is DateTime)
      {
        var serverContext = (ServerContext)_serverContext;
        return stringRenderer(ElementFactory.Utc.LocalizationContext.Format(serverContext.ConvertToUtcDateTime((DateTime)arg)));
      }
      else
      {
        return stringRenderer(_serverContext.Format(arg));
      }
    }
  }
}
