using System;

namespace Innovator.Client
{
  /// <summary>
  /// A relative date specified by a magnitude and offset
  /// </summary>
  public struct DateOffset : IComparable<DateOffset>, IComparable
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
    public DateTime AsDate(DateTimeOffset todaysDate, bool isEndDate = false)
    {
      var offset = _offset;
      DateTimeOffset result;

      if (isEndDate) offset++;
      switch (Magnitude)
      {
        case DateMagnitude.BusinessDay:
          var offsetMagn = Math.Abs(offset);
          var offsetSign = Math.Sign(offset);
          var i = 0;
          result = todaysDate;
          while (i < offsetMagn)
          {
            result = result.AddDays(offsetSign);
            if (result.DayOfWeek != DayOfWeek.Sunday && result.DayOfWeek != DayOfWeek.Saturday)
              i++;
          }
          break;
        case DateMagnitude.Week:
          result = GetWeekStart(todaysDate, FirstDayOfWeek).AddDays(offset * 7);
          break;
        case DateMagnitude.Month:
          result = new DateTimeOffset(todaysDate.Year, todaysDate.Month, 1, 0, 0, 0, todaysDate.Offset).AddMonths(offset);
          break;
        case DateMagnitude.Quarter:
          switch (todaysDate.Month)
          {
            case 1:
            case 2:
            case 3:
              result = new DateTimeOffset(todaysDate.Year, 1, 1, 0, 0, 0, todaysDate.Offset).AddMonths(offset * 3);
              break;
            case 4:
            case 5:
            case 6:
              result = new DateTimeOffset(todaysDate.Year, 4, 1, 0, 0, 0, todaysDate.Offset).AddMonths(offset * 3);
              break;
            case 7:
            case 8:
            case 9:
              result = new DateTimeOffset(todaysDate.Year, 7, 1, 0, 0, 0, todaysDate.Offset).AddMonths(offset * 3);
              break;
            default:
              result = new DateTimeOffset(todaysDate.Year, 10, 1, 0, 0, 0, todaysDate.Offset).AddMonths(offset * 3);
              break;
          }
          break;
        case DateMagnitude.Year:
          result = new DateTimeOffset(todaysDate.Year, 1, 1, 0, 0, 0, todaysDate.Offset).AddYears(offset);
          break;
        default:
          result = todaysDate.AddDays(offset);
          break;
      }

      if (isEndDate) return result.Date.AddMilliseconds(-1);
      return result.Date;
    }

    internal static DateTimeOffset GetWeekStart(DateTimeOffset value, DayOfWeek firstDayOfWeek = DayOfWeek.Sunday)
    {
      var offset = (int)firstDayOfWeek - (int)value.DayOfWeek;
      if (offset > 0) offset -= 7;
      return value.AddDays(offset);
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
      if (obj is DateOffset)
        return CompareTo((DateOffset)obj);
      throw new NotSupportedException();
    }
  }
}
