using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type mpo_MassPromotionRecord </summary>
  [ArasName("mpo_MassPromotionRecord")]
  public class mpo_MassPromotionRecord : Item, INullRelationship<mpo_MassPromotion>
  {
    protected mpo_MassPromotionRecord() { }
    public mpo_MassPromotionRecord(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static mpo_MassPromotionRecord() { Innovator.Client.Item.AddNullItem<mpo_MassPromotionRecord>(new mpo_MassPromotionRecord { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>after_promote_item_id</c> property of the item</summary>
    [ArasName("after_promote_item_id")]
    public IProperty_Text AfterPromoteItemId()
    {
      return this.Property("after_promote_item_id");
    }
    /// <summary>Retrieve the <c>before_promote_item_id</c> property of the item</summary>
    [ArasName("before_promote_item_id")]
    public IProperty_Text BeforePromoteItemId()
    {
      return this.Property("before_promote_item_id");
    }
    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>got_comment</c> property of the item</summary>
    [ArasName("got_comment")]
    public IProperty_Boolean GotComment()
    {
      return this.Property("got_comment");
    }
    /// <summary>Retrieve the <c>is_promoted</c> property of the item</summary>
    [ArasName("is_promoted")]
    public IProperty_Boolean IsPromoted()
    {
      return this.Property("is_promoted");
    }
    /// <summary>Retrieve the <c>item_config_id</c> property of the item</summary>
    [ArasName("item_config_id")]
    public IProperty_Text ItemConfigId()
    {
      return this.Property("item_config_id");
    }
    /// <summary>Retrieve the <c>item_keyed_name</c> property of the item</summary>
    [ArasName("item_keyed_name")]
    public IProperty_Text ItemKeyedName()
    {
      return this.Property("item_keyed_name");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>state_before</c> property of the item</summary>
    [ArasName("state_before")]
    public IProperty_Text StateBefore()
    {
      return this.Property("state_before");
    }
    /// <summary>Retrieve the <c>status_error</c> property of the item</summary>
    [ArasName("status_error")]
    public IProperty_Text StatusError()
    {
      return this.Property("status_error");
    }
  }
}