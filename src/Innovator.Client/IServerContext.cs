using System;
using System.Runtime.Serialization;

namespace Innovator.Client
{
  /// <summary>
  /// Context for serializing/deserializing native types (e.g. <c>DateTime</c>, <c>double</c>, <c>boolean</c>, etc.)
  /// </summary>
  public interface IServerContext :
#if SERIALIZATION
    ISerializable,
#endif
    IFormatProvider
    , ICustomFormatter
  {
    /// <summary>
    /// Gets the default language code configured for the Aras user
    /// </summary>
    string DefaultLanguageCode { get; }
    /// <summary>
    /// Gets the default language suffix configured for the Aras user
    /// </summary>
    string DefaultLanguageSuffix { get; }
    /// <summary>
    /// Gets the language code configured for the Aras user
    /// </summary>
    string LanguageCode { get; }
    /// <summary>
    /// Gets the language suffix configured for the Aras user
    /// </summary>
    string LanguageSuffix { get; }
    /// <summary>
    /// Gets the locale configured for the user.
    /// </summary>
    string Locale { get; }
    /// <summary>
    /// Gets the corporate time zone ID for the Aras installation
    /// </summary>
    string TimeZone { get; }

    /// <summary>
    /// Converts the <see cref="object"/> to a <see cref="bool"/> based on 
    /// the locale and time zone
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>null</c> if <paramref name="value"/> is null or empty. 
    /// A <see cref="bool"/> if <paramref name="value"/> is convertible.
    /// Otherwise, an exception is thrown</returns>
    bool? AsBoolean(object value);
    /// <summary>
    /// Converts the <see cref="object"/> representing a date in the corporate
    /// time zone to a <see cref="ZonedDateTime"/>
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>null</c> if <paramref name="value"/> is null or empty. 
    /// A <see cref="ZonedDateTime"/> if <paramref name="value"/> is convertible.
    /// Otherwise, an exception is thrown</returns>
    ZonedDateTime? AsZonedDateTime(object value);
    /// <summary>
    /// Converts the <see cref="object"/> to a <see cref="decimal"/> based on 
    /// the locale and time zone
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>null</c> if <paramref name="value"/> is null or empty. 
    /// A <see cref="decimal"/> if <paramref name="value"/> is convertible.
    /// Otherwise, an exception is thrown</returns>
    decimal? AsDecimal(object value);
    /// <summary>
    /// Converts the <see cref="object"/> to a <see cref="double"/> based on 
    /// the locale and time zone
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>null</c> if <paramref name="value"/> is null or empty. 
    /// A <see cref="double"/> if <paramref name="value"/> is convertible.
    /// Otherwise, an exception is thrown</returns>
    double? AsDouble(object value);
    /// <summary>
    /// Converts the <see cref="object"/> to a <see cref="int"/> based on 
    /// the locale and time zone
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>null</c> if <paramref name="value"/> is null or empty. 
    /// A <see cref="int"/> if <paramref name="value"/> is convertible.
    /// Otherwise, an exception is thrown</returns>
    int? AsInt(object value);
    /// <summary>
    /// Converts the <see cref="object"/> to a <see cref="long"/> based on 
    /// the locale and time zone
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>null</c> if <paramref name="value"/> is null or empty. 
    /// A <see cref="long"/> if <paramref name="value"/> is convertible.
    /// Otherwise, an exception is thrown</returns>
    long? AsLong(object value);
    /// <summary>
    /// Serializes the value to a string.  Dates are converted to the
    /// corporate time zone
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>A string representing the value</returns>
    string Format(object value);
  }
}
