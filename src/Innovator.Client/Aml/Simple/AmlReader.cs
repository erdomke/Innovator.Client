using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Innovator.Client
{
  internal class AmlReader : XmlReader
  {
    private IReadOnlyElement _current;
    private IEnumerator<IReadOnlyAttribute> _attrs;
    private Stack<IEnumerator<IReadOnlyElement>> _stack = new Stack<IEnumerator<IReadOnlyElement>>();
    private XmlNodeType _node = XmlNodeType.None;
    private string _localName;
    private string _prefix;
    private XmlNameTable _table;

    internal AmlReader(ServerException ex) : this(GetElement(ex)) { }
    internal AmlReader(IReadOnlyResult result) : this(GetElement(result)) { }
    internal AmlReader(IReadOnlyElement elem) : this(elem, new NameTable()) { }
    internal AmlReader(IReadOnlyElement elem, XmlNameTable table)
    {
      _stack.Push(Enumerable.Repeat(elem, 1).GetEnumerator());
      _table = table;
    }


    public override int AttributeCount
    {
      get { return _current.Attributes().Count(); }
    }

    public override string BaseURI
    {
      get { return string.Empty; }
    }

    public override int Depth
    {
      get { return _node == XmlNodeType.None ? 0 : _stack.Count; }
    }

    public override bool EOF
    {
      get { return _stack.Count < 1; }
    }

#if XMLLEGACY
    public override bool HasValue
    {
      get { return !string.IsNullOrEmpty(_current.Value); }
    }
#endif

    public override bool IsEmptyElement
    {
      get
      {
        return !_current.Elements().Any()
    && string.IsNullOrEmpty(_current.Value);
      }
    }

    public override string LocalName
    {
      get { return _localName; }
    }

    public override string NamespaceURI
    {
      get { return LookupNamespace(_prefix); }
    }

    public override XmlNameTable NameTable
    {
      get { return _table; }
    }

    public override XmlNodeType NodeType
    {
      get { return _node; }
    }

    public override string Prefix
    {
      get { return _prefix; }
    }

    public override ReadState ReadState
    {
      get
      {
        if (_node == XmlNodeType.None)
          return ReadState.Initial;
        if (_stack.Count < 1)
          return ReadState.EndOfFile;
        return ReadState.Interactive;
      }
    }

    public override string Value
    {
      get
      {
        if (_attrs != null)
          return _attrs.Current.Value;
        if (_node == XmlNodeType.Text)
          return _current.Value;
        return string.Empty;
      }
    }

#if XMLLEGACY
    public override void Close()
    {
      // Do nothing
    }
#endif

    public override string GetAttribute(string name)
    {
      return _current.Attribute(name).Value;
    }

    public override string GetAttribute(int i)
    {
      return _current.Attributes().ElementAt(i).Value;
    }

    public override string GetAttribute(string name, string namespaceURI)
    {
      var prefix = ResultWriter.PrefixFromNamespace(namespaceURI);
      if (!string.IsNullOrEmpty(prefix))
        name = prefix + ":" + name;
      return _current.Attribute(name).Value;
    }

    public override string LookupNamespace(string prefix)
    {
      return NamespaceFromPrefix(prefix);
    }

    internal static string NamespaceFromPrefix(string prefix)
    {
      switch (prefix)
      {
        case "SOAP-ENV":
          return "http://schemas.xmlsoap.org/soap/envelope/";
        case "af":
          return "http://www.aras.com/InnovatorFault";
        case "xml":
          return "http://www.w3.org/XML/1998/namespace";
        case "xmlns":
          return "http://www.w3.org/2000/xmlns/";
        case "i18n":
          return "http://www.aras.com/I18N";
      }
      return string.Empty;
    }

    public override bool MoveToAttribute(string name)
    {
      return MoveToAttribute(name, string.Empty);
    }

    public override bool MoveToAttribute(string name, string ns)
    {
      var fullName = name;
      if (!string.IsNullOrEmpty(ns))
        fullName = ResultWriter.PrefixFromNamespace(ns) + ":" + name;
      _attrs = _current.Attributes().GetEnumerator();
      while (_attrs.MoveNext())
      {
        if (_attrs.Current.Name == fullName)
        {
          _node = XmlNodeType.Attribute;
          _localName = name;
          _prefix = ResultWriter.PrefixFromNamespace(ns);
          return true;
        }
      }

      _node = XmlNodeType.Element;
      return false;
    }

    public override bool MoveToElement()
    {
      _attrs = null;
      _node = XmlNodeType.Element;
      return true;
    }

    public override bool MoveToFirstAttribute()
    {
      if (_current.Attributes().Any())
      {
        _attrs = _current.Attributes().GetEnumerator();
        _attrs.MoveNext();
        SetName(_attrs.Current.Name);
        _node = XmlNodeType.Attribute;
        return true;
      }

      _attrs = null;
      _node = XmlNodeType.Element;
      return false;
    }

    public override bool MoveToNextAttribute()
    {
      if (_attrs != null && _attrs.MoveNext())
      {
        SetName(_attrs.Current.Name);
        _node = XmlNodeType.Attribute;
        return true;
      }

      _node = XmlNodeType.Element;
      return false;
    }

    public override bool Read()
    {
      if (_stack.Count < 1)
        return false;

      _attrs = null;
      if (_node == XmlNodeType.Attribute)
        _node = XmlNodeType.Element;

      switch (_node)
      {
        case XmlNodeType.None:
          if (!_stack.Peek().MoveNext())
            return false;
          return SetCurrent(_stack.Peek().Current);
        case XmlNodeType.Element:
          if (_current.Elements().Any())
          {
            _stack.Push(_current.Elements().GetEnumerator());
            _stack.Peek().MoveNext();
            return SetCurrent(_stack.Peek().Current);
          }
          else if (!string.IsNullOrEmpty(_current.Value))
          {
            _node = XmlNodeType.Text;
            return true;
          }
          break;
        case XmlNodeType.Text:
          _node = XmlNodeType.EndElement;
          return true;
      }
      // case XmlNodeType.EndElement:
      if (_stack.Peek().MoveNext())
        return SetCurrent(_stack.Peek().Current);
      _stack.Pop();
      if (_stack.Count > 0)
      {
        SetCurrent(_stack.Peek().Current);
        _node = XmlNodeType.EndElement;
        return true;
      }
      return false;
    }

    private bool SetCurrent(IReadOnlyElement elem)
    {
      _current = elem;
      _node = XmlNodeType.Element;
      _prefix = elem.Prefix;
      _localName = elem.Name;
      return true;
    }
    private void SetName(string name)
    {
      var parts = name.Split(':');
      _localName = _table.Add(parts.Last());
      _prefix = (parts.Length > 1 ? _table.Add(parts.First()) : string.Empty);
    }

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

    public override void ResolveEntity()
    {
      // Do nothing
    }

    internal static IReadOnlyElement GetElement(IReadOnlyResult result)
    {
      if (result.Exception != null)
      {
        return GetElement(result.Exception);
      }
      else
      {
        var aml = ElementFactory.Local;
        IReadOnlyElement res;
        if (result.Items().Any())
        {
          res = new LooseLinkElement(aml, "Result", result.Items().ToArray());
        }
        else
        {
          res = aml.Element("Result", result.Value);
        }
        var body = aml.Element("SOAP-ENV:Body", res);
        if (result.Message.Exists)
          body.Add(result.Message);
        var elem = aml.Element("SOAP-ENV:Envelope", aml.Attribute("xmlns:SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"), body);
        return elem;
      }
    }
    internal static IReadOnlyElement GetElement(ServerException ex)
    {
      var curr = ex.Fault;
      while (curr.Parent.Exists && !string.IsNullOrEmpty(curr.Parent.Name))
        curr = curr.Parent;
      if (curr.Name == "Envelope" && curr.Prefix == "SOAP-ENV")
        return curr;

      var aml = ElementFactory.Local;
      var elem = aml.Element("SOAP-ENV:Envelope", aml.Attribute("xmlns:SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/")
        , aml.Element("SOAP-ENV:Body"
          , aml.Element("SOAP-ENV:Fault", aml.Attribute("xmlns:af", "http://www.aras.com/InnovatorFault")
            , ex.Fault.Elements())));
      return elem;
    }

    private class LooseLinkElement : IReadOnlyElement, ILinkedElement
    {
      private ElementFactory _amlContext;
      private List<IReadOnlyElement> _children;
      private string _name;

      public ElementFactory AmlContext { get { return _amlContext; } }
      public bool Exists { get { return true; } }
      public string Name { get { return _name; } }
      public IReadOnlyElement Parent { get; set; }
      IReadOnlyElement IReadOnlyElement.Parent { get { return this.Parent; } }
      public string Prefix { get { return string.Empty; } }
      public string Value { get { return string.Empty; } }
      public ILinkedElement Next { get; set; }

      public LooseLinkElement(ElementFactory amlContext, string name, params IReadOnlyElement[] elems)
      {
        _amlContext = amlContext;
        _name = name;
        _children = new List<IReadOnlyElement>(elems);
      }

      public void Add(params IReadOnlyElement[] elems)
      {
        _children.AddRange(elems);
      }

      public IReadOnlyAttribute Attribute(string name)
      {
        return Innovator.Client.Attribute.NullAttr;
      }

      public IEnumerable<IReadOnlyAttribute> Attributes()
      {
        return Enumerable.Empty<IReadOnlyAttribute>();
      }

      public IEnumerable<IReadOnlyElement> Elements()
      {
        return _children;
      }

      public void ToAml(XmlWriter writer, AmlWriterSettings settings)
      {
        writer.WriteStartElement(_name);
        foreach (var child in _children)
        {
          child.ToAml(writer, settings);
        }
        writer.WriteEndElement();
      }
    }
  }
}
