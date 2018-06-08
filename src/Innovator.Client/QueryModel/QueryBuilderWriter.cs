using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Innovator.Client.QueryModel
{
  internal class QueryBuilderWriter : XmlWriter
  {
    private readonly Dictionary<string, string> _attrBuffer = new Dictionary<string, string>();
    private readonly StringBuilder _buffer = new StringBuilder();
    private string _name;
    private readonly Stack<object> _meta = new Stack<object>();
    private readonly IServerContext _context;

    private readonly List<QueryItem> _items = new List<QueryItem>();
    private readonly List<ParameterReference> _parameters = new List<ParameterReference>();
    private readonly List<QueryReference> _refs = new List<QueryReference>();

    public QueryItem Query { get; private set; }
    public override WriteState WriteState { get { return WriteState.Start; } }

    public QueryBuilderWriter(IServerContext context)
    {
      _context = context;
    }

#if XMLLEGACY
    public override void Close()
    {
      Flush();
    }
#endif

    public override void Flush()
    {
      // Do nothing
    }

    public override string LookupPrefix(string ns)
    {
      return AmlToModelWriter.PrefixFromNamespace(ns);
    }

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      throw new NotSupportedException();
    }

    public override void WriteCData(string text)
    {
      WriteString(text);
    }

    public override void WriteCharEntity(char ch)
    {
      WriteString(new string(ch, 1));
    }

    public override void WriteChars(char[] buffer, int index, int count)
    {
      WriteString(new string(buffer, index, count));
    }

    public override void WriteComment(string text)
    {
      // Do nothing
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
      // Do nothing
    }

    public override void WriteEndAttribute()
    {
      var value = _buffer.ToString();
      if (_meta.Peek() is string str
        && str == "Item"
        && _name == "type")
      {
        if (value == "qry_QueryItem")
        {
          _meta.Pop();
          var item = new QueryItem(_context);
          _items.Add(item);
          _meta.Push(item);
        }
        else if (value == "qry_QueryItemSelectProperty")
        {
          _meta.Pop();
          var select = new SelectExpression();
          _meta.OfType<QueryItem>().Last().Select.Add(select);
          _meta.Push(select);
        }
        else if (value == "qry_QueryItemSortProperty")
        {
          _meta.Pop();
          var orderBy = new SortedOrderBy();
          _meta.OfType<QueryItem>().Last().OrderBy.Add(orderBy);
          _meta.Push(orderBy);
        }
        else if (value == "qry_QueryParameter")
        {
          _meta.Pop();
          var param = new ParameterReference();
          _parameters.Add(param);
          _meta.Push(param);
        }
        else if (value == "qry_QueryReference")
        {
          _meta.Pop();
          var reference = new QueryReference();
          _refs.Add(reference);
          _meta.Push(reference);
        }
      }
      _attrBuffer[_name] = value;
      _buffer.Length = 0;
    }

    public override void WriteEndDocument()
    {
      // Do nothing
    }

    public override void WriteEndElement()
    {
      var elem = _meta.Count < 1 ? null : _meta.Pop();
      var value = _buffer.ToString() ?? "";
      _buffer.Length = 0;

      if (elem is string name)
      {
        if (_meta.Peek() is QueryItem item)
        {
          switch (elem)
          {
            case "alias":
              item.Alias = value;
              break;
            case "item_type":
              if (_attrBuffer.TryGetValue("keyed_name", out var keyedName))
                item.Type = keyedName;
              else if (_attrBuffer.TryGetValue("name", out var type))
                item.Type = type;
              else
                item.Attributes["typeId"] = value;
              break;
            case "ref_id":
              item.Attributes["ref_id"] = value;
              break;
            case "filter_xml":
              item.Attributes["filter_xml"] = value;
              break;
            case "offset_fetch_xml":
              var offsetXml = XElement.Parse(value);
              item.Offset = (int?)offsetXml.Element("option")?.Element("offset");
              item.Fetch = (int?)offsetXml.Element("option")?.Element("fetch");
              break;
          }
        }
        else if (_meta.Peek() is SelectExpression select)
        {
          if (name == "property_name")
          {
            item = _meta.OfType<QueryItem>().Last();
            select.Expression = new PropertyReference(value, item);
          }
        }
        else if (_meta.Peek() is SortedOrderBy orderBy)
        {
          switch (name)
          {
            case "property_name":
              item = _meta.OfType<QueryItem>().Last();
              orderBy.Expression = new PropertyReference(value, item);
              break;
            case "sort_order":
              orderBy.SortOrder = int.Parse(value);
              break;
            case "sort_order_direction":
              orderBy.Ascending = value != "desc";
              break;
          }
        }
        else if (_meta.Peek() is ParameterReference param)
        {
          switch (name)
          {
            case "name":
              param.Name = value;
              break;
            case "value":
              param.DefaultValue = value;
              break;
          }
        }
        else if (_meta.Peek() is QueryReference queryRef)
        {
          switch (name)
          {
            case "child_ref_id":
              queryRef.ChildRefId = value;
              break;
            case "filter_xml":
              queryRef.FilterXml = value;
              break;
            case "parent_ref_id":
              queryRef.ParentRefId = value;
              break;
            case "start_query_reference_path":
              queryRef.StartQueryReferencePath = value;
              break;
          }
        }
        else if (name == "qry_QueryDefinition")
        {
          var dict = _items
            .ToDictionary(i =>
            {
              if (i.Attributes.TryGetValue("ref_id", out var refId))
                return refId;
              return Guid.NewGuid().ToArasId();
            });

          foreach (var reference in _refs.Where(r => !string.IsNullOrEmpty(r.ParentRefId)))
          {
            var parent = dict[reference.ParentRefId];
            var child = dict[reference.ChildRefId];
            parent.Joins.Add(new Join()
            {
              Condition = ProcessFilter(XElement.Parse(reference.FilterXml), x =>
              {
                if (string.Equals(x, "parent::Item", StringComparison.OrdinalIgnoreCase))
                  return parent;
                return child;
              }, null),
              Left = parent,
              Right = child,
              Type = JoinType.LeftOuter
            });
          }

          foreach (var i in _items)
          {
            if (i.Attributes.TryGetValue("filter_xml", out var filter))
            {
              i.Attributes.Remove(filter);
              i.Where = ProcessFilter(XElement.Parse(filter), x =>
              {
                if (string.Equals(x, "child::Item", StringComparison.OrdinalIgnoreCase))
                  return i.Joins[0].Right;
                return i;
              }, null);
            }
          }

          Query = dict[_refs
            .First(r => string.IsNullOrEmpty(r.ParentRefId))
            .ChildRefId];
        }
      }
    }

    private IExpression ProcessFilter(XElement elem, Func<string, QueryItem> itemCallback, PropertyReference prop)
    {
      switch (elem.Name.LocalName.ToLowerInvariant())
      {
        case "property":
          return itemCallback(elem.Attribute("query_items_xpath")?.Value)
            .GetProperty(new[] { elem.Attribute("name")?.Value });
        case "constant":
          if (long.TryParse(elem.Value, out var lng)
            || double.TryParse(elem.Value, out var dbl)
            || DateTime.TryParse(elem.Value, out var date))
          {
            return new ObjectLiteral(elem.Value, prop, _context);
          }
          else if (elem.Value.StartsWith("$"))
          {
            return _parameters.Find(p => p.Name == elem.Value.Substring(1))
              ?? new ParameterReference(elem.Value.Substring(1), false);
          }
          else
          {
            return new StringLiteral(elem.Value);
          }
        case "eq":
        case "ne":
        case "gt":
        case "ge":
        case "lt":
        case "le":
        case "like":
          var left = ProcessFilter(elem.Elements().First(), itemCallback, prop);
          var right = ProcessFilter(elem.Elements().ElementAt(1), itemCallback, left as PropertyReference);
          switch (elem.Name.LocalName.ToLowerInvariant())
          {
            case "eq":
              return new EqualsOperator() { Left = left, Right = right }.Normalize();
            case "ne":
              return new NotEqualsOperator() { Left = left, Right = right }.Normalize();
            case "gt":
              return new GreaterThanOperator() { Left = left, Right = right }.Normalize();
            case "ge":
              return new GreaterThanOrEqualsOperator() { Left = left, Right = right }.Normalize();
            case "lt":
              return new LessThanOperator() { Left = left, Right = right }.Normalize();
            case "le":
              return new LessThanOrEqualsOperator() { Left = left, Right = right }.Normalize();
            case "like":
              right = AmlLikeParser.Instance.Parse(right.ToString());
              return new LikeOperator() { Left = left, Right = right }.Normalize();
            default:
              throw new InvalidOperationException();
          }
        case "null":
          return new IsOperator()
          {
            Left = ProcessFilter(elem.Elements().First(), itemCallback, prop),
            Right = IsOperand.Null
          }.Normalize();
        case "and":
        case "or":
          var children = elem.Elements()
            .Select(e => ProcessFilter(e, itemCallback, prop))
            .ToArray();
          if (children.Length < 1)
          {
            throw new InvalidOperationException();
          }
          else if (children.Length == 1)
          {
            return children[0];
          }
          else if (string.Equals(elem.Name.LocalName, "and", StringComparison.OrdinalIgnoreCase))
          {
            var expr = new AndOperator()
            {
              Left = children[0],
              Right = children[1]
            }.Normalize();
            for (var i = 2; i < children.Length; i++)
            {
              expr = new AndOperator()
              {
                Left = expr,
                Right = children[i]
              }.Normalize();
            }
            return expr;
          }
          else if (string.Equals(elem.Name.LocalName, "or", StringComparison.OrdinalIgnoreCase))
          {
            var expr = new OrOperator()
            {
              Left = children[0],
              Right = children[1]
            }.Normalize();
            for (var i = 2; i < children.Length; i++)
            {
              expr = new OrOperator()
              {
                Left = expr,
                Right = children[i]
              }.Normalize();
            }
            return expr;
          }
          break;
        case "not":
          return new NotOperator()
          {
            Arg = ProcessFilter(elem.Elements().First(), itemCallback, prop)
          }.Normalize();
        case "condition":
          return ProcessFilter(elem.Elements().First(), itemCallback, prop);
      }
      throw new InvalidOperationException();
    }

    public override void WriteEntityRef(string name)
    {
      if (name == "amp")
      {
        WriteString("&");
      }
      else if (name == "apos")
      {
        WriteString("'");
      }
      else if (name == "gt")
      {
        WriteString(">");
      }
      else if (name == "lt")
      {
        WriteString("<");
      }
      else if (name == "quot")
      {
        WriteString("\"");
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    public override void WriteFullEndElement()
    {
      WriteEndElement();
    }

    public override void WriteProcessingInstruction(string name, string text)
    {
      throw new NotSupportedException();
    }

    public override void WriteRaw(char[] buffer, int index, int count)
    {
      throw new NotSupportedException();
    }

    public override void WriteRaw(string data)
    {
      throw new NotSupportedException();
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      _buffer.Length = 0;
      _name = localName;
    }

    public override void WriteStartDocument()
    {
      // Do nothing
    }

    public override void WriteStartDocument(bool standalone)
    {
      // Do nothing
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      _meta.Push(localName);
    }

    public override void WriteString(string text)
    {
      _buffer.Append(text);
    }

    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
      WriteString(new string(new char[] { highChar, lowChar }));
    }

    public override void WriteWhitespace(string ws)
    {
      WriteString(ws);
    }

    private class SortedOrderBy : OrderByExpression
    {
      public int SortOrder { get; set; }
    }

    private class QueryReference
    {
      public string ChildRefId { get; set; }
      public string FilterXml { get; set; }
      public string ParentRefId { get; set; }
      public string StartQueryReferencePath { get; set; }
    }
  }
}
