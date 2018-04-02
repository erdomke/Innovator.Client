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
    private string _name;
    private List<object> _stack = new List<object>();
    private Query _query = new Query();

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

    public override void Flush()
    {
      throw new NotImplementedException();
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
      _buffer.Length = 0;
      if (_stack.LastOrDefault() is ITableOperand tableOp)
      {
        var table = TableFromOp(tableOp);
        switch (_name)
        {
          case "type":
            table.Name = _buffer.ToString();
            break;
          case "typeId":
            table.Id = new Guid(_buffer.ToString());
            break;
          case "alias":
            table.Alias = _buffer.ToString();
            break;
          case "action":
            if (_buffer.ToString() != "get")
              throw new NotSupportedException("The only action(s) supported are `get`");
            break;
          case "id":
            _stack.Add(new EqualsOperator()
            {
              Left = new PropertyReference("id", table),
              Right = new StringLiteral(_buffer.ToString())
            });
            break;
          case "idlist":
            _stack.Add(new InOperator()
            {
              Left = new PropertyReference("id", table),
              Right = new ListExpression(_buffer.ToString().Split(',').Select(i => (IOperand)new StringLiteral(i)))
            });
            break;
          case "where":
            throw new NotSupportedException();
        }
      }
      else if (_stack.LastOrDefault() is PropertyReference)
      {
        switch (_name)
        {
          // Technically not valid AML, but why not.
          case "is_null":
            switch (_buffer.ToString())
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
            switch (_buffer.ToString())
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
          ((ILogical)_stack.Last()).Add(isOp);
        }
        else if (last is InOperator inOp)
        {
          inOp.Right = ListExpression.FromSqlInClause(value);
          ((ILogical)_stack.Last()).Add(inOp);
        }
        else if (last is BetweenOp betweenOp)
        {
          betweenOp.SetMinMaxFromSql(value);
          ((ILogical)_stack.Last()).Add(betweenOp);
        }
        else if (last is PropertyReference prop)
        {
          last = new EqualsOperator()
          {
            Left = prop
          };
        }

        if (last is BinaryOperator binOp)
        {
          if (value == "__now()")
          {
            binOp.Right = new FunctionExpression() { Name = "GetDate" };
          }
          else
          {
            binOp.Right = new ObjectLiteral(value, (PropertyReference)binOp.Left);
          }

          ((ILogical)_stack.Last()).Add(binOp);
        }
        else if (last is ILogical logical)
        {
          var lastLogical = _stack.OfType<ILogical>().LastOrDefault();
          if (lastLogical == null)
            _query.Where = lastLogical;
          else
            lastLogical.Add(logical);
        }
      }
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
          if (_query.From == null)
          {
            _query.From = new Table();
            _stack.Add(_query.From);
          }
          else if (_stack.Last() is PropertyReference prop)
          {
            var newTable = new Table()
            {
              TypeProvider = prop
            };
            _query.From = new Join()
            {
              Left = _query.From,
              Right = newTable,
              Type = JoinType.LeftOuter,
              Condition = new EqualsOperator()
              {
                Left = prop,
                Right = new PropertyReference("id", newTable)
              }
            };
            _stack.Add(_query.From);
          }
          else
          {
            throw new InvalidOperationException();
          }
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
          throw new NotSupportedException("Relationships are not supported at this time");
        default:
          if (_stack.Count > 0)
          {
            if (_stack.Last() is ITableOperand)
              _stack.Add(new AndOperator());
            if (_stack.Last() is ILogical)
              _stack.Add(new PropertyReference(localName, TableFromOp(_stack.OfType<ITableOperand>().Last())));
          }
          break;
      }
    }

    private Table TableFromOp(ITableOperand op)
    {
      return (op as Table) ?? (Table)((Join)op).Right;
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
  }
}
