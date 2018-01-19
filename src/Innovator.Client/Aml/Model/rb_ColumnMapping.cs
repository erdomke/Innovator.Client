using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_ColumnMapping </summary>
  [ArasName("rb_ColumnMapping")]
  public class rb_ColumnMapping : Item, INullRelationship<rb_ColumnDefinition>
  {
    protected rb_ColumnMapping() { }
    public rb_ColumnMapping(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_ColumnMapping() { Innovator.Client.Item.AddNullItem<rb_ColumnMapping>(new rb_ColumnMapping { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>template</c> property of the item</summary>
    [ArasName("template")]
    public IProperty_Text Template()
    {
      return this.Property("template");
    }
    /// <summary>Retrieve the <c>tree_row_ref_id</c> property of the item</summary>
    [ArasName("tree_row_ref_id")]
    public IProperty_Text TreeRowRefId()
    {
      return this.Property("tree_row_ref_id");
    }
  }
}