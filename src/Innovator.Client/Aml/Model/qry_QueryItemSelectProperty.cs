using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type qry_QueryItemSelectProperty </summary>
  [ArasName("qry_QueryItemSelectProperty")]
  public class qry_QueryItemSelectProperty : Item, INullRelationship<qry_QueryItem>
  {
    protected qry_QueryItemSelectProperty() { }
    public qry_QueryItemSelectProperty(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static qry_QueryItemSelectProperty() { Innovator.Client.Item.AddNullItem<qry_QueryItemSelectProperty>(new qry_QueryItemSelectProperty { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>property_name</c> property of the item</summary>
    [ArasName("property_name")]
    public IProperty_Text PropertyName()
    {
      return this.Property("property_name");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}