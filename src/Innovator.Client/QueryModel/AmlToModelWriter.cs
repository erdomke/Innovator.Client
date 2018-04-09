using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client.QueryModel
{
  public class AmlToModelWriter : XmlWriter
  {
    private readonly StringBuilder _buffer = new StringBuilder();
    private IServerContext _context;
    private string _name;
    private List<object> _stack = new List<object>();

    private QueryItem _query;
    public QueryItem Query { get { return _query; } }

    public AmlToModelWriter(IServerContext context)
    {
      _query = new QueryItem(context);
      _context = context;
    }

    /// <summary>Gets the state of the writer.</summary>
    /// <returns>One of the <see cref="WriteState" /> values.</returns>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override WriteState WriteState { get { return WriteState.Start; } }

#if XMLLEGACY
    /// <summary>Closes this stream and the underlying stream.</summary>
    /// <exception cref="InvalidOperationException">A call is made to write more output after Close has been called or the result of this call is an invalid XML document.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void Close()
    {
      Flush();
    }
#endif

    /// <summary>Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.</summary>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void Flush()
    {
      _buffer.Length = 0;
    }

    /// <summary>Returns the closest prefix defined in the current namespace scope for the namespace URI.</summary>
    /// <returns>The matching prefix or null if no matching namespace URI is found in the current scope.</returns>
    /// <param name="ns">The namespace URI whose prefix you want to find.</param>
    /// <exception cref="ArgumentException">
    ///   <paramref name="ns" /> is either null or String.Empty.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override string LookupPrefix(string ns)
    {
      return PrefixFromNamespace(ns);
    }

    internal static string PrefixFromNamespace(string ns)
    {
      switch ((ns ?? "").TrimEnd('/'))
      {
        case "http://schemas.xmlsoap.org/soap/envelope":
          return "SOAP-ENV";
        case "http://www.w3.org/XML/1998/namespace":
          return "xml";
        case "http://www.aras.com/InnovatorFault":
          return "af";
        case "http://www.aras.com/I18N":  //return "i18n";
        case "":
          return "";
      }
      throw new ArgumentException();
    }

    /// <summary>
    /// Not supported
    /// </summary>
    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Writes out a &lt;![CDATA[...]]&gt; block containing the specified text.
    /// </summary>
    /// <param name="text">The text to place inside the CDATA block.</param>
    public override void WriteCData(string text)
    {
      WriteString(text);
    }

    /// <summary>Forces the generation of a character entity for the specified Unicode character value.</summary>
    /// <param name="ch">The Unicode character for which to generate a character entity.</param>
    /// <exception cref="ArgumentException">The character is in the surrogate pair character range, 0xd800 - 0xdfff.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteCharEntity(char ch)
    {
      WriteString(new string(ch, 1));
    }

    /// <summary>Writes text one buffer at a time.</summary>
    /// <param name="buffer">Character array containing the text to write.</param>
    /// <param name="index">The position in the buffer indicating the start of the text to write.</param>
    /// <param name="count">The number of characters to write.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="buffer" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or-The buffer length minus <paramref name="index" /> is less than <paramref name="count" />; the call results in surrogate pair characters being split or an invalid surrogate pair being written.</exception>
    /// <exception cref="ArgumentException">The <paramref name="buffer" /> parameter value is not valid.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteChars(char[] buffer, int index, int count)
    {
      WriteString(new string(buffer, index, count));
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void WriteComment(string text)
    {
      // Do nothing
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
      // Do nothing
    }

    /// <summary>Closes the previous <see cref="M:System.Xml.XmlWriter.WriteStartAttribute(System.String,System.String)" /> call.</summary>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteEndAttribute()
    {
      var value = _buffer.ToString();
      _buffer.Length = 0;
      if (_stack.LastOrDefault() is Join join)
      {
        var table = join.Right;
        switch (_name)
        {
          case "type":
            table.Type = value;
            break;
          case "alias":
            table.Alias = value;
            break;
          case "action":
            if (value != "get")
              throw new NotSupportedException("The only action(s) supported are `get`");
            break;
          case "id":
            _stack.Add(new EqualsOperator()
            {
              Left = new PropertyReference("id", table),
              Right = new StringLiteral(value)
            });
            break;
          case "idlist":
            _stack.Add(new InOperator()
            {
              Left = new PropertyReference("id", table),
              Right = new ListExpression(value.Split(',').Select(i => (IOperand)new StringLiteral(i)))
            });
            break;
          case "where":
            throw new NotSupportedException();
          default:
            table.Attributes[_name] = value;
            break;
        }
      }
      else if (_stack.LastOrDefault() is PropertyReference)
      {
        switch (_name)
        {
          // Technically not valid AML, but why not.
          case "is_null":
            switch (value)
            {
              case "1":
                _stack.Add(new IsOperator() { Left = (IExpression)_stack.Pop(), Right = IsOperand.@null });
                break;
              case "0":
                _stack.Add(new IsOperator() { Left = (IExpression)_stack.Pop(), Right = IsOperand.notNull });
                break;
            }
            break;
          case "condition":
            switch (value)
            {
              case "between":
                _stack.Add(new BetweenOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "eq":
                _stack.Add(new EqualsOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "ge":
                _stack.Add(new GreaterThanOrEqualsOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "gt":
                _stack.Add(new GreaterThanOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "in":
                _stack.Add(new InOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "is":
                _stack.Add(new IsOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "is defined":
                _stack.Add(new IsOperator() { Left = (IExpression)_stack.Pop(), Right = IsOperand.defined });
                break;
              case "is not defined":
                _stack.Add(new IsOperator() { Left = (IExpression)_stack.Pop(), Right = IsOperand.notDefined });
                break;
              case "is not null":
                _stack.Add(new IsOperator() { Left = (IExpression)_stack.Pop(), Right = IsOperand.notNull });
                break;
              case "is null":
                _stack.Add(new IsOperator() { Left = (IExpression)_stack.Pop(), Right = IsOperand.@null });
                break;
              case "le":
                _stack.Add(new LessThanOrEqualsOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "like":
                _stack.Add(new LikeOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "lt":
                _stack.Add(new LessThanOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "ne":
                _stack.Add(new NotEqualsOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "not between":
                _stack.Add(new NotBetweenOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "not in":
                _stack.Add(new NotInOperator() { Left = (IExpression)_stack.Pop() });
                break;
              case "not like":
                _stack.Add(new NotLikeOperator() { Left = (IExpression)_stack.Pop() });
                break;
            }
            break;
        }
      }
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void WriteEndDocument()
    {
      // Do nothing
    }

    /// <summary>Closes one element and pops the corresponding namespace scope.</summary>
    /// <exception cref="InvalidOperationException">This results in an invalid XML document.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteEndElement()
    {
      if (_stack.Count > 0)
      {
        var value = _buffer.ToString();
        _buffer.Length = 0;

        var last = _stack.Pop();
        if (last is IsOperator isOp)
        {
          switch (value)
          {
            case "defined":
              isOp.Right = IsOperand.defined;
              break;
            case "not defined":
              isOp.Right = IsOperand.notDefined;
              break;
            case "not null":
              isOp.Right = IsOperand.notNull;
              break;
            case "null":
              isOp.Right = IsOperand.@null;
              break;
            default:
              throw new NotSupportedException();
          }
        }
        else if (last is InOperator inOp)
        {
          inOp.Right = ListExpression.FromSqlInClause(value);
        }
        else if (last is BetweenOp betweenOp)
        {
          betweenOp.SetMinMaxFromSql(value);
        }
        else if (last is PropertyReference prop)
        {
          if (_stack.OfType<Join>().Last().Right.Joins.Any(j => j.Right.TypeProvider == prop))
          {
            last = null;
          }
          else
          {
            last = new EqualsOperator()
            {
              Left = prop
            };
          }
        }

        if (!(last is ILogical) && last is BinaryOperator binOp)
        {
          if (value == "__now()")
          {
            binOp.Right = new FunctionExpression() { Name = "GetDate" };
          }
          else if (binOp is LikeOperator || binOp is NotLikeOperator)
          {
            binOp.Right = new StringLiteral(value.Replace('*', '%'));
          }
          else
          {
            binOp.Right = new ObjectLiteral(value, (PropertyReference)binOp.Left);
          }
        }

        if (last is IExpression expr)
        {
          AddToCondition(expr);
        }
        else if (last is Join join)
        {
          NormalizeItem(join.Right);
        }
      }
    }

    private void NormalizeItem(QueryItem item)
    {
      if (item.Attributes.TryGetValue("select", out var selectStmt))
      {
        var node = SelectNode.FromString(selectStmt);
        if (node.Count > 0 && string.IsNullOrEmpty(node[0].Name))
          node = node[0];

        foreach (var prop in node)
        {
          item.Select.Add(new SelectExpression() { Expression = new PropertyReference(prop.Name, item) });
          // TODO: Handle subselects
        }
        item.Attributes.Remove("select");
      }

      if (item.Attributes.TryGetValue("fetch", out var fetchStr) && int.TryParse(fetchStr, out var fetch))
      {
        item.Fetch = fetch;
        if (item.Attributes.TryGetValue("offset", out var offsetStr) && int.TryParse(offsetStr, out var offset))
          item.Offset = offset;
        else
          item.Offset = 0;
        item.Attributes.Remove("fetch");
        item.Attributes.Remove("offset");
      }
      else if (item.Attributes.TryGetValue("maxRecords", out var maxRecordsStr) && int.TryParse(maxRecordsStr, out var maxRecords))
      {
        item.Fetch = maxRecords;
        item.Offset = 0;
        item.Attributes.Remove("maxRecords");
      }
      else if (item.Attributes.TryGetValue("pagesize", out var pagesizeStr) && int.TryParse(pagesizeStr, out var pagesize))
      {
        item.Fetch = pagesize;
        if (item.Attributes.TryGetValue("page", out var pageStr) && int.TryParse(pageStr, out var page) && page > 1)
          item.Offset = (page - 1) * pagesize;
        else
          item.Offset = 0;

        item.Attributes.Remove("pagesize");
        item.Attributes.Remove("page");
      }

      if (item.Attributes.TryGetValue("orderBy", out var orderBy) && !string.IsNullOrEmpty(orderBy))
      {
        foreach (var col in orderBy.Split(',')
          .Select(c =>
          {
            var parts = c.Split(' ');
            if (parts.Length > 2)
              throw new NotSupportedException();
            if (parts.Length == 2
              && !string.Equals(parts[1], "asc", StringComparison.OrdinalIgnoreCase)
              && !string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase))
            {
              throw new NotSupportedException();
            }

            return new OrderByExpression()
            {
              Expression = new PropertyReference(parts[0], item),
              Ascending = !(parts.Length == 2 && string.Equals(parts[1], "DESC", StringComparison.OrdinalIgnoreCase))
            };
          }))
        {
          item.OrderBy.Add(col);
        }
        item.Attributes.Remove("orderBy");
      }

      var queryType = default(string);
      if (!item.Attributes.TryGetValue("queryType", out queryType))
        queryType = "Current";
      if (string.Equals(queryType, "ignore", StringComparison.OrdinalIgnoreCase)
        || string.Equals(queryType, "skip", StringComparison.OrdinalIgnoreCase))
      {
        // Do nothing
      }
      else if (string.Equals(queryType, "Current", StringComparison.OrdinalIgnoreCase))
      {
        var visitor = new PropNameVisitor();
        if (item.Where != null)
          item.Where.Visit(visitor);

        if (!visitor.PropertyNames.Contains("id") && !visitor.PropertyNames.Contains("generation"))
        {
          var whereClause = (IExpression)new EqualsOperator()
          {
            Left = new PropertyReference("is_current", item),
            Right = new BooleanLiteral(true)
          };

          if (item.Where != null)
          {
            whereClause = new AndOperator()
            {
              Left = item.Where,
              Right = whereClause
            };
          }

          item.Where = whereClause;
        }
      }
      else if (string.Equals(queryType, "Latest", StringComparison.OrdinalIgnoreCase))
      {
        var visitor = new PropNameVisitor();
        if (item.Where != null)
          item.Where.Visit(visitor);

        if (!visitor.PropertyNames.Contains("is_active_rev"))
          throw new NotSupportedException();
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    private class PropNameVisitor : SimpleVisitor
    {
      private HashSet<string> _props = new HashSet<string>();

      public HashSet<string> PropertyNames { get { return _props; } }

      public override void Visit(PropertyReference op)
      {
        base.Visit(op);
        _props.Add(op.Name);
      }
    }

    private void AddToCondition(IExpression expr)
    {
      var term = SimplifyTerm(expr);
      if (term == null)
        return;

      if (_stack.Last() is ILogical)
      {
        if (_stack.Last() is BinaryOperator binOp)
        {
          if (binOp.Left == null)
          {
            binOp.Left = term;
          }
          else if (binOp.Right == null)
          {
            binOp.Right = term;
          }
          else
          {
            var newOp = binOp is AndOperator ? (BinaryOperator)new AndOperator() : new OrOperator();
            newOp.Left = (IExpression)_stack.Pop();
            newOp.Right = term;
            _stack.Add(newOp);
          }
        }
        else if (_stack.Last() is NotOperator not)
        {
          if (not.Arg == null)
          {
            not.Arg = term;
          }
          else
          {
            not.Arg = new AndOperator()
            {
              Left = not.Arg,
              Right = term
            };
          }
        }
      }
      else if (_stack.Last() is Join join)
      {
        if (join.Right.Where == null)
        {
          join.Right.Where = term;
        }
        else
        {
          join.Right.Where = new AndOperator()
          {
            Left = join.Right.Where,
            Right = term
          };
        }
      }

    }

    private IExpression SimplifyTerm(IExpression expr)
    {
      if (expr is ILogical)
      {
        if (expr is BinaryOperator binOp)
        {
          if (binOp.Right == null)
            return binOp.Left;
        }
        else if (expr is NotOperator not)
        {
          if (not.Arg == null)
            return null;
        }
      }

      return expr;
    }

    /// <summary>Writes out an entity reference as &amp;name;.</summary>
    /// <param name="name">The name of the entity reference.</param>
    /// <exception cref="ArgumentException">
    ///   <paramref name="name" /> is either null or String.Empty.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
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

    /// <summary>Closes one element and pops the corresponding namespace scope.</summary>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteFullEndElement()
    {
      WriteEndElement();
    }

    /// <summary>
    /// Not supported
    /// </summary>
    public override void WriteProcessingInstruction(string name, string text)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Not supported
    /// </summary>
    public override void WriteRaw(string data)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Not supported
    /// </summary>
    public override void WriteRaw(char[] buffer, int index, int count)
    {
      throw new NotSupportedException();
    }

    /// <summary>Writes the start of an attribute with the specified prefix, local name, and namespace URI.</summary>
    /// <param name="prefix">The namespace prefix of the attribute.</param>
    /// <param name="localName">The local name of the attribute.</param>
    /// <param name="ns">The namespace URI for the attribute.</param>
    /// <exception cref="EncoderFallbackException">There is a character in the buffer that is a valid XML character but is not valid for the output encoding. For example, if the output encoding is ASCII, you should only use characters from the range of 0 to 127 for element and attribute names. The invalid character might be in the argument of this method or in an argument of previous methods that were writing to the buffer. Such characters are escaped by character entity references when possible (for example, in text nodes or attribute values). However, the character entity reference is not allowed in element and attribute names, comments, processing instructions, or CDATA sections. </exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      _buffer.Length = 0;
      _name = localName;
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void WriteStartDocument()
    {
      // Do nothing
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void WriteStartDocument(bool standalone)
    {
      // Do nothing
    }

    /// <summary>Writes the specified start tag and associates it with the given namespace and prefix.</summary>
    /// <param name="prefix">The namespace prefix of the element.</param>
    /// <param name="localName">The local name of the element.</param>
    /// <param name="ns">The namespace URI to associate with the element.</param>
    /// <exception cref="InvalidOperationException">The writer is closed.</exception>
    /// <exception cref="EncoderFallbackException">There is a character in the buffer that is a valid XML character but is not valid for the output encoding. For example, if the output encoding is ASCII, you should only use characters from the range of 0 to 127 for element and attribute names. The invalid character might be in the argument of this method or in an argument of previous methods that were writing to the buffer. Such characters are escaped by character entity references when possible (for example, in text nodes or attribute values). However, the character entity reference is not allowed in element and attribute names, comments, processing instructions, or CDATA sections.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      switch (localName)
      {
        case "Item":
          var prop = _stack.LastOrDefault() as PropertyReference;
          var join = new Join()
          {
            Left = _stack.OfType<Join>().LastOrDefault()?.Right,
            Right = new QueryItem(_context)
            {
              TypeProvider = prop
            },
            Type = JoinType.Inner,
          };

          if (join.Left != null)
          {
            if (prop != null)
            {
              join.Condition = new EqualsOperator()
              {
                Left = prop,
                Right = new PropertyReference("id", join.Right)
              };
              join.Left.Joins.Add(join);
            }
            else if (_stack.LastOrDefault() is Relationships)
            {
              join.Condition = new EqualsOperator()
              {
                Left = new PropertyReference("id", join.Left),
                Right = new PropertyReference("source_id", join.Right)
              };
              join.Type = JoinType.LeftOuter;
              join.Left.Joins.Add(join);
            }
            else
            {
              throw new NotSupportedException();
            }
          }
          else
          {
            _query = join.Right;
          }

          _stack.Add(join);
          break;
        case "and":
          _stack.Add(new AndOperator());
          break;
        case "or":
          _stack.Add(new OrOperator());
          break;
        case "not":
          _stack.Add(new NotOperator());
          break;
        case "Relationships":
          _stack.Add(new Relationships());
          break;
        default:
          if (_stack.Count > 0)
          {
            _stack.Add(new PropertyReference(localName, _stack.OfType<Join>().Last().Right));
          }
          break;
      }
    }

    /// <summary>Writes the given text content.</summary>
    /// <param name="text">The text to write.</param>
    /// <exception cref="ArgumentException">The text string contains an invalid surrogate pair.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteString(string text)
    {
      _buffer.Append(text);
    }

    /// <summary>Generates and writes the surrogate character entity for the surrogate character pair.</summary>
    /// <param name="lowChar">The low surrogate. This must be a value between 0xDC00 and 0xDFFF.</param>
    /// <param name="highChar">The high surrogate. This must be a value between 0xD800 and 0xDBFF.</param>
    /// <exception cref="ArgumentException">An invalid surrogate character pair was passed.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
      WriteString(new string(new char[] { highChar, lowChar }));
    }

    /// <summary>Writes out the given white space.</summary>
    /// <param name="ws">The string of white space characters.</param>
    /// <exception cref="ArgumentException">The string contains non-white space characters.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteWhitespace(string ws)
    {
      WriteString(ws);
    }

    // Placeholder class for the relationships tag
    private class Relationships
    {
    }
  }
}
