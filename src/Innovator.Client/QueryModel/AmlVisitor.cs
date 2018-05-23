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
    private QueryItem _currQuery;

    public AmlVisitor(IServerContext context, XmlWriter writer)
    {
      _writer = writer;
      _context = context;
      _sqlVisitor = new SqlServerVisitor(new XmlTextWriter(writer), new NullAmlSqlWriterSettings(), context);
    }

    public void Visit(AndOperator op)
    {
      EnterContext(op, () =>
      {
        op.Left.Visit(this);
        op.Right.Visit(this);
      });
    }

    public void Visit(BetweenOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();

      EnterContext(op, () =>
      {
        _writer.WriteStartElement(prop.Name);
        _writer.WriteAttributeString("condition", "between");
        op.Min.Visit(_sqlVisitor);
        _writer.WriteString(" and ");
        op.Max.Visit(_sqlVisitor);
        _writer.WriteEndElement();
      });
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
      EnterContext(op, () =>
      {
        _writer.WriteStartElement(prop.Name);
        op.Right.Visit(this);
        _writer.WriteEndElement();
      });
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
      EnterContext(op, () => Visit(op, "gt"));
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      EnterContext(op, () => Visit(op, "ge"));
    }

    public void Visit(InOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();

      EnterContext(op, () =>
      {
        _writer.WriteStartElement(prop.Name);
        _writer.WriteAttributeString("condition", "in");
        op.Right.Visit(this);
        _writer.WriteEndElement();
      });
    }

    public void Visit(IntegerLiteral op)
    {
      _writer.WriteString(_context.Format(op.Value));
    }

    public void Visit(IsOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();

      EnterContext(op, () =>
      {
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
      });
    }

    public void Visit(LessThanOperator op)
    {
      EnterContext(op, () => Visit(op, "lt"));
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      EnterContext(op, () => Visit(op, "le"));
    }

    public void Visit(LikeOperator op)
    {
      EnterContext(op, () => Visit(op, "like"));
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

    public void Visit(NotEqualsOperator op)
    {
      EnterContext(op, () => Visit(op, "ne"));
    }

    public void Visit(NotOperator op)
    {
      EnterContext(op, () => op.Arg.Visit(this));
    }

    public void Visit(ObjectLiteral op)
    {
      _writer.WriteString(_context.Format(op.Value));
    }

    public void Visit(OrOperator op)
    {
      EnterContext(op, () =>
      {
        op.Left.Visit(this);
        op.Right.Visit(this);
      });
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

    private void EnterContext(ITableProvider op, Action callback)
    {
      var origQuery = _currQuery;
      var path = BuildPath(origQuery, op.Table);
      var needsLogical = false;
      try
      {
        _currQuery = op.Table ?? origQuery;
        foreach (var elem in path)
        {
          _writer.WriteStartElement(elem);
          if (elem == "Item")
            _writer.WriteAttributeString("action", "get");
        }
        if (path.Count > 0)
          _logicals.Push(new AndOperator());

        needsLogical = op is NotOperator
          || (op is AndOperator && _logicals.Peek() is OrOperator)
          || (op is OrOperator && !(_logicals.Peek() is OrOperator));
        if (needsLogical)
        {
          if (op is NotOperator)
            _writer.WriteStartElement("not");
          else if (op is AndOperator)
            _writer.WriteStartElement("and");
          else if (op is OrOperator)
            _writer.WriteStartElement("or");
        }

        if (op is ILogical log)
          _logicals.Push(log);

        callback();
      }
      finally
      {
        if (op is ILogical)
          _logicals.Pop();
        if (needsLogical)
          _writer.WriteEndElement();
        if (path.Count > 0)
          _logicals.Pop();
        foreach (var elem in path)
          _writer.WriteEndElement();
        _currQuery = origQuery;
      }
    }

    private IList<string> BuildPath(QueryItem start, QueryItem end)
    {
      var path = new List<string>();

      var curr = end;
      while (curr != null && !ReferenceEquals(curr, start))
      {
        path.Add("Item");
        path.Add(curr.TypeProvider.Name);
        curr = curr.TypeProvider.Table;
      }

      path.Reverse();
      return path;
    }

    public void Visit(QueryItem query)
    {
      _logicals.Push(new AndOperator());
      _writer.WriteStartElement("Item");

      if (!string.IsNullOrEmpty(query.Type))
        _writer.WriteAttributeString("type", query.Type);
      if (!string.IsNullOrEmpty(query.Alias))
        _writer.WriteAttributeString("alias", query.Alias);
      _writer.WriteAttributeString("action", "get");
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
            throw new NotSupportedException();
          }
        }
      }
      else if (query.Offset > 0)
      {
        _writer.WriteAttributeString("page", "2");
        _writer.WriteAttributeString("pagesize", query.Offset.Value.ToString());
      }

      foreach (var attr in query.Attributes.Where(a => _attributesToWrite.Contains(a.Key)))
      {
        _writer.WriteAttributeString(attr.Key, attr.Value);
      }
      var isCurrentVisitor = new IsCurrentVisitor();
      query.Where?.Visit(isCurrentVisitor);
      if (!isCurrentVisitor.IsCurrent
        && !isCurrentVisitor.GenerationCriteria
        && !(query.Where is BooleanLiteral))
      {
        _writer.WriteAttributeString("queryType", "Latest");
      }

      var joins = query.Joins.Select(j => new AmlJoin(query, j)).ToArray();
      if (joins.Any(j => !j.IsItemProperty() && !j.IsRelationship()))
        throw new NotSupportedException();

      var node = new SelectNode();
      GetSelect(node, query, joins);
      var includeNodes = node.Where(SelectAllOnly).ToArray();
      foreach (var include in includeNodes)
        node.Remove(include);

      if (!SelectAllOnly(node))
        _writer.WriteAttributeString("select", node.ToString());

      if (query.OrderBy.Count > 0)
      {
        if (!query.OrderBy.All(s => s.Expression is PropertyReference))
          throw new NotSupportedException();
        _writer.WriteAttributeString("orderBy"
          , query.OrderBy.Select(s => ((PropertyReference)s.Expression).Name + (s.Ascending ? "" : " DESC")).GroupConcat(","));
      }

      _currQuery = query;
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
      else if (isCurrentVisitor.IsCurrent && query.Where is AndOperator andOp
        && (andOp.Left is BooleanLiteral || andOp.Right is BooleanLiteral))
      {
        var boolean = new[] { andOp.Left, andOp.Right }.OfType<BooleanLiteral>().First();
        if (!boolean.Value)
          _writer.WriteAttributeString("id", "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
      }
      else if (query.Where is BooleanLiteral boolean)
      {
        if (!boolean.Value)
          _writer.WriteAttributeString("id", "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
      }
      else
      {
        query.Where?.Visit(this);
      }

      foreach (var join in joins.Where(j => j.IsItemProperty() && j.HasCriteria()))
      {
        _writer.WriteStartElement(join.CurrentProp.Name);
        Visit(join.Table);
        _writer.WriteEndElement();
      }

      foreach (var include in includeNodes)
      {
        _writer.WriteStartElement(include.Name);
        _writer.WriteStartElement("Item");
        _writer.WriteAttributeString("action", "get");
        _writer.WriteEndElement();
        _writer.WriteEndElement();
      }

      if (joins.Any(j => j.IsRelationship()))
      {
        _writer.WriteStartElement("Relationships");
        foreach (var rel in joins.Where(j => j.IsRelationship()))
        {
          Visit(rel.Table);
        }
        _writer.WriteEndElement();
      }

      _writer.WriteEndElement();
      _logicals.Pop();
    }

    private bool SelectAllOnly(SelectNode node)
    {
      return node.Count == 1 && node[0].Name == "*" && node[0].Function == "is_not_null()";
    }

    private void GetSelect(SelectNode parent, QueryItem item, IEnumerable<AmlJoin> joins)
    {
      if (item.Select.Count > 0)
      {
        foreach (var select in item.Select)
        {
          if (select.Expression is PropertyReference prop)
          {
            parent.Add(new SelectNode(prop.Name)
              .WithFunction(select.OnlyReturnNonNull ? "is_not_null()" : ""));
          }
          else if (select.Expression is AllProperties all)
          {
            parent.Add(new SelectNode(all.XProperties ? "xp-*" : "*")
              .WithFunction(select.OnlyReturnNonNull ? "is_not_null()" : ""));
          }
          else
          {
            throw new NotSupportedException();
          }
        }
      }
      else
      {
        parent.Add(new SelectNode("*").WithFunction("is_not_null()"));
      }

      foreach (var join in joins.Where(j => j.IsItemProperty() && !j.HasCriteria() && j.Table.Select.Count > 0))
      {
        var node = parent.EnsurePath(join.CurrentProp.Name);
        GetSelect(node, join.Table, join.Table.Joins.Select(j => new AmlJoin(join.Table, j)));
      }
    }

    private class AmlJoin
    {
      public PropertyReference CurrentProp { get; set; }
      public PropertyReference OtherProp { get; set; }
      public QueryItem Table { get; set; }

      public AmlJoin(QueryItem parent, Join join)
      {
        if (!(join.Condition is EqualsOperator eq))
          throw new NotSupportedException();
        var props = new[] { eq.Left, eq.Right }
          .OfType<PropertyReference>()
          .ToArray();
        if (props.Length != 2 && join.Type == JoinType.Inner)
          throw new NotSupportedException();

        CurrentProp = props.Single(p => ReferenceEquals(p.Table, parent));
        OtherProp = props.Single(p => !ReferenceEquals(p.Table, parent));
        Table = OtherProp.Table;
      }

      public bool IsRelationship()
      {
        return CurrentProp.Name == "id" && OtherProp.Name == "source_id";
      }

      public bool IsItemProperty()
      {
        return OtherProp.Name == "id";
      }

      public bool HasCriteria()
      {
        return !((Table?.Where is EqualsOperator e && e.Left is PropertyReference p && p.Name == "is_current")
          || Table.Where == null);
      }
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

    public void Visit(NegationOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(ConcatenationOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(ParameterReference op)
    {
      _writer.WriteString("@");
      _writer.WriteString(op.Name);
      if (op.IsRaw)
        _writer.WriteString("!");
    }

    public void Visit(AllProperties op)
    {
      throw new NotSupportedException();
    }

    public void Visit(PatternList op)
    {
      var writer = new SqlPatternWriter(PatternParser.SqlServer);
      op.Visit(writer);
      _writer.WriteString(writer.ToString());
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
