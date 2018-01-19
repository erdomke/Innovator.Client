using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SecureMessageFlaggedBy </summary>
  [ArasName("SecureMessageFlaggedBy")]
  public class SecureMessageFlaggedBy : Item
  {
    protected SecureMessageFlaggedBy() { }
    public SecureMessageFlaggedBy(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SecureMessageFlaggedBy() { Innovator.Client.Item.AddNullItem<SecureMessageFlaggedBy>(new SecureMessageFlaggedBy { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>flagged_by_id</c> property of the item</summary>
    [ArasName("flagged_by_id")]
    public IProperty_Item<User> FlaggedById()
    {
      return this.Property("flagged_by_id");
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