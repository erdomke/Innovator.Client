using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  public class StaticDateTimeRange
  {
    public DateTime? EndDate { get; set; }
    public DateTime? StartDate { get; set; }
    public TimeZoneData TimeZone { get; set; }

    public StaticDateTimeRange()
    {
      this.TimeZone = TimeZoneData.Local;
    }

    public StaticDateTimeRange ToTimeZone(TimeZoneData timeZone)
    {
      if (timeZone == this.TimeZone) return this;
      var result = new StaticDateTimeRange();
      result.EndDate = EndDate.HasValue ? TimeZoneData.ConvertTime(EndDate.Value, this.TimeZone, timeZone) : (DateTime?)null;
      result.StartDate = StartDate.HasValue ? TimeZoneData.ConvertTime(StartDate.Value, this.TimeZone, timeZone) : (DateTime?)null;
      result.TimeZone = timeZone;
      return result;
    }


    public Condition Condition()
    {
      if (StartDate.HasValue && !EndDate.HasValue)
        return Client.Condition.GreaterThanEqual;
      else if (!StartDate.HasValue && EndDate.HasValue)
        return Client.Condition.LessThanEqual;
      else if (StartDate.HasValue && EndDate.HasValue)
        return Client.Condition.Between;
      return Client.Condition.Undefined;
    }
  }
}
