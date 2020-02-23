using System;
using System.Globalization;

namespace Innovator.Client
{
  public struct ZonedDateTime : IEquatable<ZonedDateTime>, IComparable<ZonedDateTime>, IComparable, IFormattable
  {
    public DateTime LocalDateTime { get { return DateTimeZone.ConvertTime(UtcDateTime, DateTimeZone.Utc, DateTimeZone.Local); } }

    public DateTime UtcDateTime { get; }

    public DateTimeZone Zone { get; }

    public DateTime ZoneDateTime { get { return Zone == DateTimeZone.Utc ? UtcDateTime : DateTimeZone.ConvertTime(UtcDateTime, DateTimeZone.Utc, Zone); } }

    public ZonedDateTime(DateTime value) : this(value, DateTimeZone.Local) { }

    public ZonedDateTime(DateTimeZone timeZone) : this(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), timeZone) { }

    public ZonedDateTime(DateTime value, DateTimeZone timeZone)
    {
      if (value.Kind == DateTimeKind.Utc)
      {
        UtcDateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
      }
      else
      {
        UtcDateTime = DateTime.SpecifyKind(DateTimeZone.ConvertTime(value, DateTimeZone.Local, DateTimeZone.Utc), DateTimeKind.Utc);
      }
      Zone = timeZone;
    }

    public ZonedDateTime Add(TimeSpan value)
    {
      return AddDays(value.TotalDays);
    }

    public ZonedDateTime AddDays(double value)
    {
      var days = Math.Truncate(value);
      // Add days in the time zone.  Add time in UTC
      return new ZonedDateTime(ZoneToUtc(ZoneDateTime.AddDays(days)).AddDays(value - days), Zone);
    }

    public ZonedDateTime AddBusinessDays(int value)
    {
      var offsetMagn = Math.Abs(value);
      var offsetSign = Math.Sign(value);
      var i = 0;
      var zone = ZoneDateTime;
      while (i < offsetMagn)
      {
        zone = zone.AddDays(offsetSign);
        if (zone.DayOfWeek != DayOfWeek.Sunday && zone.DayOfWeek != DayOfWeek.Saturday)
          i++;
      }

      return new ZonedDateTime(ZoneToUtc(zone), Zone);
    }

    public ZonedDateTime AddHours(double value)
    {
      return new ZonedDateTime(UtcDateTime.AddHours(value), Zone);
    }

    public ZonedDateTime AddMilliseconds(double value)
    {
      return new ZonedDateTime(UtcDateTime.AddMilliseconds(value), Zone);
    }

    public ZonedDateTime AddMinutes(double value)
    {
      return new ZonedDateTime(UtcDateTime.AddMinutes(value), Zone);
    }

    public ZonedDateTime AddMonths(int value)
    {
      return new ZonedDateTime(ZoneToUtc(ZoneDateTime.AddMonths(value)), Zone);
    }

    public ZonedDateTime AddSeconds(double value)
    {
      return new ZonedDateTime(UtcDateTime.AddSeconds(value), Zone);
    }

    public ZonedDateTime AddYears(int value)
    {
      return new ZonedDateTime(ZoneToUtc(ZoneDateTime.AddYears(value)), Zone);
    }

    public ZonedDateTime StartOfDay()
    {
      return new ZonedDateTime(ZoneToUtc(ZoneDateTime.Date), Zone);
    }

    public ZonedDateTime StartOfMonth()
    {
      var zone = ZoneDateTime;
      return new ZonedDateTime(ZoneToUtc(new DateTime(zone.Year, zone.Month, 1, 0, 0, 0)), Zone);
    }

    public ZonedDateTime StartOfQuarter()
    {
      var zone = ZoneDateTime;
      switch (zone.Month)
      {
        case 1:
        case 2:
        case 3:
          zone = new DateTime(zone.Year, 1, 1, 0, 0, 0);
          break;
        case 4:
        case 5:
        case 6:
          zone = new DateTime(zone.Year, 4, 1, 0, 0, 0);
          break;
        case 7:
        case 8:
        case 9:
          zone = new DateTime(zone.Year, 7, 1, 0, 0, 0);
          break;
        default:
          zone = new DateTime(zone.Year, 10, 1, 0, 0, 0);
          break;
      }
      return new ZonedDateTime(ZoneToUtc(zone), Zone);
    }

    public ZonedDateTime StartOfWeek(DayOfWeek firstDayOfWeek = DayOfWeek.Sunday)
    {
      var zone = ZoneDateTime;
      var offset = (int)firstDayOfWeek - (int)zone.DayOfWeek;
      if (offset > 0) offset -= 7;
      zone = zone.AddDays(offset);
      return new ZonedDateTime(ZoneToUtc(zone), Zone);
    }

    public ZonedDateTime StartOfYear()
    {
      var zone = ZoneDateTime;
      return new ZonedDateTime(ZoneToUtc(new DateTime(zone.Year, 1, 1, 0, 0, 0)), Zone);
    }

    public DateTimeOffset ToDateTimeOffset()
    {
      var zoned = ZoneDateTime;
      return new DateTimeOffset(zoned, Zone.GetUtcOffset(zoned));
    }

    public ZonedDateTime WithZone(DateTimeZone zone)
    {
      if (zone == Zone)
        return this;
      return new ZonedDateTime(UtcDateTime, zone);
    }

    private DateTime ZoneToUtc(DateTime value)
    {
      if (Zone == DateTimeZone.Utc)
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);

      value = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
      return DateTime.SpecifyKind(DateTimeZone.ConvertTime(value, Zone, DateTimeZone.Utc), DateTimeKind.Utc);
    }

    public static bool TryParse(string value, DateTimeZone timeZone, out ZonedDateTime instant)
    {
      if (DateTimeOffset.TryParseExact(value, "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"
          , CultureInfo.InvariantCulture, DateTimeStyles.None, out var offset)
        && !DateTime.TryParseExact(value, "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF"
          , CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var buffer))
      {
        instant = new ZonedDateTime(offset.UtcDateTime, timeZone);
        return true;
      }
      else if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date))
      {
        if (timeZone == DateTimeZone.Utc)
        {
          instant = new ZonedDateTime(DateTime.SpecifyKind(date, DateTimeKind.Utc), timeZone);
        }
        else
        {
          date = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
          date = DateTime.SpecifyKind(DateTimeZone.ConvertTime(date, timeZone, DateTimeZone.Utc), DateTimeKind.Utc);
          instant = new ZonedDateTime(date, timeZone);
        }
        return true;
      }

      instant = new ZonedDateTime();
      return false;
    }

    public static bool TryParseExact(string value, string format, IFormatProvider provider, DateTimeStyles styles, DateTimeZone timeZone, out ZonedDateTime instant)
    {
      if ((format.IndexOf('K') >= 0 || format.IndexOf('K') >= 0)
        && DateTimeOffset.TryParseExact(value, format, provider, styles, out var offset))
      {
        instant = new ZonedDateTime(offset.UtcDateTime, timeZone);
        return true;
      }
      else if (DateTime.TryParseExact(value, format, provider, styles, out var date))
      {
        date = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
        date = DateTime.SpecifyKind(DateTimeZone.ConvertTime(date, timeZone, DateTimeZone.Utc), DateTimeKind.Utc);
        instant = new ZonedDateTime(date, timeZone);
        return true;
      }

      instant = new ZonedDateTime();
      return false;
    }

    public override bool Equals(object obj)
    {
      if (obj is ZonedDateTime instant)
        return Equals(instant);
      return false;
    }

    public bool Equals(ZonedDateTime other)
    {
      return other.UtcDateTime == this.UtcDateTime
        && other.Zone == this.Zone;
    }

    public override int GetHashCode()
    {
      return UtcDateTime.GetHashCode()
        ^ Zone.GetHashCode();
    }

    public int CompareTo(ZonedDateTime other)
    {
      return UtcDateTime.CompareTo(other.UtcDateTime);
    }

    public override string ToString()
    {
      return ZoneDateTime.ToString("s");
    }

    public string ToString(string format)
    {
      return ZoneDateTime.ToString(format);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
      return ZoneDateTime.ToString(format, formatProvider);
    }

    public int CompareTo(object obj)
    {
      if (obj is ZonedDateTime zonded)
        return CompareTo(zonded);
      if (obj is DateTimeOffset offset)
        return UtcDateTime.CompareTo(offset.UtcDateTime);
      if (!(obj is DateTime date))
        throw new NotSupportedException();

      if (date.Kind == DateTimeKind.Utc)
        return UtcDateTime.CompareTo(date);

      return UtcDateTime.CompareTo(DateTimeZone.ConvertTime(date, DateTimeZone.Local, DateTimeZone.Utc));
    }

    public static bool operator ==(ZonedDateTime left, ZonedDateTime right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(ZonedDateTime left, ZonedDateTime right)
    {
      return !(left == right);
    }

    public static bool operator <(ZonedDateTime left, ZonedDateTime right)
    {
      return left.CompareTo(right) < 0;
    }

    public static bool operator <=(ZonedDateTime left, ZonedDateTime right)
    {
      return left.CompareTo(right) <= 0;
    }

    public static bool operator >(ZonedDateTime left, ZonedDateTime right)
    {
      return left.CompareTo(right) > 0;
    }

    public static bool operator >=(ZonedDateTime left, ZonedDateTime right)
    {
      return left.CompareTo(right) >= 0;
    }

    public static ZonedDateTime operator +(ZonedDateTime left, TimeSpan right)
    {
      return left.Add(right);
    }

    public static ZonedDateTime operator -(ZonedDateTime left, TimeSpan right)
    {
      return left.Add(right.Negate());
    }

    public static implicit operator ZonedDateTime(DateTime value)
    {
      return new ZonedDateTime(value, DateTimeZone.Local);
    }

    public static implicit operator ZonedDateTime(DateTimeOffset value)
    {
      return new ZonedDateTime(value.UtcDateTime, DateTimeZone.Local);
    }
  }
}
