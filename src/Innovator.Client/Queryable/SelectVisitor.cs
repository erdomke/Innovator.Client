#if REFLECTION
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Innovator.Client.Queryable
{
  internal class SelectVisitor : AmlVisitor
  {
    private SelectNode _select;
    private List<string> _buffer = new List<string>();

    public SelectNode GetSelect(Expression e)
    {
      _select = new SelectNode();
      _buffer.Clear();
      Visit(e);
      FlushBuffer();
      return _select;
    }

    private void FlushBuffer()
    {
      if (_buffer.Count > 0)
      {
        _select.EnsurePath(_buffer.ToArray());
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

    protected override void VisitProperty(string name)
    {
      _buffer.Add(name);
    }

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
    {
      FlushBuffer();
      return base.VisitMemberAssignment(assignment);
    }
  }
}
#endif
