using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type WMConfiguration </summary>
  [ArasName("WMConfiguration")]
  public class WMConfiguration : Item, IWMConfiguration
  {
    protected WMConfiguration() { }
    public WMConfiguration(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static WMConfiguration() { Innovator.Client.Item.AddNullItem<WMConfiguration>(new WMConfiguration { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>watermark_context</c> property of the item</summary>
    [ArasName("watermark_context")]
    public IProperty_Text WatermarkContext()
    {
      return this.Property("watermark_context");
    }
  }
}