using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal class SimpleVisitor : IExpressionVisitor
  {
    public virtual void Visit(AndOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(BetweenOperator op)
    {
      op.Left.Visit(this);
      op.Min.Visit(this);
      op.Max.Visit(this);
    }

    public virtual void Visit(BooleanLiteral op) { }

    public virtual void Visit(DateTimeLiteral op) { }

    public virtual void Visit(EqualsOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(FloatLiteral op) { }

    public virtual void Visit(FunctionExpression op)
    {
      foreach (var arg in op.Args)
      {
        arg.Visit(this);
      }
    }

    public virtual void Visit(GreaterThanOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(GreaterThanOrEqualsOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(InOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(IntegerLiteral op) { }

    public virtual void Visit(IsOperator op)
    {
      op.Left.Visit(this);
    }

    public virtual void Visit(LessThanOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(LessThanOrEqualsOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(LikeOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(ListExpression op)
    {
      foreach (var arg in op.Values)
      {
        arg.Visit(this);
      }
    }

    public virtual void Visit(NotBetweenOperator op)
    {
      op.Left.Visit(this);
      op.Min.Visit(this);
      op.Max.Visit(this);
    }

    public virtual void Visit(NotEqualsOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(NotInOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(NotLikeOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(NotOperator op)
    {
      op.Arg.Visit(this);
    }

    public virtual void Visit(ObjectLiteral op) { }

    public virtual void Visit(OrOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(PropertyReference op) { }

    public virtual void Visit(StringLiteral op) { }

    public virtual void Visit(MultiplicationOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(DivisionOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(ModulusOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(AdditionOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(SubtractionOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(NegationOperator op)
    {
      op.Arg.Visit(this);
    }

    public virtual void Visit(ConcatenationOperator op)
    {
      op.Left.Visit(this);
      op.Right.Visit(this);
    }

    public virtual void Visit(ParameterReference op) { }

    public virtual void Visit(AllProperties op) { }
  }
}
