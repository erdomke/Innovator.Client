using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Innovator.Client
{
  internal class ResultWriter : XmlWriter
  {
    public const string IgnoreAttribute = "~`IGNORE`~";

    public Result Result { get { return _result; } }

    protected ElementFactory _factory;
    protected string _attrName;
    protected string _value;
    protected bool _cdata;
    private bool _inElement;
    private IElementWriter _base;
    private bool _readOnly;
    private string _database;
    private Command _query;
    private StringBuilder _resultText;

    private Result _result;
    private Stack<string> _names;

    public ResultWriter(ElementFactory factory, string database, Command query, bool? readOnly = null)
    {
      _factory = factory;
      _database = database;
      _query = query;
      _result = new Result(factory, database, query);
      _names = new Stack<string>();
      _readOnly = readOnly ?? query != null;
    }

    public override WriteState WriteState
    {
      get
      {
        return WriteState.Start;
      }
    }

#if XMLLEGACY
    public override void Close()
    {
      // Do nothing
    }
#endif

    public override void Flush()
    {
      // Do nothing
    }

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
        case "http://www.aras.com/I18N":
          return "i18n";
        case "":
          return "";
      }
      throw new ArgumentException();
    }

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      throw new NotSupportedException();
    }

    public override void WriteCData(string text)
    {
      AddString(text);
      _cdata = true;
    }

    public override void WriteCharEntity(char ch)
    {
      AddString(new string(ch, 1));
    }

    public override void WriteChars(char[] buffer, int index, int count)
    {
      AddString(new string(buffer, index, count));
    }

    public override void WriteComment(string text)
    {
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
    }

    public override void WriteEndAttribute()
    {
      if (_attrName == IgnoreAttribute)
      {
        _value = null;
      }
      else if (_base != null)
      {
        _base.WriteEndAttribute(_attrName, _value);
        _value = null;
      }
    }

    public override void WriteEndDocument()
    {
    }

    public override void WriteEndElement()
    {
      _inElement = false;
      if (_base != null)
      {
        _base.WriteEndElement(_value, _cdata);
        _cdata = false;
      }
      else
      {
        if (_resultText != null)
          _resultText.Append(_value);

        var elemName = default(string);
        if (_names.Any())
          elemName = _names.Pop();

        if (elemName == "Result")
        {
          if (_resultText.Length > 0)
          {
            var str = _resultText.ToString();
            if (_names.Any() && _names.Peek() == "CompileMethodResponse" && str.StartsWith("ERROR: "))
              _result.Exception = _factory.ServerException(str).SetDetails(_database, _query);
            else
              _result.Value = str;
          }
          _resultText = null;
        }
      }
      _value = null;
    }

    public override void WriteEntityRef(string name)
    {
      if (name == "amp")
      {
        AddString("&");
      }
      else if (name == "apos")
      {
        AddString("'");
      }
      else if (name == "gt")
      {
        AddString(">");
      }
      else if (name == "lt")
      {
        AddString("<");
      }
      else if (name == "quot")
      {
        AddString("\"");
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
    }

    public override void WriteRaw(string data)
    {
      AddString(data);
    }

    public override void WriteRaw(char[] buffer, int index, int count)
    {
      AddString(new string(buffer, index, count));
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      _attrName = GetName(localName, ns);
      _value = null;
    }

    public override void WriteStartDocument()
    {
    }

    public override void WriteStartDocument(bool standalone)
    {
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      _value = null;
      _inElement = true;
      var name = GetName(localName, ns);
      if (_base != null)
      {
        _base.WriteStartElement(name);
      }
      else
      {
        switch (name)
        {
          case "Item":
            if (_names.Count < 1 || _names.Peek() != "Message")
            {
              _base = new ItemElementWriter(_factory) { ReadOnly = _readOnly };
              _base.Complete += OnComplete;
              _base.WriteStartElement(name);
            }
            break;
          case "SOAP-ENV:Fault":
            _base = new AmlElementWriter(_factory) { ReadOnly = _readOnly };
            _base.Complete += OnComplete;
            _base.WriteStartElement(name);
            break;
          case "Message":
            _base = new AmlElementWriter(_factory) { ReadOnly = _readOnly };
            _base.Complete += OnComplete;
            _base.WriteStartElement(name);
            break;
          case "Result":
            _resultText = new StringBuilder();
            break;
        }
        _names.Push(name);
      }
    }

    private void OnComplete(object sender, EventArgs e)
    {
      var itemWriter = _base as ItemElementWriter;
      if (itemWriter != null)
      {
        _result.AddReadOnly(itemWriter.Result as IReadOnlyItem);
        itemWriter.Complete -= OnComplete;
      }
      else
      {
        var elemWriter = _base as AmlElementWriter;
        if (elemWriter != null)
        {
          elemWriter.Complete -= OnComplete;
          var elem = elemWriter.Root;
          if (elem == null)
            throw new InvalidOperationException();

          if (elem.Name == "Message")
          {
            _result.AddMessageNode(elem);
          }
          else
          {
            switch (elem.ElementByName("faultcode").Value)
            {
              case "0":
                _result.Exception = new NoItemsFoundException(elem, _database, _query);
                break;
              case "1001":
                if (elem.ElementByName("detail").ElementByName("error_resolution_report").Exists)
                {
                  _result.Exception = new ValidationReportException(elem, _database, _query);
                }
                else
                {
                  _result.Exception = new ValidationException(elem, _database, _query);
                }
                break;
              default:
                _result.Exception = new ServerException(elem, _database, _query);
                break;
            }
          }
        }
        else
        {
          throw new InvalidOperationException();
        }
      }

      _base = null;
    }

    public override void WriteString(string text)
    {
      AddString(text);
    }

    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
      AddString(new string(new char[] { highChar, lowChar }));
    }

    public override void WriteWhitespace(string ws)
    {
      if (_inElement) AddString(ws);
    }

    private void AddString(string value)
    {
      if (_value == null)
        _value = value;
      else
        _value += value;
    }

    protected string GetName(string localName, string ns)
    {
      if (string.IsNullOrEmpty(ns))
        return localName;
      if (ns.TrimEnd('/') == "http://www.w3.org/2000/xmlns")
        return IgnoreAttribute;
      var prefix = LookupPrefix(ns);
      if (string.IsNullOrEmpty(prefix))
        return localName;
      return prefix + ":" + localName;
    }

    private interface IElementWriter
    {
      event EventHandler Complete;
      void WriteStartElement(string name);
      void WriteEndElement(string value, bool cdata);
      void WriteEndAttribute(string name, string value);
    }

    private class ItemElementWriter : IElementWriter
    {
      public event EventHandler Complete;

      private ElementFactory _factory;
      private Stack<IElement> _stack;
      private IElement _result;

      public bool ReadOnly { get; set; }
      public IElement Result { get { return _result; } }

      public ItemElementWriter(ElementFactory factory)
      {
        _factory = factory;
        _stack = new Stack<IElement>();
      }

      public void WriteEndAttribute(string name, string value)
      {
        var peek = _stack.Peek();
        if (name == "type" && _factory.ItemFactory != null && peek is Item)
        {
          peek = _factory.ItemFactory.NewItem(_factory, value);
          if (peek == null)
          {
            peek = _stack.Peek();
          }
          else
          {
            var old = _stack.Pop();
            old.Remove();
            peek.Add(((IReadOnlyItem)old).Attributes());
            //if (ReadOnly)
            //  ((Item)peek).SetFlag(ElementAttributes.FromDataStore);
            ((Item)peek).Parent = _stack.Count > 0 ? _stack.Peek() : null;
            PushElement(peek);
          }
        }

        switch (name)
        {
          case "id":
            if (peek is Item && value.IsGuid() && ReadOnly)
              AddAttribute(new IdAnnotation(peek, new Guid(value)));
            else
              AddAttribute(new Attribute(peek as Element, name, value));
            break;
          default:
            AddAttribute(new Attribute(peek as Element, name, value));
            break;
        }
      }

      private void AddAttribute(ILinkedAnnotation attr)
      {
        var elem = _stack.Peek() as Element;
        if (elem == null)
        {
          _stack.Peek().Add(attr);
        }
        else
        {
          elem.QuickAddAttribute(attr);
        }
      }

      public void WriteEndElement(string value, bool cdata)
      {
        if (value != null)
        {
          var peek = _stack.Peek();
          peek.Add(value);
          if (ReadOnly)
          {
            var item = peek.Parent as Item;
            switch (peek.Name)
            {
              case "generation":
                if (item != null && value == "1")
                {
                  item.SetFlag(ElementAttributes.ItemDefaultGeneration);
                  peek.Remove();
                }
                break;
              case "is_current":
                if (item != null && value == "1")
                {
                  item.SetFlag(ElementAttributes.ItemDefaultIsCurrent);
                  peek.Remove();
                }
                break;
              case "is_released":
                if (item != null && value == "0")
                {
                  item.SetFlag(ElementAttributes.ItemDefaultIsReleased);
                  peek.Remove();
                }
                break;
              case "major_rev":
                if (item != null && value == "A")
                {
                  item.SetFlag(ElementAttributes.ItemDefaultMajorRev);
                  peek.Remove();
                }
                break;
              case "new_version":
                if (item != null && value == "0")
                {
                  item.SetFlag(ElementAttributes.ItemDefaultNewVersion);
                  peek.Remove();
                }
                break;
              case "not_lockable":
                if (item != null && value == "0")
                {
                  item.SetFlag(ElementAttributes.ItemDefaultNotLockable);
                  peek.Remove();
                }
                break;
            }
          }
        }

        var iElem = _stack.Pop();

        var elem = iElem as Element;
        if (elem != null)
        {
          elem.ReadOnly = ReadOnly;
          elem.PreferCData = cdata;
        }

        if (_stack.Count < 1)
          OnComplete(EventArgs.Empty);
      }

      public void WriteStartElement(string name)
      {
        IElement curr;
        switch (name)
        {
          case "Item":
            curr = new Item(_factory) { Parent = _stack.Count > 0 ? _stack.Peek() : null };
            //if (ReadOnly)
            //  ((Item)curr).SetFlag(ElementAttributes.FromDataStore);
            break;
          case "Relationships":
            curr = new Relationships(_stack.Peek());
            break;
          case "and":
          case "or":
          case "not":
            curr = new Logical(_stack.Peek(), name);
            break;
          default:
            curr = new Property(_stack.Peek(), name);
            break;
        }
        PushElement(curr);
      }

      private void PushElement(IElement element)
      {
        if (_stack.Count > 0)
        {
          var elem = _stack.Peek() as Element;
          if (elem == null)
          {
            _stack.Peek().Add(element);
          }
          else
          {
            elem.QuickAddElement((ILinkedElement)element);
          }
        }
        else
        {
          _result = element;
        }
        _stack.Push(element);
      }

      protected virtual void OnComplete(EventArgs e)
      {
        if (Complete != null)
          Complete.Invoke(this, e);
      }

      //private Item Normalize(Item item)
      //{
      //  var idProp = item.Property("id");
      //  if (idProp.Exists && idProp.Value.IsGuid())
      //  {
      //    var typeAttr = item.Type();
      //    var idTypeAttr = idProp.Type();
      //    if (idTypeAttr.HasValue() && !typeAttr.HasValue())
      //    {
      //      typeAttr.Set(idTypeAttr.Value);
      //    }

      //    var keyedNameProp = item.KeyedName();
      //    var idKeyedNameAttr = idProp.KeyedName();
      //    if (idKeyedNameAttr.HasValue() && !keyedNameProp.HasValue())
      //    {
      //      keyedNameProp.Set(idKeyedNameAttr.Value);
      //    }

      //    idProp.Remove();
      //  }

      //  return item;
      //}
    }

    private class AmlElementWriter : IElementWriter
    {
      private ElementFactory _factory;
      private Element _root;
      private Element _curr;

      public event EventHandler Complete;

      public bool ReadOnly { get; set; }
      public Element Root { get { return _root; } }

      public AmlElementWriter(ElementFactory factory)
      {
        _factory = factory;
      }

      public void WriteEndAttribute(string name, string value)
      {
        _curr.Add(new Attribute(name, value));
      }

      public void WriteEndElement(string value, bool cdata)
      {
        if (value != null)
        {
          _curr.Value = value;
        }
        _curr.ReadOnly = ReadOnly;
        _curr.PreferCData = cdata;
        _curr = _curr.Parent as Element;
        if (_curr == null
          || (!_curr.Exists && _root != _curr))
          OnComplete(EventArgs.Empty);
      }

      public void WriteStartElement(string name)
      {
        var newElem = new AmlElement(_factory, name);
        if (_curr == null)
        {
          _root = newElem;
        }
        else
        {
          _curr.Add(newElem);
        }
        _curr = newElem;
      }

      protected virtual void OnComplete(EventArgs e)
      {
        if (Complete != null)
          Complete.Invoke(this, e);
      }
    }
  }
}
