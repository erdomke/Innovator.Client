using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Allowed Permission </summary>
  [ArasName("Allowed Permission")]
  public class AllowedPermission : Item, INullRelationship<ItemType>, IRelationship<Permission>
  {
    protected AllowedPermission() { }
    public AllowedPermission(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static AllowedPermission() { Innovator.Client.Item.AddNullItem<AllowedPermission>(new AllowedPermission { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>is_default</c> property of the item</summary>
    [ArasName("is_default")]
    public IProperty_Boolean IsDefault()
    {
      return this.Property("is_default");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}