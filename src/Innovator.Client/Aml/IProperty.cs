using System;

namespace Innovator.Client
{
  /// <summary>
  /// Base property interface used to identify elements which are properties
  /// </summary>
  public interface IReadOnlyProperty_Base : IReadOnlyElement { }

  /// <summary>
  /// Represents a property of type boolean
  /// </summary>
  public interface IReadOnlyProperty_Boolean : IReadOnlyProperty_Base
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
  }

  /// <summary>
  /// Represents a property of type date
  /// </summary>
  public interface IReadOnlyProperty_Date : IReadOnlyProperty_Base
  {
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
    /// <summary>Value converted to a <see cref="DateTimeOffset?"/> in the local timezone.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="DateTimeOffset"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTimeOffset"/></exception>
    DateTimeOffset? AsDateTimeOffset();
    /// <summary>Value converted to a <see cref="DateTimeOffset"/> in the local timezone using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="DateTimeOffset"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTimeOffset"/></exception>
    DateTimeOffset AsDateTimeOffset(DateTimeOffset defaultValue);
  }

  /// <summary>
  /// Represents a property of type integer, decimal, float, etc.
  /// </summary>
  public interface IReadOnlyProperty_Number : IReadOnlyProperty_Base
  {
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
  }

  /// <summary>
  /// Represents a property of type item
  /// </summary>
  public interface IReadOnlyProperty_Item<in T> : IReadOnlyProperty_Base
  {
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
    /// <summary>Value converted to a <see cref="IReadOnlyItem"/>.
    /// If the value cannot be converted, a 'null' item (where the
    /// <see cref="IReadOnlyElement.Exists"/> property returns <c>false</c>) is returned</summary>
    /// <example>
    /// After executing the code
    /// <code lang="C#">
    /// var item = aml.FromXml(@"&lt;Item type='File'&gt;
    ///  &lt;created_by_id keyed_name="John Smith" type="User"&gt;A5CE5E3B4E3846D8A15C1C9300EAF7B4&lt;/created_by_id&gt;
    ///&lt;/Item&gt;").AssertItem();
    /// var creator = item.CreatedById().AsItem();
    /// </code>
    /// the structure of <c>creator</c> will be
    /// <code lang="XML">
    ///&lt;Item type='User'& id='A5CE5E3B4E3846D8A15C1C9300EAF7B4'gt;
    ///  &lt;id keyed_name="John Smith" type="User"&gt;A5CE5E3B4E3846D8A15C1C9300EAF7B4&lt;/id&gt;
    ///  &lt;keyed_name&gt;John Smith&lt;/keyed_name&gt;
    ///&lt;/Item&gt;
    /// </code>
    /// </example>
    IReadOnlyItem AsItem();
  }

  /// <summary>
  /// Represents a property that is of type text/string or something similar
  /// </summary>
  public interface IReadOnlyProperty_Text : IReadOnlyProperty_Base
  {
    /// <summary>Value converted to a <see cref="string"/> using the <paramref name="defaultValue"/> if null.</summary>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="string"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    string AsString(string defaultValue);
  }

  /// <summary>
  /// Represents a property that is of type image or something similar
  /// </summary>
  public interface IReadOnlyProperty_Image : IReadOnlyProperty_Text, IReadOnlyProperty_Item<IReadOnlyItem> { }

  /// <summary>
  /// A readonly property of an item
  /// </summary>
  public interface IReadOnlyProperty
    : IReadOnlyProperty_Boolean
    , IReadOnlyProperty_Date
    , IReadOnlyProperty_Item<IReadOnlyItem>
    , IReadOnlyProperty_Number
    , IReadOnlyProperty_Text
    , IReadOnlyProperty_Image
  {

  }

  /// <summary>
  /// Base mutable property interface used to indicate elements which are properties
  /// </summary>
  public interface IProperty_Base : IElement, IReadOnlyProperty_Base
  {
    /// <summary>
    /// Set the value of the property
    /// </summary>
    /// <param name="value">Value to set the property to</param>
    /// <remarks>If the property does not exist (<see cref="IReadOnlyElement.Exists"/> = <c>false</c>),
    /// but it's parent does exist, the property is added to it's parent</remarks>
    /// <example>
    /// <code lang="C#">
    /// // If the part was created after 2016-01-01, put the name of the creator in the description
    /// if (comp.CreatedOn().AsDateTime(DateTime.MaxValue) > new DateTime(2016, 1, 1))
    /// {
    ///     edits.Property("description").Set("Created by: " + comp.CreatedById().KeyedName().Value);
    /// }
    /// </code>
    /// </example>
    void Set(object value);
  }
  /// <summary>
  /// Property of type boolean
  /// </summary>
  public interface IProperty_Boolean : IReadOnlyProperty_Boolean, IProperty_Base { }
  /// <summary>
  /// Property of type date
  /// </summary>
  public interface IProperty_Date    : IReadOnlyProperty_Date, IProperty_Base { }
  /// <summary>
  /// Property of type item
  /// </summary>
  public interface IProperty_Item<in T>    : IReadOnlyProperty_Item<T>, IProperty_Base
  {
    /// <summary>Value converted to a <see cref="IItem"/>.
    /// If the value cannot be converted, a 'null' item (where the
    /// <see cref="IReadOnlyElement.Exists"/> property returns <c>false</c>) is returned</summary>
    /// <example>
    /// After executing the code
    /// <code lang="C#">
    /// var item = aml.FromXml(@"&lt;Item type='File'&gt;
    ///  &lt;created_by_id keyed_name="John Smith" type="User"&gt;A5CE5E3B4E3846D8A15C1C9300EAF7B4&lt;/created_by_id&gt;
    ///&lt;/Item&gt;").AssertItem();
    /// var creator = item.CreatedById().AsItem();
    /// </code>
    /// the structure of <c>creator</c> will be
    /// <code lang="XML">
    ///&lt;Item type='User'& id='A5CE5E3B4E3846D8A15C1C9300EAF7B4'gt;
    ///  &lt;id keyed_name="John Smith" type="User"&gt;A5CE5E3B4E3846D8A15C1C9300EAF7B4&lt;/id&gt;
    ///  &lt;keyed_name&gt;John Smith&lt;/keyed_name&gt;
    ///&lt;/Item&gt;
    /// </code>
    /// </example>
    new IItem AsItem();
  }

  /// <summary>
  /// Property of type integer, decimal, float, etc.
  /// </summary>
  public interface IProperty_Number  : IReadOnlyProperty_Number, IProperty_Base { }
  /// <summary>
  /// Property that is of type text/string or something similar
  /// </summary>
  public interface IProperty_Text    : IReadOnlyProperty_Text, IProperty_Base { }
  /// <summary>
  /// Property that is of type image or something similar
  /// </summary>
  public interface IProperty_Image : IReadOnlyProperty_Image, IProperty_Base { }

  /// <summary>
  /// A modifiable property of an item
  /// </summary>
  public interface IProperty
    : IProperty_Boolean
    , IProperty_Date
    , IProperty_Item<IReadOnlyItem>
    , IProperty_Number
    , IProperty_Text
    , IProperty_Image
    , IReadOnlyProperty
  {

  }
}
