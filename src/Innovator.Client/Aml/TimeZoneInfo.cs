#if !TIMEZONEINFO
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Innovator.Client.Time
{
  /// <summary>Represents any time zone in the world.</summary>
  internal sealed partial class TimeZoneInfo : IEquatable<TimeZoneInfo>
  {
    /// <summary>Gets the time zone identifier.</summary>
    /// <returns>The time zone identifier.</returns>
    public string Id { get { return this.m_id; } }

    /// <summary>Gets the general display name that represents the time zone.</summary>
    /// <returns>The time zone's general display name.</returns>
    public string DisplayName
    {
      get
      {
        if (this.m_displayName != null)
        {
          return this.m_displayName;
        }
        return string.Empty;
      }
    }

    /// <summary>Gets the display name for the time zone's standard time.</summary>
    /// <returns>The display name of the time zone's standard time.</returns>
    public string StandardName
    {
      get
      {
        if (this.m_standardDisplayName != null)
        {
          return this.m_standardDisplayName;
        }
        return string.Empty;
      }
    }

    /// <summary>Gets the display name for the current time zone's daylight saving time.</summary>
    /// <returns>The display name for the time zone's daylight saving time.</returns>
    public string DaylightName
    {
      get
      {
        if (this.m_daylightDisplayName != null)
        {
          return this.m_daylightDisplayName;
        }
        return string.Empty;
      }
    }

    /// <summary>Gets the time difference between the current time zone's standard time and Coordinated Universal Time (UTC).</summary>
    /// <returns>An object that indicates the time difference between the current time zone's standard time and Coordinated Universal Time (UTC).</returns>
    public TimeSpan BaseUtcOffset
    {
      get
      {
        return this.m_baseUtcOffset;
      }
    }

    /// <summary>Gets a value indicating whether the time zone has any daylight saving time rules.</summary>
    /// <returns>true if the time zone supports daylight saving time; otherwise, false.</returns>
    public bool SupportsDaylightSavingTime
    {
      get
      {
        return this.m_supportsDaylightSavingTime;
      }
    }

    /// <summary>Retrieves an array of <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> objects that apply to the current <see cref="T:System.TimeZoneInfo" /> object.</summary>
    /// <returns>An array of objects for this time zone.</returns>
    /// <exception cref="T:System.OutOfMemoryException">The system does not have enough memory to make an in-memory copy of the adjustment rules.</exception>
    /// <filterpriority>2</filterpriority>
    public TimeZoneInfo.AdjustmentRule[] GetAdjustmentRules()
    {
      if (this.m_adjustmentRules == null)
      {
        return new TimeZoneInfo.AdjustmentRule[0];
      }
      return (TimeZoneInfo.AdjustmentRule[])this.m_adjustmentRules.Clone();
    }

    /// <summary>Returns information about the possible dates and times that an ambiguous date and time can be mapped to.</summary>
    /// <returns>An array of objects that represents possible Coordinated Universal Time (UTC) offsets that a particular date and time can be mapped to.</returns>
    /// <param name="dateTimeOffset">A date and time.</param>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="dateTimeOffset" /> is not an ambiguous time.</exception>
    /// <filterpriority>2</filterpriority>
    public TimeSpan[] GetAmbiguousTimeOffsets(DateTimeOffset dateTimeOffset)
    {
      if (!this.SupportsDaylightSavingTime)
      {
        throw new ArgumentException("DateTimeOffset is not ambiguous", "dateTimeOffset");
      }
      DateTime dateTime = TimeZoneInfo.ConvertTime(dateTimeOffset, this).DateTime;
      bool flag = false;
      TimeZoneInfo.AdjustmentRule adjustmentRuleForTime = this.GetAdjustmentRuleForTime(dateTime);
      if (adjustmentRuleForTime != null && adjustmentRuleForTime.HasDaylightSaving)
      {
        DaylightTime daylightTime = TimeZoneInfo.GetDaylightTime(dateTime.Year, adjustmentRuleForTime);
        flag = TimeZoneInfo.GetIsAmbiguousTime(dateTime, adjustmentRuleForTime, daylightTime);
      }
      if (!flag)
      {
        throw new ArgumentException("DateTimeOffset is not ambiguous", "dateTimeOffset");
      }
      TimeSpan[] array = new TimeSpan[2];
      TimeSpan timeSpan = this.m_baseUtcOffset + adjustmentRuleForTime.BaseUtcOffsetDelta;
      if (adjustmentRuleForTime.DaylightDelta > TimeSpan.Zero)
      {
        array[0] = timeSpan;
        array[1] = timeSpan + adjustmentRuleForTime.DaylightDelta;
      }
      else
      {
        array[0] = timeSpan + adjustmentRuleForTime.DaylightDelta;
        array[1] = timeSpan;
      }
      return array;
    }

    /// <summary>Calculates the offset or difference between the time in this time zone and Coordinated Universal Time (UTC) for a particular date and time.</summary>
    /// <returns>An object that indicates the time difference between Coordinated Universal Time (UTC) and the current time zone.</returns>
    /// <param name="dateTimeOffset">The date and time to determine the offset for.</param>
    public TimeSpan GetUtcOffset(DateTimeOffset dateTimeOffset)
    {
      return TimeZoneInfo.GetUtcOffsetFromUtc(dateTimeOffset.UtcDateTime, this);
    }

    /// <summary>Calculates the offset or difference between the time in this time zone and Coordinated Universal Time (UTC) for a particular date and time.</summary>
    /// <returns>An object that indicates the time difference between the two time zones.</returns>
    /// <param name="dateTime">The date and time to determine the offset for.   </param>
    public TimeSpan GetUtcOffset(DateTime dateTime)
    {
      return this.GetUtcOffset(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime, TimeZoneInfo.s_cachedData);
    }

    internal TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags)
    {
      return this.GetUtcOffset(dateTime, flags, TimeZoneInfo.s_cachedData);
    }

    private TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneInfoOptions flags, TimeZoneInfo.CachedData cachedData)
    {
      if (dateTime.Kind == DateTimeKind.Local)
      {
        if (cachedData.GetCorrespondingKind(this) != DateTimeKind.Local)
        {
          DateTime time = TimeZoneInfo.ConvertTime(dateTime, cachedData.Local, cachedData.Utc, flags);
          return TimeZoneInfo.GetUtcOffsetFromUtc(time, this);
        }
      }
      else if (dateTime.Kind == DateTimeKind.Utc)
      {
        if (cachedData.GetCorrespondingKind(this) == DateTimeKind.Utc)
        {
          return this.m_baseUtcOffset;
        }
        return TimeZoneInfo.GetUtcOffsetFromUtc(dateTime, this);
      }
      return TimeZoneInfo.GetUtcOffset(dateTime, this, flags);
    }

    /// <summary>Clears cached time zone data.</summary>
    /// <filterpriority>2</filterpriority>
    public static void ClearCachedData()
    {
      TimeZoneInfo.s_cachedData = new TimeZoneInfo.CachedData();
    }

    /// <summary>Converts a time to the time in a particular time zone.</summary>
    /// <returns>The date and time in the destination time zone.</returns>
    /// <param name="dateTimeOffset">The date and time to convert.   </param>
    /// <param name="destinationTimeZone">The time zone to convert <paramref name="dateTime" /> to.</param>
    /// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="destinationTimeZone" /> parameter is null.</exception>
    public static DateTimeOffset ConvertTime(DateTimeOffset dateTimeOffset, TimeZoneInfo destinationTimeZone)
    {
      if (destinationTimeZone == null)
      {
        throw new ArgumentNullException("destinationTimeZone");
      }
      DateTime utcDateTime = dateTimeOffset.UtcDateTime;
      TimeSpan utcOffsetFromUtc = TimeZoneInfo.GetUtcOffsetFromUtc(utcDateTime, destinationTimeZone);
      long num = utcDateTime.Ticks + utcOffsetFromUtc.Ticks;
      if (num > DateTimeOffset.MaxValue.Ticks)
      {
        return DateTimeOffset.MaxValue;
      }
      if (num < DateTimeOffset.MinValue.Ticks)
      {
        return DateTimeOffset.MinValue;
      }
      return new DateTimeOffset(num, utcOffsetFromUtc);
    }

    /// <summary>Converts a time to the time in a particular time zone.</summary>
    /// <returns>The date and time in the destination time zone.</returns>
    /// <param name="dateTime">The date and time to convert.   </param>
    /// <param name="destinationTimeZone">The time zone to convert <paramref name="dateTime" /> to.</param>
    /// <exception cref="T:System.ArgumentException">The value of the <paramref name="dateTime" /> parameter represents an invalid time.</exception>
    /// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="destinationTimeZone" /> parameter is null.</exception>
    public static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo destinationTimeZone)
    {
      if (destinationTimeZone == null)
      {
        throw new ArgumentNullException("destinationTimeZone");
      }
      if (dateTime.Ticks == 0L)
      {
        TimeZoneInfo.ClearCachedData();
      }
      TimeZoneInfo.CachedData cachedData = TimeZoneInfo.s_cachedData;
      if (dateTime.Kind == DateTimeKind.Utc)
      {
        return TimeZoneInfo.ConvertTime(dateTime, cachedData.Utc, destinationTimeZone, TimeZoneInfoOptions.None, cachedData);
      }
      return TimeZoneInfo.ConvertTime(dateTime, cachedData.Local, destinationTimeZone, TimeZoneInfoOptions.None, cachedData);
    }

    /// <summary>Converts a time from one time zone to another.</summary>
    /// <returns>The date and time in the destination time zone that corresponds to the <paramref name="dateTime" /> parameter in the source time zone.</returns>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <param name="sourceTimeZone">The time zone of <paramref name="dateTime" />.</param>
    /// <param name="destinationTimeZone">The time zone to convert <paramref name="dateTime" /> to.</param>
    /// <exception cref="T:System.ArgumentException">The <see cref="P:System.DateTime.Kind" /> property of the <paramref name="dateTime" /> parameter is <see cref="F:System.DateTimeKind.Local" />, but the <paramref name="sourceTimeZone" /> parameter does not equal <see cref="F:System.DateTimeKind.Local" />. For more information, see the Remarks section. -or-The <see cref="P:System.DateTime.Kind" /> property of the <paramref name="dateTime" /> parameter is <see cref="F:System.DateTimeKind.Utc" />, but the <paramref name="sourceTimeZone" /> parameter does not equal <see cref="P:System.TimeZoneInfo.Utc" />.-or-The <paramref name="dateTime" /> parameter is an invalid time (that is, it represents a time that does not exist because of a time zone's adjustment rules).</exception>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="sourceTimeZone" /> parameter is null.-or-The <paramref name="destinationTimeZone" /> parameter is null.</exception>
    public static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
    {
      return TimeZoneInfo.ConvertTime(dateTime, sourceTimeZone, destinationTimeZone, TimeZoneInfoOptions.None, TimeZoneInfo.s_cachedData);
    }

    internal static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone, TimeZoneInfoOptions flags)
    {
      return TimeZoneInfo.ConvertTime(dateTime, sourceTimeZone, destinationTimeZone, flags, TimeZoneInfo.s_cachedData);
    }

    private static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone, TimeZoneInfoOptions flags, TimeZoneInfo.CachedData cachedData)
    {
      if (sourceTimeZone == null)
      {
        throw new ArgumentNullException("sourceTimeZone");
      }
      if (destinationTimeZone == null)
      {
        throw new ArgumentNullException("destinationTimeZone");
      }
      DateTimeKind correspondingKind = cachedData.GetCorrespondingKind(sourceTimeZone);
      if ((flags & TimeZoneInfoOptions.NoThrowOnInvalidTime) == (TimeZoneInfoOptions)0 && dateTime.Kind != DateTimeKind.Unspecified && dateTime.Kind != correspondingKind)
      {
        throw new ArgumentException("Convert mismatch", "sourceTimeZone");
      }
      TimeZoneInfo.AdjustmentRule adjustmentRuleForTime = sourceTimeZone.GetAdjustmentRuleForTime(dateTime);
      TimeSpan t = sourceTimeZone.BaseUtcOffset;
      if (adjustmentRuleForTime != null)
      {
        t += adjustmentRuleForTime.BaseUtcOffsetDelta;
        if (adjustmentRuleForTime.HasDaylightSaving)
        {
          DaylightTime daylightTime = TimeZoneInfo.GetDaylightTime(dateTime.Year, adjustmentRuleForTime);
          if ((flags & TimeZoneInfoOptions.NoThrowOnInvalidTime) == (TimeZoneInfoOptions)0 && TimeZoneInfo.GetIsInvalidTime(dateTime, adjustmentRuleForTime, daylightTime))
          {
            throw new ArgumentException("DateTime is invalid", "dateTime");
          }
          bool isDaylightSavings = TimeZoneInfo.GetIsDaylightSavings(dateTime, adjustmentRuleForTime, daylightTime, flags);
          t += (isDaylightSavings ? adjustmentRuleForTime.DaylightDelta : TimeSpan.Zero);
        }
      }
      DateTimeKind correspondingKind2 = cachedData.GetCorrespondingKind(destinationTimeZone);
      if (dateTime.Kind != DateTimeKind.Unspecified && correspondingKind != DateTimeKind.Unspecified && correspondingKind == correspondingKind2)
      {
        return dateTime;
      }
      long ticks = dateTime.Ticks - t.Ticks;
      bool isAmbiguousDst = false;
      DateTime dateTime2 = TimeZoneInfo.ConvertUtcToTimeZone(ticks, destinationTimeZone, out isAmbiguousDst);
      if (correspondingKind2 == DateTimeKind.Local)
      {
        return new DateTime(dateTime2.Ticks, DateTimeKind.Local); // Can't specify the ambiguous parameter from the public constructure
      }
      return new DateTime(dateTime2.Ticks, correspondingKind2);
    }

    /// <summary>Determines whether the current <see cref="T:System.TimeZoneInfo" /> object and another <see cref="T:System.TimeZoneInfo" /> object are equal.</summary>
    /// <returns>true if the two <see cref="T:System.TimeZoneInfo" /> objects are equal; otherwise, false.</returns>
    /// <param name="other">A second object to compare with the current object.  </param>
    /// <filterpriority>2</filterpriority>
    public bool Equals(TimeZoneInfo other)
    {
      return other != null && string.Compare(this.m_id, other.m_id, StringComparison.OrdinalIgnoreCase) == 0 && this.HasSameRules(other);
    }

    /// <summary>Determines whether the current <see cref="T:System.TimeZoneInfo" /> object and another object are equal.</summary>
    /// <returns>true if <paramref name="obj" /> is a <see cref="T:System.TimeZoneInfo" /> object that is equal to the current instance; otherwise, false.</returns>
    /// <param name="obj">A second object to compare with the current object.  </param>
    public override bool Equals(object obj)
    {
      TimeZoneInfo timeZoneInfo = obj as TimeZoneInfo;
      return timeZoneInfo != null && this.Equals(timeZoneInfo);
    }

    /// <summary>Serves as a hash function for hashing algorithms and data structures such as hash tables.</summary>
    /// <returns>A 32-bit signed integer that serves as the hash code for this <see cref="T:System.TimeZoneInfo" /> object.</returns>
    /// <filterpriority>2</filterpriority>
    public override int GetHashCode()
    {
      return this.m_id.ToUpperInvariant().GetHashCode();
    }

    ///// <summary>Returns a sorted collection of all the time zones about which information is available on the local system.</summary>
    ///// <returns>A read-only collection of <see cref="T:System.TimeZoneInfo" /> objects.</returns>
    ///// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to store all time zone information.</exception>
    ///// <exception cref="T:System.Security.SecurityException">The user does not have permission to read from the registry keys that contain time zone information.</exception>
    //[SecuritySafeCritical]
    //[__DynamicallyInvokable]
    //public static ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
    //{
    //  TimeZoneInfo.CachedData cachedData = TimeZoneInfo.s_cachedData;
    //  TimeZoneInfo.CachedData obj = cachedData;
    //  lock (obj)
    //  {
    //    if (cachedData.m_readOnlySystemTimeZones == null)
    //    {
    //      PermissionSet permissionSet = new PermissionSet(PermissionState.None);
    //      permissionSet.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones"));
    //      permissionSet.Assert();
    //      using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones", RegistryKeyPermissionCheck.Default, RegistryRights.ExecuteKey))
    //      {
    //        if (registryKey != null)
    //        {
    //          foreach (string id in registryKey.GetSubKeyNames())
    //          {
    //            TimeZoneInfo timeZoneInfo;
    //            Exception ex;
    //            TimeZoneInfo.TryGetTimeZone(id, false, out timeZoneInfo, out ex, cachedData);
    //          }
    //        }
    //        cachedData.m_allSystemTimeZonesRead = true;
    //      }
    //      List<TimeZoneInfo> list;
    //      if (cachedData.m_systemTimeZones != null)
    //      {
    //        list = new List<TimeZoneInfo>(cachedData.m_systemTimeZones.Values);
    //      }
    //      else
    //      {
    //        list = new List<TimeZoneInfo>();
    //      }
    //      list.Sort(new TimeZoneInfo.TimeZoneInfoComparer());
    //      cachedData.m_readOnlySystemTimeZones = new ReadOnlyCollection<TimeZoneInfo>(list);
    //    }
    //  }
    //  return cachedData.m_readOnlySystemTimeZones;
    //}

    /// <summary>Indicates whether the current object and another <see cref="T:System.TimeZoneInfo" /> object have the same adjustment rules.</summary>
    /// <returns>true if the two time zones have identical adjustment rules and an identical base offset; otherwise, false.</returns>
    /// <param name="other">A second object to compare with the current <see cref="T:System.TimeZoneInfo" /> object.   </param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="other" /> parameter is null.</exception>
    public bool HasSameRules(TimeZoneInfo other)
    {
      if (other == null)
      {
        throw new ArgumentNullException("other");
      }
      if (this.m_baseUtcOffset != other.m_baseUtcOffset || this.m_supportsDaylightSavingTime != other.m_supportsDaylightSavingTime)
      {
        return false;
      }
      TimeZoneInfo.AdjustmentRule[] adjustmentRules = this.m_adjustmentRules;
      TimeZoneInfo.AdjustmentRule[] adjustmentRules2 = other.m_adjustmentRules;
      bool flag = (adjustmentRules == null && adjustmentRules2 == null) || (adjustmentRules != null && adjustmentRules2 != null);
      if (!flag)
      {
        return false;
      }
      if (adjustmentRules != null)
      {
        if (adjustmentRules.Length != adjustmentRules2.Length)
        {
          return false;
        }
        for (int i = 0; i < adjustmentRules.Length; i++)
        {
          if (!adjustmentRules[i].Equals(adjustmentRules2[i]))
          {
            return false;
          }
        }
      }
      return flag;
    }

    /// <summary>Gets a <see cref="T:System.TimeZoneInfo" /> object that represents the local time zone.</summary>
    /// <returns>An object that represents the local time zone.</returns>
    public static TimeZoneInfo Local
    {
      get
      {
        return TimeZoneInfo.s_cachedData.Local;
      }
    }

    /// <summary>Returns the current <see cref="T:System.TimeZoneInfo" /> object's display name.</summary>
    /// <returns>The value of the <see cref="P:System.TimeZoneInfo.DisplayName" /> property of the current <see cref="T:System.TimeZoneInfo" /> object.</returns>
    public override string ToString()
    {
      return this.DisplayName;
    }

    /// <summary>Gets a <see cref="T:System.TimeZoneInfo" /> object that represents the Coordinated Universal Time (UTC) zone.</summary>
    /// <returns>An object that represents the Coordinated Universal Time (UTC) zone.</returns>
    public static TimeZoneInfo Utc
    {
      get
      {
        return TimeZoneInfo.s_cachedData.Utc;
      }
    }

    //[SecurityCritical]
    //private TimeZoneInfo(Win32Native.TimeZoneInformation zone, bool dstDisabled)
    //{
    //  if (string.IsNullOrEmpty(zone.StandardName))
    //  {
    //    this.m_id = "Local";
    //  }
    //  else
    //  {
    //    this.m_id = zone.StandardName;
    //  }
    //  this.m_baseUtcOffset = new TimeSpan(0, -zone.Bias, 0);
    //  if (!dstDisabled)
    //  {
    //    Win32Native.RegistryTimeZoneInformation timeZoneInformation = new Win32Native.RegistryTimeZoneInformation(zone);
    //    TimeZoneInfo.AdjustmentRule adjustmentRule = TimeZoneInfo.CreateAdjustmentRuleFromTimeZoneInformation(timeZoneInformation, DateTime.MinValue.Date, DateTime.MaxValue.Date, zone.Bias);
    //    if (adjustmentRule != null)
    //    {
    //      this.m_adjustmentRules = new TimeZoneInfo.AdjustmentRule[1];
    //      this.m_adjustmentRules[0] = adjustmentRule;
    //    }
    //  }
    //  TimeZoneInfo.ValidateTimeZoneInfo(this.m_id, this.m_baseUtcOffset, this.m_adjustmentRules, out this.m_supportsDaylightSavingTime);
    //  this.m_displayName = zone.StandardName;
    //  this.m_standardDisplayName = zone.StandardName;
    //  this.m_daylightDisplayName = zone.DaylightName;
    //}

    private TimeZoneInfo(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, TimeZoneInfo.AdjustmentRule[] adjustmentRules, bool disableDaylightSavingTime)
    {
      bool flag;
      TimeZoneInfo.ValidateTimeZoneInfo(id, baseUtcOffset, adjustmentRules, out flag);
      if (!disableDaylightSavingTime && adjustmentRules != null && adjustmentRules.Length != 0)
      {
        this.m_adjustmentRules = (TimeZoneInfo.AdjustmentRule[])adjustmentRules.Clone();
      }
      this.m_id = id;
      this.m_baseUtcOffset = baseUtcOffset;
      this.m_displayName = displayName;
      this.m_standardDisplayName = standardDisplayName;
      this.m_daylightDisplayName = (disableDaylightSavingTime ? null : daylightDisplayName);
      this.m_supportsDaylightSavingTime = (flag && !disableDaylightSavingTime);
    }

    /// <summary>Creates a custom time zone with a specified identifier, an offset from Coordinated Universal Time (UTC), a display name, and a standard time display name.</summary>
    /// <returns>The new time zone.</returns>
    /// <param name="id">The time zone's identifier.</param>
    /// <param name="baseUtcOffset">An object that represents the time difference between this time zone and Coordinated Universal Time (UTC).</param>
    /// <param name="displayName">The display name of the new time zone.   </param>
    /// <param name="standardDisplayName">The name of the new time zone's standard time.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="id" /> parameter is null.</exception>
    /// <exception cref="T:System.ArgumentException">The <paramref name="id" /> parameter is an empty string ("").-or-The <paramref name="baseUtcOffset" /> parameter does not represent a whole number of minutes.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="baseUtcOffset" /> parameter is greater than 14 hours or less than -14 hours.</exception>
    /// <filterpriority>2</filterpriority>
    public static TimeZoneInfo CreateCustomTimeZone(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName)
    {
      return new TimeZoneInfo(id, baseUtcOffset, displayName, standardDisplayName, standardDisplayName, null, false);
    }

    /// <summary>Creates a custom time zone with a specified identifier, an offset from Coordinated Universal Time (UTC), a display name, a standard time name, a daylight saving time name, and daylight saving time rules.</summary>
    /// <returns>A <see cref="T:System.TimeZoneInfo" /> object that represents the new time zone.</returns>
    /// <param name="id">The time zone's identifier.</param>
    /// <param name="baseUtcOffset">An object that represents the time difference between this time zone and Coordinated Universal Time (UTC).</param>
    /// <param name="displayName">The display name of the new time zone.   </param>
    /// <param name="standardDisplayName">The new time zone's standard time name.</param>
    /// <param name="daylightDisplayName">The daylight saving time name of the new time zone.   </param>
    /// <param name="adjustmentRules">An array that augments the base UTC offset for a particular period. </param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="id" /> parameter is null.</exception>
    /// <exception cref="T:System.ArgumentException">The <paramref name="id" /> parameter is an empty string ("").-or-The <paramref name="baseUtcOffset" /> parameter does not represent a whole number of minutes.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="baseUtcOffset" /> parameter is greater than 14 hours or less than -14 hours.</exception>
    /// <exception cref="T:System.InvalidTimeZoneException">The adjustment rules specified in the <paramref name="adjustmentRules" /> parameter overlap.-or-The adjustment rules specified in the <paramref name="adjustmentRules" /> parameter are not in chronological order.-or-One or more elements in <paramref name="adjustmentRules" /> are null.-or-A date can have multiple adjustment rules applied to it.-or-The sum of the <paramref name="baseUtcOffset" /> parameter and the <see cref="P:System.TimeZoneInfo.AdjustmentRule.DaylightDelta" /> value of one or more objects in the <paramref name="adjustmentRules" /> array is greater than 14 hours or less than -14 hours.</exception>
    /// <filterpriority>2</filterpriority>
    public static TimeZoneInfo CreateCustomTimeZone(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, TimeZoneInfo.AdjustmentRule[] adjustmentRules)
    {
      return new TimeZoneInfo(id, baseUtcOffset, displayName, standardDisplayName, daylightDisplayName, adjustmentRules, false);
    }

    /// <summary>Creates a custom time zone with a specified identifier, an offset from Coordinated Universal Time (UTC), a display name, a standard time name, a daylight saving time name, daylight saving time rules, and a value that indicates whether the returned object reflects daylight saving time information.</summary>
    /// <returns>The new time zone. If the <paramref name="disableDaylightSavingTime" /> parameter is true, the returned object has no daylight saving time data.</returns>
    /// <param name="id">The time zone's identifier.</param>
    /// <param name="baseUtcOffset">A <see cref="T:System.TimeSpan" /> object that represents the time difference between this time zone and Coordinated Universal Time (UTC).</param>
    /// <param name="displayName">The display name of the new time zone.   </param>
    /// <param name="standardDisplayName">The standard time name of the new time zone.</param>
    /// <param name="daylightDisplayName">The daylight saving time name of the new time zone.   </param>
    /// <param name="adjustmentRules">An array of <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> objects that augment the base UTC offset for a particular period.</param>
    /// <param name="disableDaylightSavingTime">true to discard any daylight saving time-related information present in <paramref name="adjustmentRules" /> with the new object; otherwise, false.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="id" /> parameter is null.</exception>
    /// <exception cref="T:System.ArgumentException">The <paramref name="id" /> parameter is an empty string ("").-or-The <paramref name="baseUtcOffset" /> parameter does not represent a whole number of minutes.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="baseUtcOffset" /> parameter is greater than 14 hours or less than -14 hours.</exception>
    /// <exception cref="T:System.InvalidTimeZoneException">The adjustment rules specified in the <paramref name="adjustmentRules" /> parameter overlap.-or-The adjustment rules specified in the <paramref name="adjustmentRules" /> parameter are not in chronological order.-or-One or more elements in <paramref name="adjustmentRules" /> are null.-or-A date can have multiple adjustment rules applied to it.-or-The sum of the <paramref name="baseUtcOffset" /> parameter and the <see cref="P:System.TimeZoneInfo.AdjustmentRule.DaylightDelta" /> value of one or more objects in the <paramref name="adjustmentRules" /> array is greater than 14 hours or less than -14 hours.</exception>
    /// <filterpriority>2</filterpriority>
    public static TimeZoneInfo CreateCustomTimeZone(string id, TimeSpan baseUtcOffset, string displayName, string standardDisplayName, string daylightDisplayName, TimeZoneInfo.AdjustmentRule[] adjustmentRules, bool disableDaylightSavingTime)
    {
      return new TimeZoneInfo(id, baseUtcOffset, displayName, standardDisplayName, daylightDisplayName, adjustmentRules, disableDaylightSavingTime);
    }

    private TimeZoneInfo.AdjustmentRule GetAdjustmentRuleForTime(DateTime dateTime)
    {
      if (this.m_adjustmentRules == null || this.m_adjustmentRules.Length == 0)
      {
        return null;
      }
      DateTime date = dateTime.Date;
      for (int i = 0; i < this.m_adjustmentRules.Length; i++)
      {
        if (this.m_adjustmentRules[i].DateStart <= date && this.m_adjustmentRules[i].DateEnd >= date)
        {
          return this.m_adjustmentRules[i];
        }
      }
      return null;
    }

    private static DateTime ConvertUtcToTimeZone(long ticks, TimeZoneInfo destinationTimeZone, out bool isAmbiguousLocalDst)
    {
      DateTime time;
      if (ticks > DateTime.MaxValue.Ticks)
      {
        time = DateTime.MaxValue;
      }
      else if (ticks < DateTime.MinValue.Ticks)
      {
        time = DateTime.MinValue;
      }
      else
      {
        time = new DateTime(ticks);
      }
      ticks += TimeZoneInfo.GetUtcOffsetFromUtc(time, destinationTimeZone, out isAmbiguousLocalDst).Ticks;
      DateTime result;
      if (ticks > DateTime.MaxValue.Ticks)
      {
        result = DateTime.MaxValue;
      }
      else if (ticks < DateTime.MinValue.Ticks)
      {
        result = DateTime.MinValue;
      }
      else
      {
        result = new DateTime(ticks);
      }
      return result;
    }

    private static DaylightTime GetDaylightTime(int year, TimeZoneInfo.AdjustmentRule rule)
    {
      TimeSpan daylightDelta = rule.DaylightDelta;
      DateTime start = TimeZoneInfo.TransitionTimeToDateTime(year, rule.DaylightTransitionStart);
      DateTime end = TimeZoneInfo.TransitionTimeToDateTime(year, rule.DaylightTransitionEnd);
      return new DaylightTime(start, end, daylightDelta);
    }

    private static bool GetIsDaylightSavings(DateTime time, TimeZoneInfo.AdjustmentRule rule, DaylightTime daylightTime, TimeZoneInfoOptions flags)
    {
      if (rule == null)
      {
        return false;
      }
      DateTime startTime;
      DateTime endTime;
      if (time.Kind == DateTimeKind.Local)
      {
        startTime = (rule.IsStartDateMarkerForBeginningOfYear() ? new DateTime(daylightTime.Start.Year, 1, 1, 0, 0, 0) : (daylightTime.Start + daylightTime.Delta));
        endTime = (rule.IsEndDateMarkerForEndOfYear() ? new DateTime(daylightTime.End.Year + 1, 1, 1, 0, 0, 0).AddTicks(-1L) : daylightTime.End);
      }
      else
      {
        bool flag = rule.DaylightDelta > TimeSpan.Zero;
        startTime = (rule.IsStartDateMarkerForBeginningOfYear() ? new DateTime(daylightTime.Start.Year, 1, 1, 0, 0, 0) : (daylightTime.Start + (flag ? rule.DaylightDelta : TimeSpan.Zero)));
        endTime = (rule.IsEndDateMarkerForEndOfYear() ? new DateTime(daylightTime.End.Year + 1, 1, 1, 0, 0, 0).AddTicks(-1L) : (daylightTime.End + (flag ? (-rule.DaylightDelta) : TimeSpan.Zero)));
      }
      bool flag2 = TimeZoneInfo.CheckIsDst(startTime, time, endTime, false);
      if (flag2 && time.Kind == DateTimeKind.Local && TimeZoneInfo.GetIsAmbiguousTime(time, rule, daylightTime))
      {
        flag2 = false; //time.IsAmbiguousDaylightSavingTime();
      }
      return flag2;
    }

    private static bool GetIsDaylightSavingsFromUtc(DateTime time, int Year, TimeSpan utc, TimeZoneInfo.AdjustmentRule rule, out bool isAmbiguousLocalDst, TimeZoneInfo zone)
    {
      isAmbiguousLocalDst = false;
      if (rule == null)
      {
        return false;
      }
      TimeSpan t = utc + rule.BaseUtcOffsetDelta;
      DaylightTime daylightTime = TimeZoneInfo.GetDaylightTime(Year, rule);
      bool ignoreYearAdjustment = false;
      DateTime dateTime;
      if (rule.IsStartDateMarkerForBeginningOfYear() && daylightTime.Start.Year > DateTime.MinValue.Year)
      {
        TimeZoneInfo.AdjustmentRule adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(new DateTime(daylightTime.Start.Year - 1, 12, 31));
        if (adjustmentRuleForTime != null && adjustmentRuleForTime.IsEndDateMarkerForEndOfYear())
        {
          DaylightTime daylightTime2 = TimeZoneInfo.GetDaylightTime(daylightTime.Start.Year - 1, adjustmentRuleForTime);
          dateTime = daylightTime2.Start - utc - adjustmentRuleForTime.BaseUtcOffsetDelta;
          ignoreYearAdjustment = true;
        }
        else
        {
          dateTime = new DateTime(daylightTime.Start.Year, 1, 1, 0, 0, 0) - t;
        }
      }
      else
      {
        dateTime = daylightTime.Start - t;
      }
      DateTime dateTime2;
      if (rule.IsEndDateMarkerForEndOfYear() && daylightTime.End.Year < DateTime.MaxValue.Year)
      {
        TimeZoneInfo.AdjustmentRule adjustmentRuleForTime2 = zone.GetAdjustmentRuleForTime(new DateTime(daylightTime.End.Year + 1, 1, 1));
        if (adjustmentRuleForTime2 != null && adjustmentRuleForTime2.IsStartDateMarkerForBeginningOfYear())
        {
          if (adjustmentRuleForTime2.IsEndDateMarkerForEndOfYear())
          {
            dateTime2 = new DateTime(daylightTime.End.Year + 1, 12, 31) - utc - adjustmentRuleForTime2.BaseUtcOffsetDelta - adjustmentRuleForTime2.DaylightDelta;
          }
          else
          {
            DaylightTime daylightTime3 = TimeZoneInfo.GetDaylightTime(daylightTime.End.Year + 1, adjustmentRuleForTime2);
            dateTime2 = daylightTime3.End - utc - adjustmentRuleForTime2.BaseUtcOffsetDelta - adjustmentRuleForTime2.DaylightDelta;
          }
          ignoreYearAdjustment = true;
        }
        else
        {
          dateTime2 = new DateTime(daylightTime.End.Year + 1, 1, 1, 0, 0, 0).AddTicks(-1L) - t - rule.DaylightDelta;
        }
      }
      else
      {
        dateTime2 = daylightTime.End - t - rule.DaylightDelta;
      }
      DateTime t2;
      DateTime t3;
      if (daylightTime.Delta.Ticks > 0L)
      {
        t2 = dateTime2 - daylightTime.Delta;
        t3 = dateTime2;
      }
      else
      {
        t2 = dateTime;
        t3 = dateTime - daylightTime.Delta;
      }
      bool flag = TimeZoneInfo.CheckIsDst(dateTime, time, dateTime2, ignoreYearAdjustment);
      if (flag)
      {
        isAmbiguousLocalDst = (time >= t2 && time < t3);
        if (!isAmbiguousLocalDst && t2.Year != t3.Year)
        {
          try
          {
            DateTime dateTime3 = t2.AddYears(1);
            DateTime dateTime4 = t3.AddYears(1);
            isAmbiguousLocalDst = (time >= t2 && time < t3);
          }
          catch (ArgumentOutOfRangeException)
          {
          }
          if (!isAmbiguousLocalDst)
          {
            try
            {
              DateTime dateTime3 = t2.AddYears(-1);
              DateTime dateTime4 = t3.AddYears(-1);
              isAmbiguousLocalDst = (time >= t2 && time < t3);
            }
            catch (ArgumentOutOfRangeException)
            {
            }
          }
        }
      }
      return flag;
    }

    private static bool CheckIsDst(DateTime startTime, DateTime time, DateTime endTime, bool ignoreYearAdjustment)
    {
      if (!ignoreYearAdjustment)
      {
        int year = startTime.Year;
        int year2 = endTime.Year;
        if (year != year2)
        {
          endTime = endTime.AddYears(year - year2);
        }
        int year3 = time.Year;
        if (year != year3)
        {
          time = time.AddYears(year - year3);
        }
      }
      bool result;
      if (startTime > endTime)
      {
        result = (time < endTime || time >= startTime);
      }
      else
      {
        result = (time >= startTime && time < endTime);
      }
      return result;
    }

    private static bool GetIsAmbiguousTime(DateTime time, TimeZoneInfo.AdjustmentRule rule, DaylightTime daylightTime)
    {
      bool flag = false;
      if (rule == null || rule.DaylightDelta == TimeSpan.Zero)
      {
        return flag;
      }
      DateTime t;
      DateTime t2;
      if (rule.DaylightDelta > TimeSpan.Zero)
      {
        if (rule.IsEndDateMarkerForEndOfYear())
        {
          return false;
        }
        t = daylightTime.End;
        t2 = daylightTime.End - rule.DaylightDelta;
      }
      else
      {
        if (rule.IsStartDateMarkerForBeginningOfYear())
        {
          return false;
        }
        t = daylightTime.Start;
        t2 = daylightTime.Start + rule.DaylightDelta;
      }
      flag = (time >= t2 && time < t);
      if (!flag && t.Year != t2.Year)
      {
        try
        {
          DateTime t3 = t.AddYears(1);
          DateTime t4 = t2.AddYears(1);
          flag = (time >= t4 && time < t3);
        }
        catch (ArgumentOutOfRangeException)
        {
        }
        if (!flag)
        {
          try
          {
            DateTime t3 = t.AddYears(-1);
            DateTime t4 = t2.AddYears(-1);
            flag = (time >= t4 && time < t3);
          }
          catch (ArgumentOutOfRangeException)
          {
          }
        }
      }
      return flag;
    }

    private static bool GetIsInvalidTime(DateTime time, TimeZoneInfo.AdjustmentRule rule, DaylightTime daylightTime)
    {
      bool flag = false;
      if (rule == null || rule.DaylightDelta == TimeSpan.Zero)
      {
        return flag;
      }
      DateTime t;
      DateTime t2;
      if (rule.DaylightDelta < TimeSpan.Zero)
      {
        if (rule.IsEndDateMarkerForEndOfYear())
        {
          return false;
        }
        t = daylightTime.End;
        t2 = daylightTime.End - rule.DaylightDelta;
      }
      else
      {
        if (rule.IsStartDateMarkerForBeginningOfYear())
        {
          return false;
        }
        t = daylightTime.Start;
        t2 = daylightTime.Start + rule.DaylightDelta;
      }
      flag = (time >= t && time < t2);
      if (!flag && t.Year != t2.Year)
      {
        try
        {
          DateTime t3 = t.AddYears(1);
          DateTime t4 = t2.AddYears(1);
          flag = (time >= t3 && time < t4);
        }
        catch (ArgumentOutOfRangeException)
        {
        }
        if (!flag)
        {
          try
          {
            DateTime t3 = t.AddYears(-1);
            DateTime t4 = t2.AddYears(-1);
            flag = (time >= t3 && time < t4);
          }
          catch (ArgumentOutOfRangeException)
          {
          }
        }
      }
      return flag;
    }

    ///// <summary>Retrieves a <see cref="T:System.TimeZoneInfo" /> object from the registry based on its identifier.</summary>
    ///// <returns>An object whose identifier is the value of the <paramref name="id" /> parameter.</returns>
    ///// <param name="id">The time zone identifier, which corresponds to the <see cref="P:System.TimeZoneInfo.Id" /> property.      </param>
    ///// <exception cref="T:System.OutOfMemoryException">The system does not have enough memory to hold information about the time zone.</exception>
    ///// <exception cref="T:System.ArgumentNullException">The <paramref name="id" /> parameter is null.</exception>
    ///// <exception cref="T:System.TimeZoneNotFoundException">The time zone identifier specified by <paramref name="id" /> was not found. This means that a registry key whose name matches <paramref name="id" /> does not exist, or that the key exists but does not contain any time zone data.</exception>
    ///// <exception cref="T:System.Security.SecurityException">The process does not have the permissions required to read from the registry key that contains the time zone information.</exception>
    ///// <exception cref="T:System.InvalidTimeZoneException">The time zone identifier was found, but the registry data is corrupted.</exception>
    //[__DynamicallyInvokable]
    //public static TimeZoneInfo FindSystemTimeZoneById(string id)
    //{
    //  if (string.Compare(id, "UTC", StringComparison.OrdinalIgnoreCase) == 0)
    //  {
    //    return TimeZoneInfo.Utc;
    //  }
    //  if (id == null)
    //  {
    //    throw new ArgumentNullException("id");
    //  }
    //  if (id.Length == 0 || id.Length > 255 || id.Contains("\0"))
    //  {
    //    throw new TimeZoneNotFoundException(Environment.GetResourceString("TimeZoneNotFound_MissingRegistryData", new object[]
    //    {
    //    id
    //    }));
    //  }
    //  TimeZoneInfo.CachedData cachedData = TimeZoneInfo.s_cachedData;
    //  TimeZoneInfo.CachedData obj = cachedData;
    //  TimeZoneInfo result;
    //  Exception ex;
    //  TimeZoneInfo.TimeZoneInfoResult timeZoneInfoResult;
    //  lock (obj)
    //  {
    //    timeZoneInfoResult = TimeZoneInfo.TryGetTimeZone(id, false, out result, out ex, cachedData);
    //  }
    //  if (timeZoneInfoResult == TimeZoneInfo.TimeZoneInfoResult.Success)
    //  {
    //    return result;
    //  }
    //  if (timeZoneInfoResult == TimeZoneInfo.TimeZoneInfoResult.InvalidTimeZoneException)
    //  {
    //    throw new InvalidTimeZoneException(Environment.GetResourceString("InvalidTimeZone_InvalidRegistryData", new object[]
    //    {
    //    id
    //    }), ex);
    //  }
    //  if (timeZoneInfoResult == TimeZoneInfo.TimeZoneInfoResult.SecurityException)
    //  {
    //    throw new SecurityException(Environment.GetResourceString("Security_CannotReadRegistryData", new object[]
    //    {
    //    id
    //    }), ex);
    //  }
    //  throw new TimeZoneNotFoundException(Environment.GetResourceString("TimeZoneNotFound_MissingRegistryData", new object[]
    //  {
    //  id
    //  }), ex);
    //}

    private static TimeSpan GetUtcOffset(DateTime time, TimeZoneInfo zone, TimeZoneInfoOptions flags)
    {
      TimeSpan timeSpan = zone.BaseUtcOffset;
      TimeZoneInfo.AdjustmentRule adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(time);
      if (adjustmentRuleForTime != null)
      {
        timeSpan += adjustmentRuleForTime.BaseUtcOffsetDelta;
        if (adjustmentRuleForTime.HasDaylightSaving)
        {
          DaylightTime daylightTime = TimeZoneInfo.GetDaylightTime(time.Year, adjustmentRuleForTime);
          bool isDaylightSavings = TimeZoneInfo.GetIsDaylightSavings(time, adjustmentRuleForTime, daylightTime, flags);
          timeSpan += (isDaylightSavings ? adjustmentRuleForTime.DaylightDelta : TimeSpan.Zero);
        }
      }
      return timeSpan;
    }

    private static TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone)
    {
      bool flag;
      return TimeZoneInfo.GetUtcOffsetFromUtc(time, zone, out flag);
    }

    private static TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone, out bool isDaylightSavings)
    {
      bool flag;
      return TimeZoneInfo.GetUtcOffsetFromUtc(time, zone, out isDaylightSavings, out flag);
    }

    internal static TimeSpan GetUtcOffsetFromUtc(DateTime time, TimeZoneInfo zone, out bool isDaylightSavings, out bool isAmbiguousLocalDst)
    {
      isDaylightSavings = false;
      isAmbiguousLocalDst = false;
      TimeSpan timeSpan = zone.BaseUtcOffset;
      TimeZoneInfo.AdjustmentRule adjustmentRuleForTime;
      int year;
      if (time > TimeZoneInfo.s_maxDateOnly)
      {
        adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(DateTime.MaxValue);
        year = 9999;
      }
      else if (time < TimeZoneInfo.s_minDateOnly)
      {
        adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(DateTime.MinValue);
        year = 1;
      }
      else
      {
        DateTime dateTime = time + timeSpan;
        year = dateTime.Year;
        adjustmentRuleForTime = zone.GetAdjustmentRuleForTime(dateTime);
      }
      if (adjustmentRuleForTime != null)
      {
        timeSpan += adjustmentRuleForTime.BaseUtcOffsetDelta;
        if (adjustmentRuleForTime.HasDaylightSaving)
        {
          isDaylightSavings = TimeZoneInfo.GetIsDaylightSavingsFromUtc(time, year, zone.m_baseUtcOffset, adjustmentRuleForTime, out isAmbiguousLocalDst, zone);
          timeSpan += (isDaylightSavings ? adjustmentRuleForTime.DaylightDelta : TimeSpan.Zero);
        }
      }
      return timeSpan;
    }

    private static DateTime TransitionTimeToDateTime(int year, TimeZoneInfo.TransitionTime transitionTime)
    {
      DateTime timeOfDay = transitionTime.TimeOfDay;
      DateTime result;
      if (transitionTime.IsFixedDateRule)
      {
        int num = DateTime.DaysInMonth(year, transitionTime.Month);
        result = new DateTime(year, transitionTime.Month, (num < transitionTime.Day) ? num : transitionTime.Day, timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
      }
      else if (transitionTime.Week <= 4)
      {
        result = new DateTime(year, transitionTime.Month, 1, timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
        int dayOfWeek = (int)result.DayOfWeek;
        int num2 = transitionTime.DayOfWeek - (DayOfWeek)dayOfWeek;
        if (num2 < 0)
        {
          num2 += 7;
        }
        num2 += 7 * (transitionTime.Week - 1);
        if (num2 > 0)
        {
          result = result.AddDays((double)num2);
        }
      }
      else
      {
        int day = DateTime.DaysInMonth(year, transitionTime.Month);
        result = new DateTime(year, transitionTime.Month, day, timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
        int dayOfWeek2 = (int)result.DayOfWeek;
        int num3 = dayOfWeek2 - (int)transitionTime.DayOfWeek;
        if (num3 < 0)
        {
          num3 += 7;
        }
        if (num3 > 0)
        {
          result = result.AddDays((double)(-(double)num3));
        }
      }
      return result;
    }

    internal static bool UtcOffsetOutOfRange(TimeSpan offset)
    {
      return offset.TotalHours < -14.0 || offset.TotalHours > 14.0;
    }

    private static void ValidateTimeZoneInfo(string id, TimeSpan baseUtcOffset, TimeZoneInfo.AdjustmentRule[] adjustmentRules, out bool adjustmentRulesSupportDst)
    {
      if (id == null)
      {
        throw new ArgumentNullException("id");
      }
      if (id.Length == 0)
      {
        throw new ArgumentException("Invalid id", "id");
      }
      if (TimeZoneInfo.UtcOffsetOutOfRange(baseUtcOffset))
      {
        throw new ArgumentOutOfRangeException("baseUtcOffset", "UTC Offset is outside of the valid range");
      }
      if (baseUtcOffset.Ticks % 600000000L != 0L)
      {
        throw new ArgumentException("TimeSpan has seconds", "baseUtcOffset");
      }
      adjustmentRulesSupportDst = false;
      if (adjustmentRules != null && adjustmentRules.Length != 0)
      {
        adjustmentRulesSupportDst = true;
        TimeZoneInfo.AdjustmentRule adjustmentRule = null;
        for (int i = 0; i < adjustmentRules.Length; i++)
        {
          TimeZoneInfo.AdjustmentRule adjustmentRule2 = adjustmentRule;
          adjustmentRule = adjustmentRules[i];
          if (adjustmentRule == null)
          {
            throw new InvalidTimeZoneException("Adjustment rules cannot contain nulls");
          }
          if (TimeZoneInfo.UtcOffsetOutOfRange(baseUtcOffset + adjustmentRule.DaylightDelta))
          {
            throw new InvalidTimeZoneException("UTC Offset and daylight delta are out of range");
          }
          if (adjustmentRule2 != null && adjustmentRule.DateStart <= adjustmentRule2.DateEnd)
          {
            throw new InvalidTimeZoneException("Adjustment rules are out of order");
          }
        }
      }
    }

    private string m_id;

    private string m_displayName;

    private string m_standardDisplayName;

    private string m_daylightDisplayName;

    private TimeSpan m_baseUtcOffset;

    private bool m_supportsDaylightSavingTime;

    private TimeZoneInfo.AdjustmentRule[] m_adjustmentRules;

    private const string c_displayValue = "Display";

    private const string c_daylightValue = "Dlt";

    private const string c_standardValue = "Std";

    private const string c_muiDisplayValue = "MUI_Display";

    private const string c_muiDaylightValue = "MUI_Dlt";

    private const string c_muiStandardValue = "MUI_Std";

    private const string c_timeZoneInfoValue = "TZI";

    private const string c_firstEntryValue = "FirstEntry";

    private const string c_lastEntryValue = "LastEntry";

    private const string c_utcId = "UTC";

    private const string c_localId = "Local";

    private const int c_maxKeyLength = 255;

    private const int c_regByteLength = 44;

    private const long c_ticksPerMillisecond = 10000L;

    private const long c_ticksPerSecond = 10000000L;

    private const long c_ticksPerMinute = 600000000L;

    private const long c_ticksPerHour = 36000000000L;

    private const long c_ticksPerDay = 864000000000L;

    private const long c_ticksPerDayRange = 863999990000L;

    private static TimeZoneInfo.CachedData s_cachedData = new TimeZoneInfo.CachedData();

    private static DateTime s_maxDateOnly = new DateTime(9999, 12, 31);

    private static DateTime s_minDateOnly = new DateTime(1, 1, 2);

    private enum TimeZoneInfoResult
    {
      Success,
      TimeZoneNotFoundException,
      InvalidTimeZoneException,
      SecurityException
    }

    public static TimeZoneInfo ByStandardName(string name)
    {
      return _cache[_standardNames[name]];
    }
    public static TimeZoneInfo ById(string name)
    {
      return _cache[name];
    }

    private class CachedData
    {
      private TimeZoneInfo CreateLocal()
      {
        TimeZoneInfo result;
        lock (this)
        {
          TimeZoneInfo timeZoneInfo = this.m_localTimeZone;
          if (timeZoneInfo == null)
          {
            timeZoneInfo = _cache[System.TimeZoneInfo.Local.StandardName];
            timeZoneInfo = new TimeZoneInfo(timeZoneInfo.m_id, timeZoneInfo.m_baseUtcOffset, timeZoneInfo.m_displayName, timeZoneInfo.m_standardDisplayName, timeZoneInfo.m_daylightDisplayName, timeZoneInfo.m_adjustmentRules, false);
            this.m_localTimeZone = timeZoneInfo;
          }
          result = timeZoneInfo;
        }
        return result;
      }

      public TimeZoneInfo Local
      {
        get
        {
          TimeZoneInfo timeZoneInfo = this.m_localTimeZone;
          if (timeZoneInfo == null)
          {
            timeZoneInfo = this.CreateLocal();
          }
          return timeZoneInfo;
        }
      }

      private TimeZoneInfo CreateUtc()
      {
        TimeZoneInfo result;
        lock (this)
        {
          TimeZoneInfo timeZoneInfo = this.m_utcTimeZone;
          if (timeZoneInfo == null)
          {
            timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone("UTC", TimeSpan.Zero, "UTC", "UTC");
            this.m_utcTimeZone = timeZoneInfo;
          }
          result = timeZoneInfo;
        }
        return result;
      }

      public TimeZoneInfo Utc
      {
        get
        {
          TimeZoneInfo timeZoneInfo = this.m_utcTimeZone;
          if (timeZoneInfo == null)
          {
            timeZoneInfo = this.CreateUtc();
          }
          return timeZoneInfo;
        }
      }

      public DateTimeKind GetCorrespondingKind(TimeZoneInfo timeZone)
      {
        DateTimeKind result;
        if (timeZone == this.m_utcTimeZone)
        {
          result = DateTimeKind.Utc;
        }
        else if (timeZone == this.m_localTimeZone)
        {
          result = DateTimeKind.Local;
        }
        else
        {
          result = DateTimeKind.Unspecified;
        }
        return result;
      }

      private volatile TimeZoneInfo m_localTimeZone;

      private volatile TimeZoneInfo m_utcTimeZone;
    }

    private class OffsetAndRule
    {
      public OffsetAndRule(int year, TimeSpan offset, TimeZoneInfo.AdjustmentRule rule)
      {
        this.year = year;
        this.offset = offset;
        this.rule = rule;
      }

      public int year;

      public TimeSpan offset;

      public TimeZoneInfo.AdjustmentRule rule;
    }

    /// <summary>Provides information about a time zone adjustment, such as the transition to and from daylight saving time.</summary>
    /// <filterpriority>2</filterpriority>
    public sealed class AdjustmentRule : IEquatable<AdjustmentRule>
    {
      private TimeSpan _baseUtcOffsetDelta;
      private DateTime _dateEnd;
      private DateTime _dateStart;
      private TimeSpan _daylightDelta;
      private TransitionTime _daylightTransitionEnd;
      private TransitionTime _daylightTransitionStart;

      /// <summary>Gets the date when the adjustment rule takes effect.</summary>
      /// <returns>A <see cref="T:System.DateTime" /> value that indicates when the adjustment rule takes effect.</returns>
      public DateTime DateStart { get { return this._dateStart; } }

      /// <summary>Gets the date when the adjustment rule ceases to be in effect.</summary>
      /// <returns>A <see cref="T:System.DateTime" /> value that indicates the end date of the adjustment rule.</returns>
      public DateTime DateEnd { get { return this._dateEnd; } }

      /// <summary>Gets the amount of time that is required to form the time zone's daylight saving time. This amount of time is added to the time zone's offset from Coordinated Universal Time (UTC).</summary>
      /// <returns>A <see cref="T:System.TimeSpan" /> object that indicates the amount of time to add to the standard time changes as a result of the adjustment rule.</returns>
      public TimeSpan DaylightDelta { get { return this._daylightDelta; } }

      /// <summary>Gets information about the annual transition from standard time to daylight saving time.</summary>
      /// <returns>A <see cref="T:TransitionTime" /> object that defines the annual transition from a time zone's standard time to daylight saving time.</returns>
      public TransitionTime DaylightTransitionStart { get { return this._daylightTransitionStart; } }

      /// <summary>Gets information about the annual transition from daylight saving time back to standard time.</summary>
      /// <returns>A <see cref="T:TransitionTime" /> object that defines the annual transition from daylight saving time back to the time zone's standard time.</returns>
      public TransitionTime DaylightTransitionEnd { get { return this._daylightTransitionEnd; } }

      internal TimeSpan BaseUtcOffsetDelta { get { return this._baseUtcOffsetDelta; } }

      internal bool HasDaylightSaving
      {
        get
        {
          return this.DaylightDelta != TimeSpan.Zero
            || this.DaylightTransitionStart.TimeOfDay != DateTime.MinValue
            || this.DaylightTransitionEnd.TimeOfDay != DateTime.MinValue.AddMilliseconds(1.0);
        }
      }

      /// <summary>Determines whether the current <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> object is equal to a second <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> object.</summary>
      /// <returns>true if both <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> objects have equal values; otherwise, false.</returns>
      /// <param name="other">The object to compare with the current object.</param>
      /// <filterpriority>2</filterpriority>
      public bool Equals(AdjustmentRule other)
      {
        var flag = other != null
          && this._dateStart == other._dateStart
          && this._dateEnd == other._dateEnd
          && this._daylightDelta == other._daylightDelta
          && this._baseUtcOffsetDelta == other._baseUtcOffsetDelta;
        return flag
          && this._daylightTransitionEnd.Equals(other._daylightTransitionEnd)
          && this._daylightTransitionStart.Equals(other._daylightTransitionStart);
      }

      /// <summary>Serves as a hash function for hashing algorithms and data structures such as hash tables.</summary>
      /// <returns>A 32-bit signed integer that serves as the hash code for the current <see cref="T:System.TimeZoneInfo.AdjustmentRule" /> object.</returns>
      /// <filterpriority>2</filterpriority>
      public override int GetHashCode()
      {
        return this._dateStart.GetHashCode();
      }

      private AdjustmentRule()
      {
      }

      /// <summary>Creates a new adjustment rule for a particular time zone.</summary>
      /// <returns>An object that represents the new adjustment rule.</returns>
      /// <param name="dateStart">The effective date of the adjustment rule. If the value of the <paramref name="dateStart" /> parameter is DateTime.MinValue.Date, this is the first adjustment rule in effect for a time zone.   </param>
      /// <param name="dateEnd">The last date that the adjustment rule is in force. If the value of the <paramref name="dateEnd" /> parameter is DateTime.MaxValue.Date, the adjustment rule has no end date.</param>
      /// <param name="daylightDelta">The time change that results from the adjustment. This value is added to the time zone's <see cref="P:System.TimeZoneInfo.BaseUtcOffset" /> property to obtain the correct daylight offset from Coordinated Universal Time (UTC). This value can range from -14 to 14. </param>
      /// <param name="daylightTransitionStart">An object that defines the start of daylight saving time.</param>
      /// <param name="daylightTransitionEnd">An object that defines the end of daylight saving time.   </param>
      /// <exception cref="T:System.ArgumentException">The <see cref="P:System.DateTime.Kind" /> property of the <paramref name="dateStart" /> or <paramref name="dateEnd" /> parameter does not equal <see cref="F:System.DateTimeKind.Unspecified" />.-or-The <paramref name="daylightTransitionStart" /> parameter is equal to the <paramref name="daylightTransitionEnd" /> parameter.-or-The <paramref name="dateStart" /> or <paramref name="dateEnd" /> parameter includes a time of day value.</exception>
      /// <exception cref="T:System.ArgumentOutOfRangeException">
      ///   <paramref name="dateEnd" /> is earlier than <paramref name="dateStart" />.-or-<paramref name="daylightDelta" /> is less than -14 or greater than 14.-or-The <see cref="P:System.TimeSpan.Milliseconds" /> property of the <paramref name="daylightDelta" /> parameter is not equal to 0.-or-The <see cref="P:System.TimeSpan.Ticks" /> property of the <paramref name="daylightDelta" /> parameter does not equal a whole number of seconds.</exception>
      public static AdjustmentRule CreateAdjustmentRule(DateTime dateStart, DateTime dateEnd, TimeSpan daylightDelta, TransitionTime daylightTransitionStart, TransitionTime daylightTransitionEnd)
      {
        AdjustmentRule.ValidateAdjustmentRule(dateStart, dateEnd, daylightDelta, daylightTransitionStart, daylightTransitionEnd);
        return new AdjustmentRule
        {
          _dateStart = dateStart,
          _dateEnd = dateEnd,
          _daylightDelta = daylightDelta,
          _daylightTransitionStart = daylightTransitionStart,
          _daylightTransitionEnd = daylightTransitionEnd,
          _baseUtcOffsetDelta = TimeSpan.Zero
        };
      }

      internal static AdjustmentRule CreateAdjustmentRule(DateTime dateStart, DateTime dateEnd, TimeSpan daylightDelta, TransitionTime daylightTransitionStart, TransitionTime daylightTransitionEnd, TimeSpan baseUtcOffsetDelta)
      {
        AdjustmentRule adjustmentRule = AdjustmentRule.CreateAdjustmentRule(dateStart, dateEnd, daylightDelta, daylightTransitionStart, daylightTransitionEnd);
        adjustmentRule._baseUtcOffsetDelta = baseUtcOffsetDelta;
        return adjustmentRule;
      }

      internal bool IsStartDateMarkerForBeginningOfYear()
      {
        return this.DaylightTransitionStart.Month == 1 && this.DaylightTransitionStart.Day == 1 && this.DaylightTransitionStart.TimeOfDay.Hour == 0 && this.DaylightTransitionStart.TimeOfDay.Minute == 0 && this.DaylightTransitionStart.TimeOfDay.Second == 0 && this._dateStart.Year == this._dateEnd.Year;
      }

      internal bool IsEndDateMarkerForEndOfYear()
      {
        return this.DaylightTransitionEnd.Month == 1 && this.DaylightTransitionEnd.Day == 1 && this.DaylightTransitionEnd.TimeOfDay.Hour == 0 && this.DaylightTransitionEnd.TimeOfDay.Minute == 0 && this.DaylightTransitionEnd.TimeOfDay.Second == 0 && this._dateStart.Year == this._dateEnd.Year;
      }

      private static void ValidateAdjustmentRule(DateTime dateStart, DateTime dateEnd, TimeSpan daylightDelta, TransitionTime daylightTransitionStart, TransitionTime daylightTransitionEnd)
      {
        if (dateStart.Kind != DateTimeKind.Unspecified)
          throw new ArgumentException("DateTime Kind must be unspecified", "dateStart");
        if (dateEnd.Kind != DateTimeKind.Unspecified)
          throw new ArgumentException("DateTime Kind must be unspecified", "dateEnd");

        //if (daylightTransitionStart.Equals(daylightTransitionEnd))
        //  throw new ArgumentException("Transition times are identical", "daylightTransitionEnd");

        if (dateStart > dateEnd)
          throw new ArgumentException("Dates are out of order", "dateStart");
        if (UtcOffsetOutOfRange(daylightDelta))
          throw new ArgumentOutOfRangeException("daylightDelta", daylightDelta, "UTC offset is outside of the range");

        if (daylightDelta.Ticks % 600000000L != 0L)
          throw new ArgumentException("Timespan cannot have seconds", "daylightDelta");
        if (dateStart.TimeOfDay != TimeSpan.Zero)
          throw new ArgumentException("Date cannot have a time of day", "dateStart");
        if (dateEnd.TimeOfDay != TimeSpan.Zero)
          throw new ArgumentException("Date cannot have a time of day", "dateEnd");
      }

      internal static bool UtcOffsetOutOfRange(TimeSpan offset)
      {
        return offset.TotalHours < -14.0 || offset.TotalHours > 14.0;
      }
    }

    /// <summary>Provides information about a specific time change, such as the change from daylight saving time to standard time or vice versa, in a particular time zone.</summary>
    /// <filterpriority>2</filterpriority>
    public struct TransitionTime : IEquatable<TransitionTime>
    {
      private byte _day;
      private DayOfWeek _dayOfWeek;
      private bool _isFixedDateRule;
      private byte _month;
      private DateTime _timeOfDay;
      private byte _week;

      /// <summary>Gets the hour, minute, and second at which the time change occurs.</summary>
      /// <returns>The time of day at which the time change occurs.</returns>
      public DateTime TimeOfDay { get { return this._timeOfDay; } }

      /// <summary>Gets the month in which the time change occurs.</summary>
      /// <returns>The month in which the time change occurs.</returns>
      public int Month { get { return (int)this._month; } }

      /// <summary>Gets the week of the month in which a time change occurs.</summary>
      /// <returns>The week of the month in which the time change occurs.</returns>
      public int Week { get { return (int)this._week; } }

      /// <summary>Gets the day on which the time change occurs.</summary>
      /// <returns>The day on which the time change occurs.</returns>
      public int Day { get { return (int)this._day; } }

      /// <summary>Gets the day of the week on which the time change occurs.</summary>
      /// <returns>The day of the week on which the time change occurs.</returns>
      public DayOfWeek DayOfWeek { get { return this._dayOfWeek; } }

      /// <summary>Gets a value indicating whether the time change occurs at a fixed date and time (such as November 1) or a floating date and time (such as the last Sunday of October).</summary>
      /// <returns>true if the time change rule is fixed-date; false if the time change rule is floating-date.</returns>
      public bool IsFixedDateRule { get { return this._isFixedDateRule; } }

      /// <summary>Determines whether an object has identical values to the current <see cref="T:System.TimeZoneInfo.TransitionTime" /> object.</summary>
      /// <returns>true if the two objects are equal; otherwise, false.</returns>
      /// <param name="obj">An object to compare with the current <see cref="T:System.TimeZoneInfo.TransitionTime" /> object.   </param>
      /// <filterpriority>2</filterpriority>
      public override bool Equals(object obj)
      {
        return obj is TransitionTime && this.Equals((TransitionTime)obj);
      }

      /// <summary>Determines whether two specified <see cref="T:System.TimeZoneInfo.TransitionTime" /> objects are equal.</summary>
      /// <returns>true if <paramref name="t1" /> and <paramref name="t2" /> have identical values; otherwise, false. </returns>
      /// <param name="t1">The first object to compare.</param>
      /// <param name="t2">The second object to compare.</param>
      public static bool operator ==(TransitionTime t1, TransitionTime t2)
      {
        return t1.Equals(t2);
      }

      /// <summary>Determines whether two specified <see cref="T:System.TimeZoneInfo.TransitionTime" /> objects are not equal.</summary>
      /// <returns>true if <paramref name="t1" /> and <paramref name="t2" /> have any different member values; otherwise, false.</returns>
      /// <param name="t1">The first object to compare.</param>
      /// <param name="t2">The second object to compare.</param>
      public static bool operator !=(TransitionTime t1, TransitionTime t2)
      {
        return !t1.Equals(t2);
      }

      /// <summary>Determines whether the current <see cref="T:System.TimeZoneInfo.TransitionTime" /> object has identical values to a second <see cref="T:System.TimeZoneInfo.TransitionTime" /> object.</summary>
      /// <returns>true if the two objects have identical property values; otherwise, false.</returns>
      /// <param name="other">An object to compare to the current instance. </param>
      /// <filterpriority>2</filterpriority>
      public bool Equals(TransitionTime other)
      {
        var flag = this._isFixedDateRule == other._isFixedDateRule
          && this._timeOfDay == other._timeOfDay
          && this._month == other._month;
        if (flag)
        {
          if (other._isFixedDateRule)
          {
            flag = (this._day == other._day);
          }
          else
          {
            flag = (this._week == other._week && this._dayOfWeek == other._dayOfWeek);
          }
        }
        return flag;
      }

      /// <summary>Serves as a hash function for hashing algorithms and data structures such as hash tables.</summary>
      /// <returns>A 32-bit signed integer that serves as the hash code for this <see cref="T:System.TimeZoneInfo.TransitionTime" /> object.</returns>
      /// <filterpriority>2</filterpriority>
      public override int GetHashCode()
      {
        return (int)this._month ^ (int)this._week << 8;
      }

      /// <summary>Defines a time change that uses a fixed-date rule (that is, a time change that occurs on a specific day of a specific month). </summary>
      /// <returns>Data about the time change.</returns>
      /// <param name="timeOfDay">The time at which the time change occurs. This parameter corresponds to the <see cref="P:System.TimeZoneInfo.TransitionTime.TimeOfDay" /> property. For details, see Remarks.</param>
      /// <param name="month">The month in which the time change occurs. This parameter corresponds to the <see cref="P:System.TimeZoneInfo.TransitionTime.Month" /> property. </param>
      /// <param name="day">The day of the month on which the time change occurs. This parameter corresponds to the <see cref="P:System.TimeZoneInfo.TransitionTime.Day" /> property. </param>
      /// <exception cref="T:System.ArgumentException">The <paramref name="timeOfDay" /> parameter has a non-default date component.-or-The <paramref name="timeOfDay" /> parameter's <see cref="P:System.DateTime.Kind" /> property is not <see cref="F:System.DateTimeKind.Unspecified" />.-or-The <paramref name="timeOfDay" /> parameter does not represent a whole number of milliseconds.</exception>
      /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="month" /> parameter is less than 1 or greater than 12.-or-The <paramref name="day" /> parameter is less than 1 or greater than 31.</exception>
      public static TransitionTime CreateFixedDateRule(DateTime timeOfDay, int month, int day)
      {
        return CreateTransitionTime(timeOfDay, month, 1, day, DayOfWeek.Sunday, true);
      }

      /// <summary>Defines a time change that uses a floating-date rule (that is, a time change that occurs on a specific day of a specific week of a specific month). </summary>
      /// <returns>Data about the time change.</returns>
      /// <param name="timeOfDay">The time at which the time change occurs. This parameter corresponds to the <see cref="P:System.TimeZoneInfo.TransitionTime.TimeOfDay" /> property. For details, see Remarks.</param>
      /// <param name="month">The month in which the time change occurs. This parameter corresponds to the <see cref="P:System.TimeZoneInfo.TransitionTime.Month" /> property. </param>
      /// <param name="week">The week of the month in which the time change occurs. Its value can range from 1 to 5, with 5 representing the last week of the month. This parameter corresponds to the <see cref="P:System.TimeZoneInfo.TransitionTime.Week" /> property. </param>
      /// <param name="dayOfWeek">The day of the week on which the time change occurs. This parameter corresponds to the <see cref="P:System.TimeZoneInfo.TransitionTime.DayOfWeek" /> property. </param>
      /// <exception cref="T:System.ArgumentException">The <paramref name="timeOfDay" /> parameter has a non-default date component.-or-The <paramref name="timeOfDay" /> parameter does not represent a whole number of milliseconds.-or-The <paramref name="timeOfDay" /> parameter's <see cref="P:System.DateTime.Kind" /> property is not <see cref="F:System.DateTimeKind.Unspecified" />.</exception>
      /// <exception cref="T:System.ArgumentOutOfRangeException">
      ///   <paramref name="month" /> is less than 1 or greater than 12.-or-<paramref name="week" /> is less than 1 or greater than 5.-or-The <paramref name="dayOfWeek" /> parameter is not a member of the <see cref="T:System.DayOfWeek" /> enumeration.</exception>
      public static TransitionTime CreateFloatingDateRule(DateTime timeOfDay, int month, int week, DayOfWeek dayOfWeek)
      {
        return CreateTransitionTime(timeOfDay, month, week, 1, dayOfWeek, false);
      }

      private static TransitionTime CreateTransitionTime(DateTime timeOfDay, int month, int week, int day, DayOfWeek dayOfWeek, bool isFixedDateRule)
      {
        ValidateTransitionTime(timeOfDay, month, week, day, dayOfWeek);
        return new TransitionTime
        {
          _isFixedDateRule = isFixedDateRule,
          _timeOfDay = timeOfDay,
          _dayOfWeek = dayOfWeek,
          _day = (byte)day,
          _week = (byte)week,
          _month = (byte)month
        };
      }

      private static void ValidateTransitionTime(DateTime timeOfDay, int month, int week, int day, DayOfWeek dayOfWeek)
      {
        if (timeOfDay.Kind != DateTimeKind.Unspecified)
          throw new ArgumentException("DateTime Kind must be unspecified", "timeOfDay");

        if (month < 1 || month > 12)
          throw new ArgumentOutOfRangeException("month");
        if (day < 1 || day > 31)
          throw new ArgumentOutOfRangeException("day");
        if (week < 1 || week > 5)
          throw new ArgumentOutOfRangeException("week");
        if (dayOfWeek < DayOfWeek.Sunday || dayOfWeek > DayOfWeek.Saturday)
          throw new ArgumentOutOfRangeException("dayOfWeek");
        if (timeOfDay.Year != 1 || timeOfDay.Month != 1 || timeOfDay.Day != 1 || timeOfDay.Ticks % 10000L != 0L)
          throw new ArgumentException("DateTime cannot have ticks", "timeOfDay");
      }

      public DateTime ToDateTime(int year)
      {
        var timeOfDay = TimeOfDay;
        DateTime result;
        if (IsFixedDateRule)
        {
          var num = DateTime.DaysInMonth(year, Month);
          result = new DateTime(year, Month, (num < Day) ? num : Day
            , timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
        }
        else if (Week <= 4)
        {
          result = new DateTime(year, Month, 1
            , timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
          var days = (int)DayOfWeek - (int)result.DayOfWeek;
          if (days < 0)
            days += 7;
          days += 7 * (Week - 1);
          if (days > 0)
            result = result.AddDays((double)days);
        }
        else
        {
          var day = DateTime.DaysInMonth(year, Month);
          result = new DateTime(year, Month, day
            , timeOfDay.Hour, timeOfDay.Minute, timeOfDay.Second, timeOfDay.Millisecond);
          var days = (int)result.DayOfWeek - (int)DayOfWeek;
          if (days < 0)
            days += 7;
          if (days > 0)
            result = result.AddDays((double)(-(double)days));
        }
        return result;
      }
    }

    private class TimeZoneInfoComparer : IComparer<TimeZoneInfo>
    {
      int IComparer<TimeZoneInfo>.Compare(TimeZoneInfo x, TimeZoneInfo y)
      {
        int num = x.BaseUtcOffset.CompareTo(y.BaseUtcOffset);
        if (num != 0)
        {
          return num;
        }
        return string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
      }
    }
  }

  /// <summary>Defines the period of daylight saving time.</summary>
  public class DaylightTime
  {
    internal TimeSpan _delta;
    internal DateTime _end;
    internal DateTime _start;

    private DaylightTime()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DaylightTime" /> class with the specified start, end, and time difference information.</summary>
    /// <param name="start">The object that represents the date and time when daylight saving time begins. The value must be in local time. </param>
    /// <param name="end">The object that represents the date and time when daylight saving time ends. The value must be in local time. </param>
    /// <param name="delta">The object that represents the difference between standard time and daylight saving time, in ticks. </param>
    public DaylightTime(DateTime start, DateTime end, TimeSpan delta)
    {
      this._start = start;
      this._end = end;
      this._delta = delta;
    }

    /// <summary>Gets the object that represents the date and time when the daylight saving period begins.</summary>
    /// <returns>The object that represents the date and time when the daylight saving period begins. The value is in local time.</returns>
    public DateTime Start { get { return this._start; } }

    /// <summary>Gets the object that represents the date and time when the daylight saving period ends.</summary>
    /// <returns>The object that represents the date and time when the daylight saving period ends. The value is in local time.</returns>
    public DateTime End { get { return this._end; } }

    /// <summary>Gets the time interval that represents the difference between standard time and daylight saving time.</summary>
    /// <returns>The time interval that represents the difference between standard time and daylight saving time.</returns>
    public TimeSpan Delta { get { return this._delta; } }
  }

  [Flags]
  internal enum TimeZoneInfoOptions
  {
    None = 1,
    NoThrowOnInvalidTime = 2
  }
}
#endif
