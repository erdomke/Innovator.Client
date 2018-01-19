using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Permission_ItemClassification </summary>
  [ArasName("Permission_ItemClassification")]
  public class Permission_ItemClassification : Item
  {
    protected Permission_ItemClassification() { }
    public Permission_ItemClassification(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Permission_ItemClassification() { Innovator.Client.Item.AddNullItem<Permission_ItemClassification>(new Permission_ItemClassification { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
  }
}