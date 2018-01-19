using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SelfServiceReportSettings </summary>
  [ArasName("SelfServiceReportSettings")]
  public class SelfServiceReportSettings : Item, INullRelationship<Preference>
  {
    protected SelfServiceReportSettings() { }
    public SelfServiceReportSettings(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SelfServiceReportSettings() { Innovator.Client.Item.AddNullItem<SelfServiceReportSettings>(new SelfServiceReportSettings { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>hide_advanced_for_all</c> property of the item</summary>
    [ArasName("hide_advanced_for_all")]
    public IProperty_Boolean HideAdvancedForAll()
    {
      return this.Property("hide_advanced_for_all");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}