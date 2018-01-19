using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xClass_xPropertyDefinition </summary>
  [ArasName("xClass_xPropertyDefinition")]
  public class xClass_xPropertyDefinition : Item, INullRelationship<xClass>, IRelationship<xPropertyDefinition>
  {
    protected xClass_xPropertyDefinition() { }
    public xClass_xPropertyDefinition(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xClass_xPropertyDefinition() { Innovator.Client.Item.AddNullItem<xClass_xPropertyDefinition>(new xClass_xPropertyDefinition { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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