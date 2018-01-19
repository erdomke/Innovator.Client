using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Base Notification Message </summary>
  [ArasName("Base Notification Message")]
  public class BaseNotificationMessage : Item, IBaseNotificationMessage
  {
    protected BaseNotificationMessage() { }
    public BaseNotificationMessage(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static BaseNotificationMessage() { Innovator.Client.Item.AddNullItem<BaseNotificationMessage>(new BaseNotificationMessage { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>acknowledge</c> property of the item</summary>
    [ArasName("acknowledge")]
    public IProperty_Text Acknowledge()
    {
      return this.Property("acknowledge");
    }
    /// <summary>Retrieve the <c>priority</c> property of the item</summary>
    [ArasName("priority")]
    public IProperty_Text Priority()
    {
      return this.Property("priority");
    }
    /// <summary>Retrieve the <c>target</c> property of the item</summary>
    [ArasName("target")]
    public IProperty_Item<Identity> Target()
    {
      return this.Property("target");
    }
    /// <summary>Retrieve the <c>type</c> property of the item</summary>
    [ArasName("type")]
    public IProperty_Text Type()
    {
      return this.Property("type");
    }
  }
}