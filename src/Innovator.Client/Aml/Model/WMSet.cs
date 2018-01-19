using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type WMSet </summary>
  [ArasName("WMSet")]
  public class WMSet : Item
  {
    protected WMSet() { }
    public WMSet(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static WMSet() { Innovator.Client.Item.AddNullItem<WMSet>(new WMSet { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>event_context</c> property of the item</summary>
    [ArasName("event_context")]
    public IProperty_Text EventContext()
    {
      return this.Property("event_context");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>wm_type</c> property of the item</summary>
    [ArasName("wm_type")]
    public IProperty_Item<WMType> WmType()
    {
      return this.Property("wm_type");
    }
  }
}