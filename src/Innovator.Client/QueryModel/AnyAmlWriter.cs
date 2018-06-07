using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client.QueryModel
{
  internal class AnyAmlWriter : XmlWriter
  {
    private XmlWriter _writer;
    private StringBuilder _buffer = new StringBuilder();
    private string _name;
    private IServerContext _context;

    public QueryItem Query
    {
      get
      {
        return (_writer as AmlToModelWriter)?.Query
          ?? (_writer as QueryBuilderWriter)?.Query;
      }
    }

    public override WriteState WriteState => _writer.WriteState;

    public AnyAmlWriter(IServerContext context)
    {
      _context = context;
      _writer = new AmlToModelWriter(context);
    }

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
      _writer.Flush();
    }

    public override string LookupPrefix(string ns)
    {
      return _writer.LookupPrefix(ns);
    }

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      _writer.WriteBase64(buffer, index, count);
    }

    public override void WriteCData(string text)
    {
      _writer.WriteCData(text);
      _buffer.Append(text);
    }

    public override void WriteCharEntity(char ch)
    {
      _writer.WriteCharEntity(ch);
    }

    public override void WriteChars(char[] buffer, int index, int count)
    {
      _writer.WriteChars(buffer, index, count);
      _buffer.Append(buffer, index, count);
    }

    public override void WriteComment(string text)
    {
      _writer.WriteComment(text);
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
      _writer.WriteDocType(name, pubid, sysid, subset);
    }

    public override void WriteEndAttribute()
    {
      _writer.WriteEndAttribute();
      if (_name == "action" && _buffer.ToString() == "query_ExecuteQueryDefinition")
      {
        _writer = new QueryBuilderWriter(_context);
        _writer.WriteStartElement("Item");
        _writer.WriteAttributeString("type", "qry_QueryDefinition");
        _writer.WriteAttributeString("action", "query_ExecuteQueryDefinition");
      }
    }

    public override void WriteEndDocument()
    {
      _writer.WriteEndDocument();
    }

    public override void WriteEndElement()
    {
      _writer.WriteEndElement();
    }

    public override void WriteEntityRef(string name)
    {
      _writer.WriteEntityRef(name);
    }

    public override void WriteFullEndElement()
    {
      _writer.WriteFullEndElement();
    }

    public override void WriteProcessingInstruction(string name, string text)
    {
      _writer.WriteProcessingInstruction(name, text);
    }

    public override void WriteRaw(char[] buffer, int index, int count)
    {
      _writer.WriteRaw(buffer, index, count);
    }

    public override void WriteRaw(string data)
    {
      _writer.WriteRaw(data);
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      _writer.WriteStartAttribute(prefix, localName, ns);
      _name = localName;
    }

    public override void WriteStartDocument()
    {
      _writer.WriteStartDocument();
    }

    public override void WriteStartDocument(bool standalone)
    {
      _writer.WriteStartDocument(standalone);
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      _writer.WriteStartElement(prefix, localName, ns);
    }

    public override void WriteString(string text)
    {
      _writer.WriteString(text);
      _buffer.Append(text);
    }

    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
      _writer.WriteSurrogateCharEntity(lowChar, highChar);
    }

    public override void WriteWhitespace(string ws)
    {
      _writer.WriteWhitespace(ws);
      _buffer.Append(ws);
    }
  }
}
