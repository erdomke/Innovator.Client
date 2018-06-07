using System;
namespace Innovator.Client
{
  /// <summary>
  /// A read-only AML attribute
  /// </summary>
  public interface IReadOnlyAttribute : ICoercible
  {
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
