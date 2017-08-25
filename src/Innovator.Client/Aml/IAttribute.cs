using System;
namespace Innovator.Client
{
  /// <summary>
  /// A read-only AML attribute
  /// </summary>
  public interface IReadOnlyAttribute
  {
    /// <summary>Value converted to a <see cref="bool?"/>.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="bool"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="bool"/></exception>
    bool? AsBoolean();
    /// <summary>Value converted to a <see cref="bool"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="bool"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="bool"/></exception>
    bool AsBoolean(bool defaultValue);
    /// <summary>Value converted to a <see cref="DateTime?"/> in the local timezone.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="DateTime"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTime"/></exception>
    DateTime? AsDateTime();
    /// <summary>Value converted to a <see cref="DateTime"/> in the local timezone using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="DateTime"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTime"/></exception>
    /// <example>
    /// <code lang="C#">
    /// // If the part was created after 2016-01-01, put the name of the creator in the description
    /// if (comp.CreatedOn().AsDateTime(DateTime.MaxValue) &gt; new DateTime(2016, 1, 1))
    /// {
    ///     edits.Property("description").Set("Created by: " + comp.CreatedById().KeyedName().Value);
    /// }
    /// </code>
    /// </example>
    DateTime AsDateTime(DateTime defaultValue);
    /// <summary>Value converted to a <see cref="DateTime?"/> in the UTC timezone.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="DateTime"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTime"/></exception>
    DateTime? AsDateTimeUtc();
    /// <summary>Value converted to a <see cref="DateTime"/> in the UTC timezone using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="DateTime"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTime"/></exception>
    DateTime AsDateTimeUtc(DateTime defaultValue);
    /// <summary>Value converted to a <see cref="double?"/>.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="double"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="double"/></exception>
    double? AsDouble();
    /// <summary>Value converted to a <see cref="double"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="double"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="double"/></exception>
    double AsDouble(double defaultValue);
    /// <summary>Value converted to a <see cref="Guid?"/>.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="Guid"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="Guid"/></exception>
    Guid? AsGuid();
    /// <summary>Value converted to a <see cref="Guid"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="Guid"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="Guid"/></exception>
    Guid AsGuid(Guid defaultValue);
    /// <summary>Value converted to a <see cref="int?"/>.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="int"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="int"/></exception>
    int? AsInt();
    /// <summary>Value converted to a <see cref="int"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="int"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="int"/></exception>
    int AsInt(int defaultValue);
    /// <summary>Value converted to a <see cref="long?"/>.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="long"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="long"/></exception>
    long? AsLong();
    /// <summary>Value converted to a <see cref="long"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="long"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="long"/></exception>
    long AsLong(long defaultValue);
    /// <summary>Value converted to a <see cref="string"/> using the <paramref name="defaultValue"/> if null.</summary>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="string"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    string AsString(string defaultValue);
    /// <summary>Returns <c>true</c> if this element actually exists in the underlying AML,
    /// otherwise, returns <c>false</c> to indicate that the element is just a null placeholder
    /// put in place to reduce unnecessary null reference checks</summary>
    /// <value><c>true</c> if this element actually exists, <c>false</c> otherwise</value>
    bool Exists { get; }
    /// <summary>Get the local XML name of the attribute</summary>
    string Name { get; }
    /// <summary>Get the string value of the attribute</summary>
    string Value { get; }
  }
  /// <summary>
  /// A modifiable AML attribute
  /// </summary>
  public interface IAttribute : IReadOnlyAttribute
  {
    /// <summary>
    /// Set the value of this attribute
    /// </summary>
    /// <param name="value">New attribute value</param>
    /// <remarks>If the attribute does not exist (<see cref="IReadOnlyAttribute.Exists"/> = <c>false</c>),
    /// but it's parent does exist, the attribute is added to it's parent</remarks>
    void Set(object value);
    /// <summary>
    /// Remove the attribute from it's parent
    /// </summary>
    void Remove();
    /// <summary>Get or set the string value of the attribute</summary>
    new string Value { get; set; }
  }
}
