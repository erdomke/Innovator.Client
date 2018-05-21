using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public interface IExpressionVisitor
  {
    void Visit(AndOperator op);
    void Visit(BetweenOperator op);
    void Visit(BooleanLiteral op);
    void Visit(DateTimeLiteral op);
    void Visit(EqualsOperator op);
    void Visit(FloatLiteral op);
    void Visit(FunctionExpression op);
    void Visit(GreaterThanOperator op);
    void Visit(GreaterThanOrEqualsOperator op);
    void Visit(InOperator op);
    void Visit(IntegerLiteral op);
    void Visit(IsOperator op);
    void Visit(LessThanOperator op);
    void Visit(LessThanOrEqualsOperator op);
    void Visit(LikeOperator op);
    void Visit(ListExpression op);
    void Visit(NotBetweenOperator op);
    void Visit(NotEqualsOperator op);
    void Visit(NotInOperator op);
    void Visit(NotLikeOperator op);
    void Visit(NotOperator op);
    void Visit(ObjectLiteral op);
    void Visit(OrOperator op);
    void Visit(PropertyReference op);
    void Visit(StringLiteral op);
    void Visit(MultiplicationOperator op);
    void Visit(DivisionOperator op);
    void Visit(ModulusOperator op);
    void Visit(AdditionOperator op);
    void Visit(SubtractionOperator op);
    void Visit(NegationOperator op);
    void Visit(ConcatenationOperator op);
    void Visit(ParameterReference op);
    void Visit(AllProperties op);
  }
}
