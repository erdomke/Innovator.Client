#if REFLECTION
using Innovator.Client.QueryModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Innovator.Client.Queryable
{
  internal class SelectVisitor : AmlVisitor
  {
    private QueryItem _table;
    private readonly List<string> _buffer = new List<string>();
    private readonly List<PropertyReference> _props = new List<PropertyReference>();

    public IEnumerable<PropertyReference> GetProperties(Expression e, QueryItem table)
    {
      _props.Clear();
      _table = table;
      _buffer.Clear();
      Visit(e);
      FlushBuffer();
      return _props;
    }

    private void FlushBuffer()
    {
      if (_buffer.Count > 0)
      {
        _props.Add(_table.GetProperty(_buffer));
        _buffer.Clear();
      }
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      FlushBuffer();
      return base.VisitBinary(b);
    }

    protected override Expression VisitMethodCall(MethodCallExpression m)
    {
      if (VisitAmlMethod(m))
        return m;
      return base.VisitMethodCall(m);
    }


    protected override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
    {
      foreach (var exp in original)
      {
        Visit(exp);
        FlushBuffer();
      }
      return original;
    }

    protected override object VisitProperty(Expression table, string name)
    {
      base.VisitProperty(table, name);
      _buffer.Add(name);
      return null;
    }

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
    {
      FlushBuffer();
      return base.VisitMemberAssignment(assignment);
    }
  }
}
#endif
