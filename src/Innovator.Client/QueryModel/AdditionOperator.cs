using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// Represents the addition of two numeric expressions
  /// </summary>
  /// <seealso cref="Innovator.Client.QueryModel.BinaryOperator" />
  /// <seealso cref="Innovator.Client.QueryModel.INormalize" />
  public class AdditionOperator : BinaryOperator, INormalize
  {
    /// <summary>
    /// Gets the precedence used when evaluating the order of operations
    /// </summary>
    /// <remarks>
    /// Higher precedence operators are evaluated first in the absence of parentheses.  For example
    /// the precedence of multiplication is greater than addition.
    /// </remarks>
    public override int Precedence => (int)PrecedenceLevel.Additive;

    /// <summary>
    /// Tell the specified visitor to process this expression component.
    /// </summary>
    /// <param name="visitor">The visitor.</param>
    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    /// <summary>
    /// Return a normalized version of the expression
    /// </summary>
    /// <returns>
    /// If the expression can be normalized (e.g. by placing the property reference on the left
    /// of a binary operator), return a new, more normalized expression.  If the expression cannot
    /// be normalized, return the current expression.
    /// </returns>
    public IExpression Normalize()
    {
      if (Expressions.TryGetLong(Left, out var i1)
        && Expressions.TryGetLong(Right, out var i2))
      {
        return new IntegerLiteral(i1 + i2);
      }
      else if (Expressions.TryGetDouble(Left, out var d1)
        && Expressions.TryGetDouble(Right, out var d2))
      {
        return new FloatLiteral(d1 + d2);
      }

      return this;
    }
  }
}
