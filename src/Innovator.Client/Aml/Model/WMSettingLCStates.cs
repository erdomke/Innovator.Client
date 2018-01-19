using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type WMSettingLCStates </summary>
  [ArasName("WMSettingLCStates")]
  public class WMSettingLCStates : Item, INullRelationship<WMSettings>, IRelationship<LifeCycleState>
  {
    protected WMSettingLCStates() { }
    public WMSettingLCStates(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static WMSettingLCStates() { Innovator.Client.Item.AddNullItem<WMSettingLCStates>(new WMSettingLCStates { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
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