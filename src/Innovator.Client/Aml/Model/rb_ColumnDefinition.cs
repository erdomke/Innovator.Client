using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_ColumnDefinition </summary>
  [ArasName("rb_ColumnDefinition")]
  public class rb_ColumnDefinition : Item, INullRelationship<rb_TreeGridViewDefinition>
  {
    protected rb_ColumnDefinition() { }
    public rb_ColumnDefinition(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_ColumnDefinition() { Innovator.Client.Item.AddNullItem<rb_ColumnDefinition>(new rb_ColumnDefinition { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>builder_method</c> property of the item</summary>
    [ArasName("builder_method")]
    public IProperty_Item<Method> BuilderMethod()
    {
      return this.Property("builder_method");
    }
    /// <summary>Retrieve the <c>cell_view_type</c> property of the item</summary>
    [ArasName("cell_view_type")]
    public IProperty_Text CellViewType()
    {
      return this.Property("cell_view_type");
    }
    /// <summary>Retrieve the <c>data_template</c> property of the item</summary>
    [ArasName("data_template")]
    public IProperty_Text DataTemplate()
    {
      return this.Property("data_template");
    }
    /// <summary>Retrieve the <c>header</c> property of the item</summary>
    [ArasName("header")]
    public IProperty_Text Header()
    {
      return this.Property("header");
    }
    /// <summary>Retrieve the <c>header_style_ref_id</c> property of the item</summary>
    [ArasName("header_style_ref_id")]
    public IProperty_Text HeaderStyleRefId()
    {
      return this.Property("header_style_ref_id");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>position_order</c> property of the item</summary>
    [ArasName("position_order")]
    public IProperty_Number PositionOrder()
    {
      return this.Property("position_order");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>style_ref_id</c> property of the item</summary>
    [ArasName("style_ref_id")]
    public IProperty_Text StyleRefId()
    {
      return this.Property("style_ref_id");
    }
    /// <summary>Retrieve the <c>template</c> property of the item</summary>
    [ArasName("template")]
    public IProperty_Text Template()
    {
      return this.Property("template");
    }
    /// <summary>Retrieve the <c>width</c> property of the item</summary>
    [ArasName("width")]
    public IProperty_Number Width()
    {
      return this.Property("width");
    }
  }
}