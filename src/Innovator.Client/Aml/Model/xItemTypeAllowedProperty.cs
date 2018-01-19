using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xItemTypeAllowedProperty </summary>
  [ArasName("xItemTypeAllowedProperty")]
  public class xItemTypeAllowedProperty : Item, INullRelationship<ItemType>, IRelationship<xPropertyDefinition>
  {
    protected xItemTypeAllowedProperty() { }
    public xItemTypeAllowedProperty(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xItemTypeAllowedProperty() { Innovator.Client.Item.AddNullItem<xItemTypeAllowedProperty>(new xItemTypeAllowedProperty { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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