using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_UI_CellEditor </summary>
  [ArasName("rb_UI_CellEditor")]
  public class rb_UI_CellEditor : Item, INullRelationship<rb_TreeGridViewDefinition>
  {
    protected rb_UI_CellEditor() { }
    public rb_UI_CellEditor(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_UI_CellEditor() { Innovator.Client.Item.AddNullItem<rb_UI_CellEditor>(new rb_UI_CellEditor { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>cell_view_type</c> property of the item</summary>
    [ArasName("cell_view_type")]
    public IProperty_Text CellViewType()
    {
      return this.Property("cell_view_type");
    }
    /// <summary>Retrieve the <c>icon_template</c> property of the item</summary>
    [ArasName("icon_template")]
    public IProperty_Text IconTemplate()
    {
      return this.Property("icon_template");
    }
    /// <summary>Retrieve the <c>innovator_type_name</c> property of the item</summary>
    [ArasName("innovator_type_name")]
    public IProperty_Text InnovatorTypeName()
    {
      return this.Property("innovator_type_name");
    }
    /// <summary>Retrieve the <c>item_id_template</c> property of the item</summary>
    [ArasName("item_id_template")]
    public IProperty_Text ItemIdTemplate()
    {
      return this.Property("item_id_template");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>text_template</c> property of the item</summary>
    [ArasName("text_template")]
    public IProperty_Text TextTemplate()
    {
      return this.Property("text_template");
    }
  }
}