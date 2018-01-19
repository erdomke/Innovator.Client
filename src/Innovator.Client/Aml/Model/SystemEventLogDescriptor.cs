using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SystemEventLogDescriptor </summary>
  [ArasName("SystemEventLogDescriptor")]
  public class SystemEventLogDescriptor : Item
  {
    protected SystemEventLogDescriptor() { }
    public SystemEventLogDescriptor(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SystemEventLogDescriptor() { Innovator.Client.Item.AddNullItem<SystemEventLogDescriptor>(new SystemEventLogDescriptor { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>log_level</c> property of the item</summary>
    [ArasName("log_level")]
    public IProperty_Number LogLevel()
    {
      return this.Property("log_level");
    }
    /// <summary>Retrieve the <c>log_message</c> property of the item</summary>
    [ArasName("log_message")]
    public IProperty_Text LogMessage()
    {
      return this.Property("log_message");
    }
    /// <summary>Retrieve the <c>system_event</c> property of the item</summary>
    [ArasName("system_event")]
    public IProperty_Item<SystemEvent> SystemEvent()
    {
      return this.Property("system_event");
    }
  }
}