using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type mp_MacPolicyControlledType </summary>
  [ArasName("mp_MacPolicyControlledType")]
  public class mp_MacPolicyControlledType : Item, INullRelationship<mp_MacPolicy>, IRelationship<ItemType>
  {
    protected mp_MacPolicyControlledType() { }
    public mp_MacPolicyControlledType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static mp_MacPolicyControlledType() { Innovator.Client.Item.AddNullItem<mp_MacPolicyControlledType>(new mp_MacPolicyControlledType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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