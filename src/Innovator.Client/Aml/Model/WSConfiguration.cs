using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type WSConfiguration </summary>
  [ArasName("WSConfiguration")]
  public class WSConfiguration : Item
  {
    protected WSConfiguration() { }
    public WSConfiguration(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static WSConfiguration() { Innovator.Client.Item.AddNullItem<WSConfiguration>(new WSConfiguration { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>compat_mode</c> property of the item</summary>
    [ArasName("compat_mode")]
    public IProperty_Boolean CompatMode()
    {
      return this.Property("compat_mode");
    }
    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
  }
}