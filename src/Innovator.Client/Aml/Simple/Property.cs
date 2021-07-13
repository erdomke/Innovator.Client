using System;
using System.Collections;
using System.Linq;

namespace Innovator.Client
{
  internal class Property : Element, IProperty
#if FILEIO
    , IConvertible
#endif
  {
    private IElement _parent;

    public override string Name { get; }
    public override ILinkedElement Next { get; set; }
    public override IElement Parent
    {
      get { return _parent ?? AmlElement.NullElem; }
      set { _parent = value; }
    }
    public override string Prefix { get; }

    private object NeutralValue()
    {
      if (!this.Exists || Attribute("is_null").AsBoolean(false)) return null;
      var neutral = Attribute("neutral_value");
      if (neutral.HasValue()) return neutral.Value;

      return _content;
    }

    public Property(string name, params object[] content)
    {
      var kvp = XmlUtils.GetXmlNamePrefix(name);
      Prefix = kvp.Key;
      Name = kvp.Value;
      if (content == null)
        this.IsNull().Set(true);
      else if (content.Length > 0)
        Add(content);
    }

    public Property(IElement parent, string name, params object[] content)
    {
      var kvp = XmlUtils.GetXmlNamePrefix(name);
      Prefix = kvp.Key;
      Name = kvp.Value;
      _parent = parent;
      if (content?.Length > 0)
      {
        for (var i = 0; i < content.Length; i++)
          base.Add(content[i]);
      }
    }

    private Property(IElement parent, Property clone)
    {
      Name = clone.Name;
      Prefix = clone.Prefix;
      _parent = parent;
      CopyData(clone);
    }

    static Property()
    {
      NullProp = new Property(null) { ReadOnly = true };
      DefaultGeneration = new Property("generation", "1") { ReadOnly = true };
      DefaultGeneration.Next = DefaultGeneration;
      DefaultIsCurrent = new Property("is_current", "1") { ReadOnly = true };
      DefaultIsCurrent.Next = DefaultIsCurrent;
      DefaultIsReleased = new Property("is_released", "0") { ReadOnly = true };
      DefaultIsReleased.Next = DefaultIsReleased;
      DefaultMajorRev = new Property("major_rev", "A") { ReadOnly = true };
      DefaultMajorRev.Next = DefaultMajorRev;
      DefaultNewVersion = new Property("new_version", "0") { ReadOnly = true };
      DefaultNewVersion.Next = DefaultNewVersion;
      DefaultNotLockable = new Property("not_lockable", "0") { ReadOnly = true };
      DefaultNotLockable.Next = DefaultNotLockable;
    }

    public static Property NullProp { get; }
    public static Property DefaultGeneration { get; }
    public static Property DefaultIsCurrent { get; }
    public static Property DefaultIsReleased { get; }
    public static Property DefaultMajorRev { get; }
    public static Property DefaultNewVersion { get; }
    public static Property DefaultNotLockable { get; }

    public bool? AsBoolean()
    {
      if (!this.Exists) return null;
      return (_parent == null ? ElementFactory.Local : _parent.AmlContext).LocalizationContext.AsBoolean(NeutralValue());
    }

    public DateTime? AsDateTime()
    {
      if (!this.Exists) return null;
      return (_parent == null ? ElementFactory.Local : _parent.AmlContext).LocalizationContext.AsDateTime(NeutralValue());
    }

    public DateTimeOffset? AsDateTimeOffset()
    {
      if (!this.Exists) return null;
      return (_parent == null ? ElementFactory.Local : _parent.AmlContext).LocalizationContext.AsZonedDateTime(NeutralValue())?.ToDateTimeOffset();
    }

    public DateTime? AsDateTimeUtc()
    {
      if (!this.Exists) return null;
      return (_parent == null ? ElementFactory.Local : _parent.AmlContext).LocalizationContext.AsDateTimeUtc(NeutralValue());
    }

    public Guid? AsGuid()
    {
      if (!this.Exists || _content == null) return null;
      var str = _content.ToString();
      if (str.StartsWith(Utils.VaultPicturePrefix, StringComparison.OrdinalIgnoreCase))
        str = str.Substring(Utils.VaultPicturePrefix.Length);
      return new Guid(str);
    }

    public int? AsInt()
    {
      if (!this.Exists) return null;
      return (_parent == null ? ElementFactory.Local : _parent.AmlContext).LocalizationContext.AsInt(NeutralValue());
    }

    public IItem AsItem()
    {
      if (!this.Exists)
      {
        if (_parent?.Exists != true)
          return Item.GetNullItem<Item>();
        return new Item(AmlContext)
        {
          Parent = this,
          IsNull = true,
          ReadOnly = false
        };
      }

      var item = _content as IItem;
      var typeAttr = Attribute("type");
      if (item == null && IsGuid() && typeAttr.Exists)
      {
        var aml = AmlContext ?? ElementFactory.Local;
        item = aml.Item(aml.Type(typeAttr.Value), aml.Id(AsGuid()));
        var keyedName = Attribute("keyed_name");
        if (keyedName.Exists)
        {
          item.Property("keyed_name").Set(keyedName.Value);
          item.Add(aml.IdProp(aml.Attribute("keyed_name", keyedName.Value), aml.Type(typeAttr.Value), AsGuid()));
        }
        else
        {
          item.Add(aml.IdProp(aml.Type(typeAttr.Value), AsGuid()));
        }
      }
      else if (item == null
        && _content is string
        && ((string)_content).StartsWith(Utils.VaultPicturePrefix, StringComparison.OrdinalIgnoreCase))
      {
        var aml = AmlContext ?? ElementFactory.Local;
        var id = ((string)_content).Substring(Utils.VaultPicturePrefix.Length);
        item = aml.Item(aml.Type("File"), aml.Id(id)
          , aml.IdProp(aml.Type("File"), id));
      }
      return item ?? Item.GetNullItem<Item>();
    }

    private bool IsGuid()
    {
      return _content is Guid || (_content is string && ((string)_content).IsGuid());
    }

    public long? AsLong()
    {
      if (!this.Exists) return null;
      return (_parent == null ? ElementFactory.Local : _parent.AmlContext).LocalizationContext.AsLong(NeutralValue());
    }

    public double? AsDouble()
    {
      if (!this.Exists) return null;
      return (_parent == null ? ElementFactory.Local : _parent.AmlContext).LocalizationContext.AsDouble(NeutralValue());
    }

    public string AsString(string defaultValue)
    {
      if (!this.Exists || Attribute("is_null").AsBoolean(false))
        return defaultValue;
      return this.Value ?? defaultValue;
    }

    public override IElement Add(object content)
    {
      if (!Exists && _parent != null)
        AddToParent();

      return AddBase(content);
    }

    public void Set(object value)
    {
      AssertModifiable();
      if (!Exists)
      {
        if (_parent == null)
          throw new InvalidOperationException();
        AddToParent();
      }

      _content = null;
      AddBase(value);
    }

    private void AddToParent()
    {
      _parent.Add(this);
      var itemParent = _parent as Item;
      if (itemParent?.Exists == false && itemParent?.ReadOnly == false)
      {
        itemParent.IsNull = false;
        itemParent.Parent.Add(itemParent);
      }
    }

    private IElement AddBase(object content)
    {
      var flat = Flatten(content);
      var result = base.Add(!(flat is string)
          && flat is IEnumerable e
          && !e.OfType<IReadOnlyAttribute>().Any()
          && !e.OfType<IReadOnlyElement>().Any()
        ? (AmlContext ?? ElementFactory.Local).LocalizationContext.Format(flat)
        : flat);
      var isNull = this.IsNull();
      if (_content == null
#if DBDATA
        || _content == DBNull.Value
#endif
      )
      {
        isNull.Set(true);
      }
      else
      {
        isNull.Remove();

        if (_content is IRange range)
        {
          this.Condition().Set(Condition.Between);

          if (_content is Range<DateOffset> dateRange)
          {
            Attribute(ParameterSubstitution.DateRangeAttribute)
              .Set(ParameterSubstitution.SerializeDateRange(dateRange));
          }
        }
      }
      return result;
    }

    /// <summary>
    /// Remove single element arrays
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Flattened value</returns>
    private object Flatten(object value)
    {
      if (!(value is string) && value is IEnumerable e)
      {
        var enumerator = e.GetEnumerator();
        try
        {
          var count = 0;
          var single = default(object);
          while (enumerator.MoveNext())
          {
            count++;
            single = enumerator.Current;
            if (count > 1)
              return value;
          }
          if (count == 1)
            return Flatten(single);
        }
        finally
        {
          (enumerator as IDisposable)?.Dispose();
        }
      }
      return value;
    }

    IReadOnlyItem IReadOnlyProperty_Item<IReadOnlyItem>.AsItem()
    {
      var result = AsItem();
      var elem = result as Element;
      if (elem != null)
        elem.ReadOnly = true;
      return result;
    }

    protected override Element Clone(IElement newParent)
    {
      return new Property(newParent, this);
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
