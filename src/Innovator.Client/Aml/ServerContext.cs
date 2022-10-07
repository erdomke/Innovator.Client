using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#if DBDATA
using System.Data.SqlTypes;
#endif
#if SERIALIZATION
using System.Runtime.Serialization;
#endif

namespace Innovator.Client
{
  /// <summary>
  /// Context for serializing/deserializing native types (e.g. <c>DateTime</c>, <c>double</c>, <c>boolean</c>, etc.)
  /// </summary>
#if SERIALIZATION
  [Serializable]
#endif
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public sealed class ServerContext : IServerContext
  {
    internal static Func<ZonedDateTime> _clock = () => new ZonedDateTime(DateTime.UtcNow, DateTimeZone.Local);

    private const string DoubleFixedPoint = "0.###################################################################################################################################################################################################################################################################################################################################################";
    private readonly static Regex TimeZoneMatch = new Regex(@"(^|\s)[+-]\d{1,2}(:\d{1,2})?($|\s)");
    private DateTimeZone _timeZoneCorporate;
    private DateTimeZone _timeZoneUser;

    /// <summary>
    /// Gets the default language code configured for the Aras user
    /// </summary>
    public string DefaultLanguageCode { get; set; }

    /// <summary>
    /// Gets the default language suffix configured for the Aras user
    /// </summary>
    public string DefaultLanguageSuffix { get; set; }

    /// <summary>
    /// Gets the language code configured for the Aras user
    /// </summary>
    public string LanguageCode { get; set; }

    /// <summary>
    /// Gets the language suffix configured for the Aras user
    /// </summary>
    public string LanguageSuffix { get; set; }

    /// <summary>
    /// Gets the locale configured for the user.
    /// </summary>
    public string Locale { get; set; }

    /// <summary>
    /// Gets the corporate time zone ID for the Aras installation
    /// </summary>
    public string TimeZoneCorporate
    {
      get { return _timeZoneCorporate.Id; }
      set { _timeZoneCorporate = DateTimeZone.ById(value); }
    }

    /// <summary>
    /// Gets the user time zone ID
    /// </summary>
    public string TimeZoneUser
    {
      get { return _timeZoneUser.Id; }
      set { _timeZoneUser = DateTimeZone.ById(value); }
    }

    internal DateTimeZone ZoneCorporate
    {
      get { return _timeZoneCorporate; }
    }

    internal DateTimeZone ZoneUser
    {
      get { return _timeZoneUser; }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
      get
      {
        return "Context: " + Locale + " / " + TimeZoneCorporate;
      }
    }

    /// <summary>
    /// Create a server context with corporate and user timezones.
    /// </summary>
    /// <param name="utc">Indicates the UTC time zone should be used for corporate.
    /// Otherwise, the local time zone will be used.</param>
    /// <param name="userTimeZoneName">The time zone to be used for the user.</param>
    /// <remarks>The corporate timezone will be updated after the user login</remarks>
    public ServerContext(bool utc, string userTimeZoneName = null) : this(userTimeZoneName)
    {
      if (utc)
      {
        _timeZoneCorporate = DateTimeZone.Utc;
        if (string.IsNullOrEmpty(userTimeZoneName))
        {
          _timeZoneUser = DateTimeZone.Utc;
        }
      }
      else
      {
        _timeZoneCorporate = DateTimeZone.Local;
      }
    }

    /// <summary>
    /// Create a server context using the specified corporate and user timezone names
    /// </summary>
    /// <param name="corporateTimeZoneName"></param>
    /// <param name="userTimeZoneName"></param>
    /// <remarks>The corporate timezone will be updated after the user login</remarks>
    public ServerContext(string corporateTimeZoneName, string userTimeZoneName = null) : this(userTimeZoneName)
    {
      this.TimeZoneCorporate = corporateTimeZoneName;
    }

    private ServerContext(string userTimeZoneName = null)
    {
      this.DefaultLanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
      this.LanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
      this.Locale = CultureInfo.CurrentCulture.Name;
      if (string.IsNullOrEmpty(userTimeZoneName))
      {
        _timeZoneUser = DateTimeZone.Local;
      }
      else
      {
        TimeZoneUser = userTimeZoneName;
      }
    }

#if SERIALIZATION
    private ServerContext(SerializationInfo info, StreamingContext context)
    {
      this.DefaultLanguageCode = info.GetString("default_language_code");
      this.DefaultLanguageSuffix = info.GetString("default_language_suffix");
      this.LanguageCode = info.GetString("language_code");
      this.LanguageSuffix = info.GetString("language_suffix");
      this.Locale = info.GetString("locale");
      this.TimeZoneCorporate = info.GetString("time_zone");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("default_language_code", this.DefaultLanguageCode);
      info.AddValue("default_language_suffix", this.DefaultLanguageSuffix);
      info.AddValue("language_code", this.LanguageCode);
      info.AddValue("language_suffix", this.LanguageSuffix);
      info.AddValue("locale", this.Locale);
      info.AddValue("time_zone", this.TimeZoneCorporate);
    }
#endif

    /// <summary>
    /// Converts the <see cref="object" /> to a <see cref="bool" /> based on
    /// the locale and time zone
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <c>null</c> if <paramref name="value" /> is null or empty.
    /// A <see cref="bool" /> if <paramref name="value" /> is convertible.
    /// Otherwise, an exception is thrown
    /// </returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="bool"/></exception>
    public bool? AsBoolean(object value)
    {
      if (value == null) return null;
      if (value is bool b) return b;
      if (!(value is string str))
        throw new InvalidCastException();

      if (str == "0")
      {
        return false;
      }
      else if (str == "1")
      {
        return true;
      }
      else if (str == "")
      {
        return null;
      }
      else
      {
        throw new InvalidCastException();
      }
    }

    /// <summary>
    /// Converts the <see cref="object" /> representing a date in the corporate
    /// time zone to a <see cref="ZonedDateTime" />
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <c>null</c> if <paramref name="value" /> is null or empty.
    /// A <see cref="ZonedDateTime" /> if <paramref name="value" /> is convertible.
    /// Otherwise, an exception is thrown
    /// </returns>
    /// <exception cref="InvalidCastException"></exception>
    public ZonedDateTime? AsZonedDateTime(object value)
    {
      if (!this.TryParseDateTime(value, out var result))
        throw new InvalidCastException();
      return result;
    }

    /// <summary>
    /// Converts the <see cref="object" /> to a <see cref="decimal" /> based on
    /// the locale and time zone
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <c>null</c> if <paramref name="value" /> is null or empty.
    /// A <see cref="decimal" /> if <paramref name="value" /> is convertible.
    /// Otherwise, an exception is thrown
    /// </returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="decimal"/></exception>
    public decimal? AsDecimal(object value)
    {
      if (value == null || (value is string str && str.Length < 1))
        return null;
      return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="object" /> to a <see cref="double" /> based on
    /// the locale and time zone
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <c>null</c> if <paramref name="value" /> is null or empty.
    /// A <see cref="double" /> if <paramref name="value" /> is convertible.
    /// Otherwise, an exception is thrown
    /// </returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="double"/></exception>
    public double? AsDouble(object value)
    {
      if (value == null || (value is string str && str.Length < 1))
        return null;
      return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="object" /> to a <see cref="int" /> based on
    /// the locale and time zone
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <c>null</c> if <paramref name="value" /> is null or empty.
    /// A <see cref="int" /> if <paramref name="value" /> is convertible.
    /// Otherwise, an exception is thrown
    /// </returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="int"/></exception>
    public int? AsInt(object value)
    {
      if (value == null || (value is string str && str.Length < 1))
        return null;
      if ((value is double d && Math.Abs(d % 1) > (double.Epsilon * 100))
        || (value is float f && Math.Abs(f % 1) > (float.Epsilon * 100)))
      {
        throw new InvalidCastException();
      }
      return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="object" /> to a <see cref="long" /> based on
    /// the locale and time zone
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <c>null</c> if <paramref name="value" /> is null or empty.
    /// A <see cref="long" /> if <paramref name="value" /> is convertible.
    /// Otherwise, an exception is thrown
    /// </returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="long"/></exception>
    public long? AsLong(object value)
    {
      if (value == null || (value is string str && str.Length < 1))
        return null;
      if ((value is double d && Math.Abs(d % 1) > (double.Epsilon * 100))
        || (value is float f && Math.Abs(f % 1) > (float.Epsilon * 100)))
      {
        throw new InvalidCastException();
      }
      return Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Serializes the value to a string.  Dates are converted to the
    /// corporate time zone
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>
    /// A string representing the value
    /// </returns>
    public string Format(object value)
    {
      return Format(value
        , n => n.ToString(DoubleFixedPoint, CultureInfo.InvariantCulture)
        , s => s.ToString());
    }

    private string Format(object value, Func<IFormattable, string> numberRenderer, Func<object, string> stringRenderer)
    {
      IFormattable number;
      if (value == null
#if DBDATA
        || value == DBNull.Value
#endif
      )
      {
        return null;
      }
      else if (value is bool b)
      {
        return b ? "1" : "0";
      }
      else if (value is Condition condition)
      {
        switch (condition)
        {
          case Condition.Between:
            return "between";
          case Condition.Equal:
            return "eq";
          case Condition.GreaterThan:
            return "gt";
          case Condition.GreaterThanEqual:
            return "ge";
          case Condition.In:
            return "in";
          case Condition.Is:
            return "is";
          case Condition.IsNotNull:
            return "is not null";
          case Condition.IsNull:
            return "is null";
          case Condition.LessThan:
            return "lt";
          case Condition.LessThanEqual:
            return "le";
          case Condition.Like:
            return "like";
          case Condition.NotBetween:
            return "not between";
          case Condition.NotEqual:
            return "ne";
          case Condition.NotIn:
            return "not in";
          case Condition.NotLike:
            return "not like";
          default:
            throw new NotSupportedException();
        }
      }
      else if (TryCastNumber(value, out number))
      {
        return numberRenderer(number);
      }
      else if (value is Guid guid)
      {
        return guid.ToString("N").ToUpperInvariant();
      }
      else if (value is DateTime dateTime)
      {
        return Render(dateTime);
      }
      else if (value is QueryModel.DateTimeLiteral dateTimeLit)
      {
        return Render(dateTimeLit.Value);
      }
      else if (value is ZonedDateTime zoned)
      {
        return Render(zoned.UtcDateTime);
      }
      else if (value is Range<DateOffset> dateRange)
      {
        var statDates = (dateRange).AsDateRange(this.Now());
        return stringRenderer(statDates.Minimum.ToString("s") + " and " + statDates.Maximum.ToString("s"));
      }
      else if (value is IRange range)
      {
        return Format(range.Minimum, numberRenderer, stringRenderer)
          + " and "
          + Format(range.Maximum, numberRenderer, stringRenderer);
      }
      else if (value is DateOffset dateOffset)
      {
        return Render(dateOffset.AsDate(this.Now()));
      }
      else if (value is QueryModel.DateOffsetLiteral offsetLit)
      {
        return Render(offsetLit.Offset.AsDate(this.Now()));
      }
      else if (value is QueryModel.PatternList pattern)
      {
        return QueryModel.AmlLikeParser.Instance.Render(pattern);
      }
      else if (value is IReadOnlyItem item)
      {
        return item.Id();
      }
      else if (value is IReadOnlyProperty_Base prop)
      {
        return prop.Value;
      }
      else if (value is IReadOnlyAttribute attr)
      {
        return attr.Value;
      }
      else if (value is SelectNode node)
      {
        return node.ToString();
      }
      else
      {
        return RenderSqlEnum(value, true, numberRenderer, stringRenderer, null);
      }
    }

    internal static string RenderSqlEnum(object value, bool quoteStrings, Func<object, string> format)
    {
      return RenderSqlEnum(value, quoteStrings, f => format(f), format, () => "`EMTPY_LIST_MUST_MATCH_0_ITEMS!`");
    }

    internal static string RenderSqlEnum(object value, bool quoteStrings, Func<IFormattable, string> numberRenderer, Func<object, string> stringRenderer, Func<string> emptyEnumRenderer)
    {
      if (value is string)
        return stringRenderer(value);

      var first = true;
      var builder = new StringBuilder();

      if (TryGetNumericEnumerable(value, out var enumerable))
      {
        foreach (var item in enumerable)
        {
          if (!first) builder.Append(",");
          builder.Append(numberRenderer((IFormattable)item));
          first = false;
        }

        if (first)
          return emptyEnumRenderer?.Invoke();
      }
      else
      {
        enumerable = value as IEnumerable;
        if (enumerable != null)
        {
          foreach (var item in enumerable)
          {
            if (!first) builder.Append(",");
            if (quoteStrings)
              builder.Append(SqlFormatter.Quote(stringRenderer(item).Replace("'", "''")));
            else
              builder.Append(stringRenderer(item));
            first = false;
          }

          // Nothing was written as there were not values in the IEnumerable
          // Therefore, write a bogus value to match zero results
          if (first)
          {
            var empty = emptyEnumRenderer?.Invoke();
            if (empty == null)
              return null;
            return quoteStrings
              ? "N'" + emptyEnumRenderer?.Invoke() + "'"
              : emptyEnumRenderer?.Invoke();
          }
        }
        else
        {
          return stringRenderer(value);
        }
      }

      return builder.ToString();
    }

    private static bool TryGetNumericEnumerable(object value, out IEnumerable enumerable)
    {
      if (value is IEnumerable<short>
        || value is IEnumerable<int>
        || value is IEnumerable<long>
        || value is IEnumerable<ushort>
        || value is IEnumerable<uint>
        || value is IEnumerable<ulong>
        || value is IEnumerable<byte>
        || value is IEnumerable<decimal>)
      {
        enumerable = (IEnumerable)value;
        return true;
      }
      else if (value is IEnumerable<float>)
      {
        enumerable = ((IEnumerable<float>)value).Cast<decimal>();
        return true;
      }
      else if (value is IEnumerable<double>)
      {
        enumerable = ((IEnumerable<double>)value).Cast<decimal>();
        return true;
      }
      enumerable = null;
      return false;
    }

    private string Render(DateTime value)
    {
      return ConvertDateTime(value, _timeZoneCorporate).ToString("s");
    }

    public DateTime ConvertToUtcDateTime(DateTime value)
    {
      return ConvertDateTime(value, DateTimeZone.Utc);
    }

    private DateTime ConvertDateTime(DateTime value, DateTimeZone to)
    {
      var converted = value;
      if (value.Kind == DateTimeKind.Utc)
      {
        if (_timeZoneCorporate != DateTimeZone.Utc)
        {
          converted = DateTimeZone.ConvertTime(value, DateTimeZone.Utc, to);
        }
      }
      else if (value.Kind == DateTimeKind.Local)
      {
        converted = DateTimeZone.ConvertTime(value, DateTimeZone.Local, to);
      }
      else
      {
        converted = DateTimeZone.ConvertTime(value, _timeZoneUser, to);
      }
      return converted;
    }

    /// <summary>
    /// Returns an object that provides formatting services for the specified type.
    /// </summary>
    /// <param name="formatType">An object that specifies the type of format object to return.</param>
    /// <returns>
    /// An instance of the object specified by <paramref name="formatType" />, if the <see cref="T:System.IFormatProvider" /> implementation can supply that type of object; otherwise, null.
    /// </returns>
    public object GetFormat(Type formatType)
    {
      if (formatType == typeof(ICustomFormatter))
        return this;
      else
        return null;
    }

    /// <summary>
    /// Converts the value of a specified object to an equivalent string representation using specified format and culture-specific formatting information.
    /// </summary>
    /// <param name="format">A format string containing formatting specifications.</param>
    /// <param name="arg">An object to format.</param>
    /// <param name="formatProvider">An object that supplies format information about the current instance.</param>
    /// <returns>
    /// The string representation of the value of <paramref name="arg" />, formatted as specified by <paramref name="format" /> and <paramref name="formatProvider" />.
    /// </returns>
    public string Format(string format, object arg, IFormatProvider formatProvider)
    {
      // Check whether this is an appropriate callback
      if (!this.Equals(formatProvider)) return null;
      if (arg == null) return null;

      var parts = (format ?? "").Split(':');
      var sql = new SqlFormatter(this);
      string formatString;

      switch (parts[0])
      {
        case "aml":
          formatString = string.Join(":", parts, 1, parts.Length - 1);
          return Format(arg,
            n => n.ToString(format, CultureInfo.InvariantCulture),
            o => GetString(o, formatString));
        case "sql":
          formatString = string.Join(":", parts, 1, parts.Length - 1);
          return XmlEscape(sql.Format(formatString, arg, sql));
        case "rawsql":
          formatString = string.Join(":", parts, 1, parts.Length - 1);
          return sql.Format(formatString, arg, sql);
        default:
          formatString = format;
          return Format(arg,
            n => n.ToString(format, CultureInfo.InvariantCulture),
            o => XmlEscape(GetString(o, formatString)));
      }
    }

    internal static string GetString(object obj, string format)
    {
      var formattable = obj as IFormattable;
      if (!string.IsNullOrEmpty(format) && formattable != null)
        return formattable.ToString(format, CultureInfo.InvariantCulture);

#if XMLLEGACY
      var node = obj as System.Xml.XmlNode;
      if (node != null) return node.OuterXml;
#endif

      return obj.ToString();
    }

    internal static string XmlEscape(string value)
    {
      return new StringBuilder(value.Length + 10).AppendEscapedXml(value).ToString();
    }

    internal static bool TryCastNumber(object value, out IFormattable number)
    {
      if (value is short || value is int || value is long
        || value is ushort || value is uint || value is ulong
        || value is byte)
      {
        number = (IFormattable)value;
        return true;
      }
      else if (value is float || value is double || value is decimal)
      {
        number = (IFormattable)value;
        return true;
      }
      number = null;
      return false;
    }
  }
}
