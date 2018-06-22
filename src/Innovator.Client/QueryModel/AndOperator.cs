using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// Represents a logical AND of two expressions
  /// </summary>
  /// <seealso cref="Innovator.Client.QueryModel.BinaryOperator" />
  /// <seealso cref="Innovator.Client.QueryModel.ILogical" />
  /// <seealso cref="Innovator.Client.QueryModel.INormalize" />
  public class AndOperator : BinaryOperator, ILogical, INormalize
  {
    /// <summary>
    /// Gets the precedence used when evaluating the order of operations
    /// </summary>
    /// <remarks>
    /// Higher precedence operators are evaluated first in the absence of parentheses.  For example
    /// the precedence of multiplication is greater than addition.
    /// </remarks>
    public override int Precedence => (int)PrecedenceLevel.And;

    QueryItem ITableProvider.Table { get; set; }

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
      if (Left is BooleanLiteral boolLeft)
      {
        if (boolLeft.Value)
          return Right;
        else
          return boolLeft;
      }
      else if (Right is BooleanLiteral boolRight)
      {
        if (boolRight.Value)
          return Left;
        else
          return boolRight;
      }
      else if (Left is null || Left is IgnoreNode)
      {
        return Right;
      }
      else if (Right is null || Right is IgnoreNode)
      {
        return Left;
      }

      if (Left is PropertyReference)
      {
        Left = new EqualsOperator()
        {
          Left = Left,
          Right = new BooleanLiteral(true)
        }.Normalize();
      }
      if (Right is PropertyReference)
      {
        Right = new EqualsOperator()
        {
          Left = Right,
          Right = new BooleanLiteral(true)
        }.Normalize();
      }

      SetTable();
      return this;
    }
  }
}
