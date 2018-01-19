using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_IndexedConfigurationType </summary>
  [ArasName("ES_IndexedConfigurationType")]
  public class ES_IndexedConfigurationType : Item, INullRelationship<ES_IndexedConfiguration>, IRelationship<ES_IndexedType>
  {
    protected ES_IndexedConfigurationType() { }
    public ES_IndexedConfigurationType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_IndexedConfigurationType() { Innovator.Client.Item.AddNullItem<ES_IndexedConfigurationType>(new ES_IndexedConfigurationType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>discovery_path</c> property of the item</summary>
    [ArasName("discovery_path")]
    public IProperty_Text DiscoveryPath()
    {
      return this.Property("discovery_path");
    }
    /// <summary>Retrieve the <c>discovery_type</c> property of the item</summary>
    [ArasName("discovery_type")]
    public IProperty_Text DiscoveryType()
    {
      return this.Property("discovery_type");
    }
    /// <summary>Retrieve the <c>on_delete_method</c> property of the item</summary>
    [ArasName("on_delete_method")]
    public IProperty_Item<Method> OnDeleteMethod()
    {
      return this.Property("on_delete_method");
    }
    /// <summary>Retrieve the <c>on_discover_method</c> property of the item</summary>
    [ArasName("on_discover_method")]
    public IProperty_Item<Method> OnDiscoverMethod()
    {
      return this.Property("on_discover_method");
    }
    /// <summary>Retrieve the <c>on_version_method</c> property of the item</summary>
    [ArasName("on_version_method")]
    public IProperty_Item<Method> OnVersionMethod()
    {
      return this.Property("on_version_method");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}