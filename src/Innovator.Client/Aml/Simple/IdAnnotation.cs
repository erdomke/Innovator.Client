using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Innovator.Client
{
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  internal class IdAnnotation : IReadOnlyProperty, IReadOnlyAttribute, ILinkedAnnotation
  {
    private readonly IReadOnlyElement _parent;
    private Guid _value;

    public ElementFactory AmlContext
    {
      get { return _parent == null ? ElementFactory.Local : _parent.AmlContext; }
    }
    public bool Exists { get { return _parent != null; } }
    public string Name { get { return "id"; } }
    public ILinkedAnnotation Next { get; set; }
    public IReadOnlyElement Parent { get { return _parent ?? Item.GetNullItem<Item>(); } }
    public string Prefix { get { return string.Empty; } }
    public string Value { get { return _value.ToArasId(); } }

    public IdAnnotation(IReadOnlyElement parent, Guid value)
    {
      _parent = parent;
      _value = value;
    }

    private string DebuggerDisplay
    {
      get { return "id='" + _value.ToArasId() + "'"; }
    }

    public Guid? AsGuid()
    {
      return _value;
    }

    public string AsString(string defaultValue)
    {
      return this.Value ?? defaultValue;
    }

    #region IReadOnlyProperty

    public bool? AsBoolean()
    {
      throw new NotSupportedException();
    }
    public DateTime? AsDateTime()
    {
      throw new NotSupportedException();
    }
    public DateTimeOffset? AsDateTimeOffset()
    {
      throw new NotSupportedException();
    }
    public DateTime? AsDateTimeUtc()
    {
      throw new NotSupportedException();
    }
    public double? AsDouble()
    {
      throw new NotSupportedException();
    }
    public int? AsInt()
    {
      throw new NotSupportedException();
    }
    public IReadOnlyItem AsItem()
    {
      return _parent as IReadOnlyItem;
    }
    public long? AsLong()
    {
      throw new NotSupportedException();
    }
    #endregion

    public IReadOnlyAttribute Attribute(string name)
    {
      return Attributes().FirstOrDefault(a => a.Name == name)
        ?? Client.Attribute.NullAttr;
    }

    public IEnumerable<IReadOnlyAttribute> Attributes()
    {
      var item = _parent as IReadOnlyItem;
      if (item != null)
      {
        var type = item.Type();
        if (type.Exists)
          yield return type;
        var keyedName = item.KeyedName();
        if (keyedName.Exists)
        {
          var result = new Attribute("keyed_name", keyedName.Value);
          result.Next = result;
          yield return result;
        }
      }
    }

    public IEnumerable<IReadOnlyElement> Elements()
    {
      return Enumerable.Empty<IReadOnlyElement>();
    }

    public void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      writer.WriteStartElement("id");
      foreach (var attr in Attributes())
      {
        writer.WriteAttributeString(attr.Name, attr.Value);
      }
      writer.WriteString(this.Value);
      writer.WriteEndElement();
    }
  }
}
