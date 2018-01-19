using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type mp_PolicyAccessEnvAttribute </summary>
  [ArasName("mp_PolicyAccessEnvAttribute")]
  public class mp_PolicyAccessEnvAttribute : Item
  {
    protected mp_PolicyAccessEnvAttribute() { }
    public mp_PolicyAccessEnvAttribute(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static mp_PolicyAccessEnvAttribute() { Innovator.Client.Item.AddNullItem<mp_PolicyAccessEnvAttribute>(new mp_PolicyAccessEnvAttribute { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>get_value_method</c> property of the item</summary>
    [ArasName("get_value_method")]
    public IProperty_Item<Method> GetValueMethod()
    {
      return this.Property("get_value_method");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>type</c> property of the item</summary>
    [ArasName("type")]
    public IProperty_Text Type()
    {
      return this.Property("type");
    }
  }
}