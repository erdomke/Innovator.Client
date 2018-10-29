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
      get { return IanaNameToWindows(_timeZone.Id); }
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

    private static string IanaNameToWindows(string ianaTimeZoneName)
    {
      switch (ianaTimeZoneName?.ToLowerInvariant())
      {
        case "africa/abidjan": return "Greenwich Standard Time";
        case "africa/accra": return "Greenwich Standard Time";
        case "africa/algiers": return "W. Central Africa Standard Time";
        case "africa/bissau": return "Greenwich Standard Time";
        case "africa/cairo": return "Egypt Standard Time";
        case "africa/casablanca": return "Morocco Standard Time";
        case "africa/ceuta": return "Romance Standard Time";
        case "africa/el_aaiun": return "Morocco Standard Time";
        case "africa/johannesburg": return "South Africa Standard Time";
        case "africa/juba": return "E. Africa Standard Time";
        case "africa/khartoum": return "Sudan Standard Time";
        case "africa/lagos": return "W. Central Africa Standard Time";
        case "africa/maputo": return "South Africa Standard Time";
        case "africa/monrovia": return "Greenwich Standard Time";
        case "africa/nairobi": return "E. Africa Standard Time";
        case "africa/ndjamena": return "W. Central Africa Standard Time";
        case "africa/sao_tome": return "Sao Tome Standard Time";
        case "africa/tripoli": return "Libya Standard Time";
        case "africa/tunis": return "W. Central Africa Standard Time";
        case "africa/windhoek": return "Namibia Standard Time";
        case "america/adak": return "Aleutian Standard Time";
        case "america/anchorage": return "Alaskan Standard Time";
        case "america/araguaina": return "Tocantins Standard Time";
        case "america/argentina/buenos_aires": return "Argentina Standard Time";
        case "america/argentina/catamarca": return "Argentina Standard Time";
        case "america/argentina/cordoba": return "Argentina Standard Time";
        case "america/argentina/jujuy": return "Argentina Standard Time";
        case "america/argentina/la_rioja": return "Argentina Standard Time";
        case "america/argentina/mendoza": return "Argentina Standard Time";
        case "america/argentina/rio_gallegos": return "Argentina Standard Time";
        case "america/argentina/salta": return "Argentina Standard Time";
        case "america/argentina/san_juan": return "Argentina Standard Time";
        case "america/argentina/san_luis": return "Argentina Standard Time";
        case "america/argentina/tucuman": return "Argentina Standard Time";
        case "america/argentina/ushuaia": return "Argentina Standard Time";
        case "america/asuncion": return "Paraguay Standard Time";
        case "america/atikokan": return "SA Pacific Standard Time";
        case "america/bahia": return "Bahia Standard Time";
        case "america/bahia_banderas": return "Central Standard Time (Mexico)";
        case "america/barbados": return "SA Western Standard Time";
        case "america/belem": return "SA Eastern Standard Time";
        case "america/belize": return "Central America Standard Time";
        case "america/blanc-sablon": return "SA Western Standard Time";
        case "america/boa_vista": return "SA Western Standard Time";
        case "america/bogota": return "SA Pacific Standard Time";
        case "america/boise": return "Mountain Standard Time";
        case "america/cambridge_bay": return "Mountain Standard Time";
        case "america/campo_grande": return "Central Brazilian Standard Time";
        case "america/cancun": return "Eastern Standard Time (Mexico)";
        case "america/caracas": return "Venezuela Standard Time";
        case "america/cayenne": return "SA Eastern Standard Time";
        case "america/chicago": return "Central Standard Time";
        case "america/chihuahua": return "Mountain Standard Time (Mexico)";
        case "america/costa_rica": return "Central America Standard Time";
        case "america/creston": return "US Mountain Standard Time";
        case "america/cuiaba": return "Central Brazilian Standard Time";
        case "america/curacao": return "SA Western Standard Time";
        case "america/danmarkshavn": return "UTC";
        case "america/dawson": return "Pacific Standard Time";
        case "america/dawson_creek": return "US Mountain Standard Time";
        case "america/denver": return "Mountain Standard Time";
        case "america/detroit": return "Eastern Standard Time";
        case "america/edmonton": return "Mountain Standard Time";
        case "america/eirunepe": return "SA Pacific Standard Time";
        case "america/el_salvador": return "Central America Standard Time";
        case "america/fort_nelson": return "US Mountain Standard Time";
        case "america/fortaleza": return "SA Eastern Standard Time";
        case "america/glace_bay": return "Atlantic Standard Time";
        case "america/godthab": return "Greenland Standard Time";
        case "america/goose_bay": return "Atlantic Standard Time";
        case "america/grand_turk": return "Turks And Caicos Standard Time";
        case "america/guatemala": return "Central America Standard Time";
        case "america/guayaquil": return "SA Pacific Standard Time";
        case "america/guyana": return "SA Western Standard Time";
        case "america/halifax": return "Atlantic Standard Time";
        case "america/havana": return "Cuba Standard Time";
        case "america/hermosillo": return "US Mountain Standard Time";
        case "america/indiana/indianapolis": return "US Eastern Standard Time";
        case "america/indiana/knox": return "Central Standard Time";
        case "america/indiana/marengo": return "US Eastern Standard Time";
        case "america/indiana/petersburg": return "Eastern Standard Time";
        case "america/indiana/tell_city": return "Central Standard Time";
        case "america/indiana/vevay": return "US Eastern Standard Time";
        case "america/indiana/vincennes": return "Eastern Standard Time";
        case "america/indiana/winamac": return "Eastern Standard Time";
        case "america/inuvik": return "Mountain Standard Time";
        case "america/iqaluit": return "Eastern Standard Time";
        case "america/jamaica": return "SA Pacific Standard Time";
        case "america/juneau": return "Alaskan Standard Time";
        case "america/kentucky/louisville": return "Eastern Standard Time";
        case "america/kentucky/monticello": return "Eastern Standard Time";
        case "america/la_paz": return "SA Western Standard Time";
        case "america/lima": return "SA Pacific Standard Time";
        case "america/los_angeles": return "Pacific Standard Time";
        case "america/maceio": return "SA Eastern Standard Time";
        case "america/managua": return "Central America Standard Time";
        case "america/manaus": return "SA Western Standard Time";
        case "america/martinique": return "SA Western Standard Time";
        case "america/matamoros": return "Central Standard Time";
        case "america/mazatlan": return "Mountain Standard Time (Mexico)";
        case "america/menominee": return "Central Standard Time";
        case "america/merida": return "Central Standard Time (Mexico)";
        case "america/metlakatla": return "Alaskan Standard Time";
        case "america/mexico_city": return "Central Standard Time (Mexico)";
        case "america/miquelon": return "Saint Pierre Standard Time";
        case "america/moncton": return "Atlantic Standard Time";
        case "america/monterrey": return "Central Standard Time (Mexico)";
        case "america/montevideo": return "Montevideo Standard Time";
        case "america/nassau": return "Eastern Standard Time";
        case "america/new_york": return "Eastern Standard Time";
        case "america/nipigon": return "Eastern Standard Time";
        case "america/nome": return "Alaskan Standard Time";
        case "america/noronha": return "UTC-02";
        case "america/north_dakota/beulah": return "Central Standard Time";
        case "america/north_dakota/center": return "Central Standard Time";
        case "america/north_dakota/new_salem": return "Central Standard Time";
        case "america/ojinaga": return "Mountain Standard Time";
        case "america/panama": return "SA Pacific Standard Time";
        case "america/pangnirtung": return "Eastern Standard Time";
        case "america/paramaribo": return "SA Eastern Standard Time";
        case "america/phoenix": return "US Mountain Standard Time";
        case "america/port_of_spain": return "SA Western Standard Time";
        case "america/port-au-prince": return "Haiti Standard Time";
        case "america/porto_velho": return "SA Western Standard Time";
        case "america/puerto_rico": return "SA Western Standard Time";
        case "america/punta_arenas": return "Magallanes Standard Time";
        case "america/rainy_river": return "Central Standard Time";
        case "america/rankin_inlet": return "Central Standard Time";
        case "america/recife": return "SA Eastern Standard Time";
        case "america/regina": return "Canada Central Standard Time";
        case "america/resolute": return "Central Standard Time";
        case "america/rio_branco": return "SA Pacific Standard Time";
        case "america/santarem": return "SA Eastern Standard Time";
        case "america/santiago": return "Pacific SA Standard Time";
        case "america/santo_domingo": return "SA Western Standard Time";
        case "america/sao_paulo": return "E. South America Standard Time";
        case "america/scoresbysund": return "Azores Standard Time";
        case "america/sitka": return "Alaskan Standard Time";
        case "america/st_johns": return "Newfoundland Standard Time";
        case "america/swift_current": return "Canada Central Standard Time";
        case "america/tegucigalpa": return "Central America Standard Time";
        case "america/thule": return "Atlantic Standard Time";
        case "america/thunder_bay": return "Eastern Standard Time";
        case "america/tijuana": return "Pacific Standard Time (Mexico)";
        case "america/toronto": return "Eastern Standard Time";
        case "america/vancouver": return "Pacific Standard Time";
        case "america/whitehorse": return "Pacific Standard Time";
        case "america/winnipeg": return "Central Standard Time";
        case "america/yakutat": return "Alaskan Standard Time";
        case "america/yellowknife": return "Mountain Standard Time";
        case "antarctica/casey": return "W. Australia Standard Time";
        case "antarctica/davis": return "SE Asia Standard Time";
        case "antarctica/dumontdurville": return "West Pacific Standard Time";
        case "antarctica/macquarie": return "Central Pacific Standard Time";
        case "antarctica/mawson": return "West Asia Standard Time";
        case "antarctica/palmer": return "Magallanes Standard Time";
        case "antarctica/rothera": return "SA Eastern Standard Time";
        case "antarctica/syowa": return "E. Africa Standard Time";
        case "antarctica/vostok": return "Central Asia Standard Time";
        case "asia/almaty": return "Central Asia Standard Time";
        case "asia/amman": return "Jordan Standard Time";
        case "asia/anadyr": return "Russia Time Zone 11";
        case "asia/aqtau": return "West Asia Standard Time";
        case "asia/aqtobe": return "West Asia Standard Time";
        case "asia/ashgabat": return "West Asia Standard Time";
        case "asia/atyrau": return "West Asia Standard Time";
        case "asia/baghdad": return "Arabic Standard Time";
        case "asia/baku": return "Azerbaijan Standard Time";
        case "asia/bangkok": return "SE Asia Standard Time";
        case "asia/barnaul": return "Altai Standard Time";
        case "asia/beirut": return "Middle East Standard Time";
        case "asia/bishkek": return "Central Asia Standard Time";
        case "asia/brunei": return "Singapore Standard Time";
        case "asia/chita": return "Transbaikal Standard Time";
        case "asia/choibalsan": return "Ulaanbaatar Standard Time";
        case "asia/colombo": return "Sri Lanka Standard Time";
        case "asia/damascus": return "Syria Standard Time";
        case "asia/dhaka": return "Bangladesh Standard Time";
        case "asia/dili": return "Tokyo Standard Time";
        case "asia/dubai": return "Arabian Standard Time";
        case "asia/dushanbe": return "West Asia Standard Time";
        case "asia/famagusta": return "GTB Standard Time";
        case "asia/gaza": return "West Bank Standard Time";
        case "asia/hebron": return "West Bank Standard Time";
        case "asia/ho_chi_minh": return "SE Asia Standard Time";
        case "asia/hong_kong": return "China Standard Time";
        case "asia/hovd": return "W. Mongolia Standard Time";
        case "asia/irkutsk": return "North Asia East Standard Time";
        case "asia/jakarta": return "SE Asia Standard Time";
        case "asia/jayapura": return "Tokyo Standard Time";
        case "asia/jerusalem": return "Israel Standard Time";
        case "asia/kabul": return "Afghanistan Standard Time";
        case "asia/kamchatka": return "Kamchatka Standard Time";
        case "asia/karachi": return "Pakistan Standard Time";
        case "asia/kathmandu": return "Nepal Standard Time";
        case "asia/khandyga": return "Yakutsk Standard Time";
        case "asia/kolkata": return "India Standard Time";
        case "asia/krasnoyarsk": return "North Asia Standard Time";
        case "asia/kuala_lumpur": return "Singapore Standard Time";
        case "asia/kuching": return "Singapore Standard Time";
        case "asia/macau": return "China Standard Time";
        case "asia/magadan": return "Magadan Standard Time";
        case "asia/makassar": return "Singapore Standard Time";
        case "asia/manila": return "Singapore Standard Time";
        case "asia/nicosia": return "GTB Standard Time";
        case "asia/novokuznetsk": return "North Asia Standard Time";
        case "asia/novosibirsk": return "N. Central Asia Standard Time";
        case "asia/omsk": return "Omsk Standard Time";
        case "asia/oral": return "West Asia Standard Time";
        case "asia/pontianak": return "SE Asia Standard Time";
        case "asia/pyongyang": return "North Korea Standard Time";
        case "asia/qatar": return "Arab Standard Time";
        case "asia/qyzylorda": return "Central Asia Standard Time";
        case "asia/riyadh": return "Arab Standard Time";
        case "asia/sakhalin": return "Sakhalin Standard Time";
        case "asia/samarkand": return "West Asia Standard Time";
        case "asia/seoul": return "Korea Standard Time";
        case "asia/shanghai": return "China Standard Time";
        case "asia/singapore": return "Singapore Standard Time";
        case "asia/srednekolymsk": return "Russia Time Zone 10";
        case "asia/taipei": return "Taipei Standard Time";
        case "asia/tashkent": return "West Asia Standard Time";
        case "asia/tbilisi": return "Georgian Standard Time";
        case "asia/tehran": return "Iran Standard Time";
        case "asia/thimphu": return "Bangladesh Standard Time";
        case "asia/tokyo": return "Tokyo Standard Time";
        case "asia/tomsk": return "Tomsk Standard Time";
        case "asia/ulaanbaatar": return "Ulaanbaatar Standard Time";
        case "asia/urumqi": return "Central Asia Standard Time";
        case "asia/ust-nera": return "Vladivostok Standard Time";
        case "asia/vladivostok": return "Vladivostok Standard Time";
        case "asia/yakutsk": return "Yakutsk Standard Time";
        case "asia/yangon": return "Myanmar Standard Time";
        case "asia/yekaterinburg": return "Ekaterinburg Standard Time";
        case "asia/yerevan": return "Caucasus Standard Time";
        case "atlantic/azores": return "Azores Standard Time";
        case "atlantic/bermuda": return "Atlantic Standard Time";
        case "atlantic/canary": return "GMT Standard Time";
        case "atlantic/cape_verde": return "Cape Verde Standard Time";
        case "atlantic/faroe": return "GMT Standard Time";
        case "atlantic/madeira": return "GMT Standard Time";
        case "atlantic/reykjavik": return "Greenwich Standard Time";
        case "atlantic/south_georgia": return "UTC-02";
        case "atlantic/st_helena": return "Greenwich Standard Time";
        case "atlantic/stanley": return "SA Eastern Standard Time";
        case "australia/adelaide": return "Cen. Australia Standard Time";
        case "australia/brisbane": return "E. Australia Standard Time";
        case "australia/broken_hill": return "Cen. Australia Standard Time";
        case "australia/currie": return "Tasmania Standard Time";
        case "australia/darwin": return "AUS Central Standard Time";
        case "australia/eucla": return "Aus Central W. Standard Time";
        case "australia/hobart": return "Tasmania Standard Time";
        case "australia/lindeman": return "E. Australia Standard Time";
        case "australia/lord_howe": return "Lord Howe Standard Time";
        case "australia/melbourne": return "AUS Eastern Standard Time";
        case "australia/perth": return "W. Australia Standard Time";
        case "australia/sydney": return "AUS Eastern Standard Time";
        case "cst6cdt": return "Central Standard Time";
        case "est5edt": return "Eastern Standard Time";
        case "etc/gmt+1": return "Cape Verde Standard Time";
        case "etc/gmt+10": return "Hawaiian Standard Time";
        case "etc/gmt+11": return "UTC-11";
        case "etc/gmt+12": return "Dateline Standard Time";
        case "etc/gmt+2": return "UTC-02";
        case "etc/gmt+3": return "SA Eastern Standard Time";
        case "etc/gmt+4": return "SA Western Standard Time";
        case "etc/gmt+5": return "SA Pacific Standard Time";
        case "etc/gmt+6": return "Central America Standard Time";
        case "etc/gmt+7": return "US Mountain Standard Time";
        case "etc/gmt+8": return "UTC-08";
        case "etc/gmt+9": return "UTC-09";
        case "etc/gmt-1": return "W. Central Africa Standard Time";
        case "etc/gmt-10": return "West Pacific Standard Time";
        case "etc/gmt-11": return "Central Pacific Standard Time";
        case "etc/gmt-12": return "UTC+12";
        case "etc/gmt-13": return "UTC+13";
        case "etc/gmt-14": return "Line Islands Standard Time";
        case "etc/gmt-2": return "South Africa Standard Time";
        case "etc/gmt-3": return "E. Africa Standard Time";
        case "etc/gmt-4": return "Arabian Standard Time";
        case "etc/gmt-5": return "West Asia Standard Time";
        case "etc/gmt-6": return "Central Asia Standard Time";
        case "etc/gmt-7": return "SE Asia Standard Time";
        case "etc/gmt-8": return "Singapore Standard Time";
        case "etc/gmt-9": return "Tokyo Standard Time";
        case "etc/utc": return "UTC";
        case "europe/amsterdam": return "W. Europe Standard Time";
        case "europe/andorra": return "W. Europe Standard Time";
        case "europe/astrakhan": return "Astrakhan Standard Time";
        case "europe/athens": return "GTB Standard Time";
        case "europe/belgrade": return "Central Europe Standard Time";
        case "europe/berlin": return "W. Europe Standard Time";
        case "europe/brussels": return "Romance Standard Time";
        case "europe/bucharest": return "GTB Standard Time";
        case "europe/budapest": return "Central Europe Standard Time";
        case "europe/chisinau": return "E. Europe Standard Time";
        case "europe/copenhagen": return "Romance Standard Time";
        case "europe/dublin": return "GMT Standard Time";
        case "europe/gibraltar": return "W. Europe Standard Time";
        case "europe/helsinki": return "FLE Standard Time";
        case "europe/istanbul": return "Turkey Standard Time";
        case "europe/kaliningrad": return "Kaliningrad Standard Time";
        case "europe/kiev": return "FLE Standard Time";
        case "europe/kirov": return "Russian Standard Time";
        case "europe/lisbon": return "GMT Standard Time";
        case "europe/london": return "GMT Standard Time";
        case "europe/luxembourg": return "W. Europe Standard Time";
        case "europe/madrid": return "Romance Standard Time";
        case "europe/malta": return "W. Europe Standard Time";
        case "europe/minsk": return "Belarus Standard Time";
        case "europe/monaco": return "W. Europe Standard Time";
        case "europe/moscow": return "Russian Standard Time";
        case "europe/oslo": return "W. Europe Standard Time";
        case "europe/paris": return "Romance Standard Time";
        case "europe/prague": return "Central Europe Standard Time";
        case "europe/riga": return "FLE Standard Time";
        case "europe/rome": return "W. Europe Standard Time";
        case "europe/samara": return "Russia Time Zone 3";
        case "europe/saratov": return "Saratov Standard Time";
        case "europe/simferopol": return "Russian Standard Time";
        case "europe/sofia": return "FLE Standard Time";
        case "europe/stockholm": return "W. Europe Standard Time";
        case "europe/tallinn": return "FLE Standard Time";
        case "europe/tirane": return "Central Europe Standard Time";
        case "europe/ulyanovsk": return "Astrakhan Standard Time";
        case "europe/uzhgorod": return "FLE Standard Time";
        case "europe/vienna": return "W. Europe Standard Time";
        case "europe/vilnius": return "FLE Standard Time";
        case "europe/volgograd": return "Russian Standard Time";
        case "europe/warsaw": return "Central European Standard Time";
        case "europe/zaporozhye": return "FLE Standard Time";
        case "europe/zurich": return "W. Europe Standard Time";
        case "indian/chagos": return "Central Asia Standard Time";
        case "indian/christmas": return "SE Asia Standard Time";
        case "indian/cocos": return "Myanmar Standard Time";
        case "indian/kerguelen": return "West Asia Standard Time";
        case "indian/mahe": return "Mauritius Standard Time";
        case "indian/maldives": return "West Asia Standard Time";
        case "indian/mauritius": return "Mauritius Standard Time";
        case "indian/reunion": return "Mauritius Standard Time";
        case "mst7mdt": return "Mountain Standard Time";
        case "pacific/apia": return "Samoa Standard Time";
        case "pacific/auckland": return "New Zealand Standard Time";
        case "pacific/bougainville": return "Bougainville Standard Time";
        case "pacific/chatham": return "Chatham Islands Standard Time";
        case "pacific/chuuk": return "West Pacific Standard Time";
        case "pacific/easter": return "Easter Island Standard Time";
        case "pacific/efate": return "Central Pacific Standard Time";
        case "pacific/enderbury": return "UTC+13";
        case "pacific/fakaofo": return "UTC+13";
        case "pacific/fiji": return "Fiji Standard Time";
        case "pacific/funafuti": return "UTC+12";
        case "pacific/galapagos": return "Central America Standard Time";
        case "pacific/gambier": return "UTC-09";
        case "pacific/guadalcanal": return "Central Pacific Standard Time";
        case "pacific/guam": return "West Pacific Standard Time";
        case "pacific/honolulu": return "Hawaiian Standard Time";
        case "pacific/kiritimati": return "Line Islands Standard Time";
        case "pacific/kosrae": return "Central Pacific Standard Time";
        case "pacific/kwajalein": return "UTC+12";
        case "pacific/majuro": return "UTC+12";
        case "pacific/marquesas": return "Marquesas Standard Time";
        case "pacific/nauru": return "UTC+12";
        case "pacific/niue": return "UTC-11";
        case "pacific/norfolk": return "Norfolk Standard Time";
        case "pacific/noumea": return "Central Pacific Standard Time";
        case "pacific/pago_pago": return "UTC-11";
        case "pacific/palau": return "Tokyo Standard Time";
        case "pacific/pitcairn": return "UTC-08";
        case "pacific/pohnpei": return "Central Pacific Standard Time";
        case "pacific/port_moresby": return "West Pacific Standard Time";
        case "pacific/rarotonga": return "Hawaiian Standard Time";
        case "pacific/tahiti": return "Hawaiian Standard Time";
        case "pacific/tarawa": return "UTC+12";
        case "pacific/tongatapu": return "Tonga Standard Time";
        case "pacific/wake": return "UTC+12";
        case "pacific/wallis": return "UTC+12";
        case "pst8pdt": return "Pacific Standard Time";
      }
      return ianaTimeZoneName;
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
