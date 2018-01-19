using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xClass_xProperty_Flatten </summary>
  [ArasName("xClass_xProperty_Flatten")]
  public class xClass_xProperty_Flatten : Item, INullRelationship<xClass>, IRelationship<xPropertyDefinition>
  {
    protected xClass_xProperty_Flatten() { }
    public xClass_xProperty_Flatten(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xClass_xProperty_Flatten() { Innovator.Client.Item.AddNullItem<xClass_xProperty_Flatten>(new xClass_xProperty_Flatten { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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