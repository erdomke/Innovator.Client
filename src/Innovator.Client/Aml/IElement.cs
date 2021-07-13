using System.Collections.Generic;

namespace Innovator.Client
{
  /// <summary>A read-only AML element.  This element could be an Item, property, result tag, or something else</summary>
  public interface IReadOnlyElement : IAmlNode
  {
    /// <summary>Retrieve the attribute with the specified name</summary>
    /// <param name="name">Name of the XML attribute</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>If the property exists, a valid <see cref="IReadOnlyAttribute"/> will be returned</description></item>
    ///   <item><description>If the property does not exists, a "null" <see cref="IReadOnlyAttribute"/> will be returned where <see cref="IReadOnlyAttribute.Exists"/> = <c>false</c></description></item>
    /// </list></returns>
    IReadOnlyAttribute Attribute(string name);
    /// <summary>Retrieve all attributes specified for the element</summary>
    /// <returns>An <see cref="IEnumerable{IReadOnlyAttribute}"/> containing all of the defined attributes</returns>
    IEnumerable<IReadOnlyAttribute> Attributes();
    /// <summary>Retrieve all child elements</summary>
    IEnumerable<IReadOnlyElement> Elements();
    /// <summary>Retrieve the context used for rendering primitive values</summary>
    ElementFactory AmlContext { get; }
    /// <summary>Returns <c>true</c> if this element actually exists in the underlying AML,
    /// otherwise, returns <c>false</c> to indicate that the element is just a null placeholder
    /// put in place to reduce unnecessary null reference checks</summary>
    bool Exists { get; }
    /// <summary>Local XML name of the element (e.g. <c>Item</c>, <c>Relationships</c>, 
    /// <c>config_id</c>, etc.).  To access the property <c>&lt;name&gt;</c>, there will be a 
    /// method such as <see cref="Model.ItemType.NameProp"/></summary>
    /// <value>The XML name of the element</value>
    string Name { get; }
    /// <summary>Retrieve the parent element</summary>
    IReadOnlyElement Parent { get; }
    /// <summary>The namespace prefix for the element</summary>
    string Prefix { get; }
    /// <summary>String value of the element</summary>
    string Value { get; }
  }
  /// <summary>A modifiable AML element.  This element could be an Item, property, result tag, or something else</summary>
  public interface IElement : IReadOnlyElement
  {
    /// <summary>Retrieve the attribute with the specified name</summary>
    /// <param name="name">Name of the XML attribute</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>If the property exists, a valid <see cref="IAttribute"/> will be returned</description></item>
    ///   <item><description>If the property does not exists, a "null" <see cref="IAttribute"/> will be returned where <see cref="IReadOnlyAttribute.Exists"/> = <c>false</c></description></item>
    /// </list></returns>
    new IAttribute Attribute(string name);
    /// <summary>Retrieve all attributes specified for the element</summary>
    new IEnumerable<IAttribute> Attributes();
    /// <summary>Retrieve all child elements</summary>
    new IEnumerable<IElement> Elements();
    /// <summary>Retrieve the parent element</summary>
    new IElement Parent { get; }
    /// <summary>Add new content to the element</summary>
    /// <param name="content"><see cref="IElement"/>, <see cref="IAttribute"/>, or <see cref="object"/> to add as a child of the element</param>
    /// <returns>The current element for chaining additional calls</returns>
    IElement Add(object content);
    /// <summary>Remove the element from its parent</summary>
    void Remove();
    /// <summary>Remove attributes from the element</summary>
    void RemoveAttributes();
    /// <summary>Remove child nodes from the element</summary>
    void RemoveNodes();
  }

  /// <summary>
  /// A logical element representing an <c>&lt;and&gt;</c>, <c>&lt;or&gt;</c>, or <c>&lt;not&gt;</c> AML tag
  /// </summary>
  public interface ILogical : IElement { }

  /// <summary>
  /// A logical element representing an <c>&lt;and&gt;</c>, <c>&lt;or&gt;</c>, or <c>&lt;not&gt;</c> AML tag
  /// </summary>
  public interface IReadOnlyLogical : IReadOnlyElement { }

  /// <summary>
  /// A <c>&lt;Relationships&gt;</c> tag
  /// </summary>
  public interface IRelationships : IElement, IEnumerable<IItem> { }

  /// <summary>
  /// A <c>&lt;Relationships&gt;</c> tag
  /// </summary>
  public interface IReadOnlyRelationships : IReadOnlyElement, IEnumerable<IReadOnlyItem> { }
}
