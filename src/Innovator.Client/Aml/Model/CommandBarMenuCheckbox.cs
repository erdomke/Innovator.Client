using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type CommandBarMenuCheckbox </summary>
  [ArasName("CommandBarMenuCheckbox")]
  public class CommandBarMenuCheckbox : Item, ICommandBarItem
  {
    protected CommandBarMenuCheckbox() { }
    public CommandBarMenuCheckbox(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static CommandBarMenuCheckbox() { Innovator.Client.Item.AddNullItem<CommandBarMenuCheckbox>(new CommandBarMenuCheckbox { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>additional_data</c> property of the item</summary>
    [ArasName("additional_data")]
    public IProperty_Text AdditionalData()
    {
      return this.Property("additional_data");
    }
    /// <summary>Retrieve the <c>group_id</c> property of the item</summary>
    [ArasName("group_id")]
    public IProperty_Text GroupId()
    {
      return this.Property("group_id");
    }
    /// <summary>Retrieve the <c>image</c> property of the item</summary>
    [ArasName("image")]
    public IProperty_Text Image()
    {
      return this.Property("image");
    }
    /// <summary>Retrieve the <c>include_events</c> property of the item</summary>
    [ArasName("include_events")]
    public IProperty_Text IncludeEvents()
    {
      return this.Property("include_events");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
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
    /// <summary>Retrieve the <c>parent_menu</c> property of the item</summary>
    [ArasName("parent_menu")]
    public IProperty_Item<CommandBarMenu> ParentMenu()
    {
      return this.Property("parent_menu");
    }
  }
}