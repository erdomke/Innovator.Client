using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Item Action </summary>
  [ArasName("Item Action")]
  public class ItemAction : Item, ICuiDependency, INullRelationship<ItemType>, IRelationship<Action>
  {
    protected ItemAction() { }
    public ItemAction(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ItemAction() { Innovator.Client.Item.AddNullItem<ItemAction>(new ItemAction { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}