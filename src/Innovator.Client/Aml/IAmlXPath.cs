using System.Collections.Generic;

namespace Innovator.Client
{
  /// <summary>
  /// Class which supports executing XPath against an <see cref="IReadOnlyElement"/>
  /// </summary>
  public interface IAmlXPath
  {
    /// <summary>
    /// Evaluates an XPath expression.
    /// </summary>
    /// <param name="expression">A <c>string</c> that contains an XPath expression.</param>
    /// <returns>An object that can contain a <c>bool</c>, a <c>double</c>, a <c>string</c>, or an <see cref="IEnumerable{T}"/></returns>
    object Evaluate(string expression);
    /// <summary>
    /// Selects an <see cref="IReadOnlyElement"/> using a XPath expression.
    /// </summary>
    /// <param name="expression">A <c>string</c> that contains an XPath expression.</param>
    /// <returns>An <see cref="IReadOnlyElement"/>, or a null <see cref="IReadOnlyElement"/> (<see cref="IReadOnlyElement.Exists"/> = <c>false</c>).</returns>
    IReadOnlyElement SelectElement(string expression);
    /// <summary>
    /// Selects a collection of <see cref="IReadOnlyElement"/> elements using a XPath expression.
    /// </summary>
    /// <param name="expression">A <c>string</c> that contains an XPath expression.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IReadOnlyElement"/> that contains the selected elements.</returns>
    IEnumerable<IReadOnlyElement> SelectElements(string expression);
  }
}
