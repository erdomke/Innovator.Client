using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Permission_PropertyValue </summary>
  [ArasName("Permission_PropertyValue")]
  public class Permission_PropertyValue : Item
  {
    protected Permission_PropertyValue() { }
    public Permission_PropertyValue(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Permission_PropertyValue() { Innovator.Client.Item.AddNullItem<Permission_PropertyValue>(new Permission_PropertyValue { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
  }
}