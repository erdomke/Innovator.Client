using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  /// <summary>
  /// Represents metadata for a timezone including daylight savings time history
  /// </summary>
  public partial class TimeZoneData : IEquatable<TimeZoneData>
  {
    public override bool Equals(object obj)
    {
      var tzd = obj as TimeZoneData;
      if (tzd == null)
        return false;
      return Equals(tzd);
    }

    public static bool operator ==(TimeZoneData a, TimeZoneData b)
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

    public static bool operator !=(TimeZoneData a, TimeZoneData b)
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
    public bool Equals(TimeZoneData other)
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
    /// <see cref="TimeZoneData.Utc"/>
    /// -or-
    /// The
    /// <paramref name="value"/> parameter is an invalid time (that is, it represents a time that does
    /// not exist because of a time zone's adjustment rules).</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="from"/>parameter is null.
    /// -or-
    /// The <param name="to"> parameter is null.
    /// </exception>
    public static DateTime ConvertTime(DateTime value, TimeZoneData from, TimeZoneData to)
    {
      return TimeZoneInfo.ConvertTime(value, from._timeZone, to._timeZone);
    }

    /// <summary>
    /// Retrieves a <see cref="TimeZoneData"/> object from the registry based on its identifier.
    /// </summary>
    /// <param name="value">The time zone identifier, which corresponds to the <see cref="TimeZoneData.Id"/> property.</param>
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
    public static TimeZoneData ById(string value)
    {
      return new TimeZoneData() { _timeZone = TimeZoneInfo.FindSystemTimeZoneById(value) };
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
    public static DateTimeOffset ConvertTime(DateTimeOffset value, TimeZoneData to)
    {
      return TimeZoneInfo.ConvertTime(value, to._timeZone);
    }

    private static readonly TimeZoneData _local = new TimeZoneData() { _timeZone = TimeZoneInfo.Local };
    private static readonly TimeZoneData _utc = new TimeZoneData() { _timeZone = TimeZoneInfo.Utc };
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
    public bool Equals(TimeZoneData other)
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
    /// <see cref="TimeZoneData.Utc"/>
    /// -or-
    /// The
    /// <paramref name="value"/> parameter is an invalid time (that is, it represents a time that does
    /// not exist because of a time zone's adjustment rules).</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="from"/>parameter is null.
    /// -or-
    /// The <param name="to"> parameter is null.
    /// </exception>
    public static DateTime ConvertTime(DateTime value, TimeZoneData from, TimeZoneData to)
    {
      return Time.TimeZoneInfo.ConvertTime(value, from._timeZone, to._timeZone);
    }

    /// <summary>
    /// Retrieves a <see cref="TimeZoneData"/> object from the registry based on its identifier.
    /// </summary>
    /// <param name="value">The time zone identifier, which corresponds to the <see cref="TimeZoneData.Id"/> property.</param>
    /// <returns>An object whose identifier is the value of the <paramref name="value"/> parameter.</returns>
    /// <exception cref="OutOfMemoryException">The system does not have enough memory to hold information about the time zone.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> parameter is null</exception>
    /// <exception cref="TimeZoneNotFoundException">The time zone identifier specified by id was not found. This means that a registry
    /// key whose name matches id does not exist, or that the key exists but does not
    /// contain any time zone data.</exception>
    /// <exception cref="Security.SecurityException">The process does not have the permissions required to read from the registry
    /// key that contains the time zone information.
    /// </exception>
    public static TimeZoneData ById(string value)
    {
      return new TimeZoneData() { _timeZone = Time.TimeZoneInfo.ById(value) };
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
    public static DateTimeOffset ConvertTime(DateTimeOffset value, TimeZoneData to)
    {
      return Time.TimeZoneInfo.ConvertTime(value, to._timeZone);
    }

    private static readonly TimeZoneData _local = new TimeZoneData()
    {
      _timeZone = Time.TimeZoneInfo.ByStandardName(TimeZoneInfo.Local.StandardName)
    };
    private static readonly TimeZoneData _utc = new TimeZoneData()
    {
      _timeZone = Time.TimeZoneInfo.ById("UTC")
    };
#endif

    public static TimeZoneData Local { get { return _local; } }
    public static TimeZoneData Utc { get { return _utc; } }
  }

#if DEBUG && TIMEZONEINFO
  /// <summary>
  /// Used to generate the timezone data
  /// Innovator.Client.TzUtils.GenerateRecords(@"C:\Users\eric.domke\Documents\Code\Innovator.Client\src\Innovator.Client\Aml\TimeZoneData.Records.cs")
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
              writer.Write(rule.DaylightDelta.TotalSeconds);
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
