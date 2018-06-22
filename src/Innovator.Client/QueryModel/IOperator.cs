using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// An operator which performs an operation on one or more arguments
  /// </summary>
  /// <seealso cref="Innovator.Client.QueryModel.IExpression" />
  public interface IOperator : IExpression
  {
    /// <summary>
    /// Gets the precedence used when evaluating the order of operations
    /// </summary>
    /// <remarks>
    /// Higher precedence operators are evaluated first in the absence of parentheses.  For example
    /// the precedence of multiplication is greater than addition.
    /// </remarks>
    int Precedence { get; }
  }
}
