using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Permission_ExplicitDefine </summary>
  [ArasName("Permission_ExplicitDefine")]
  public class Permission_ExplicitDefine : Item
  {
    protected Permission_ExplicitDefine() { }
    public Permission_ExplicitDefine(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Permission_ExplicitDefine() { Innovator.Client.Item.AddNullItem<Permission_ExplicitDefine>(new Permission_ExplicitDefine { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
  }
}