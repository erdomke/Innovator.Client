using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xClassifiableItem_xClass </summary>
  [ArasName("xClassifiableItem_xClass")]
  public class xClassifiableItem_xClass : Item, IxClassifiableItem_xClass, INullRelationship<IxPropertyContainerItem>, IRelationship<xClass>
  {
    protected xClassifiableItem_xClass() { }
    public xClassifiableItem_xClass(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xClassifiableItem_xClass() { Innovator.Client.Item.AddNullItem<xClassifiableItem_xClass>(new xClassifiableItem_xClass { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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