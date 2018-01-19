using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_TreeRowDefinition </summary>
  [ArasName("rb_TreeRowDefinition")]
  public class rb_TreeRowDefinition : Item, INullRelationship<rb_TreeGridViewDefinition>
  {
    protected rb_TreeRowDefinition() { }
    public rb_TreeRowDefinition(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_TreeRowDefinition() { Innovator.Client.Item.AddNullItem<rb_TreeRowDefinition>(new rb_TreeRowDefinition { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>query_item_ref_id</c> property of the item</summary>
    [ArasName("query_item_ref_id")]
    public IProperty_Text QueryItemRefId()
    {
      return this.Property("query_item_ref_id");
    }
    /// <summary>Retrieve the <c>ref_id</c> property of the item</summary>
    [ArasName("ref_id")]
    public IProperty_Text RefId()
    {
      return this.Property("ref_id");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}