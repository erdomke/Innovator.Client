using System;
using System.Collections.Generic;

namespace Innovator.Client
{
  /// <summary>
  /// Represents a modifiable result of an Aras query that can either be a string, an exception, or zero or more items
  /// </summary>
  /// <remarks>
  /// The result of an Aras query might take one of the following forms:
  /// <list type="table">
  ///   <item>
  ///     <term>Single Item</term><description>To access the item, use the <see cref="AssertItem(string)"/> method</description>
  ///   </item>
  ///   <item>
  ///     <term>Item Collection</term><description>To access the items, use the <see cref="IReadOnlyResult.Items"/> method</description>
  ///   </item>
  ///   <item>
  ///     <term>String Result</term><description>To access the string, use the <see cref="Value"/> property</description>
  ///   </item>
  ///   <item>
  ///     <term>Error</term><description>To access the error, use the <see cref="Exception"/> property.  The other methods/properties will raise this exception if accessed.</description>
  ///   </item>
  /// </list>
  /// </remarks>
  public interface IResult : IReadOnlyResult, IErrorBuilder
  {
    /// <summary>Return a single item.  If that is not possible, throw an appropriate
    /// exception (e.g. the exception returned by the server where possible)</summary>
    /// <param name="type">If specified, throw an exception if the item doesn't have the specified type</param>
    /// <returns>A single <see cref="IItem"/></returns>
    /// <exception cref="ServerException">If the result does not represent exactly one item, either the exception returned from 
    /// the server or a new exception will be thrown</exception>
    new IItem AssertItem(string type = null);
    /// <summary>Add the specified item to the result</summary>
    /// <param name="item">Item to add</param>
    /// <returns>The current <see cref="IResult"/> object for chaining additional commands</returns>
    IResult Add(IItem item);
    /// <summary>Adds content to the <c>Message</c> element of the result tag</summary>
    /// <param name="content">Content to add to the <c>Message</c> element</param>
    void AddMessage(params object[] content);
    /// <summary>Get or set the string value of the result</summary>
    /// <value>The string value of the result</value>
    new string Value { get; set; }
  }

  /// <summary>
  /// Represents a read-only result of an Aras query that can either be a string, an exception, or zero or more items
  /// </summary>
  /// <remarks>
  /// The result of an Aras query might take one of the following forms:
  /// <list type="table">
  ///   <item>
  ///     <term>Single Item</term><description>To access the item, use the <see cref="AssertItem(string)"/> method</description>
  ///   </item>
  ///   <item>
  ///     <term>Item Collection</term><description>To access the items, use the <see cref="Items"/> method</description>
  ///   </item>
  ///   <item>
  ///     <term>String Result</term><description>To access the string, use the <see cref="Value"/> property</description>
  ///   </item>
  ///   <item>
  ///     <term>Error</term><description>To access the error, use the <see cref="Exception"/> property.  The other methods/properties will raise this exception if accessed.</description>
  ///   </item>
  /// </list>
  /// </remarks>
  public interface IReadOnlyResult : IAmlNode
  {
    /// <summary>Return a single item.  If that is not possible, throw an appropriate
    /// exception (e.g. the exception returned by the server where possible)</summary>
    /// <param name="type">If specified, throw an exception if the item doesn't have the specified type</param>
    /// <returns>A single <see cref="IReadOnlyItem"/></returns>
    /// <exception cref="ServerException">If the result does not represent exactly one item, either the exception returned from 
    /// the server or a new exception will be thrown</exception>
    IReadOnlyItem AssertItem(string type = null);
    /// <summary>Return an enumerable of items.  Throw an exception for any error including 'No items found'</summary>
    /// <exception cref="ServerException">If the result does not have at least one item, either the exception returned from 
    /// the server or a new exception will be thrown</exception>
    IEnumerable<IReadOnlyItem> AssertItems();
    /// <summary>Do nothing other than throw an exception if there is an error other than 'No Items Found'</summary>
    /// <exception cref="ServerException">If an exception was returned from the server (including 
    /// <see cref="NoItemsFoundException"/>), the exception will be thrown</exception>
    /// <returns>The current <see cref="IReadOnlyResult"/> for chaining additional methods</returns>
    IReadOnlyResult AssertNoError();
    /// <summary>Return an exception (if there is one), otherwise, return <c>null</c></summary>
    ServerException Exception { get; }
    /// <summary>Return an enumerable of items.  Throw an exception if there is an error other than 'No Items Found'</summary>
    /// <exception cref="ServerException">If the exception returned from the server is anything other than
    /// <see cref="NoItemsFoundException"/>, the exception will be thrown</exception>
    IEnumerable<IReadOnlyItem> Items();
    /// <summary>Get messages (such as permissions warnings) from the database</summary>
    IReadOnlyElement Message { get; }
    /// <summary>Return the string value of the result</summary>
    string Value { get; }
  }
}
