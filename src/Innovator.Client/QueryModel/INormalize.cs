using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// An expression which can be converted to a more normal form (e.g. property references are
  /// on the left of an operator instead of the right).
  /// </summary>
  /// <seealso cref="Innovator.Client.QueryModel.IExpression" />
  internal interface INormalize : IExpression
  {
    /// <summary>
    /// Return a normalized version of the expression
    /// </summary>
    /// <returns>
    /// If the expression can be normalized (e.g. by placing the property reference on the left
    /// of a binary operator), return a new, more normalized expression.  If the expression cannot
    /// be normalized, return the current expression.
    /// </returns>
    IExpression Normalize();
  }
}
