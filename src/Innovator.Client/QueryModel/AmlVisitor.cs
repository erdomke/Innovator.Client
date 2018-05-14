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
    private static readonly HashSet<string> _attributesToWrite = new HashSet<string>()
    {
      "queryDate"
    };

    private XmlWriter _writer;
    private SqlServerVisitor _sqlVisitor;
    private IServerContext _context;

    private Stack<ILogical> _logicals = new Stack<ILogical>();

    public AmlVisitor(IServerContext context, XmlWriter writer)
    {
      _writer = writer;
      _context = context;
      _sqlVisitor = new SqlServerVisitor(new XmlTextWriter(writer), new NullAmlSqlWriterSettings(), context);
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
      if (IsIsCurrentCriteria(op))
        return;
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
      Visit(op, "gt");
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      Visit(op, "ge");
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
        case IsOperand.Defined:
          condition = "is defined";
          break;
        case IsOperand.NotDefined:
          condition = "is not defined";
          break;
        case IsOperand.NotNull:
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
      Visit(op, "lt");
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      Visit(op, "le");
    }

    public void Visit(LikeOperator op)
    {
      Visit(op, "like");
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
      Visit(op, "ne");
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
      Visit(op, "not like");
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

    private void Visit(BinaryOperator op, string condition)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      _writer.WriteStartElement(prop.Name);
      _writer.WriteAttributeString("condition", condition);
      op.Right.Visit(this);
      _writer.WriteEndElement();
    }

    public void Visit(QueryItem query)
    {
      _logicals.Push(new AndOperator());
      _writer.WriteStartElement("Item");

      if (!string.IsNullOrEmpty(query.Type))
        _writer.WriteAttributeString("type", query.Type);
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
      foreach (var attr in query.Attributes.Where(a => _attributesToWrite.Contains(a.Key)))
      {
        _writer.WriteAttributeString(attr.Key, attr.Value);
      }
      var isCurrentVisitor = new IsCurrentVisitor();
      query.Where?.Visit(isCurrentVisitor);
      if (!isCurrentVisitor.IsCurrent
        && !isCurrentVisitor.GenerationCriteria)
      {
        _writer.WriteAttributeString("queryType", "Latest");
      }

      if (query.Select.Any())
      {
        if (!query.Select.All(s => s.Expression is PropertyReference))
          throw new NotSupportedException();
        _writer.WriteAttributeString("select"
          , query.Select.Select(s => ((PropertyReference)s.Expression).Name).GroupConcat(","));
      }

      if (query.OrderBy.Any())
      {
        if (!query.OrderBy.All(s => s.Expression is PropertyReference))
          throw new NotSupportedException();
        _writer.WriteAttributeString("orderBy"
          , query.OrderBy.Select(s => ((PropertyReference)s.Expression).Name + (s.Ascending ? "" : " DESC")).GroupConcat(","));
      }

      if (isCurrentVisitor.IdExpr != null)
      {
        if (isCurrentVisitor.IdExpr is StringLiteral str)
        {
          _writer.WriteAttributeString("id", str.Value);
        }
        else if (isCurrentVisitor.IdExpr is ListExpression listOp)
        {
          _writer.WriteAttributeString("idlist"
            , listOp.Values.OfType<StringLiteral>().Select(s => s.Value).GroupConcat(","));
        }
        else
        {
          throw new NotSupportedException();
        }
      }
      else
      {
        query.Where?.Visit(this);
      }

      var rels = new List<QueryItem>();
      foreach (var join in query.Joins.Where(j => j.Condition is EqualsOperator))
      {
        var eq = (EqualsOperator)join.Condition;
        var props = new[] {
          eq.Left as PropertyReference,
          eq.Right as PropertyReference
        }.Where(p => p != null).ToArray();
        if (props.Length != 2 && join.Type == JoinType.Inner)
          throw new NotSupportedException();

        var currProp = props.Single(p => object.ReferenceEquals(p.Table, query));
        var otherProp = props.Single(p => !object.ReferenceEquals(p.Table, query));
        if (currProp.Name == "id" && otherProp.Name == "source_id")
        {
          rels.Add(join.Right);
        }
        else if (otherProp.Name == "id")
        {
          _writer.WriteStartElement(currProp.Name);
          Visit(join.Right);
          _writer.WriteEndElement();
        }
        else
        {
          throw new NotSupportedException();
        }
      }

      if (rels.Count > 0)
      {
        _writer.WriteStartElement("Relationships");
        foreach (var rel in rels)
        {
          Visit(rel);
        }
        _writer.WriteEndElement();
      }

      _writer.WriteEndElement();
      _logicals.Pop();
    }

    private static bool IsIsCurrentCriteria(EqualsOperator op)
    {
      var prop = op.Left as PropertyReference;
      var boolLiteral = op.Right as BooleanLiteral;
      return prop?.Name == "is_current"
        && boolLiteral?.Value == true;
    }

    public void Visit(MultiplicationOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(DivisionOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(ModulusOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(AdditionOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(SubtractionOperator op)
    {
      throw new NotSupportedException();
    }

    private class IsCurrentVisitor : SimpleVisitor
    {
      private List<IOperator> _ops = new List<IOperator>();

      public bool GenerationCriteria { get; set; }
      public bool IsCurrent { get; set; } = false;
      public IExpression IdExpr { get; set; }

      public override void Visit(AndOperator op)
      {
        _ops.Add(op);
        base.Visit(op);
        _ops.Pop();
      }

      public override void Visit(EqualsOperator op)
      {
        IsCurrent = IsIsCurrentCriteria(op);
        if ((op.Left as PropertyReference)?.Name == "id"
          && op.Right is StringLiteral
          && _ops.All(o => o is AndOperator))
        {
          GenerationCriteria = true;
          IdExpr = op.Right;
        }
      }

      public override void Visit(InOperator op)
      {
        if ((op.Left as PropertyReference)?.Name == "id"
          && _ops.All(o => o is AndOperator))
        {
          GenerationCriteria = true;
          IdExpr = op.Right;
        }
      }

      public override void Visit(NotOperator op)
      {
        _ops.Add(op);
        base.Visit(op);
        _ops.Pop();
      }

      public override void Visit(OrOperator op)
      {
        _ops.Add(op);
        base.Visit(op);
        _ops.Pop();
      }

      public override void Visit(PropertyReference op)
      {
        if (op.Name == "generation" || op.Name == "id")
          GenerationCriteria = true;
      }
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
