using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_TreeRowReference </summary>
  [ArasName("rb_TreeRowReference")]
  public class rb_TreeRowReference : Item, INullRelationship<rb_TreeGridViewDefinition>
  {
    protected rb_TreeRowReference() { }
    public rb_TreeRowReference(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_TreeRowReference() { Innovator.Client.Item.AddNullItem<rb_TreeRowReference>(new rb_TreeRowReference { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>child_ref_id</c> property of the item</summary>
    [ArasName("child_ref_id")]
    public IProperty_Text ChildRefId()
    {
      return this.Property("child_ref_id");
    }
    /// <summary>Retrieve the <c>parent_ref_id</c> property of the item</summary>
    [ArasName("parent_ref_id")]
    public IProperty_Text ParentRefId()
    {
      return this.Property("parent_ref_id");
    }
    /// <summary>Retrieve the <c>reference_type</c> property of the item</summary>
    [ArasName("reference_type")]
    public IProperty_Text ReferenceType()
    {
      return this.Property("reference_type");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>view_order</c> property of the item</summary>
    [ArasName("view_order")]
    public IProperty_Number ViewOrder()
    {
      return this.Property("view_order");
    }
  }
}