using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type qry_QueryItem </summary>
  [ArasName("qry_QueryItem")]
  public class qry_QueryItem : Item, INullRelationship<qry_QueryDefinition>
  {
    protected qry_QueryItem() { }
    public qry_QueryItem(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static qry_QueryItem() { Innovator.Client.Item.AddNullItem<qry_QueryItem>(new qry_QueryItem { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>alias</c> property of the item</summary>
    [ArasName("alias")]
    public IProperty_Text Alias()
    {
      return this.Property("alias");
    }
    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>filter_xml</c> property of the item</summary>
    [ArasName("filter_xml")]
    public IProperty_Text FilterXml()
    {
      return this.Property("filter_xml");
    }
    /// <summary>Retrieve the <c>item_type</c> property of the item</summary>
    [ArasName("item_type")]
    public IProperty_Item<ItemType> ItemType()
    {
      return this.Property("item_type");
    }
    /// <summary>Retrieve the <c>ref_id</c> property of the item</summary>
    [ArasName("ref_id")]
    public IProperty_Text RefId()
    {
      return this.Property("ref_id");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}