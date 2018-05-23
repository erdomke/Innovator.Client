using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class NotOperator : UnaryOperator, ILogical, INormalize
  {
    public override int Precedence => (int)PrecedenceLevel.Not;
    QueryItem ITableProvider.Table { get; set; }

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public IExpression Normalize()
    {
      if (Arg is EqualsOperator eq)
        return new NotEqualsOperator() { Left = eq.Left, Right = eq.Right }.Normalize();
      else if (Arg is NotEqualsOperator neq)
        return new EqualsOperator() { Left = neq.Left, Right = neq.Right }.Normalize();
      else if (Arg is LessThanOperator less)
        return new GreaterThanOrEqualsOperator() { Left = less.Left, Right = less.Right }.Normalize();
      else if (Arg is LessThanOrEqualsOperator lessEq)
        return new GreaterThanOperator() { Left = lessEq.Left, Right = lessEq.Right }.Normalize();
      else if (Arg is GreaterThanOperator gt)
        return new LessThanOrEqualsOperator() { Left = gt.Left, Right = gt.Right }.Normalize();
      else if (Arg is GreaterThanOrEqualsOperator gtEq)
        return new LessThanOperator() { Left = gtEq.Left, Right = gtEq.Right }.Normalize();
      else if (Arg is NotOperator not)
        return not.Arg;
      else if (Arg is BooleanLiteral boolean)
        return new BooleanLiteral(!boolean.Value);
      else if (Arg is PropertyReference)
        return new EqualsOperator() { Left = Arg, Right = new BooleanLiteral(false) }.Normalize();

      if (Arg is ITableProvider tbl)
        ((ITableProvider)this).Table = tbl.Table;
      return this;
    }
  }
}
