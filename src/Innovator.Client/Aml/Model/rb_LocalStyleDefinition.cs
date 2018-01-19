using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_LocalStyleDefinition </summary>
  [ArasName("rb_LocalStyleDefinition")]
  public class rb_LocalStyleDefinition : Item, INullRelationship<rb_LocalStyle>
  {
    protected rb_LocalStyleDefinition() { }
    public rb_LocalStyleDefinition(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_LocalStyleDefinition() { Innovator.Client.Item.AddNullItem<rb_LocalStyleDefinition>(new rb_LocalStyleDefinition { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>justification</c> property of the item</summary>
    [ArasName("justification")]
    public IProperty_Text Justification()
    {
      return this.Property("justification");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}