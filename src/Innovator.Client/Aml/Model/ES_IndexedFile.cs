using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_IndexedFile </summary>
  [ArasName("ES_IndexedFile")]
  public class ES_IndexedFile : Item, INullRelationship<ES_IndexedConfiguration>
  {
    protected ES_IndexedFile() { }
    public ES_IndexedFile(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_IndexedFile() { Innovator.Client.Item.AddNullItem<ES_IndexedFile>(new ES_IndexedFile { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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
    /// <summary>Retrieve the <c>extensions</c> property of the item</summary>
    [ArasName("extensions")]
    public IProperty_Text Extensions()
    {
      return this.Property("extensions");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}