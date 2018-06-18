using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class AndOperator : BinaryOperator, ILogical, INormalize
  {
    public override int Precedence => (int)PrecedenceLevel.And;
    QueryItem ITableProvider.Table { get; set; }

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

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
