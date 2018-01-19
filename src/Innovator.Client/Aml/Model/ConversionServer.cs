using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ConversionServer </summary>
  [ArasName("ConversionServer")]
  public class ConversionServer : Item
  {
    protected ConversionServer() { }
    public ConversionServer(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ConversionServer() { Innovator.Client.Item.AddNullItem<ConversionServer>(new ConversionServer { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>impersonation_user_id</c> property of the item</summary>
    [ArasName("impersonation_user_id")]
    public IProperty_Item<User> ImpersonationUserId()
    {
      return this.Property("impersonation_user_id");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>url</c> property of the item</summary>
    [ArasName("url")]
    public IProperty_Text Url()
    {
      return this.Property("url");
    }
  }
}