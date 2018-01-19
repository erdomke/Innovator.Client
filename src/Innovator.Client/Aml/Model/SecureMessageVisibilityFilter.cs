using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SecureMessageVisibilityFilter </summary>
  [ArasName("SecureMessageVisibilityFilter")]
  public class SecureMessageVisibilityFilter : Item, IRelationship<Identity>
  {
    protected SecureMessageVisibilityFilter() { }
    public SecureMessageVisibilityFilter(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SecureMessageVisibilityFilter() { Innovator.Client.Item.AddNullItem<SecureMessageVisibilityFilter>(new SecureMessageVisibilityFilter { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>is_creator</c> property of the item</summary>
    [ArasName("is_creator")]
    public IProperty_Boolean IsCreator()
    {
      return this.Property("is_creator");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>source_type</c> property of the item</summary>
    [ArasName("source_type")]
    public IProperty_Item<ItemType> SourceType()
    {
      return this.Property("source_type");
    }
  }
}