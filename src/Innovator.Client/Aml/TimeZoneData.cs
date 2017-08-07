using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
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

    public string Id
    {
      get { return _timeZone.Id; }
    }

    public TimeSpan GetUtcOffset(DateTime dateTime)
    {
      return _timeZone.GetUtcOffset(dateTime);
    }

    public override int GetHashCode()
    {
      return _timeZone.GetHashCode();
    }
    public bool Equals(TimeZoneData other)
    {
      return _timeZone.Equals(other._timeZone);
    }

    public static DateTime ConvertTime(DateTime value, TimeZoneData from, TimeZoneData to)
    {
      return TimeZoneInfo.ConvertTime(value, from._timeZone, to._timeZone);
    }

    public static TimeZoneData ById(string value)
    {
      return new TimeZoneData() { _timeZone = TimeZoneInfo.FindSystemTimeZoneById(value) };
    }

    public static DateTimeOffset ConvertTime(DateTimeOffset value, TimeZoneData to)
    {
      return TimeZoneInfo.ConvertTime(value, to._timeZone);
    }

    private static readonly TimeZoneData _local = new TimeZoneData() { _timeZone = TimeZoneInfo.Local };
    private static readonly TimeZoneData _utc = new TimeZoneData() { _timeZone = TimeZoneInfo.Utc };
#else
    private Innovator.Client.Time.TimeZoneInfo _timeZone;

    public string Id
    {
      get { return _timeZone.Id; }
    }

    public TimeSpan GetUtcOffset(DateTime dateTime)
    {
      return _timeZone.GetUtcOffset(dateTime);
    }

    public override int GetHashCode()
    {
      return _timeZone.GetHashCode();
    }
    public bool Equals(TimeZoneData other)
    {
      return _timeZone.Equals(other._timeZone);
    }

    public static DateTime ConvertTime(DateTime value, TimeZoneData from, TimeZoneData to)
    {
      return Time.TimeZoneInfo.ConvertTime(value, from._timeZone, to._timeZone);
    }

    public static TimeZoneData ById(string value)
    {
      return new TimeZoneData() { _timeZone = Time.TimeZoneInfo.ById(value) };
    }

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
