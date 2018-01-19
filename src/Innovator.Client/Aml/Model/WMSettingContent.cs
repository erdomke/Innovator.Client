using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type WMSettingContent </summary>
  [ArasName("WMSettingContent")]
  public class WMSettingContent : Item, INullRelationship<WMSettings>
  {
    protected WMSettingContent() { }
    public WMSettingContent(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static WMSettingContent() { Innovator.Client.Item.AddNullItem<WMSettingContent>(new WMSettingContent { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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
    /// <summary>Retrieve the <c>wm_configuration_lookup_method</c> property of the item</summary>
    [ArasName("wm_configuration_lookup_method")]
    public IProperty_Item<Method> WmConfigurationLookupMethod()
    {
      return this.Property("wm_configuration_lookup_method");
    }
    /// <summary>Retrieve the <c>wm_content_lookup_method</c> property of the item</summary>
    [ArasName("wm_content_lookup_method")]
    public IProperty_Item<Method> WmContentLookupMethod()
    {
      return this.Property("wm_content_lookup_method");
    }
    /// <summary>Retrieve the <c>wm_content_update_method</c> property of the item</summary>
    [ArasName("wm_content_update_method")]
    public IProperty_Item<Method> WmContentUpdateMethod()
    {
      return this.Property("wm_content_update_method");
    }
    /// <summary>Retrieve the <c>wm_type</c> property of the item</summary>
    [ArasName("wm_type")]
    public IProperty_Item<WMType> WmType()
    {
      return this.Property("wm_type");
    }
  }
}