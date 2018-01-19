using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type WMSettings </summary>
  [ArasName("WMSettings")]
  public class WMSettings : Item
  {
    protected WMSettings() { }
    public WMSettings(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static WMSettings() { Innovator.Client.Item.AddNullItem<WMSettings>(new WMSettings { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>default_wm_set</c> property of the item</summary>
    [ArasName("default_wm_set")]
    public IProperty_Item<WMSet> DefaultWmSet()
    {
      return this.Property("default_wm_set");
    }
    /// <summary>Retrieve the <c>item_type</c> property of the item</summary>
    [ArasName("item_type")]
    public IProperty_Item<ItemType> ItemType()
    {
      return this.Property("item_type");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
  }
}