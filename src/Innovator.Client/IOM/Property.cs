using System;
using System.Linq;
using System.Xml;

namespace Innovator.Client.IOM
{
  internal class Property : Element, IProperty
  {
    private string _name;

    public override string Name => base.Name ?? _name;
    public override IElement Parent { get; set; }

    public Property(Element parent, string name)
    {
      Parent = parent;
      _name = name;
    }

    public Property(Element parent, XmlElement node)
    {
      Parent = parent;
      Xml = node;
    }

    private object NeutralValue()
    {
      if (!this.Exists || Attribute("is_null").AsBoolean(false)) return null;
      var neutral = Attribute("neutral_value");
      if (neutral.HasValue()) return neutral.Value;

      var item = Xml.ChildNodes.OfType<XmlElement>().FirstOrDefault();
      if (item != null)
        return AsItem().Id();

      return Xml.InnerText;
    }

    public bool? AsBoolean()
    {
      if (!this.Exists) return null;
      return (Parent == null ? ElementFactory.Local : Parent.AmlContext).LocalizationContext.AsBoolean(NeutralValue());
    }

    public DateTime? AsDateTime()
    {
      if (!this.Exists) return null;
      return (Parent == null ? ElementFactory.Local : Parent.AmlContext).LocalizationContext.AsDateTime(NeutralValue());
    }

    public DateTimeOffset? AsDateTimeOffset()
    {
      if (!this.Exists) return null;
      return (Parent == null ? ElementFactory.Local : Parent.AmlContext).LocalizationContext.AsZonedDateTime(NeutralValue())?.ToDateTimeOffset();
    }

    public DateTime? AsDateTimeUtc()
    {
      if (!this.Exists) return null;
      return (Parent == null ? ElementFactory.Local : Parent.AmlContext).LocalizationContext.AsDateTimeUtc(NeutralValue());
    }

    public double? AsDouble()
    {
      if (!this.Exists) return null;
      return (Parent == null ? ElementFactory.Local : Parent.AmlContext).LocalizationContext.AsDouble(NeutralValue());
    }

    public Guid? AsGuid()
    {
      var content = NeutralValue();
      if (!this.Exists || content == null) return null;
      var str = content.ToString();
      if (str.StartsWith(Utils.VaultPicturePrefix, StringComparison.OrdinalIgnoreCase))
        str = str.Substring(Utils.VaultPicturePrefix.Length);
      return new Guid(str);
    }

    public int? AsInt()
    {
      if (!this.Exists) return null;
      return (Parent == null ? ElementFactory.Local : Parent.AmlContext).LocalizationContext.AsInt(NeutralValue());
    }

    public IItem AsItem()
    {
      if (!this.Exists)
      {
        if (Parent?.Exists != true)
          return Client.Item.GetNullItem<Client.Item>();
        return new Item((Innovator)AmlContext, this);
      }

      var item = Xml.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.LocalName == "Item");
      var typeName = Xml.GetAttribute("type");
      if (item == null && !string.IsNullOrEmpty(typeName) && Xml.InnerText.IsGuid())
      {
        var newDoc = new XmlDocument();
        item = newDoc.CreateElement("Item");
        newDoc.AppendChild(item);

        var id = Xml.InnerText;
        item.SetAttribute("type", typeName);
        item.SetAttribute("id", id);

        var idNode = newDoc.CreateElement("id");
        idNode.SetAttribute("type", typeName);
        idNode.InnerText = id;
        item.AppendChild(idNode);

        var keyedName = Xml.GetAttribute("keyed_name");
        if (!string.IsNullOrEmpty(keyedName))
        {
          idNode.SetAttribute("keyed_name", keyedName);
          var keyedNameNode = newDoc.CreateElement("keyed_name");
          keyedNameNode.InnerText = keyedName;
          item.AppendChild(keyedNameNode);
        }
      }
      else if (item == null && Xml.InnerText?.StartsWith(Utils.VaultPicturePrefix) == true)
      {
        var newDoc = new XmlDocument();
        item = newDoc.CreateElement("Item");
        newDoc.AppendChild(item);

        var id = Xml.InnerText.Substring(Utils.VaultPicturePrefix.Length);
        item.SetAttribute("type", "File");
        item.SetAttribute("id", id);

        var idNode = newDoc.CreateElement("id");
        idNode.SetAttribute("type", "File");
        idNode.InnerText = id;
        item.AppendChild(idNode);
      }

      if (item == null)
        return Client.Item.GetNullItem<Client.Item>();
      return new Item((Innovator)Parent.AmlContext, item) { Parent = this };
    }

    public long? AsLong()
    {
      if (!this.Exists) return null;
      return (Parent == null ? ElementFactory.Local : Parent.AmlContext).LocalizationContext.AsLong(NeutralValue());
    }

    public string AsString(string defaultValue)
    {
      if (!this.Exists || Attribute("is_null").AsBoolean(false))
        return defaultValue;
      return this.Value;
    }

    public override IElement Add(object content)
    {
      if (!Exists && Parent != null)
        AddToParent();
      return AddBase(content);
    }

    public void Set(object value)
    {
      if (!Exists)
      {
        if (Parent == null)
          throw new InvalidOperationException();
        AddToParent();
      }

      Xml.InnerText = null;
      AddBase(value);
    }

    IReadOnlyItem IReadOnlyProperty_Item<IReadOnlyItem>.AsItem()
    {
      return AsItem();
    }

    private void AddToParent()
    {
      var parentNode = ((Element)Parent).Xml;
      Xml = parentNode.OwnerDocument.CreateElement(_name);
      parentNode.AppendChild(Xml);
    }

    private IElement AddBase(object content)
    {
      var result = base.Add(content);
      if (content == null
#if DBDATA
        || content == DBNull.Value
#endif
      )
      {
        Xml.SetAttribute("is_null", "1");
        Xml.IsEmpty = true;
      }
      else
      {
        Xml.RemoveAttribute("is_null");

        if (content is IRange range)
        {
          Xml.SetAttribute("condition", "between");

          if (content is Range<DateOffset> dateRange)
          {
            Xml.SetAttribute(ParameterSubstitution.DateRangeAttribute
              , ParameterSubstitution.SerializeDateRange(dateRange));
          }
        }
      }

      return result;
    }
  }
}
