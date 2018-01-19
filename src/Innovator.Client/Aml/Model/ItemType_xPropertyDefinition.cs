using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ItemType_xPropertyDefinition </summary>
  [ArasName("ItemType_xPropertyDefinition")]
  public class ItemType_xPropertyDefinition : Item, INullRelationship<ItemType>, IRelationship<xPropertyDefinition>
  {
    protected ItemType_xPropertyDefinition() { }
    public ItemType_xPropertyDefinition(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ItemType_xPropertyDefinition() { Innovator.Client.Item.AddNullItem<ItemType_xPropertyDefinition>(new ItemType_xPropertyDefinition { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>define_permission_behavior</c> property of the item</summary>
    [ArasName("define_permission_behavior")]
    public IProperty_Text DefinePermissionBehavior()
    {
      return this.Property("define_permission_behavior");
    }
    /// <summary>Retrieve the <c>define_permission_id</c> property of the item</summary>
    [ArasName("define_permission_id")]
    public IProperty_Item<Permission_ExplicitDefine> DefinePermissionId()
    {
      return this.Property("define_permission_id");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>value_permission_behavior</c> property of the item</summary>
    [ArasName("value_permission_behavior")]
    public IProperty_Text ValuePermissionBehavior()
    {
      return this.Property("value_permission_behavior");
    }
    /// <summary>Retrieve the <c>value_permission_id</c> property of the item</summary>
    [ArasName("value_permission_id")]
    public IProperty_Item<Permission_PropertyValue> ValuePermissionId()
    {
      return this.Property("value_permission_id");
    }
  }
}