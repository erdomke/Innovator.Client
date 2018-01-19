using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type CommandBarSectionDependency </summary>
  [ArasName("CommandBarSectionDependency")]
  public class CommandBarSectionDependency : Item, INullRelationship<CommandBarSection>, IRelationship<ItemType>
  {
    protected CommandBarSectionDependency() { }
    public CommandBarSectionDependency(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static CommandBarSectionDependency() { Innovator.Client.Item.AddNullItem<CommandBarSectionDependency>(new CommandBarSectionDependency { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}