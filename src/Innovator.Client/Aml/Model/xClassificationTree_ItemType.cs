using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xClassificationTree_ItemType </summary>
  [ArasName("xClassificationTree_ItemType")]
  public class xClassificationTree_ItemType : Item, INullRelationship<xClassificationTree>, IRelationship<ItemType>
  {
    protected xClassificationTree_ItemType() { }
    public xClassificationTree_ItemType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xClassificationTree_ItemType() { Innovator.Client.Item.AddNullItem<xClassificationTree_ItemType>(new xClassificationTree_ItemType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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
  }
}