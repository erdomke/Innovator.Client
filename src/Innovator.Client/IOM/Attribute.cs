#if XMLLEGACY
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client.IOM
{
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  internal class Attribute : IAttribute
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Element _parent;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private XmlAttribute _node;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _name;

    public bool Exists => _node != null;

    public string Name => _node?.LocalName ?? _name;

    public string Value
    {
      get => _node?.Value;
      set => Set(value);
    }

    string IReadOnlyAttribute.Value => this.Value;

    private string DebuggerDisplay
    {
      get { return string.Format("{0}='{1}'", _name, Value); }
    }

    public Attribute(Element parent, string name)
    {
      _parent = parent;
      _name = name;
    }

    public Attribute(Element parent, XmlAttribute node)
    {
      _parent = parent;
      _node = node;
    }

    public bool? AsBoolean()
    {
      if (_node == null) return null;
      return _parent.AmlContext.LocalizationContext.AsBoolean(_node.Value);
    }

    public DateTime? AsDateTime()
    {
      if (_node == null) return null;
      return _parent.AmlContext.LocalizationContext.AsDateTime(_node.Value);
    }

    public DateTime? AsDateTimeUtc()
    {
      if (_node == null) return null;
      return _parent.AmlContext.LocalizationContext.AsDateTimeUtc(_node.Value);
    }

    public double? AsDouble()
    {
      if (_node == null) return null;
      return _parent.AmlContext.LocalizationContext.AsDouble(_node.Value);
    }

    public Guid? AsGuid()
    {
      if (string.IsNullOrEmpty(_node?.Value)) return null;
      return new Guid(_node.Value);
    }

    public int? AsInt()
    {
      if (_node == null) return null;
      return _parent.AmlContext.LocalizationContext.AsInt(_node.Value);
    }

    public long? AsLong()
    {
      if (_node == null) return null;
      return _parent.AmlContext.LocalizationContext.AsLong(_node.Value);
    }

    public string AsString(string defaultValue)
    {
      if (_node == null) return defaultValue;
      return this.Value;
    }

    public void Remove()
    {
      _parent.Xml.RemoveAttribute(Name);
      _name = Name;
      _node = null;
    }

    public void Set(object value)
    {
      if (_parent == null)
        throw new InvalidOperationException("Cannot modify a read only element");

      if (_node == null)
      {
        if (value != null)
        {
          _parent.Xml.SetAttribute(_name, _parent.AmlContext.LocalizationContext.Format(value));
          _node = _parent.Xml.GetAttributeNode(_name);
        }
      }
      else
      {
        if (value == null)
          Remove();
        else
          _node.Value = _parent.AmlContext.LocalizationContext.Format(value);
      }

    }
  }
}
#endif
