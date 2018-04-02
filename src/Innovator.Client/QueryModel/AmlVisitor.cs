using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client.QueryModel
{
  class AmlVisitor : IQueryVisitor
  {
    private XmlWriter _writer;

    public void Visit(AndOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(BetweenOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(BooleanLiteral op)
    {
      throw new NotImplementedException();
    }

    public void Visit(DateTimeLiteral op)
    {
      throw new NotImplementedException();
    }

    public void Visit(EqualsOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(FloatLiteral op)
    {
      throw new NotImplementedException();
    }

    public void Visit(FunctionExpression op)
    {
      throw new NotImplementedException();
    }

    public void Visit(GreaterThanOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(InOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(IntegerLiteral op)
    {
      throw new NotImplementedException();
    }

    public void Visit(IsOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(LessThanOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(LikeOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(ListExpression op)
    {
      throw new NotImplementedException();
    }

    public void Visit(NotBetweenOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(NotEqualsOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(NotInOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(NotLikeOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(NotOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(ObjectLiteral op)
    {
      throw new NotImplementedException();
    }

    public void Visit(OrOperator op)
    {
      throw new NotImplementedException();
    }

    public void Visit(PropertyReference op)
    {
      throw new NotImplementedException();
    }

    public void Visit(StringLiteral op)
    {
      throw new NotImplementedException();
    }

    public void Visit(Join op)
    {
      throw new NotImplementedException();
    }

    public void Visit(Table op)
    {
      throw new NotImplementedException();
    }

    public void Visit(Query query)
    {
      throw new NotImplementedException();
    }
  }
}
