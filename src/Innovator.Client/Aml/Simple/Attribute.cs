using System;
using System.Diagnostics;

namespace Innovator.Client
{
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  internal class Attribute : IAttribute, ILinkedAnnotation
#if FILEIO
    , IConvertible
#endif
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Element _parent;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _name;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private object _content;

    public bool Exists { get { return Next != null; } }
    public string Name { get { return _name; } }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ILinkedAnnotation Next { get; set; }
    public string Value
    {
      get
      {
        return _parent == null || _parent.AmlContext == null
          ? _content as string
          : _parent.AmlContext.LocalizationContext.Format(_content);
      }
      set { Set(value); }
    }

    private string DebuggerDisplay
    {
      get { return string.Format("{0}='{1}'", _name, _parent?.AmlContext?.LocalizationContext.Format(_content) ?? _content); }
    }

    public Attribute(string name)
    {
      _name = name;
    }
    public Attribute(string name, object value)
    {
      _name = name;
      _content = value;
    }
    public Attribute(Element parent, string name)
    {
      _name = name;
      _parent = parent;
    }
    public Attribute(Element parent, string name, object value)
    {
      _name = name;
      _parent = parent;
      _content = value;
    }
    public Attribute(Element parent, IReadOnlyAttribute attr)
    {
      _name = attr.Name;
      _content = attr.Value;
      _parent = parent;
    }
    public static Attribute NullAttr { get; } = new Attribute(null);

    public void Set(object value)
    {
      if (_parent != null && _parent.ReadOnly)
        throw new InvalidOperationException("Cannot modify a read only element");
      if (!Exists)
        _parent.Add(this);
      _content = value;
    }

    public void Remove()
    {
      if (_parent != null)
      {
        if (_parent.ReadOnly)
          throw new InvalidOperationException("Cannot modify a read only element");
        _parent.RemoveAttribute(this);
      }
    }

    public bool? AsBoolean()
    {
      if (!this.Exists) return null;
      return _parent.AmlContext.LocalizationContext.AsBoolean(_content);
    }

    public DateTime? AsDateTime()
    {
      if (!this.Exists) return null;
      return _parent.AmlContext.LocalizationContext.AsDateTime(_content);
    }

    public DateTime? AsDateTimeUtc()
    {
      if (!this.Exists) return null;
      return _parent.AmlContext.LocalizationContext.AsDateTimeUtc(_content);
    }

    public Guid? AsGuid()
    {
      if (!this.Exists || _content == null) return null;
      return new Guid(_content.ToString());
    }

    public int? AsInt()
    {
      if (!this.Exists) return null;
      return _parent.AmlContext.LocalizationContext.AsInt(_content);
    }

    public long? AsLong()
    {
      if (!this.Exists) return null;
      return _parent.AmlContext.LocalizationContext.AsLong(_content);
    }

    public double? AsDouble()
    {
      if (!this.Exists) return null;
      return _parent.AmlContext.LocalizationContext.AsDouble(_content);
    }

    public string AsString(string defaultValue)
    {
      if (!this.Exists)
        return defaultValue;
      return this.Value ?? defaultValue;
    }

    public static Attribute TryGet(object value, Element newParent)
    {
      var impl = value as Attribute;
      if (impl != null)
      {
        if (impl._parent == null || impl._parent == newParent)
        {
          impl._parent = newParent;
          return impl;
        }
        return new Attribute(newParent, impl);
      }

      var attr = value as IReadOnlyAttribute;
      if (attr != null)
      {
        return new Attribute(newParent, attr);
      }

      return null;
    }

    public override string ToString()
    {
      return Value;
    }

#if FILEIO
    #region IConvertible
    TypeCode IConvertible.GetTypeCode()
    {
      return TypeCode.Object;
    }

    bool IConvertible.ToBoolean(IFormatProvider provider)
    {
      return AsBoolean().Value;
    }

    char IConvertible.ToChar(IFormatProvider provider)
    {
      if (Value?.Length == 1)
        return Value[0];
      throw new InvalidCastException();
    }

    sbyte IConvertible.ToSByte(IFormatProvider provider)
    {
      return Convert.ToSByte(AsInt().Value);
    }

    byte IConvertible.ToByte(IFormatProvider provider)
    {
      return Convert.ToByte(AsInt().Value);
    }

    short IConvertible.ToInt16(IFormatProvider provider)
    {
      return Convert.ToInt16(AsInt().Value);
    }

    ushort IConvertible.ToUInt16(IFormatProvider provider)
    {
      return Convert.ToUInt16(AsInt().Value);
    }

    int IConvertible.ToInt32(IFormatProvider provider)
    {
      return AsInt().Value;
    }

    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
      return Convert.ToUInt32(AsInt().Value);
    }

    long IConvertible.ToInt64(IFormatProvider provider)
    {
      return AsLong().Value;
    }

    ulong IConvertible.ToUInt64(IFormatProvider provider)
    {
      return Convert.ToUInt64(AsLong().Value);
    }

    float IConvertible.ToSingle(IFormatProvider provider)
    {
      return Convert.ToSingle(AsDouble().Value);
    }

    double IConvertible.ToDouble(IFormatProvider provider)
    {
      return AsDouble().Value;
    }

    decimal IConvertible.ToDecimal(IFormatProvider provider)
    {
      if (!this.Exists)
        throw new InvalidCastException();
      return _parent.AmlContext.LocalizationContext.AsDecimal(_content).Value;
    }

    DateTime IConvertible.ToDateTime(IFormatProvider provider)
    {
      return AsDateTime().Value;
    }

    string IConvertible.ToString(IFormatProvider provider)
    {
      return Value;
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider provider)
    {
      return Convert.ChangeType(Value, conversionType, provider);
    }
    #endregion
#endif
  }
}
