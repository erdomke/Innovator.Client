using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client.QueryModel
{
  class AmlVisitor : IQueryVisitor
  {
    private XmlWriter _writer;
    private SqlServerVisitor _sqlVisitor;
    private IServerContext _context;

    private Stack<ILogical> _logicals = new Stack<ILogical>();

    public AmlVisitor(XmlWriter writer)
    {
      _writer = writer;
      _sqlVisitor = new SqlServerVisitor(new XmlTextWriter(writer), new NullAmlSqlWriterSettings());
    }

    public void Visit(AndOperator op)
    {
      var needsElem = _logicals.Peek() is OrOperator;
      if (needsElem)
        _writer.WriteStartElement("and");

      _logicals.Push(op);
      op.Left.Visit(this);
      op.Right.Visit(this);
      _logicals.Pop();

      if (needsElem)
        _writer.WriteEndElement();
    }

    public void Visit(BetweenOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "between");
      op.Min.Visit(_sqlVisitor);
      _writer.WriteString(" and ");
      op.Max.Visit(_sqlVisitor);
      _writer.WriteEndElement();
    }

    public void Visit(BooleanLiteral op)
    {
      _writer.WriteString(_context.Format(op.Value));
    }

    public void Visit(DateTimeLiteral op)
    {
      _writer.WriteString(_context.Format(op.Value));
    }

    public void Visit(EqualsOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(FloatLiteral op)
    {
      _writer.WriteString(_context.Format(op.Value));
    }

    public void Visit(FunctionExpression op)
    {
      if (string.Equals(op.Name, "GetDate", StringComparison.OrdinalIgnoreCase)
        || string.Equals(op.Name, "GetUtcDate", StringComparison.OrdinalIgnoreCase))
      {
        _writer.WriteString("__now()");
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    public void Visit(GreaterThanOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "gt");
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "ge");
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(InOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "in");
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(IntegerLiteral op)
    {
      _writer.WriteString(_context.Format(op.Value));
    }

    public void Visit(IsOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);

      var condition = default(string);
      switch (op.Right)
      {
        case IsOperand.defined:
          condition = "is defined";
          break;
        case IsOperand.notDefined:
          condition = "is not defined";
          break;
        case IsOperand.notNull:
          condition = "is not null";
          break;
        default:
          condition = "is null";
          break;
      }
      _writer.WriteAttributeString("condition", condition);
      _writer.WriteEndElement();
    }

    public void Visit(LessThanOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "lt");
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "le");
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(LikeOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "like");
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(ListExpression op)
    {
      var first = true;
      foreach (var arg in op.Values)
      {
        if (!first)
          _writer.WriteString(", ");
        first = false;
        arg.Visit(_sqlVisitor);
      }
    }

    public void Visit(NotBetweenOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "not between");
      op.Min.Visit(_sqlVisitor);
      _writer.WriteString(" and ");
      op.Max.Visit(_sqlVisitor);
      _writer.WriteEndElement();
    }

    public void Visit(NotEqualsOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "ne");
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(NotInOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "not in");
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(NotLikeOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", "not like");
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(NotOperator op)
    {
      _writer.WriteStartElement("not");
      op.Arg.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(ObjectLiteral op)
    {
      _writer.WriteString(op.Value);
    }

    public void Visit(OrOperator op)
    {
      var needsElem = !(_logicals.Peek() is OrOperator);
      if (needsElem)
        _writer.WriteStartElement("or");

      _logicals.Push(op);
      op.Left.Visit(this);
      op.Right.Visit(this);
      _logicals.Pop();

      if (needsElem)
        _writer.WriteEndElement();
    }

    public void Visit(PropertyReference op)
    {
      throw new NotSupportedException();
    }

    public void Visit(StringLiteral op)
    {
      _writer.WriteString(op.Value);
    }

    public void Visit(QueryItem query)
    {
      _logicals.Push(new AndOperator());
      _writer.WriteStartElement("Item");

      if (!string.IsNullOrEmpty(query.Name))
        _writer.WriteAttributeString("type", query.Name);
      if (!string.IsNullOrEmpty(query.Alias))
        _writer.WriteAttributeString("alias", query.Alias);

      if (query.Fetch > 0)
      {
        if ((query.Offset ?? 0) < 1)
        {
          _writer.WriteAttributeString("maxRecords", query.Fetch.Value.ToString());
        }
        else
        {
          if ((query.Offset.Value % query.Fetch) == 0)
          {
            _writer.WriteAttributeString("page", (query.Offset.Value / query.Fetch + 1).ToString());
            _writer.WriteAttributeString("pagesize", query.Fetch.Value.ToString());
          }
          else
          {
            _writer.WriteAttributeString("fetch", query.Fetch.Value.ToString());
            _writer.WriteAttributeString("offset", query.Offset.Value.ToString());
          }
        }
      }

      _writer.WriteAttributeString("action", "get");
      foreach (var attr in query.Attributes)
      {
        _writer.WriteAttributeString(attr.Key, attr.Value);
      }
      query.Where?.Visit(this);
      _writer.WriteEndElement();
      _logicals.Pop();
    }

    private class XmlTextWriter : TextWriter
    {
      private XmlWriter _writer;

      public override Encoding Encoding => Encoding.UTF8;

      public XmlTextWriter(XmlWriter writer)
      {
        _writer = writer;
      }

      public override void Write(char value)
      {
        _writer.WriteString(value.ToString());
      }

      public override void Write(char[] buffer, int index, int count)
      {
        _writer.WriteChars(buffer, index, count);
      }

      public override void Write(string value)
      {
        _writer.WriteString(value);
      }
    }
  }
}
