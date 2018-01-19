using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type mp_MacPolicyExemptIdentity </summary>
  [ArasName("mp_MacPolicyExemptIdentity")]
  public class mp_MacPolicyExemptIdentity : Item, INullRelationship<mp_MacPolicy>, IRelationship<Identity>
  {
    protected mp_MacPolicyExemptIdentity() { }
    public mp_MacPolicyExemptIdentity(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static mp_MacPolicyExemptIdentity() { Innovator.Client.Item.AddNullItem<mp_MacPolicyExemptIdentity>(new mp_MacPolicyExemptIdentity { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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