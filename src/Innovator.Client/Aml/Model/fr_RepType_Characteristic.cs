using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type fr_RepType_Characteristic </summary>
  [ArasName("fr_RepType_Characteristic")]
  public class fr_RepType_Characteristic : Item, INullRelationship<fr_RepresentationType>, IRelationship<xPropertyDefinition>
  {
    protected fr_RepType_Characteristic() { }
    public fr_RepType_Characteristic(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static fr_RepType_Characteristic() { Innovator.Client.Item.AddNullItem<fr_RepType_Characteristic>(new fr_RepType_Characteristic { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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