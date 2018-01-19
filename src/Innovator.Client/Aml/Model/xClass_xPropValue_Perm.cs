using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xClass_xPropValue_Perm </summary>
  [ArasName("xClass_xPropValue_Perm")]
  public class xClass_xPropValue_Perm : Item, INullRelationship<xClass>, IRelationship<Permission_PropertyValue>
  {
    protected xClass_xPropValue_Perm() { }
    public xClass_xPropValue_Perm(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xClass_xPropValue_Perm() { Innovator.Client.Item.AddNullItem<xClass_xPropValue_Perm>(new xClass_xPropValue_Perm { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>itemtype_id</c> property of the item</summary>
    [ArasName("itemtype_id")]
    public IProperty_Item<ItemType> ItemtypeId()
    {
      return this.Property("itemtype_id");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}