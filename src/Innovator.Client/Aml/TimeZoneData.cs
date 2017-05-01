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
    private TzRecord _timeZone;
    private string _id;

    public string Id
    {
      get { return _id; }
    }

    public static TimeZoneData ById(string value)
    {
      return new TimeZoneData()
      {
        _timeZone = _data[value],
        _id = value
      };
    }

    public TimeSpan GetUtcOffset(DateTime dateTime)
    {
      return dateTime - ConvertFromLocal(dateTime, this);
    }

    public override int GetHashCode()
    {
      return _timeZone.GetHashCode() ^ _id.GetHashCode();
    }
    public bool Equals(TimeZoneData other)
    {
      return _timeZone.Equals(other._timeZone) && _id.Equals(other._id);
    }

    public static DateTime ConvertTime(DateTime value, TimeZoneData from, TimeZoneData to)
    {
      return ConvertToLocal(ConvertFromLocal(value, from), to);
    }

    public static DateTimeOffset ConvertTime(DateTimeOffset value, TimeZoneData to)
    {
      var universal = value.ToUniversalTime().DateTime;
      var result = ConvertToLocal(universal, to);
      return new DateTimeOffset(result, result - universal);
    }

    private static DateTime ConvertToLocal(DateTime value, TimeZoneData data)
    {
      var record = data._timeZone;
      var hist = record.History.Single(h => value >= h.TransitionStart && value < h.TransitionEnd);
      var result = value.AddSeconds(record.Offset + hist.DeltaSeconds);

      if (TimeZoneInfo.Local.StandardName == data._id)
        return DateTime.SpecifyKind(result, DateTimeKind.Local);
      return DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
    }

    private static DateTime ConvertFromLocal(DateTime value, TimeZoneData data)
    {
      var record = data._timeZone;

      //Presented local date can be located near transition dates, we should check 2 boundary periods
      //to get their delta_seconds offsets and check if local date falls in unexisting time interval,
      //to raise an error or if date is ambiguous we assume that it belongs to latest period and use its offset.
      var valueSt = value.AddSeconds(-1 * record.Offset);
      var hist1 = record.History.Single(h => valueSt >= h.TransitionStart && valueSt < h.TransitionEnd);

      var valueSt2 = valueSt.AddSeconds(-1 * hist1.DeltaSeconds);
      var hist2 = record.History.Single(h => valueSt2 >= h.TransitionStart && valueSt2 < h.TransitionEnd);

      var transEnd1 = (DateTime.MaxValue - hist1.TransitionEnd).TotalSeconds > hist1.DeltaSeconds
        ? hist1.TransitionEnd.AddSeconds(hist1.DeltaSeconds) : DateTime.MaxValue;
      var transEnd2 = (DateTime.MaxValue - hist2.TransitionEnd).TotalSeconds > hist2.DeltaSeconds
        ? hist2.TransitionEnd.AddSeconds(hist2.DeltaSeconds) : DateTime.MaxValue;

      if ((valueSt >= transEnd1 && valueSt < hist2.TransitionStart.AddSeconds(hist2.DeltaSeconds))
        || (valueSt >= transEnd2 && valueSt < hist1.TransitionStart.AddSeconds(hist1.DeltaSeconds)))
        throw new InvalidOperationException("Specified local time " + value.ToString() + " is invalid for '" + data._id + "' timezone.");

      return DateTime.SpecifyKind(valueSt.AddSeconds(-1 * hist2.DeltaSeconds), DateTimeKind.Utc);
    }

    private struct TzRecord
    {
      public int Offset { get; set; }
      public TzHistory[] History { get; set; }
    }

    private struct TzHistory
    {
      public DateTime TransitionStart { get; set; }
      public DateTime TransitionEnd { get; set; }
      public int DeltaSeconds { get; set; }
    }

    static TimeZoneData()
    {
      _local = TimeZoneData.ById(TimeZoneInfo.Local.StandardName);
      _utc = TimeZoneData.ById("UTC");
    }

    private static readonly TimeZoneData _local;
    private static readonly TimeZoneData _utc;
#endif

    public static TimeZoneData Local { get { return _local; } }
    public static TimeZoneData Utc { get { return _utc; } }
  }
}
