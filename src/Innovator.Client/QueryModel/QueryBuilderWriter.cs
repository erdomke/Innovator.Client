using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            case "ref_id":
              queryRef.RefId = value;
              break;
            case "start_query_reference_path":
              queryRef.StartQueryReferencePath = value;
              break;
          }
        }
        else if (name == "qry_QueryDefinition")
        {
          var settings = new FilterSettings()
          {
            Items = _items
              .Where(i => i.Attributes.ContainsKey("ref_id"))
              .ToDictionary(i => i.Attributes["ref_id"]),
          };

          foreach (var reference in _refs.Where(r => !string.IsNullOrEmpty(r.ParentRefId)))
          {
            var parent = settings.Items[reference.ParentRefId];
            var child = settings.Items[reference.ChildRefId];
            settings.ItemCallback = x =>
            {
              if (string.Equals(x, "parent::Item", StringComparison.OrdinalIgnoreCase))
                return parent;
              return child;
            };
            var join = new Join()
            {
              Condition = ProcessFilter(XElement.Parse(reference.FilterXml), settings, null),
              Left = parent,
              Right = child,
              Type = JoinType.LeftOuter
            };
            parent.Joins.Add(join);
            settings.Joins[reference.RefId] = join;
          }

          foreach (var i in _items)
          {
            if (i.Attributes.TryGetValue("filter_xml", out var filter))
            {
              settings.ItemCallback = x =>
              {
                if (string.Equals(x, "child::Item", StringComparison.OrdinalIgnoreCase))
                  return i.Joins[0].Right;
                return i;
              };
              i.Attributes.Remove(filter);
              i.Where = ProcessFilter(XElement.Parse(filter), settings, null);
            }
          }

          var rootRef = _refs.First(r => string.IsNullOrEmpty(r.ParentRefId));

          Query = settings.Items[rootRef.ChildRefId];
          if (!string.IsNullOrEmpty(rootRef.FilterXml))
          {
            settings.ItemCallback = x =>
            {
              if (string.Equals(x, "child::Item", StringComparison.OrdinalIgnoreCase))
                return Query.Joins[0].Right;
              return Query;
            };
            Query.AddCondition(ProcessFilter(XElement.Parse(rootRef.FilterXml), settings, null));
          }
          Query.RebalanceCriteria();
        }
      }
    }

    private IExpression ReplaceParameters(string value)
    {
      const char invalid = '\uFFFF';

      var parameters = new List<ParameterReference>();
      var str = Regex.Replace(value, @"(\$\$)|(\$\w+)", match =>
      {
        if (match.Value == "$$")
          return "$";

        var param = _parameters.Find(p => p.Name == match.Value.Substring(1));
        if (param == null)
          throw new ArgumentException($"Invalid parameter name `{match.Value}`");

        parameters.Add(param);
        return invalid.ToString();
      });

      if (str == invalid.ToString() && parameters.Count == 1)
        return parameters[0];
      if (parameters.Count == 0)
        return new StringLiteral(str);

      var parts = str.Split(invalid);
      var expressions = new List<IExpression>();
      for (var i = 0; i < parts.Length; i++)
      {
        if (i > 0)
          expressions.Add(parameters[i - 1]);
        if (!string.IsNullOrEmpty(parts[i]))
          expressions.Add(new StringLiteral(parts[i]));
      }

      var result = expressions[0];
      for (var i = 1; i < expressions.Count; i++)
      {
        result = new ConcatenationOperator()
        {
          Left = result,
          Right = expressions[i]
        }.Normalize();
      }
      return result;
    }

    private class FilterSettings
    {
      public Func<string, QueryItem> ItemCallback { get; set; }
      public Dictionary<string, QueryItem> Items { get; set; }
      public Dictionary<string, Join> Joins { get; } = new Dictionary<string, Join>();
    }

    private IExpression ProcessFilter(XElement elem, FilterSettings settings, PropertyReference prop)
    {
      var elemName = elem.Name.LocalName.ToLowerInvariant();
      switch (elemName)
      {
        case "property":
          return settings.ItemCallback(elem.Attribute("query_items_xpath")?.Value)
            .GetProperty(new[] { elem.Attribute("name")?.Value });
        case "constant":
          if (long.TryParse(elem.Value, out var lng)
            || double.TryParse(elem.Value, out var dbl)
            || DateTime.TryParse(elem.Value, out var date))
          {
            return new ObjectLiteral(elem.Value, prop, _context);
          }
          else if (elem.Value.IndexOf('$') >= 0)
          {
            return ReplaceParameters(elem.Value);
          }
          else
          {
            return new StringLiteral(elem.Value);
          }
        case "count":
          var path = elem.Element("query_reference_path").Value.Split('/');

          var result = new CountAggregate();
          foreach (var table in path.Select(s => settings.Joins[s].Right))
          {
            result.TablePath.Add(table);
          }
          return result;
        case "eq":
        case "ne":
        case "gt":
        case "ge":
        case "lt":
        case "le":
        case "like":
          var left = ProcessFilter(elem.Elements().First(), settings, prop);
          var right = ProcessFilter(elem.Elements().ElementAt(1), settings, left as PropertyReference);

          if (left is CountAggregate cnt
            && right is IntegerLiteral iLit
            && Parents(elem)
              .All(p => string.Equals(p.Name.LocalName, "condition", StringComparison.OrdinalIgnoreCase)
                || string.Equals(p.Name.LocalName, "and", StringComparison.OrdinalIgnoreCase))
            && ((elemName == "gt" && iLit.Value == 0)
              || (elemName == "ge" && iLit.Value == 1)))
          {
            var refPath = elem.Element("count").Element("query_reference_path").Value.Split('/');
            foreach (var join in refPath.Select(s => settings.Joins[s]))
            {
              join.Type = JoinType.Inner;
            }
            return IgnoreNode.Instance;
          }

          switch (elemName)
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
            Left = ProcessFilter(elem.Elements().First(), settings, prop),
            Right = IsOperand.Null
          }.Normalize();
        case "and":
        case "or":
          var children = elem.Elements()
            .Select(e => ProcessFilter(e, settings, prop))
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
            Arg = ProcessFilter(elem.Elements().First(), settings, prop)
          }.Normalize();
        case "condition":
          return ProcessFilter(elem.Elements().First(), settings, prop);
      }
      throw new InvalidOperationException();
    }

    private IEnumerable<XElement> Parents(XElement elem)
    {
      var curr = elem.Parent;
      while (curr != null)
      {
        yield return curr;
        curr = curr.Parent;
      }
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
      public string RefId { get; set; }
      public string StartQueryReferencePath { get; set; }
    }
  }
}
