using System;
using System.Diagnostics;
using System.Globalization;
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
    private DateTimeZone _timeZone;

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
    public string TimeZone
    {
      get { return _timeZone.Id; }
      set { _timeZone = DateTimeZone.ById(value); }
    }

    internal DateTimeZone Zone
    {
      get { return _timeZone; }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
      get
      {
        return "Context: " + Locale + " / " + TimeZone;
      }
    }

    /// <summary>
    /// Create a server context in either the local or UTC time zone
    /// </summary>
    /// <param name="utc">Indicates the UTC time zone should be used. 
    /// Otherwise, the local time zone will be used</param>
    public ServerContext(bool utc)
    {
      _timeZone = utc ? DateTimeZone.Utc : DateTimeZone.Local;
      this.DefaultLanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
      this.LanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
      this.Locale = CultureInfo.CurrentCulture.Name;
    }

    /// <summary>
    /// Create a server context using the specified time zone name
    /// </summary>
    public ServerContext(string timeZoneName)
    {
      this.TimeZone = timeZoneName;
    }

#if SERIALIZATION
    private ServerContext(SerializationInfo info, StreamingContext context)
    {
      this.DefaultLanguageCode = info.GetString("default_language_code");
      this.DefaultLanguageSuffix = info.GetString("default_language_suffix");
      this.LanguageCode = info.GetString("language_code");
      this.LanguageSuffix = info.GetString("language_suffix");
      this.Locale = info.GetString("locale");
      this.TimeZone = info.GetString("time_zone");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("default_language_code", this.DefaultLanguageCode);
      info.AddValue("default_language_suffix", this.DefaultLanguageSuffix);
      info.AddValue("language_code", this.LanguageCode);
      info.AddValue("language_suffix", this.LanguageSuffix);
      info.AddValue("locale", this.Locale);
      info.AddValue("time_zone", this.TimeZone);
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
        if (b)
        {
          return "1";
        }
        else
        {
          return "0";
        }
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
        return dateOffset.AsDate(this.Now()).ToString("s");
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
      else
      {
        return stringRenderer(value.ToString());
      }
    }

    private string Render(DateTime value)
    {
      var converted = value;
      if (value.Kind == DateTimeKind.Utc)
      {
        if (_timeZone != DateTimeZone.Utc)
        {
          converted = DateTimeZone.ConvertTime(value, DateTimeZone.Utc, _timeZone);
        }
      }
      else if (_timeZone != DateTimeZone.Local) // Assume local
      {
        converted = DateTimeZone.ConvertTime(value, DateTimeZone.Local, _timeZone);
      }
      return converted.ToString("s");
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
