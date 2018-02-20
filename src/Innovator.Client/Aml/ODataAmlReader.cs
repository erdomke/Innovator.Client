using Json.Embed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// An XML reader for reading JSON OData responses as if they were AML responses.
  /// </summary>
  /// <seealso cref="System.Xml.XmlReader" />
  public class ODataAmlReader : XmlReader
  {
    private readonly IEnumerator<Node> _nodeEnum;
    private IEnumerator<KeyValuePair<string, string>> _attrs;
    private XmlNameTable _table = new NameTable();
    private XmlNodeType _node = XmlNodeType.None;
    private int _depth = 0;

    /// <summary>
    /// Gets the number of attributes on the current node.
    /// </summary>
    public override int AttributeCount
    {
      get { return _nodeEnum.Current.Attributes?.Count ?? 0; }
    }

    /// <summary>
    /// Gets the base URI of the current node.
    /// </summary>
    public override string BaseURI
    {
      get { return string.Empty; }
    }

    /// <summary>
    /// Gets the depth of the current node in the XML document.
    /// </summary>
    public override int Depth
    {
      get { return _depth; }
    }

    /// <summary>
    /// Gets a value indicating whether the reader is positioned at the end of the stream.
    /// </summary>
    public override bool EOF
    {
      get { return ReadState == ReadState.EndOfFile; }
    }

#if XMLLEGACY
    /// <summary>
    /// Gets a value indicating whether the current node can have a <see cref="Value"/>.
    /// </summary>
    public override bool HasValue
    {
      get { return _node == XmlNodeType.Text || _node == XmlNodeType.Attribute; }
    }
#endif

    /// <summary>
    /// Gets a value indicating whether the current node is an empty element (for example, &lt;MyElement/&gt;).
    /// </summary>
    public override bool IsEmptyElement
    {
      get { return false; }
    }

    /// <summary>
    /// Gets the local name of the current node.
    /// </summary>
    public override string LocalName
    {
      get
      {
        return _table.Add(_attrs?.Current.Key.Split(':').Last() ?? _nodeEnum.Current.Value);
      }
    }

    /// <summary>
    /// Gets the namespace URI (as defined in the W3C Namespace specification) of the node on which the reader is positioned.
    /// </summary>
    public override string NamespaceURI
    {
      get
      {
        if ((_attrs?.Current.Key.IndexOf(':') ?? -1) > 0)
          return LookupNamespace(_attrs.Current.Key.Split(':').First());
        return _nodeEnum.Current.NamespaceUri ?? "";
      }
    }

    /// <summary>
    /// Gets the <see cref="T:System.Xml.XmlNameTable" /> associated with this implementation.
    /// </summary>
    public override XmlNameTable NameTable
    {
      get { return _table; }
    }

    /// <summary>
    /// Gets the type of the current node.
    /// </summary>
    public override XmlNodeType NodeType
    {
      get { return _node; }
    }

    /// <summary>
    /// Gets the namespace prefix associated with the current node.
    /// </summary>
    public override string Prefix
    {
      get
      {
        if ((_attrs?.Current.Key.IndexOf(':') ?? -1) > 0)
          return _table.Add(_attrs.Current.Key.Split(':').First());
        return string.IsNullOrEmpty(_nodeEnum.Current.Prefix)
          ? ""
          : _table.Add(_nodeEnum.Current.Prefix);
      }
    }

    /// <summary>
    /// Gets the state of the reader.
    /// </summary>
    public override ReadState ReadState
    {
      get
      {
        if (_node == XmlNodeType.None)
          return ReadState.Initial;
        if (_depth < 1)
          return ReadState.EndOfFile;
        return ReadState.Interactive;
      }
    }

    /// <summary>
    /// Gets the text value of the current node.
    /// </summary>
    public override string Value
    {
      get
      {
        if (_attrs != null)
          return _attrs.Current.Value;
        if (_node == XmlNodeType.Text)
          return _nodeEnum.Current.Value;
        return string.Empty;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataAmlReader"/> class.
    /// </summary>
    /// <param name="oDataReader">The OData reader.</param>
    public ODataAmlReader(TextReader oDataReader)
    {
      _nodeEnum = ReadJson(new JsonTextReader(oDataReader)).GetEnumerator();
    }

#if XMLLEGACY
    /// <summary>
    /// Changes the <see cref="ReadState"/> to <see cref="System.Xml.ReadState.Closed"/>
    /// </summary>
    public override void Close()
    {
      // Do nothing
    }
#endif

    /// <summary>
    /// Gets the value of the attribute with the specified index.
    /// </summary>
    /// <param name="i">The index of the attribute. The index is zero-based. (The first attribute has index 0.)</param>
    /// <returns>
    /// The value of the specified attribute. This method does not move the reader.
    /// </returns>
    public override string GetAttribute(int i)
    {
      return _nodeEnum.Current.Attributes[i].Value;
    }

    /// <summary>
    /// Gets the value of the attribute with the specified <see cref="P:System.Xml.XmlReader.Name" />.
    /// </summary>
    /// <param name="name">The qualified name of the attribute.</param>
    /// <returns>
    /// The value of the specified attribute. If the attribute is not found or the value is String.Empty, null is returned.
    /// </returns>
    public override string GetAttribute(string name)
    {
      return _nodeEnum.Current.Attributes.Single(k => k.Key == "name").Value;
    }

    /// <summary>
    /// Gets the value of the attribute with the specified <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" />.
    /// </summary>
    /// <param name="name">The local name of the attribute.</param>
    /// <param name="namespaceURI">The namespace URI of the attribute.</param>
    /// <returns>
    /// The value of the specified attribute. If the attribute is not found or the value is String.Empty, null is returned. This method does not move the reader.
    /// </returns>
    public override string GetAttribute(string name, string namespaceURI)
    {
      var prefix = ResultWriter.PrefixFromNamespace(namespaceURI);
      if (!string.IsNullOrEmpty(prefix))
        name = prefix + ":" + name;
      return GetAttribute(name);
    }

    /// <summary>
    /// Resolves a namespace prefix in the current element's scope.
    /// </summary>
    /// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass an empty string.</param>
    /// <returns>
    /// The namespace URI to which the prefix maps or null if no matching prefix is found.
    /// </returns>
    public override string LookupNamespace(string prefix)
    {
      return AmlReader.NamespaceFromPrefix(prefix);
    }

    /// <summary>
    /// Moves to the attribute with the specified <see cref="P:System.Xml.XmlReader.Name" />.
    /// </summary>
    /// <param name="name">The qualified name of the attribute.</param>
    /// <returns>
    /// <c>true</c> if the attribute is found; otherwise, <c>false</c>. If <c>false</c>, the reader's position does not change.
    /// </returns>
    public override bool MoveToAttribute(string name)
    {
      return MoveToAttribute(name, string.Empty);
    }

    /// <summary>
    /// Moves to the attribute with the specified <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" />.
    /// </summary>
    /// <param name="name">The local name of the attribute.</param>
    /// <param name="ns">The namespace URI of the attribute.</param>
    /// <returns>
    /// <c>true</c> if the attribute is found; otherwise, <c>false</c>. If <c>false</c>, the reader's position does not change.
    /// </returns>
    public override bool MoveToAttribute(string name, string ns)
    {
      var fullName = name;
      if (!string.IsNullOrEmpty(ns))
        fullName = ResultWriter.PrefixFromNamespace(ns) + ":" + name;
      _attrs = _nodeEnum.Current.Attributes.GetEnumerator();
      while (_attrs.MoveNext())
      {
        if (_attrs.Current.Key == fullName)
        {
          _node = XmlNodeType.Attribute;
          return true;
        }
      }

      MoveToElement();
      return false;
    }

    /// <summary>
    /// Moves to the element that contains the current attribute node.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the reader is positioned on an attribute (the reader moves to the element that owns the attribute); <c>false</c> if the reader is not positioned on an attribute (the position of the reader does not change).
    /// </returns>
    public override bool MoveToElement()
    {
      _attrs = null;
      _node = XmlNodeType.Element;
      return true;
    }

    /// <summary>
    /// Moves to the first attribute.
    /// </summary>
    /// <returns>
    /// <c>true</c> if an attribute exists (the reader moves to the first attribute); otherwise, <c>false</c> (the position of the reader does not change).
    /// </returns>
    public override bool MoveToFirstAttribute()
    {
      if (_nodeEnum.Current.Attributes?.Count > 0)
      {
        _attrs = _nodeEnum.Current.Attributes.GetEnumerator();
        _attrs.MoveNext();
        _node = XmlNodeType.Attribute;
        return true;
      }

      MoveToElement();
      return false;
    }

    /// <summary>
    /// Moves to the next attribute.
    /// </summary>
    /// <returns>
    /// <c>true</c> if there is a next attribute; <c>false</c> if there are no more attributes.
    /// </returns>
    public override bool MoveToNextAttribute()
    {
      if (_attrs?.MoveNext() == true)
      {
        _node = XmlNodeType.Attribute;
        return true;
      }

      MoveToElement();
      return false;
    }

    /// <summary>
    /// Teads the next node from the stream.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the next node was read successfully; otherwise, <c>false</c>.
    /// </returns>
    public override bool Read()
    {
      var result = _nodeEnum.MoveNext();
      if (result)
      {
        _node = _nodeEnum.Current.NodeType;
        if (_node == XmlNodeType.Element)
          _depth++;
        else if (_node == XmlNodeType.EndElement)
          _depth--;
      }
      return result;
    }

    /// <summary>
    /// Parses the attribute value into one or more <see cref="XmlNodeType.Text"/>,
    /// <see cref="XmlNodeType.EntityReference"/>, or <see cref="XmlNodeType.EndEntity"/> nodes.
    /// </summary>
    /// <returns>
    /// <c>true</c> if there are nodes to return. <c>false</c> if the reader is not positioned on
    /// an attribute node when the initial call is made or if all the attribute values have been
    /// read. An empty attribute, such as, <c>misc=""</c>, returns <c>true</c> with a single node
    /// with a value of <see cref="string.Empty"/>.
    /// </returns>
    public override bool ReadAttributeValue()
    {
      if (_node == XmlNodeType.Text)
      {
        _node = XmlNodeType.Attribute;
        return false;
      }
      else
      {
        _node = XmlNodeType.Text;
        return true;
      }
    }

    /// <summary>
    /// Resolves the entity reference for EntityReference nodes.
    /// </summary>
    public override void ResolveEntity()
    {
      // Do nothing
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="T:System.Xml.XmlReader" /> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (disposing)
        _nodeEnum.Dispose();
    }

    private static IEnumerable<Node> ReadJson(JsonTextReader reader)
    {
      using (reader)
      {
        yield return new Node()
        {
          Prefix = "SOAP-ENV",
          Value = "Envelope",
          NamespaceUri = "http://schemas.xmlsoap.org/soap/envelope/",
          NodeType = XmlNodeType.Element,
          Attributes = new KeyValuePair<string, string>[]
          {
            new KeyValuePair<string, string>("xmlns:SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"),
          }
        };
        yield return new Node()
        {
          Prefix = "SOAP-ENV",
          Value = "Body",
          NamespaceUri = "http://schemas.xmlsoap.org/soap/envelope/",
          NodeType = XmlNodeType.Element
        };

        var props = new Stack<string>();
        var inValue = false;
        var itemType = default(string);
        var isError = true;

        while (reader.Read())
        {
          switch (reader.TokenType)
          {
            case JsonToken.StartArray:
            case JsonToken.StartObject:
              var curr = props.Count > 0 ? props.Peek() : "";
              var propName = curr.TrimEnd('[', '{');
              if (curr != "" && propName == curr)
                props.Pop();
              props.Push(propName + (reader.TokenType == JsonToken.StartArray ? "[" : "{"));

              if (propName == "value" && reader.TokenType == JsonToken.StartArray)
              {
                inValue = true;
              }
              else if (propName == "value" && reader.TokenType == JsonToken.StartObject)
              {
                yield return new Node()
                {
                  Value = "Item",
                  NodeType = XmlNodeType.Element,
                  Attributes = new KeyValuePair<string, string>[]
                  {
                    new KeyValuePair<string, string>("type", itemType),
                  }
                };
              }
              else if (propName == "error")
              {
                isError = true;
                yield return new Node()
                {
                  Prefix = "SOAP-ENV",
                  Value = "Fault",
                  NamespaceUri = "http://schemas.xmlsoap.org/soap/envelope/",
                  NodeType = XmlNodeType.Element,
                  Attributes = new KeyValuePair<string, string>[]
                  {
                  new KeyValuePair<string, string>("xmlns:af", "http://www.aras.com/InnovatorFault"),
                  }
                };
              }
              else if (inValue && reader.TokenType == JsonToken.StartObject)
              {
                yield return new Node()
                {
                  Value = propName,
                  NodeType = XmlNodeType.Element
                };
                yield return new Node()
                {
                  Value = "Item",
                  NodeType = XmlNodeType.Element
                };
              }
              break;
            case JsonToken.PropertyName:
              props.Push(reader.Value);
              break;
            case JsonToken.Boolean:
            case JsonToken.Number:
            case JsonToken.String:
              if (props.Peek() == "@odata.context")
              {
                itemType = reader.Value.Split('#').Last();
                var idx = itemType.IndexOf('(');
                if (idx > 0)
                  itemType = itemType.Substring(0, idx);
                yield return new Node()
                {
                  Value = "Result",
                  NodeType = XmlNodeType.Element
                };
                isError = false;
              }
              else if (inValue && props.Peek() == "@odata.id")
              {
                yield return new Node()
                {
                  Value = "id",
                  NodeType = XmlNodeType.Element
                };
                yield return new Node()
                {
                  Value = reader.Value.Split('\'')[1],
                  NodeType = XmlNodeType.Text
                };
                yield return new Node()
                {
                  Value = "id",
                  NodeType = XmlNodeType.EndElement
                };
              }
              else if (inValue)
              {
                yield return new Node()
                {
                  Value = props.Peek(),
                  NodeType = XmlNodeType.Element
                };
                yield return new Node()
                {
                  Value = reader.Value,
                  NodeType = XmlNodeType.Text
                };
                yield return new Node()
                {
                  Value = props.Peek(),
                  NodeType = XmlNodeType.EndElement
                };
              }
              else if (isError)
              {
                if (props.Peek() == "code")
                {
                  var value = reader.Value;
                  if (value == "NotFound")
                    value = "0";
                  yield return new Node()
                  {
                    Value = "faultcode",
                    NodeType = XmlNodeType.Element
                  };
                  yield return new Node()
                  {
                    Value = value,
                    NodeType = XmlNodeType.Text
                  };
                  yield return new Node()
                  {
                    Value = "faultcode",
                    NodeType = XmlNodeType.EndElement
                  };
                }
                else if (props.Peek() == "message")
                {
                  yield return new Node()
                  {
                    Value = "faultstring",
                    NodeType = XmlNodeType.Element
                  };
                  yield return new Node()
                  {
                    Value = reader.Value,
                    NodeType = XmlNodeType.Text
                  };
                  yield return new Node()
                  {
                    Value = "faultstring",
                    NodeType = XmlNodeType.EndElement
                  };
                }
              }

              if (!props.Peek().EndsWith("["))
                props.Pop();
              break;
            case JsonToken.Null:
              if (inValue)
              {
                yield return new Node()
                {
                  Value = props.Peek(),
                  NodeType = XmlNodeType.Element,
                  Attributes = new KeyValuePair<string, string>[]
                  {
                    new KeyValuePair<string, string>("is_null", "1"),
                  }
                };
                yield return new Node()
                {
                  Value = props.Peek(),
                  NodeType = XmlNodeType.EndElement
                };
              }

              if (!props.Peek().EndsWith("["))
                props.Pop();
              break;
            case JsonToken.EndArray:
            case JsonToken.EndObject:
              var last = props.Pop();
              if (inValue && reader.TokenType == JsonToken.EndObject)
              {
                yield return new Node()
                {
                  Value = "Item",
                  NodeType = XmlNodeType.EndElement
                };
                var name = last.TrimEnd('{');
                if (name != "value")
                {
                  yield return new Node()
                  {
                    Value = name,
                    NodeType = XmlNodeType.EndElement
                  };
                }
              }
              else if (last == "value[")
              {
                inValue = false;
              }
              break;
          }
        }

        if (isError)
        {
          yield return new Node()
          {
            Prefix = "SOAP-ENV",
            Value = "Fault",
            NamespaceUri = "http://schemas.xmlsoap.org/soap/envelope/",
            NodeType = XmlNodeType.EndElement
          };
        }
        else
        {
          yield return new Node()
          {
            Value = "Result",
            NodeType = XmlNodeType.EndElement
          };
        }

        yield return new Node()
        {
          Prefix = "SOAP-ENV",
          Value = "Body",
          NamespaceUri = "http://schemas.xmlsoap.org/soap/envelope/",
          NodeType = XmlNodeType.EndElement
        };

        yield return new Node()
        {
          Prefix = "SOAP-ENV",
          Value = "Envelope",
          NamespaceUri = "http://schemas.xmlsoap.org/soap/envelope/",
          NodeType = XmlNodeType.EndElement
        };
      }
    }

    private struct Node
    {
      public string Prefix { get; set; }
      public string Value { get; set; }
      public string NamespaceUri { get; set; }
      public XmlNodeType NodeType { get; set; }
      public IList<KeyValuePair<string, string>> Attributes { get; set; }
    }
  }
}
