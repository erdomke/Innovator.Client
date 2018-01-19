using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xClass_Classification_Perm </summary>
  [ArasName("xClass_Classification_Perm")]
  public class xClass_Classification_Perm : Item, INullRelationship<xClass>, IRelationship<Permission_ItemClassification>
  {
    protected xClass_Classification_Perm() { }
    public xClass_Classification_Perm(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xClass_Classification_Perm() { Innovator.Client.Item.AddNullItem<xClass_Classification_Perm>(new xClass_Classification_Perm { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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