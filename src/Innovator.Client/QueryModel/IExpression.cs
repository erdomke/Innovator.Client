using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// An expression used in a query
  /// </summary>
  public interface IExpression
  {
    /// <summary>
    /// Tell the specified visitor to process this expression component.
    /// </summary>
    /// <param name="visitor">The visitor.</param>
    void Visit(IExpressionVisitor visitor);
  }
}
