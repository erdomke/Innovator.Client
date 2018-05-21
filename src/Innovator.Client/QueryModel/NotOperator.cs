using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class NotOperator : UnaryOperator, ILogical
  {
    public override int Precedence => (int)PrecedenceLevel.Not;

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public IExpression Normalize()
    {
      if (Arg is EqualsOperator eq)
        return new NotEqualsOperator() { Left = eq.Left, Right = eq.Right };
      else if (Arg is NotEqualsOperator neq)
        return new EqualsOperator() { Left = neq.Left, Right = neq.Right };
      else if (Arg is BetweenOperator btw)
        return new NotBetweenOperator() { Left = btw.Left, Min = btw.Min, Max = btw.Max };
      else if (Arg is NotBetweenOperator nbtw)
        return new BetweenOperator() { Left = nbtw.Left, Min = nbtw.Min, Max = nbtw.Max };
      else if (Arg is InOperator inOp)
        return new NotInOperator() { Left = inOp.Left, Right = inOp.Right };
      else if (Arg is NotInOperator ninOp)
        return new InOperator() { Left = ninOp.Left, Right = ninOp.Right };
      else if (Arg is LikeOperator like)
        return new NotLikeOperator() { Left = like.Left, Right = like.Right };
      else if (Arg is NotLikeOperator nlike)
        return new LikeOperator() { Left = nlike.Left, Right = nlike.Right };
      else if (Arg is LessThanOperator less)
        return new GreaterThanOrEqualsOperator() { Left = less.Left, Right = less.Right };
      else if (Arg is LessThanOrEqualsOperator lessEq)
        return new GreaterThanOperator() { Left = lessEq.Left, Right = lessEq.Right };
      else if (Arg is GreaterThanOperator gt)
        return new LessThanOrEqualsOperator() { Left = gt.Left, Right = gt.Right };
      else if (Arg is GreaterThanOrEqualsOperator gtEq)
        return new LessThanOperator() { Left = gtEq.Left, Right = gtEq.Right };
      else if (Arg is NotOperator not)
        return not.Arg;
      else if (Arg is PropertyReference prop)
        return new EqualsOperator() { Left = prop, Right = new BooleanLiteral(false) };

      return this;
    }
  }
}
