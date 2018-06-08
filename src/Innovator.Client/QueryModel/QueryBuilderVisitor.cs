using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client.QueryModel
{
  class QueryBuilderVisitor : IQueryVisitor
  {
    private XmlWriter _writer;
    private XmlWriter _conditionWriter;
    private IServerContext _context;
    private List<ParameterReference> _parameters = new List<ParameterReference>();


    public void Visit(QueryItem query)
    {
      var refId = Guid.NewGuid().ToArasId();

      _writer.WriteStartElement("Item");
      _writer.WriteAttributeString("type", "qry_QueryDefinition");
      _writer.WriteAttributeString("action", "qry_ExecuteQueryDefinition");

      _writer.WriteStartElement("Relationships");

      _writer.WriteStartElement("Item");
      _writer.WriteAttributeString("type", "qry_QueryItem");
      _writer.WriteElementString("alias", query.Alias);
      _writer.WriteStartElement("item_type");
      _writer.WriteAttributeString("keyed_name", query.Type);
      _writer.WriteEndElement();
      _writer.WriteElementString("ref_id", refId);

      if (!query.Select.All(s => s.Expression is PropertyReference))
        throw new NotSupportedException();

      if (!query.OrderBy.All(s => s.Expression is PropertyReference))
        throw new NotSupportedException();

      _writer.WriteStartElement("Relationships");

      foreach (var select in query.Select)
      {
        _writer.WriteStartElement("Item");
        _writer.WriteAttributeString("type", "qry_QueryItemSelectProperty");
        _writer.WriteElementString("property_name", ((PropertyReference)select.Expression).Name);
        _writer.WriteEndElement(); // Item
      }

      var idx = 10;
      foreach (var orderBy in query.OrderBy)
      {
        _writer.WriteStartElement("Item");
        _writer.WriteAttributeString("type", "qry_QueryItemSortProperty");
        _writer.WriteElementString("property_name", ((PropertyReference)orderBy.Expression).Name);
        _writer.WriteElementString("sort_order_direction", orderBy.Ascending ? "asc" : "desc");
        _writer.WriteElementString("sort_order", _context.Format(idx));
        _writer.WriteEndElement(); // Item
        idx += 10;
      }

      _writer.WriteEndElement();  // Relationships

      var where = query.Where;
      if (query.Version is CurrentVersion)
      {
        where = new AndOperator()
        {
          Left = where,
          Right = new EqualsOperator()
          {
            Left = new PropertyReference("is_current", query),
            Right = new BooleanLiteral(true)
          }.Normalize()
        }.Normalize();
      }

      if (where != null)
      {
        using (var strWriter = new StringWriter())
        using (_conditionWriter = XmlWriter.Create(strWriter, new XmlWriterSettings()
        {
          OmitXmlDeclaration = true
        }))
        {
          _conditionWriter.WriteStartElement("condition");
          query.Where.Visit(this);
          _conditionWriter.WriteEndElement();

          _conditionWriter.Flush();
          strWriter.Flush();

          _writer.WriteStartElement("filter_xml");
          _writer.WriteCData(strWriter.ToString());
          _writer.WriteEndElement();
        }
        _conditionWriter = null;
      }

      if (query.Offset.HasValue || query.Fetch.HasValue)
      {
        using (var strWriter = new StringWriter())
        using (_conditionWriter = XmlWriter.Create(strWriter, new XmlWriterSettings()
        {
          OmitXmlDeclaration = true
        }))
        {
          _conditionWriter.WriteStartElement("configuration");
          _conditionWriter.WriteStartElement("option");
          if (query.Offset.HasValue)
            _conditionWriter.WriteElementString("offset", _context.Format(query.Offset));
          if (query.Fetch.HasValue)
            _conditionWriter.WriteElementString("fetch", _context.Format(query.Fetch));
          _conditionWriter.WriteEndElement();
          _conditionWriter.WriteEndElement();

          _conditionWriter.Flush();
          strWriter.Flush();

          _writer.WriteStartElement("offset_fetch_xml");
          _writer.WriteCData(strWriter.ToString());
          _writer.WriteEndElement();
        }
        _conditionWriter = null;
      }

      _writer.WriteEndElement(); // Item

      foreach (var param in _parameters)
      {
        _writer.WriteStartElement("Item");
        _writer.WriteAttributeString("type", "qry_QueryParameter");
        _writer.WriteElementString("name", param.Name);
        _writer.WriteEndElement(); // Item
      }

      _writer.WriteStartElement("Item");
      _writer.WriteAttributeString("type", "qry_QueryReference");
      _writer.WriteElementString("child_ref_id", refId);
      _writer.WriteEndElement(); // Item

      _writer.WriteEndElement(); // Relationships
      _writer.WriteEndElement(); // Item
    }

    public void Visit(AndOperator op)
    {
      _conditionWriter.WriteStartElement("and");
      foreach (var part in Flatten(op))
      {
        part.Visit(this);
      }
      _conditionWriter.WriteEndElement();
    }

    public void Visit(BetweenOperator op)
    {
      op.ToConditional().Visit(this);
    }

    public void Visit(BooleanLiteral op)
    {
      _conditionWriter.WriteElementString("constant", _context.Format(op.Value));
    }

    public void Visit(DateTimeLiteral op)
    {
      _conditionWriter.WriteElementString("constant", _context.Format(op.Value));
    }

    public void Visit(EqualsOperator op)
    {
      _conditionWriter.WriteStartElement("eq");
      op.Left.Visit(this);
      op.Right.Visit(this);
      _conditionWriter.WriteEndElement();
    }

    public void Visit(FloatLiteral op)
    {
      _conditionWriter.WriteElementString("constant", _context.Format(op.Value));
    }

    void IExpressionVisitor.Visit(FunctionExpression op)
    {
      throw new NotSupportedException();
    }

    public void Visit(GreaterThanOperator op)
    {
      _conditionWriter.WriteStartElement("gt");
      op.Left.Visit(this);
      op.Right.Visit(this);
      _conditionWriter.WriteEndElement();
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      _conditionWriter.WriteStartElement("ge");
      op.Left.Visit(this);
      op.Right.Visit(this);
      _conditionWriter.WriteEndElement();
    }

    public void Visit(InOperator op)
    {
      op.ToConditional().Visit(this);
    }

    public void Visit(IntegerLiteral op)
    {
      _conditionWriter.WriteElementString("constant", _context.Format(op.Value));
    }

    public void Visit(IsOperator op)
    {
      switch (op.Right)
      {
        case IsOperand.NotDefined:
        case IsOperand.NotNull:
          _conditionWriter.WriteStartElement("not");
          _conditionWriter.WriteStartElement("null");
          op.Left.Visit(this);
          _conditionWriter.WriteEndElement();
          _conditionWriter.WriteEndElement();
          break;
        default:
          _conditionWriter.WriteStartElement("null");
          op.Left.Visit(this);
          _conditionWriter.WriteEndElement();
          break;
      }
    }

    public void Visit(LessThanOperator op)
    {
      _conditionWriter.WriteStartElement("lt");
      op.Left.Visit(this);
      op.Right.Visit(this);
      _conditionWriter.WriteEndElement();
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      _conditionWriter.WriteStartElement("le");
      op.Left.Visit(this);
      op.Right.Visit(this);
      _conditionWriter.WriteEndElement();
    }

    public void Visit(LikeOperator op)
    {
      _conditionWriter.WriteStartElement("like");
      op.Left.Visit(this);
      op.Right.Visit(this);
      _conditionWriter.WriteEndElement();
    }

    void IExpressionVisitor.Visit(ListExpression op)
    {
      throw new NotSupportedException();
    }

    public void Visit(NotEqualsOperator op)
    {
      _conditionWriter.WriteStartElement("ne");
      op.Left.Visit(this);
      op.Right.Visit(this);
      _conditionWriter.WriteEndElement();
    }

    public void Visit(NotOperator op)
    {
      _conditionWriter.WriteStartElement("not");
      op.Arg.Visit(this);
      _conditionWriter.WriteEndElement();
    }

    public void Visit(ObjectLiteral op)
    {
      _conditionWriter.WriteElementString("constant", _context.Format(op.Value));
    }

    public void Visit(OrOperator op)
    {
      _conditionWriter.WriteStartElement("or");
      foreach (var part in Flatten(op))
      {
        part.Visit(this);
      }
      _conditionWriter.WriteEndElement();
    }

    public void Visit(PropertyReference op)
    {
      _conditionWriter.WriteStartElement("property");
      _conditionWriter.WriteAttributeString("name", op.Name);
      _conditionWriter.WriteEndElement();
    }

    public void Visit(StringLiteral op)
    {
      _conditionWriter.WriteElementString("constant", _context.Format(op.Value));
    }

    void IExpressionVisitor.Visit(MultiplicationOperator op)
    {
      throw new NotSupportedException();
    }

    void IExpressionVisitor.Visit(DivisionOperator op)
    {
      throw new NotSupportedException();
    }

    void IExpressionVisitor.Visit(ModulusOperator op)
    {
      throw new NotSupportedException();
    }

    void IExpressionVisitor.Visit(AdditionOperator op)
    {
      throw new NotSupportedException();
    }

    void IExpressionVisitor.Visit(SubtractionOperator op)
    {
      throw new NotSupportedException();
    }

    void IExpressionVisitor.Visit(NegationOperator op)
    {
      throw new NotSupportedException();
    }

    void IExpressionVisitor.Visit(ConcatenationOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(ParameterReference op)
    {
      _conditionWriter.WriteElementString("constant", "$" + op.Name);
      _parameters.Add(op);
    }

    void IExpressionVisitor.Visit(AllProperties op)
    {
      throw new NotSupportedException();
    }

    public void Visit(PatternList op)
    {
      _writer.WriteString(PatternParser.SqlServer.Render(op));
    }

    private IEnumerable<IExpression> Flatten<T>(T op) where T : BinaryOperator
    {
      var parts = new List<IExpression>()
      {
        op.Left,
        op.Right
      };

      var i = 0;
      while (i < parts.Count)
      {
        var same = parts[i] as T;
        if (same == null)
        {
          i++;
        }
        else
        {
          parts.RemoveAt(i);
          parts.Add(same.Left);
          parts.Add(same.Right);
        }
      }

      return parts;
    }
  }
}
