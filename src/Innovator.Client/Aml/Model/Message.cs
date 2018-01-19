using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Message </summary>
  [ArasName("Message")]
  public class Message : Item, IBaseNotificationMessage
  {
    protected Message() { }
    public Message(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Message() { Innovator.Client.Item.AddNullItem<Message>(new Message { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>acknowledge</c> property of the item</summary>
    [ArasName("acknowledge")]
    public IProperty_Text Acknowledge()
    {
      return this.Property("acknowledge");
    }
    /// <summary>Retrieve the <c>custom_html</c> property of the item</summary>
    [ArasName("custom_html")]
    public IProperty_Text CustomHtml()
    {
      return this.Property("custom_html");
    }
    /// <summary>Retrieve the <c>exit_button_label</c> property of the item</summary>
    [ArasName("exit_button_label")]
    public IProperty_Text ExitButtonLabel()
    {
      return this.Property("exit_button_label");
    }
    /// <summary>Retrieve the <c>expiration_date</c> property of the item</summary>
    [ArasName("expiration_date")]
    public IProperty_Date ExpirationDate()
    {
      return this.Property("expiration_date");
    }
    /// <summary>Retrieve the <c>height</c> property of the item</summary>
    [ArasName("height")]
    public IProperty_Number Height()
    {
      return this.Property("height");
    }
    /// <summary>Retrieve the <c>icon</c> property of the item</summary>
    [ArasName("icon")]
    public IProperty_Text Icon()
    {
      return this.Property("icon");
    }
    /// <summary>Retrieve the <c>is_standard_template</c> property of the item</summary>
    [ArasName("is_standard_template")]
    public IProperty_Boolean IsStandardTemplate()
    {
      return this.Property("is_standard_template");
    }
    /// <summary>Retrieve the <c>message_number</c> property of the item</summary>
    [ArasName("message_number")]
    public IProperty_Text MessageNumber()
    {
      return this.Property("message_number");
    }
    /// <summary>Retrieve the <c>ok_button_label</c> property of the item</summary>
    [ArasName("ok_button_label")]
    public IProperty_Text OkButtonLabel()
    {
      return this.Property("ok_button_label");
    }
    /// <summary>Retrieve the <c>priority</c> property of the item</summary>
    [ArasName("priority")]
    public IProperty_Text Priority()
    {
      return this.Property("priority");
    }
    /// <summary>Retrieve the <c>show_exit_button</c> property of the item</summary>
    [ArasName("show_exit_button")]
    public IProperty_Boolean ShowExitButton()
    {
      return this.Property("show_exit_button");
    }
    /// <summary>Retrieve the <c>show_ok_button</c> property of the item</summary>
    [ArasName("show_ok_button")]
    public IProperty_Boolean ShowOkButton()
    {
      return this.Property("show_ok_button");
    }
    /// <summary>Retrieve the <c>target</c> property of the item</summary>
    [ArasName("target")]
    public IProperty_Item<Identity> Target()
    {
      return this.Property("target");
    }
    /// <summary>Retrieve the <c>text</c> property of the item</summary>
    [ArasName("text")]
    public IProperty_Text Text()
    {
      return this.Property("text");
    }
    /// <summary>Retrieve the <c>title</c> property of the item</summary>
    [ArasName("title")]
    public IProperty_Text Title()
    {
      return this.Property("title");
    }
    /// <summary>Retrieve the <c>type</c> property of the item</summary>
    [ArasName("type")]
    public IProperty_Text Type()
    {
      return this.Property("type");
    }
    /// <summary>Retrieve the <c>url</c> property of the item</summary>
    [ArasName("url")]
    public IProperty_Text Url()
    {
      return this.Property("url");
    }
    /// <summary>Retrieve the <c>width</c> property of the item</summary>
    [ArasName("width")]
    public IProperty_Number Width()
    {
      return this.Property("width");
    }
  }
}