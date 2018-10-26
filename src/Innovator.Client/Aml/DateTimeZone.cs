using System;

namespace Innovator.Client
{
  /// <summary>
  /// Represents metadata for a timezone including daylight savings time history
  /// </summary>
  public partial class DateTimeZone : IEquatable<DateTimeZone>
  {
    /// <summary>
    /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
      var tzd = obj as DateTimeZone;
      if (tzd == null)
        return false;
      return Equals(tzd);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">The b.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(DateTimeZone a, DateTimeZone b)
    {
      // If both are null, or both are same instance, return true.
      if (System.Object.ReferenceEquals(a, b))
        return true;

      // If one is null, but not both, return false.
      if (((object)a == null) || ((object)b == null))
        return false;

      // Return true if the fields match:
      return a.Equals(b);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">The b.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(DateTimeZone a, DateTimeZone b)
    {
      return !(a == b);
    }

#if TIMEZONEINFO
    private TimeZoneInfo _timeZone;

    /// <summary>
    /// Gets the time zone identifier.
    /// </summary>
    /// <value>
    /// The time zone identifier.
    /// </value>
    public string Id
    {
      get { return _timeZone.Id; }
    }

    /// <summary>
    /// Calculates the offset or difference between the time in this time zone and Coordinated
    /// Universal Time (UTC) for a particular date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to determine the offset for.</param>
    /// <returns>An object that indicates the time difference between the two time zones. </returns>
    public TimeSpan GetUtcOffset(DateTime dateTime)
    {
      return _timeZone.GetUtcOffset(dateTime);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      return _timeZone.GetHashCode();
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(DateTimeZone other)
    {
      return _timeZone.Equals(other._timeZone);
    }

    /// <summary>
    /// Converts a time from one time zone to another.
    /// </summary>
    /// <param name="value">The date and time to convert.</param>
    /// <param name="from">The time zone of <paramref name="value"/>.</param>
    /// <param name="to">The time zone to convert <paramref name="value"/> to.</param>
    /// <returns>The date and time in the destination time zone that corresponds to the <paramref name="value"/>
    /// parameter in the source time zone.</returns>
    /// <exception cref="ArgumentException">The <see cref="System.DateTime.Kind"/> property of the 
    /// <paramref name="value"/> parameter is <see cref="System.DateTimeKind.Local"/>, but the 
    /// <paramref name="from"/> parameter does not equal <see cref="System.DateTimeKind.Local"/>.
    /// -or-
    /// The <see cref="System.DateTime.Kind"/> property of the <paramref name="value"/> parameter is 
    /// <see cref="System.DateTimeKind.Utc"/>, but the <paramref name="from"/> parameter does not equal 
    /// <see cref="DateTimeZone.Utc"/>
    /// -or-
    /// The
    /// <paramref name="value"/> parameter is an invalid time (that is, it represents a time that does
    /// not exist because of a time zone's adjustment rules).</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="from"/>parameter is null.
    /// -or-
    /// The <paramref name="to"/> parameter is null.
    /// </exception>
    public static DateTime ConvertTime(DateTime value, DateTimeZone from, DateTimeZone to)
    {
      return TimeZoneInfo.ConvertTime(value, from._timeZone, to._timeZone);
    }

    /// <summary>
    /// Retrieves a <see cref="DateTimeZone"/> object from the registry based on its identifier.
    /// </summary>
    /// <param name="value">The time zone identifier, which corresponds to the <see cref="DateTimeZone.Id"/> property.</param>
    /// <returns>An object whose identifier is the value of the <paramref name="value"/> parameter.</returns>
    /// <exception cref="OutOfMemoryException">The system does not have enough memory to hold information about the time zone.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> parameter is null</exception>
    /// <exception cref="TimeZoneNotFoundException">The time zone identifier specified by id was not found. This means that a registry
    /// key whose name matches id does not exist, or that the key exists but does not
    /// contain any time zone data.</exception>
    /// <exception cref="Security.SecurityException">The process does not have the permissions required to read from the registry
    /// key that contains the time zone information.
    /// </exception>
    /// <exception cref="System.InvalidTimeZoneException">
    /// The time zone identifier was found, but the registry data is corrupted.    
    /// </exception>
    public static DateTimeZone ById(string value)
    {
      var zone = default(TimeZoneInfo);
      try
      {
        zone = TimeZoneInfo.FindSystemTimeZoneById(value);
      }
      catch (Exception)
      {
        zone = TimeZoneInfo.FindSystemTimeZoneById(WindowsToIanaName(value));
      }
      
      return new DateTimeZone() { _timeZone = zone };
    }

    /// <summary>
    /// Converts a time to the time in a particular time zone.
    /// </summary>
    /// <param name="value">The date and time to convert.</param>
    /// <param name="to">The time zone to convert dateTime to.</param>
    /// <returns>The date and time in the destination time zone.</returns>
    /// <exception cref="ArgumentNullException">
    /// The value of the <paramref name="to"/> parameter is null.
    /// </exception>
    public static DateTimeOffset ConvertTime(DateTimeOffset value, DateTimeZone to)
    {
      return TimeZoneInfo.ConvertTime(value, to._timeZone);
    }

    private static readonly DateTimeZone _local = new DateTimeZone() { _timeZone = TimeZoneInfo.Local };
    private static readonly DateTimeZone _utc = new DateTimeZone() { _timeZone = TimeZoneInfo.Utc };
#else
    private Innovator.Client.Time.TimeZoneInfo _timeZone;

    /// <summary>
    /// Gets the time zone identifier.
    /// </summary>
    /// <value>
    /// The time zone identifier.
    /// </value>
    public string Id
    {
      get { return _timeZone.Id; }
    }

    /// <summary>
    /// Calculates the offset or difference between the time in this time zone and Coordinated
    /// Universal Time (UTC) for a particular date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to determine the offset for.</param>
    /// <returns>An object that indicates the time difference between the two time zones. </returns>
    public TimeSpan GetUtcOffset(DateTime dateTime)
    {
      return _timeZone.GetUtcOffset(dateTime);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      return _timeZone.GetHashCode();
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(DateTimeZone other)
    {
      return _timeZone.Equals(other._timeZone);
    }

    /// <summary>
    /// Converts a time from one time zone to another.
    /// </summary>
    /// <param name="value">The date and time to convert.</param>
    /// <param name="from">The time zone of <paramref name="value"/>.</param>
    /// <param name="to">The time zone to convert <paramref name="value"/> to.</param>
    /// <returns>The date and time in the destination time zone that corresponds to the <paramref name="value"/>
    /// parameter in the source time zone.</returns>
    /// <exception cref="ArgumentException">The <see cref="System.DateTime.Kind"/> property of the 
    /// <paramref name="value"/> parameter is <see cref="System.DateTimeKind.Local"/>, but the 
    /// <paramref name="from"/> parameter does not equal <see cref="System.DateTimeKind.Local"/>.
    /// -or-
    /// The <see cref="System.DateTime.Kind"/> property of the <paramref name="value"/> parameter is 
    /// <see cref="System.DateTimeKind.Utc"/>, but the <paramref name="from"/> parameter does not equal 
    /// <see cref="DateTimeZone.Utc"/>
    /// -or-
    /// The
    /// <paramref name="value"/> parameter is an invalid time (that is, it represents a time that does
    /// not exist because of a time zone's adjustment rules).</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="from"/>parameter is null.
    /// -or-
    /// The <paramref name="to"/> parameter is null.
    /// </exception>
    public static DateTime ConvertTime(DateTime value, DateTimeZone from, DateTimeZone to)
    {
      return Time.TimeZoneInfo.ConvertTime(value, from._timeZone, to._timeZone);
    }

    /// <summary>
    /// Retrieves a <see cref="DateTimeZone"/> object from the registry based on its identifier.
    /// </summary>
    /// <param name="value">The time zone identifier, which corresponds to the <see cref="DateTimeZone.Id"/> property.</param>
    /// <returns>An object whose identifier is the value of the <paramref name="value"/> parameter.</returns>
    /// <exception cref="OutOfMemoryException">The system does not have enough memory to hold information about the time zone.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> parameter is null</exception>
    /// <exception cref="TimeZoneNotFoundException">The time zone identifier specified by id was not found. This means that a registry
    /// key whose name matches id does not exist, or that the key exists but does not
    /// contain any time zone data.</exception>
    /// <exception cref="Security.SecurityException">The process does not have the permissions required to read from the registry
    /// key that contains the time zone information.
    /// </exception>
    public static DateTimeZone ById(string value)
    {
      return new DateTimeZone() { _timeZone = Time.TimeZoneInfo.ById(value) };
    }

    /// <summary>
    /// Converts a time to the time in a particular time zone.
    /// </summary>
    /// <param name="value">The date and time to convert.</param>
    /// <param name="to">The time zone to convert dateTime to.</param>
    /// <returns>The date and time in the destination time zone.</returns>
    /// <exception cref="ArgumentNullException">
    /// The value of the <paramref name="to"/> parameter is null.
    /// </exception>
    public static DateTimeOffset ConvertTime(DateTimeOffset value, DateTimeZone to)
    {
      return Time.TimeZoneInfo.ConvertTime(value, to._timeZone);
    }

    private static readonly DateTimeZone _local = new DateTimeZone()
    {
      _timeZone = Time.TimeZoneInfo.ByStandardName(TimeZoneInfo.Local.StandardName)
    };
    private static readonly DateTimeZone _utc = new DateTimeZone()
    {
      _timeZone = Time.TimeZoneInfo.ById("UTC")
    };
#endif

    /// <summary>
    /// Gets time zone information for the local time zone.
    /// </summary>
    /// <value>
    /// The time zone information for the local time zone.
    /// </value>
    public static DateTimeZone Local { get { return _local; } }

    /// <summary>
    /// Gets time zone information for the UTC time zone.
    /// </summary>
    /// <value>
    /// The time zone information for the UTC time zone.
    /// </value>
    public static DateTimeZone Utc { get { return _utc; } }

    private static string WindowsToIanaName(string windowsTimeZoneName)
    {
      switch (windowsTimeZoneName?.ToLowerInvariant())
      {
        case "aus central standard time": return "Australia/Darwin";
        case "aus eastern standard time": return "Australia/Sydney";
        case "afghanistan standard time": return "Asia/Kabul";
        case "alaskan standard time": return "America/Anchorage";
        case "aleutian standard time": return "America/Adak";
        case "altai standard time": return "Asia/Barnaul";
        case "arab standard time": return "Asia/Riyadh";
        case "arabian standard time": return "Asia/Dubai";
        case "arabic standard time": return "Asia/Baghdad";
        case "argentina standard time": return "America/Argentina/Buenos_Aires";
        case "astrakhan standard time": return "Europe/Astrakhan";
        case "atlantic standard time": return "America/Halifax";
        case "aus central w. standard time": return "Australia/Eucla";
        case "azerbaijan standard time": return "Asia/Baku";
        case "azores standard time": return "Atlantic/Azores";
        case "bahia standard time": return "America/Bahia";
        case "bangladesh standard time": return "Asia/Dhaka";
        case "belarus standard time": return "Europe/Minsk";
        case "bougainville standard time": return "Pacific/Bougainville";
        case "canada central standard time": return "America/Regina";
        case "cape verde standard time": return "Atlantic/Cape_Verde";
        case "caucasus standard time": return "Asia/Yerevan";
        case "cen. australia standard time": return "Australia/Adelaide";
        case "central america standard time": return "America/Guatemala";
        case "central asia standard time": return "Asia/Almaty";
        case "central brazilian standard time": return "America/Cuiaba";
        case "central europe standard time": return "Europe/Budapest";
        case "central european standard time": return "Europe/Warsaw";
        case "central pacific standard time": return "Pacific/Guadalcanal";
        case "central standard time (mexico)": return "America/Mexico_City";
        case "central standard time": return "America/Chicago";
        case "chatham islands standard time": return "Pacific/Chatham";
        case "china standard time": return "Asia/Shanghai";
        case "cuba standard time": return "America/Havana";
        case "dateline standard time": return "Etc/GMT+12";
        case "e. africa standard time": return "Africa/Nairobi";
        case "e. australia standard time": return "Australia/Brisbane";
        case "e. europe standard time": return "Europe/Chisinau";
        case "e. south america standard time": return "America/Sao_Paulo";
        case "easter island standard time": return "Pacific/Easter";
        case "eastern standard time (mexico)": return "America/Cancun";
        case "eastern standard time": return "America/New_York";
        case "egypt standard time": return "Africa/Cairo";
        case "ekaterinburg standard time": return "Asia/Yekaterinburg";
        case "fle standard time": return "Europe/Kiev";
        case "fiji standard time": return "Pacific/Fiji";
        case "gmt standard time": return "Europe/London";
        case "gtb standard time": return "Europe/Bucharest";
        case "georgian standard time": return "Asia/Tbilisi";
        case "greenland standard time": return "America/Godthab";
        case "greenwich standard time": return "Atlantic/Reykjavik";
        case "haiti standard time": return "America/Port-au-Prince";
        case "hawaiian standard time": return "Pacific/Honolulu";
        case "india standard time": return "Asia/Kolkata";
        case "iran standard time": return "Asia/Tehran";
        case "israel standard time": return "Asia/Jerusalem";
        case "jordan standard time": return "Asia/Amman";
        case "kaliningrad standard time": return "Europe/Kaliningrad";
        case "kamchatka standard time": return "Asia/Kamchatka";
        case "korea standard time": return "Asia/Seoul";
        case "libya standard time": return "Africa/Tripoli";
        case "line islands standard time": return "Pacific/Kiritimati";
        case "lord howe standard time": return "Australia/Lord_Howe";
        case "magadan standard time": return "Asia/Magadan";
        case "magallanes standard time": return "America/Punta_Arenas";
        case "marquesas standard time": return "Pacific/Marquesas";
        case "mauritius standard time": return "Indian/Mauritius";
        case "mid-atlantic standard time": return "Etc/GMT+2";
        case "middle east standard time": return "Asia/Beirut";
        case "montevideo standard time": return "America/Montevideo";
        case "morocco standard time": return "Africa/Casablanca";
        case "mountain standard time (mexico)": return "America/Chihuahua";
        case "mountain standard time": return "America/Denver";
        case "myanmar standard time": return "Asia/Yangon";
        case "n. central asia standard time": return "Asia/Novosibirsk";
        case "namibia standard time": return "Africa/Windhoek";
        case "nepal standard time": return "Asia/Kathmandu";
        case "new zealand standard time": return "Pacific/Auckland";
        case "newfoundland standard time": return "America/St_Johns";
        case "norfolk standard time": return "Pacific/Norfolk";
        case "north asia east standard time": return "Asia/Irkutsk";
        case "north asia standard time": return "Asia/Krasnoyarsk";
        case "north korea standard time": return "Asia/Pyongyang";
        case "omsk standard time": return "Asia/Omsk";
        case "pacific sa standard time": return "America/Santiago";
        case "pacific standard time (mexico)": return "America/Tijuana";
        case "pacific standard time": return "America/Los_Angeles";
        case "pakistan standard time": return "Asia/Karachi";
        case "paraguay standard time": return "America/Asuncion";
        case "romance standard time": return "Europe/Paris";
        case "russia time zone 10": return "Asia/Srednekolymsk";
        case "russia time zone 11": return "Asia/Kamchatka";
        case "russia time zone 3": return "Europe/Samara";
        case "russian standard time": return "Europe/Moscow";
        case "sa eastern standard time": return "America/Cayenne";
        case "sa pacific standard time": return "America/Bogota";
        case "sa western standard time": return "America/La_Paz";
        case "se asia standard time": return "Asia/Bangkok";
        case "saint pierre standard time": return "America/Miquelon";
        case "sakhalin standard time": return "Asia/Sakhalin";
        case "samoa standard time": return "Pacific/Apia";
        case "sao tome standard time": return "Africa/Sao_Tome";
        case "saratov standard time": return "Europe/Saratov";
        case "singapore standard time": return "Asia/Singapore";
        case "south africa standard time": return "Africa/Johannesburg";
        case "sri lanka standard time": return "Asia/Colombo";
        case "sudan standard time": return "Africa/Khartoum";
        case "syria standard time": return "Asia/Damascus";
        case "taipei standard time": return "Asia/Taipei";
        case "tasmania standard time": return "Australia/Hobart";
        case "tocantins standard time": return "America/Araguaina";
        case "tokyo standard time": return "Asia/Tokyo";
        case "tomsk standard time": return "Asia/Tomsk";
        case "tonga standard time": return "Pacific/Tongatapu";
        case "transbaikal standard time": return "Asia/Chita";
        case "turkey standard time": return "Europe/Istanbul";
        case "turks and caicos standard time": return "America/Grand_Turk";
        case "us eastern standard time": return "America/Indiana/Indianapolis";
        case "us mountain standard time": return "America/Phoenix";
        case "utc+12": return "Etc/GMT-12";
        case "utc+13": return "Etc/GMT-13";
        case "utc": return "Etc/UTC";
        case "utc-02": return "Etc/GMT+2";
        case "utc-08": return "Etc/GMT+8";
        case "utc-09": return "Etc/GMT+9";
        case "utc-11": return "Etc/GMT+11";
        case "ulaanbaatar standard time": return "Asia/Ulaanbaatar";
        case "venezuela standard time": return "America/Caracas";
        case "vladivostok standard time": return "Asia/Vladivostok";
        case "w. australia standard time": return "Australia/Perth";
        case "w. central africa standard time": return "Africa/Lagos";
        case "w. europe standard time": return "Europe/Berlin";
        case "w. mongolia standard time": return "Asia/Hovd";
        case "west asia standard time": return "Asia/Tashkent";
        case "west bank standard time": return "Asia/Hebron";
        case "west pacific standard time": return "Pacific/Port_Moresby";
        case "yakutsk standard time": return "Asia/Yakutsk";
      }

      return windowsTimeZoneName;
    }
  }

#if DEBUG && TIMEZONEINFO
  /// <summary>
  /// Used to generate the timezone data
  /// Innovator.Client.TzUtils.GenerateRecords(@"C:\Users\eric.domke\Documents\Code\Innovator.Client\src\Innovator.Client\Aml\TimeZoneInfo.Records.cs")
  /// </summary>
  public class TzUtils
  {
    public static void GenerateRecords(string path)
    {
      using (var writer = new System.IO.StreamWriter(path))
      {
        writer.Write(@"#if !TIMEZONEINFO
using System;
using System.Collections.Generic;

namespace Innovator.Client.Time
{
  internal sealed partial class TimeZoneInfo
  {
    private static Dictionary<string, TimeZoneInfo> _cache = new Dictionary<string, TimeZoneInfo>(StringComparer.OrdinalIgnoreCase)
    {
");
        foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
        {
          //TimeZoneInfo.CreateCustomTimeZone(tz.Id, tz.BaseUtcOffset, tz.DisplayName, tz.StandardName, tz.DaylightName, tz.GetAdjustmentRules(), !tz.SupportsDaylightSavingTime);
          writer.Write("      { \"");
          writer.Write(tz.Id);
          writer.Write("\", TimeZoneInfo.CreateCustomTimeZone(\"");
          writer.Write(tz.Id);
          writer.Write("\", TimeSpan.FromSeconds(");
          writer.Write(tz.BaseUtcOffset.TotalSeconds.ToString());
          writer.Write("), \"");
          writer.Write(tz.DisplayName);
          writer.Write("\", \"");
          writer.Write(tz.StandardName);
          writer.Write("\", \"");
          writer.Write(tz.DaylightName);
          writer.WriteLine("\", new AdjustmentRule [] {");
          foreach (var rule in tz.GetAdjustmentRules())
          {
            var prop = rule.GetType().GetProperty("BaseUtcOffsetDelta", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var baseOffset = (TimeSpan)prop.GetValue(rule, null);

            writer.Write("    AdjustmentRule.CreateAdjustmentRule(new DateTime(");
            writer.Write("{0:yyyy, M, d, H, m, s}, DateTimeKind.Unspecified), new DateTime(", rule.DateStart);
            writer.Write("{0:yyyy, M, d, H, m, s}, DateTimeKind.Unspecified), ", rule.DateEnd);
            writer.Write("TimeSpan.FromSeconds(");
            writer.Write(rule.DaylightDelta.TotalSeconds);
            writer.Write("), ");
            AppendTransition(rule.DaylightTransitionStart, writer);
            writer.Write(", ");
            AppendTransition(rule.DaylightTransitionEnd, writer);
            if (baseOffset != TimeSpan.Zero)
            {
              writer.Write(", TimeSpan.FromSeconds(");
              writer.Write(baseOffset.TotalSeconds);
              writer.Write(")");
            }
            writer.WriteLine("),");
          }
          writer.Write("      }, ");
          writer.Write(tz.SupportsDaylightSavingTime ? "false" : "true");
          writer.WriteLine(") },");
        }

        writer.WriteLine(@"    };

    private static Dictionary<string, string> _standardNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
");

        foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
        {
          writer.Write("      {\"");
          writer.Write(tz.StandardName);
          writer.Write("\", \"");
          writer.Write(tz.Id);
          writer.WriteLine("\" },");
        }
        writer.WriteLine(@"    };");
        writer.WriteLine("  }");
        writer.WriteLine("}");
        writer.WriteLine("#endif");
      }
    }

    private static void AppendTransition(TimeZoneInfo.TransitionTime trans, System.IO.TextWriter writer)
    {
      if (trans.IsFixedDateRule)
      {
        writer.Write("TransitionTime.CreateFixedDateRule(new DateTime(");
        writer.Write("{0:yyyy, M, d, H, m, s}, DateTimeKind.Unspecified), ", trans.TimeOfDay);
        writer.Write(trans.Month);
        writer.Write(", ");
        writer.Write(trans.Day);
        writer.Write(")");
      }
      else
      {
        writer.Write("TransitionTime.CreateFloatingDateRule(new DateTime(");
        writer.Write("{0:yyyy, M, d, H, m, s}, DateTimeKind.Unspecified), ", trans.TimeOfDay);
        writer.Write(trans.Month);
        writer.Write(", ");
        writer.Write(trans.Week);
        writer.Write(", DayOfWeek.");
        writer.Write(trans.DayOfWeek);
        writer.Write(")");
      }
    }
  }
#endif
}
