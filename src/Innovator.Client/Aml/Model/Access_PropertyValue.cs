using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Access_PropertyValue </summary>
  [ArasName("Access_PropertyValue")]
  public class Access_PropertyValue : Item, INullRelationship<Permission_PropertyValue>, IRelationship<Identity>
  {
    protected Access_PropertyValue() { }
    public Access_PropertyValue(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Access_PropertyValue() { Innovator.Client.Item.AddNullItem<Access_PropertyValue>(new Access_PropertyValue { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>can_change_access</c> property of the item</summary>
    [ArasName("can_change_access")]
    public IProperty_Boolean CanChangeAccess()
    {
      return this.Property("can_change_access");
    }
    /// <summary>Retrieve the <c>can_get</c> property of the item</summary>
    [ArasName("can_get")]
    public IProperty_Boolean CanGet()
    {
      return this.Property("can_get");
    }
    /// <summary>Retrieve the <c>can_update</c> property of the item</summary>
    [ArasName("can_update")]
    public IProperty_Boolean CanUpdate()
    {
      return this.Property("can_update");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}