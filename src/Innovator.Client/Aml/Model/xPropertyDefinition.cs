using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xPropertyDefinition </summary>
  [ArasName("xPropertyDefinition")]
  public class xPropertyDefinition : Item, IPropertyDefinition
  {
    protected xPropertyDefinition() { }
    public xPropertyDefinition(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xPropertyDefinition() { Innovator.Client.Item.AddNullItem<xPropertyDefinition>(new xPropertyDefinition { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>column_alignment</c> property of the item</summary>
    [ArasName("column_alignment")]
    public IProperty_Text ColumnAlignment()
    {
      return this.Property("column_alignment");
    }
    /// <summary>Retrieve the <c>column_width</c> property of the item</summary>
    [ArasName("column_width")]
    public IProperty_Number ColumnWidth()
    {
      return this.Property("column_width");
    }
    /// <summary>Retrieve the <c>data_source</c> property of the item</summary>
    [ArasName("data_source")]
    public IProperty_Item<ItemType> DataSource()
    {
      return this.Property("data_source");
    }
    /// <summary>Retrieve the <c>data_type</c> property of the item</summary>
    [ArasName("data_type")]
    public IProperty_Text DataType()
    {
      return this.Property("data_type");
    }
    /// <summary>Retrieve the <c>default_value</c> property of the item</summary>
    [ArasName("default_value")]
    public IProperty_Text DefaultValue()
    {
      return this.Property("default_value");
    }
    /// <summary>Retrieve the <c>is_indexed</c> property of the item</summary>
    [ArasName("is_indexed")]
    public IProperty_Boolean IsIndexed()
    {
      return this.Property("is_indexed");
    }
    /// <summary>Retrieve the <c>is_required</c> property of the item</summary>
    [ArasName("is_required")]
    public IProperty_Boolean IsRequired()
    {
      return this.Property("is_required");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>pattern</c> property of the item</summary>
    [ArasName("pattern")]
    public IProperty_Text Pattern()
    {
      return this.Property("pattern");
    }
    /// <summary>Retrieve the <c>prec</c> property of the item</summary>
    [ArasName("prec")]
    public IProperty_Number Prec()
    {
      return this.Property("prec");
    }
    /// <summary>Retrieve the <c>private_permission_behavior</c> property of the item</summary>
    [ArasName("private_permission_behavior")]
    public IProperty_Text PrivatePermissionBehavior()
    {
      return this.Property("private_permission_behavior");
    }
    /// <summary>Retrieve the <c>readonly</c> property of the item</summary>
    [ArasName("readonly")]
    public IProperty_Boolean Readonly()
    {
      return this.Property("readonly");
    }
    /// <summary>Retrieve the <c>scale</c> property of the item</summary>
    [ArasName("scale")]
    public IProperty_Number Scale()
    {
      return this.Property("scale");
    }
    /// <summary>Retrieve the <c>stored_length</c> property of the item</summary>
    [ArasName("stored_length")]
    public IProperty_Number StoredLength()
    {
      return this.Property("stored_length");
    }
    /// <summary>Retrieve the <c>track_history</c> property of the item</summary>
    [ArasName("track_history")]
    public IProperty_Boolean TrackHistory()
    {
      return this.Property("track_history");
    }
  }
}