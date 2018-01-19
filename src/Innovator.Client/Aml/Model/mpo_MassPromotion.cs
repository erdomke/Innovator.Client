using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type mpo_MassPromotion </summary>
  [ArasName("mpo_MassPromotion")]
  public class mpo_MassPromotion : Item
  {
    protected mpo_MassPromotion() { }
    public mpo_MassPromotion(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static mpo_MassPromotion() { Innovator.Client.Item.AddNullItem<mpo_MassPromotion>(new mpo_MassPromotion { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>idlist</c> property of the item</summary>
    [ArasName("idlist")]
    public IProperty_Text Idlist()
    {
      return this.Property("idlist");
    }
    /// <summary>Retrieve the <c>item_number</c> property of the item</summary>
    [ArasName("item_number")]
    public IProperty_Text ItemNumber()
    {
      return this.Property("item_number");
    }
    /// <summary>Retrieve the <c>item_type_id</c> property of the item</summary>
    [ArasName("item_type_id")]
    public IProperty_Text ItemTypeId()
    {
      return this.Property("item_type_id");
    }
    /// <summary>Retrieve the <c>itemtype_name</c> property of the item</summary>
    [ArasName("itemtype_name")]
    public IProperty_Text ItemtypeName()
    {
      return this.Property("itemtype_name");
    }
    /// <summary>Retrieve the <c>life_cycle_map_id</c> property of the item</summary>
    [ArasName("life_cycle_map_id")]
    public IProperty_Text LifeCycleMapId()
    {
      return this.Property("life_cycle_map_id");
    }
    /// <summary>Retrieve the <c>life_cycle_map_name</c> property of the item</summary>
    [ArasName("life_cycle_map_name")]
    public IProperty_Text LifeCycleMapName()
    {
      return this.Property("life_cycle_map_name");
    }
    /// <summary>Retrieve the <c>promoted_by_id</c> property of the item</summary>
    [ArasName("promoted_by_id")]
    public IProperty_Item<User> PromotedById()
    {
      return this.Property("promoted_by_id");
    }
    /// <summary>Retrieve the <c>qty_failed</c> property of the item</summary>
    [ArasName("qty_failed")]
    public IProperty_Text QtyFailed()
    {
      return this.Property("qty_failed");
    }
    /// <summary>Retrieve the <c>qty_promoted</c> property of the item</summary>
    [ArasName("qty_promoted")]
    public IProperty_Text QtyPromoted()
    {
      return this.Property("qty_promoted");
    }
    /// <summary>Retrieve the <c>qty_total</c> property of the item</summary>
    [ArasName("qty_total")]
    public IProperty_Text QtyTotal()
    {
      return this.Property("qty_total");
    }
    /// <summary>Retrieve the <c>target_state</c> property of the item</summary>
    [ArasName("target_state")]
    public IProperty_Text TargetState()
    {
      return this.Property("target_state");
    }
    /// <summary>Retrieve the <c>transition_comment</c> property of the item</summary>
    [ArasName("transition_comment")]
    public IProperty_Text TransitionComment()
    {
      return this.Property("transition_comment");
    }
  }
}