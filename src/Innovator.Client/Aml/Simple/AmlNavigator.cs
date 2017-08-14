using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
#if XMLLEGACY
using System.Xml.XPath;
#endif

namespace Innovator.Client
{
#if XMLLEGACY
  internal class AmlNavigator : XPathNavigator, IAmlXPath
  {
    private IReadOnlyElement _current;
    private IEnumerator<IReadOnlyAttribute> _attrs;
    private Stack<ElementList> _stack = new Stack<ElementList>();
    private XPathNodeType _node;
    private string _localName;
    private string _prefix;
    private XmlNameTable _table = new NameTable();

    public override string BaseURI
    {
      get { return string.Empty; }
    }

    public override bool HasAttributes
    {
      get
      {
        if (_node == XPathNodeType.Text ||
          _node == XPathNodeType.Attribute)
          return false;
        return _current.Attributes().Any();
      }
    }

    public override bool HasChildren
    {
      get
      {
        if (_node == XPathNodeType.Text ||
          _node == XPathNodeType.Attribute)
          return false;
        return _current.Elements().Any()
          || !string.IsNullOrEmpty(_current.Value);
      }
    }

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

    public override string Name
    {
      get
      {
        if (string.IsNullOrEmpty(_prefix))
          return _localName;
        return _prefix + ":" + _localName;
      }
    }

    public override string NamespaceURI
    {
      get { return AmlReader.NamespaceFromPrefix(_prefix); }
    }

    public override XmlNameTable NameTable
    {
      get { return _table; }
    }

    public override XPathNodeType NodeType
    {
      get { return _node; }
    }

    public override string Prefix
    {
      get { return _prefix; }
    }

    public override object UnderlyingObject
    {
      get
      {
        return _current;
      }
    }

    public override string Value
    {
      get
      {
        if (_attrs != null)
          return _attrs.Current.Value;
        if (_node == XPathNodeType.Text)
          return _current.Value;
        return string.Empty;
      }
    }

    internal AmlNavigator(ServerException ex) : this(AmlReader.GetElement(ex)) { }
    internal AmlNavigator(IReadOnlyResult result) : this(AmlReader.GetElement(result)) { }
    internal AmlNavigator(IReadOnlyElement elem)
    {
      var parents = new List<IReadOnlyElement>() { elem };
      var curr = elem;
      while (curr.Parent.Exists && !string.IsNullOrEmpty(curr.Parent.Name))
      {
        parents.Add(curr.Parent);
        curr = curr.Parent;
      }
      parents.Reverse();

      _stack.Push(new ElementList(parents[0]));
      _stack.Peek().MoveNext();
      for (var i = 1; i < parents.Count; i++)
      {
        var list = new ElementList(parents[i - 1].Elements());
        list.MoveTo(parents[i]);
        _stack.Push(list);
      }
      SetCurrent(_stack.Peek().Current);
    }
    private AmlNavigator(AmlNavigator source)
    {
      MoveTo(source);
    }

    public override bool CheckValidity(XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
    {
      throw new NotSupportedException();
    }

    public override XPathNavigator Clone()
    {
      return new AmlNavigator(this);
    }

    public override bool IsSamePosition(XPathNavigator other)
    {
      var aml = other as AmlNavigator;
      if (aml == null)
        return false;

      return _current == aml._current && _node == aml._node;
    }

    public override bool MoveTo(XPathNavigator other)
    {
      var aml = other as AmlNavigator;
      if (aml == null)
        return false;

      _current = aml._current;
      if (aml._attrs != null)
      {
        _attrs = _current.Attributes().GetEnumerator();
        while (_attrs.MoveNext() && !_attrs.Current.Equals(aml._attrs.Current))
          ;
      }
      _node = aml._node;
      _localName = aml._localName;
      _prefix = aml._prefix;
      _stack = new Stack<ElementList>(aml._stack.Select(e => e.Clone()));
      return true;
    }

    public override bool MoveToAttribute(string localName, string namespaceURI)
    {
      var fullName = localName;
      if (!string.IsNullOrEmpty(namespaceURI))
        fullName = ResultWriter.PrefixFromNamespace(namespaceURI) + ":" + localName;
      _attrs = _current.Attributes().GetEnumerator();
      while (_attrs.MoveNext())
      {
        if (_attrs.Current.Name == fullName)
        {
          _node = XPathNodeType.Attribute;
          _localName = localName;
          _prefix = ResultWriter.PrefixFromNamespace(namespaceURI);
          return true;
        }
      }

      _node = XPathNodeType.Element;
      return false;
    }

    public override bool MoveToChild(XPathNodeType type)
    {
      switch (type)
      {
        case XPathNodeType.Attribute:
          return MoveToFirstAttribute();
        case XPathNodeType.All:
          return MoveToFirstChild();
        case XPathNodeType.Element:
          if (_current.Elements().Any())
          {
            _stack.Push(new ElementList(_current.Elements()));
            _stack.Peek().MoveNext();
            return SetCurrent(_stack.Peek().Current);
          }
          return false;
        case XPathNodeType.Text:
          if (!string.IsNullOrEmpty(_current.Value))
          {
            _node = XPathNodeType.Text;
            return true;
          }
          return false;
      }
      return false;
    }

    public override bool MoveToChild(string localName, string namespaceURI)
    {
      if (!_current.Elements().Any())
        return false;

      var fullName = localName;
      if (!string.IsNullOrEmpty(namespaceURI))
        fullName = ResultWriter.PrefixFromNamespace(namespaceURI) + ":" + localName;

      var list = new ElementList(_current.Elements());
      while (list.MoveNext())
      {
        if (list.Current.Name == fullName)
        {
          _stack.Push(list);
          return SetCurrent(_stack.Peek().Current);
        }
      }
      return false;
    }

    public override bool MoveToFirstAttribute()
    {
      if (!_current.Attributes().Any() || _node != XPathNodeType.Element)
        return false;

      _attrs = _current.Attributes().GetEnumerator();
      _attrs.MoveNext();
      SetName(_attrs.Current.Name);
      _node = XPathNodeType.Attribute;
      return true;
    }

    public override bool MoveToFirstChild()
    {
      if (_current.Elements().Any())
      {
        _stack.Push(new ElementList(_current.Elements()));
        _stack.Peek().MoveNext();
        return SetCurrent(_stack.Peek().Current);
      }
      else if (!string.IsNullOrEmpty(_current.Value) && _node != XPathNodeType.Text)
      {
        _node = XPathNodeType.Text;
        return true;
      }
      return false;
    }

    public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
    {
      // TODO: Need to figure out what this is doing
      return false;
    }

    public override bool MoveToId(string id)
    {
      throw new NotSupportedException();
    }

    public override bool MoveToNext()
    {
      if (_stack.Count < 1 || _node != XPathNodeType.Element)
        return false;

      if (_stack.Peek().TryMoveNext())
        return SetCurrent(_stack.Peek().Current);
      return false;
    }

    public override bool MoveToNext(XPathNodeType type)
    {
      if (type == XPathNodeType.All || type == XPathNodeType.Element)
        return MoveToNext();
      return false;
    }

    public override bool MoveToNext(string localName, string namespaceURI)
    {
      if (_stack.Count < 1 || _node == XPathNodeType.Text)
        return false;

      var pos = _stack.Peek().Position;
      var fullName = localName;
      if (!string.IsNullOrEmpty(namespaceURI))
        fullName = ResultWriter.PrefixFromNamespace(namespaceURI) + ":" + localName;

      while (_stack.Peek().MoveNext())
      {
        if (_stack.Peek().Current.Name == fullName)
          return SetCurrent(_stack.Peek().Current);
      }

      _stack.Peek().Position = pos;
      return false;
    }

    public override bool MoveToNextAttribute()
    {
      if (_attrs == null || !_attrs.MoveNext())
        return false;

      SetName(_attrs.Current.Name);
      _node = XPathNodeType.Attribute;
      return true;
    }

    public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
    {
      // TODO: Need to figure out what this is doing
      return false;
    }

    public override bool MoveToParent()
    {
      if (_node == XPathNodeType.Text
        || _node == XPathNodeType.Attribute)
      {
        _attrs = null;
        _node = XPathNodeType.Element;
        return true;
      }

      _stack.Pop();
      if (_stack.Count < 1)
        return false;
      return SetCurrent(_stack.Peek().Current);
    }

    public override bool MoveToPrevious()
    {
      if (_stack.Count < 1 || _node != XPathNodeType.Element)
        return false;

      if (_stack.Peek().TryMovePrevious())
        return SetCurrent(_stack.Peek().Current);
      return false;
    }

    public override XmlReader ReadSubtree()
    {
      return new AmlReader(_current, _table);
    }

    object IAmlXPath.Evaluate(string expression)
    {
      var obj = Evaluate(expression);
      var iter = obj as XPathNodeIterator;
      if (iter != null)
      {
        var result = SimplifyIterator(iter);
        if (result.Length < 1)
          return null;
        if (result.Length == 1)
          return result[0];

        if (result.OfType<IReadOnlyElement>().Any())
          return result.OfType<IReadOnlyElement>().ToArray();
        if (result.OfType<string>().Any())
          return result.OfType<string>().ToArray();

        return result;
      }
      else
      {
        return obj;
      }
    }

    public IReadOnlyElement SelectElement(string expression)
    {
      return SelectElements(expression).FirstOrDefault() ?? AmlElement.NullElem;
    }

    public IEnumerable<IReadOnlyElement> SelectElements(string expression)
    {
      var obj = Evaluate(expression);
      var iter = obj as XPathNodeIterator;
      if (iter != null)
      {
        return SimplifyIterator(iter).OfType<IReadOnlyElement>();
      }
      else
      {
        return new[] { (IReadOnlyElement)obj };
      }
    }

    private object[] SimplifyIterator(XPathNodeIterator iter)
    {
      var result = new object[iter.Count];
      while (iter.MoveNext())
      {
        switch (iter.Current.NodeType)
        {
          case XPathNodeType.Attribute:
          case XPathNodeType.Text:
            result[iter.CurrentPosition - 1] = iter.Current.Value;
            break;
          default:
            result[iter.CurrentPosition - 1] = iter.Current.UnderlyingObject;
            break;
        }
      }
      return result;
    }

    private bool SetCurrent(IReadOnlyElement elem)
    {
      _current = elem;
      _node = XPathNodeType.Element;
      SetName(elem.Name);
      _attrs = null;
      return true;
    }

    private void SetName(string name)
    {
      var parts = name.Split(':');
      _localName = _table.Add(parts.Last());
      _prefix = (parts.Length > 1 ? _table.Add(parts.First()) : string.Empty);
    }

    private class ElementList : IEnumerator<IReadOnlyElement>
    {
      IReadOnlyElement[] _array;
      int _curr;

      public IReadOnlyElement Current
      {
        get { return _array[_curr]; }
      }
      public int Position
      {
        get { return _curr; }
        set { _curr = value; }
      }

      object IEnumerator.Current
      {
        get { return this.Current; }
      }

      private ElementList() { }
      public ElementList(IReadOnlyElement elem)
      {
        _array = new[] { elem };
        Reset();
      }
      public ElementList(IEnumerable<IReadOnlyElement> elem)
      {
        _array = elem.ToArray();
        Reset();
      }

      public ElementList Clone()
      {
        return new ElementList()
        {
          _array = this._array,
          _curr = this._curr
        };
      }

      public bool MoveNext()
      {
        return (++_curr) < _array.Length;
      }

      public void MoveTo(IReadOnlyElement elem)
      {
        _curr = Array.IndexOf(_array, elem);
      }

      public bool MovePrevious()
      {
        return (--_curr) >= 0;
      }

      public bool TryMoveNext()
      {
        if ((++_curr) < _array.Length)
          return true;
        _curr--;
        return false;
      }

      public bool TryMovePrevious()
      {
        if ((--_curr) >= 0)
          return true;
        _curr++;
        return false;
      }

      public void Dispose()
      {
        // Do nothing
      }

      public void Reset()
      {
        _curr = -1;
      }
    }
  }
#endif
}
