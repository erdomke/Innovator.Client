using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Item Report </summary>
  [ArasName("Item Report")]
  public class ItemReport : Item, ICuiDependency, INullRelationship<ItemType>, IRelationship<Report>
  {
    protected ItemReport() { }
    public ItemReport(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ItemReport() { Innovator.Client.Item.AddNullItem<ItemReport>(new ItemReport { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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