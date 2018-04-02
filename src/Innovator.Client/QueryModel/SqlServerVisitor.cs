using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class SqlServerVisitor : IQueryVisitor
  {
    private TextWriter _writer;
    private Stack<ILogical> _logicals = new Stack<ILogical>();
    private IServerContext _context = ElementFactory.Utc.LocalizationContext;

    public void Visit(Query query)
    {
      _writer.Write("select ");

      if (query.Select.Count < 0)
      {
        _writer.Write("*");
      }
      else
      {
        var first = true;
        foreach (var prop in query.Select)
        {
          if (!first)
            _writer.Write(", ");
          first = false;
          prop.Visit(this);
        }
      }

      _writer.Write(" from ");
      query.From.Visit(this);
      if (query.Where != null)
      {
        _writer.Write(" where ");
        query.Where.Visit(this);
      }
      if (query.OrderBy.Count > 0)
      {
        _writer.Write(" order by ");
        var first = true;
        foreach (var prop in query.OrderBy)
        {
          if (!first)
            _writer.Write(", ");
          first = false;
          Visit(prop);
        }
      }
    }

    private void Visit(OrderByExpression op)
    {
      op.Expression.Visit(this);
      if (!op.Ascending)
        _writer.Write(" DESC");
    }

    public void Visit(AndOperator op)
    {
      var paren = _logicals.Count > 0 && _logicals.Peek() is NotOperator;

      if (paren)
        _writer.Write('(');
      _logicals.Push(op);
      var first = true;
      foreach (var expr in op.Args)
      {
        if (!first)
          _writer.Write(" and ");
        first = false;
        expr.Visit(this);
      }
      _logicals.Pop();
      if (paren)
        _writer.Write(')');
    }

    public void Visit(BetweenOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" between ");
      op.Min.Visit(this);
      _writer.Write(" and ");
      op.Max.Visit(this);
    }

    public void Visit(BooleanLiteral op)
    {
      _writer.Write('\'');
      _writer.Write(_context.Format(op.Value));
      _writer.Write('\'');
    }

    public void Visit(DateTimeLiteral op)
    {
      _writer.Write('\'');
      _writer.Write(_context.Format(op.Value));
      _writer.Write('\'');
    }

    public void Visit(EqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" = ");
      op.Right.Visit(this);
    }

    public void Visit(FloatLiteral op)
    {
      _writer.Write(_context.Format(op.Value));
    }

    public void Visit(FunctionExpression op)
    {
      var name = op.Name;
      switch (name.ToLowerInvariant())
      {
        case "getdate":
          name = "GetUtcDate";
          break;
      }
      _writer.Write(name);
      _writer.Write('(');
      var first = true;
      foreach (var arg in op.Args)
      {
        if (!first)
          _writer.Write(", ");
        first = false;
        arg.Visit(this);
      }
      _writer.Write(')');
    }

    public void Visit(GreaterThanOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" > ");
      op.Right.Visit(this);
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" >= ");
      op.Right.Visit(this);
    }

    public void Visit(InOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" in ");
      op.Right.Visit(this);
    }

    public void Visit(IntegerLiteral op)
    {
      _writer.Write(_context.Format(op.Value));
    }

    public void Visit(IsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" is ");
      switch (op.Right)
      {
        case IsOperand.@null:
        case IsOperand.notDefined:
          _writer.Write(" null");
          break;
        default:
          _writer.Write(" not null");
          break;
      }
    }

    public void Visit(LessThanOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" < ");
      op.Right.Visit(this);
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" <= ");
      op.Right.Visit(this);
    }

    public void Visit(LikeOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" like ");
      op.Right.Visit(this);
    }

    public void Visit(ListExpression op)
    {
      _writer.Write('(');
      var first = true;
      foreach (var arg in op.Values)
      {
        if (!first)
          _writer.Write(", ");
        first = false;
        arg.Visit(this);
      }
      _writer.Write(')');
    }

    public void Visit(NotBetweenOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" not between ");
      op.Min.Visit(this);
      _writer.Write(" and ");
      op.Max.Visit(this);
    }

    public void Visit(NotEqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" <> ");
      op.Right.Visit(this);
    }

    public void Visit(NotInOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" not in ");
      op.Right.Visit(this);
    }

    public void Visit(NotLikeOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" not like ");
      op.Right.Visit(this);
    }

    public void Visit(NotOperator op)
    {
      _logicals.Push(op);
      op.Right.Visit(this);
      _logicals.Pop();
    }

    public void Visit(ObjectLiteral op)
    {
      if (op.Value.IsGuid())
        _writer.Write('\'');
      else
        _writer.Write("N'");
      _writer.Write(op.Value.Replace("'", "''"));
      _writer.Write('\'');
    }

    public void Visit(OrOperator op)
    {
      var paren = _logicals.Count > 0 && !(_logicals.Peek() is OrOperator);

      if (paren)
        _writer.Write('(');
      _logicals.Push(op);
      var first = true;
      foreach (var expr in op.Args)
      {
        if (!first)
          _writer.Write(" or ");
        first = false;
        expr.Visit(this);
      }
      _logicals.Pop();
      if (paren)
        _writer.Write(')');
    }

    public void Visit(PropertyReference op)
    {
      _writer.Write('[');
      _writer.Write(op.Table.Alias);
      _writer.Write("].[");
      _writer.Write(op.Name);
      _writer.Write(']');
    }

    public void Visit(StringLiteral op)
    {
      if (op.Value.IsGuid())
        _writer.Write('\'');
      else
        _writer.Write("N'");
      _writer.Write(op.Value.Replace("'", "''"));
      _writer.Write('\'');
    }

    public void Visit(Join op)
    {
      op.Left.Visit(this);
      switch (op.Type)
      {
        case JoinType.LeftOuter:
          _writer.Write(" left join ");
          break;
        case JoinType.RightOuter:
          _writer.Write(" right join ");
          break;
        default:
          _writer.Write(" inner join ");
          break;
      }
      op.Right.Visit(this);
      if (op.Condition != null)
      {
        _writer.Write(" on ");
        op.Condition.Visit(this);
      }
    }

    public void Visit(Table op)
    {
      if (!string.IsNullOrEmpty(op.Namespace))
      {
        _writer.Write(op.Namespace);
        _writer.Write('.');
      }
      _writer.Write('[');
      _writer.Write(op.Name);
      _writer.Write(']');
      if (!string.Equals(op.Alias, op.Name, StringComparison.OrdinalIgnoreCase))
      {
        _writer.Write(" as ");
        _writer.Write(op.Alias);
      }
    }
  }
}
