using System;

namespace Innovator.Client
{
  /// <summary>
  /// A relative date specified by a magnitude and offset
  /// </summary>
  public struct DateOffset : IEquatable<DateOffset>, IComparable<DateOffset>, IComparable
  {
    private readonly short _offset;

    /// <summary>
    /// First day of the week to use for week offset calculations
    /// </summary>
    /// <value>
    /// The first day of week.  Defaults to <see cref="DayOfWeek.Sunday"/>
    /// </value>
    public DayOfWeek FirstDayOfWeek { get; set; }

    /// <summary>
    /// Magnitude of the offset (e.g. days, weeks, months, etc).
    /// </summary>
    public DateMagnitude Magnitude { get; }

    /// <summary>
    /// The amount of time units to offset
    /// </summary>
    public int Offset { get { return _offset; } }

    /// <summary>
    /// Initializes a structure to represent a relative date specified by a magnitude and offset
    /// </summary>
    /// <param name="offset">The amount of time units to offset.</param>
    /// <param name="magnitude">The magnitude of the offset (e.g. days, weeks, months, etc).</param>
    public DateOffset(short offset, DateMagnitude magnitude)
    {
      _offset = offset;
      Magnitude = magnitude;
      FirstDayOfWeek = DayOfWeek.Sunday;
    }

    /// <summary>
    /// Convert the date offset to an actual date relative to today
    /// </summary>
    /// <param name="todaysDate">This should be expressed in the local timezone</param>
    /// <param name="isEndDate">Indicates whether this date be used as the end date of an inclusive range</param>
    public DateTime AsDate(ZonedDateTime todaysDate, bool isEndDate = false)
    {
      var offset = _offset;
      ZonedDateTime result;

      if (isEndDate) offset++;
      switch (Magnitude)
      {
        case DateMagnitude.BusinessDay:
          result = todaysDate.StartOfDay().AddBusinessDays(offset);
          break;
        case DateMagnitude.Week:
          result = todaysDate.StartOfWeek(FirstDayOfWeek).AddDays(offset * 7);
          break;
        case DateMagnitude.Month:
          result = todaysDate.StartOfMonth().AddMonths(offset);
          break;
        case DateMagnitude.Quarter:
          result = todaysDate.StartOfQuarter().AddMonths(offset * 3);
          break;
        case DateMagnitude.Year:
          result = todaysDate.StartOfYear().AddYears(offset);
          break;
        default:
          result = todaysDate.AddDays(offset);
          break;
      }

      if (isEndDate) return result.StartOfDay().AddMilliseconds(-1).LocalDateTime;
      return result.StartOfDay().LocalDateTime;
    }

    private const int DaysInFourYears = 1461;
    private const int MonthsInFourYears = 48;
    private const int QuartersInFourYears = 16;

    private int OffsetInDays()
    {
      switch (Magnitude)
      {
        case DateMagnitude.BusinessDay:
          return 7 * _offset / 5;
        case DateMagnitude.Month:
          return DaysInFourYears * _offset / MonthsInFourYears;
        case DateMagnitude.Quarter:
          return DaysInFourYears * _offset / QuartersInFourYears;
        case DateMagnitude.Week:
          return _offset * 7;
        case DateMagnitude.Year:
          return DaysInFourYears * _offset / 4;
      }
      return _offset;
    }

    /// <summary>
    /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>
    /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order.
    /// </returns>
    public int CompareTo(DateOffset other)
    {
      return OffsetInDays() - other.OffsetInDays();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.
    /// </returns>
    /// <exception cref="NotSupportedException">If <paramref name="obj"/> is not of type <see cref="DateOffset"/></exception>
    public int CompareTo(object obj)
    {
      if (obj is DateOffset offset)
        return CompareTo(offset);
      throw new NotSupportedException();
    }

    public override bool Equals(object obj)
    {
      if (obj is DateOffset offset)
        return Equals(offset);
      return false;
    }

    public override int GetHashCode()
    {
      return this._offset.GetHashCode()
        ^ this.Magnitude.GetHashCode()
        ^ this.FirstDayOfWeek.GetHashCode();
    }

    public bool Equals(DateOffset other)
    {
      return this._offset == other._offset
        && this.Magnitude == other.Magnitude
        && this.FirstDayOfWeek == other.FirstDayOfWeek;
    }

    public static bool operator ==(DateOffset left, DateOffset right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(DateOffset left, DateOffset right)
    {
      return !(left == right);
    }

    public static bool operator <(DateOffset left, DateOffset right)
    {
      return left.CompareTo(right) < 0;
    }

    public static bool operator <=(DateOffset left, DateOffset right)
    {
      return left.CompareTo(right) <= 0;
    }

    public static bool operator >(DateOffset left, DateOffset right)
    {
      return left.CompareTo(right) > 0;
    }

    public static bool operator >=(DateOffset left, DateOffset right)
    {
      return left.CompareTo(right) >= 0;
    }
  }
}
