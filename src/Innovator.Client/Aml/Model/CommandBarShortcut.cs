using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type CommandBarShortcut </summary>
  [ArasName("CommandBarShortcut")]
  public class CommandBarShortcut : Item, ICommandBarItem
  {
    protected CommandBarShortcut() { }
    public CommandBarShortcut(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static CommandBarShortcut() { Innovator.Client.Item.AddNullItem<CommandBarShortcut>(new CommandBarShortcut { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>additional_data</c> property of the item</summary>
    [ArasName("additional_data")]
    public IProperty_Text AdditionalData()
    {
      return this.Property("additional_data");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>on_click_handler</c> property of the item</summary>
    [ArasName("on_click_handler")]
    public IProperty_Item<Method> OnClickHandler()
    {
      return this.Property("on_click_handler");
    }
    /// <summary>Retrieve the <c>on_init_handler</c> property of the item</summary>
    [ArasName("on_init_handler")]
    public IProperty_Item<Method> OnInitHandler()
    {
      return this.Property("on_init_handler");
    }
    /// <summary>Retrieve the <c>shortcut</c> property of the item</summary>
    [ArasName("shortcut")]
    public IProperty_Text Shortcut()
    {
      return this.Property("shortcut");
    }
  }
}