using System.Collections.Generic;

namespace Innovator.Client
{
  /// <summary>
  /// Represents an Aras Item that is read only.  By default, the connection object returns IReadOnlyItems to encourage
  /// treating the results as immutable
  /// </summary>
  /// <remarks>
  /// An <see cref="IReadOnlyItem"/> only ever represents a single <c>Item</c> AML tag.  It never represents multiple
  /// items, a string, or an exception
  /// <code>&lt;Item&gt;
  ///  &lt;prop1&gt;a&lt;/prop1&gt;
  ///  &lt;prop2&gt;b&lt;/prop2&gt;
  ///&lt;/Item&gt;</code>
  /// </remarks>
  public interface IReadOnlyItem : IReadOnlyElement, IItemRef
  {
    /// <summary>Creates a duplicate of the item object.  All properties (including the ID) are preserved</summary>
    /// <returns>A mutable copy of the current item</returns>
    IItem Clone();
    /// <summary>Returns a reference to the property with the specified name</summary>
    /// <param name="name">Name of the property</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>If the property exists, a valid <see cref="IReadOnlyProperty"/> will be returned</description></item>
    ///   <item><description>If the property does not exists, a "null" <see cref="IReadOnlyProperty"/> will be returned where <see cref="IReadOnlyElement.Exists"/> = <c>false</c></description></item>
    /// </list></returns>
    IReadOnlyProperty Property(string name);
    /// <summary>Returns a reference to the property with the specified name and language</summary>
    /// <param name="name">Name of the property</param>
    /// <param name="lang">Language of the (multilingual) property</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>If the property exists, a valid <see cref="IReadOnlyProperty"/> will be returned</description></item>
    ///   <item><description>If the property does not exists, a "null" <see cref="IReadOnlyProperty"/> will be returned where <see cref="IReadOnlyElement.Exists"/> = <c>false</c></description></item>
    /// </list></returns>
    IReadOnlyProperty Property(string name, string lang);
    /// <summary>Returns the set of relationships associated with this item</summary>
    /// <returns>An <see cref="IEnumerable{IReadOnlyItem}"/> containing all relationship items</returns>
    IEnumerable<IReadOnlyItem> Relationships();
    /// <summary>Returns the set of relationships associated with this item of the specified type</summary>
    /// <param name="type">Name of the ItemType for the relationships you wish to retrieve</param>
    /// <returns>An <see cref="IEnumerable{IReadOnlyItem}"/> containing relationship items whose type name is equal to <paramref name="type"/></returns>
    IEnumerable<IReadOnlyItem> Relationships(string type);
  }
  /// <summary>
  /// Represents an Aras Item that is mutable/modifiable
  /// </summary>
  /// <remarks>
  /// An <see cref="IItem"/> only ever represents a single <c>Item</c> AML tag.  It never represents multiple
  /// items, a string, or an exception
  /// <code>&lt;Item&gt;
  ///  &lt;prop1&gt;a&lt;/prop1&gt;
  ///  &lt;prop2&gt;b&lt;/prop2&gt;
  ///&lt;/Item&gt;</code>
  /// </remarks>
  public interface IItem : IElement, IReadOnlyItem
  {
    /// <summary>Returns a reference to the property with the specified name</summary>
    /// <remarks>If the property does not exist, a non-null object will be returned that has an <c>Exists</c> member which will return <c>false</c></remarks>
    /// <param name="name">Name of the property</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>If the property exists, a valid <see cref="IProperty"/> will be returned</description></item>
    ///   <item><description>If the property does not exists, a "null" <see cref="IProperty"/> will be returned where <see cref="IReadOnlyElement.Exists"/> = <c>false</c></description></item>
    /// </list></returns>
    new IProperty Property(string name);
    /// <summary>Returns a reference to the property with the specified name and language</summary>
    /// <remarks>If the property does not exist, a non-null object will be returned that has an <c>Exists</c> member which will return <c>false</c></remarks>
    /// <param name="name">Name of the property</param>
    /// <param name="lang">Language of the (multilingual) property</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>If the property exists, a valid <see cref="IProperty"/> will be returned</description></item>
    ///   <item><description>If the property does not exists, a "null" <see cref="IProperty"/> will be returned where <see cref="IReadOnlyElement.Exists"/> = <c>false</c></description></item>
    /// </list></returns>
    new IProperty Property(string name, string lang);
    /// <summary>Returns the set of relationships associated with this item</summary>
    new IRelationships Relationships();
    /// <summary>Returns the set of relationships associated with this item of the specified type</summary>
    /// <param name="type">Name of the ItemType for the relationships you wish to retrieve</param>
    new IEnumerable<IItem> Relationships(string type);
  }
}
