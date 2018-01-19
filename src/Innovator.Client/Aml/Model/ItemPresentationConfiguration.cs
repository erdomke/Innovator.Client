using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ItemPresentationConfiguration </summary>
  [ArasName("ItemPresentationConfiguration")]
  public class ItemPresentationConfiguration : Item
  {
    protected ItemPresentationConfiguration() { }
    public ItemPresentationConfiguration(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ItemPresentationConfiguration() { Innovator.Client.Item.AddNullItem<ItemPresentationConfiguration>(new ItemPresentationConfiguration { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>client_type</c> property of the item</summary>
    [ArasName("client_type")]
    public IProperty_Text ClientType()
    {
      return this.Property("client_type");
    }
    /// <summary>Retrieve the <c>item_id</c> property of the item</summary>
    [ArasName("item_id")]
    public IProperty_Item<IPresentableItems> ItemId()
    {
      return this.Property("item_id");
    }
    /// <summary>Retrieve the <c>item_type_id</c> property of the item</summary>
    [ArasName("item_type_id")]
    public IProperty_Item<ItemType> ItemTypeId()
    {
      return this.Property("item_type_id");
    }
    /// <summary>Retrieve the <c>presentation_config_id</c> property of the item</summary>
    [ArasName("presentation_config_id")]
    public IProperty_Item<PresentationConfiguration> PresentationConfigId()
    {
      return this.Property("presentation_config_id");
    }
  }
}