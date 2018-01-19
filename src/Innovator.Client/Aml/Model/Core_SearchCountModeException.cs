using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Core_SearchCountModeException </summary>
  [ArasName("Core_SearchCountModeException")]
  public class Core_SearchCountModeException : Item, INullRelationship<Preference>, IRelationship<ItemType>
  {
    protected Core_SearchCountModeException() { }
    public Core_SearchCountModeException(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Core_SearchCountModeException() { Innovator.Client.Item.AddNullItem<Core_SearchCountModeException>(new Core_SearchCountModeException { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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