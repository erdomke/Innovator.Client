using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// Represents an interface for building an error message
  /// </summary>
  public interface IErrorBuilder
  {
    /// <summary>
    /// Specify the context of the error message
    /// </summary>
    /// <param name="item">Item which the error is about</param>
    /// <returns>The same <see cref="IErrorBuilder"/> instance for chaining additional calls</returns>
    IErrorBuilder ErrorContext(IReadOnlyItem item);
    /// <summary>
    /// Add an error message
    /// </summary>
    /// <param name="message">Message to return to the user</param>
    /// <returns>The same <see cref="IErrorBuilder"/> instance for chaining additional calls</returns>
    IErrorBuilder ErrorMsg(string message);
    /// <summary>
    /// Add an error message specifying the properties the message pertains to
    /// </summary>
    /// <param name="message">Message to return to the user</param>
    /// <param name="properties">Name of the properties (e.g. <c>created_by_id</c>) to which the error pertains</param>
    /// <returns>The same <see cref="IErrorBuilder"/> instance for chaining additional calls</returns>
    IErrorBuilder ErrorMsg(string message, params string[] properties);
    /// <summary>
    /// Add an error message specifying the properties the message pertains to
    /// </summary>
    /// <param name="message">Message to return to the user</param>
    /// <param name="properties">Name of the properties (e.g. <c>created_by_id</c>) to which the error pertains</param>
    /// <returns>The same <see cref="IErrorBuilder"/> instance for chaining additional calls</returns>
    IErrorBuilder ErrorMsg(string message, IEnumerable<string> properties);
  }
}
