using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Hide Related In </summary>
  [ArasName("Hide Related In")]
  public class HideRelatedIn : Item, INullRelationship<RelationshipType>
  {
    protected HideRelatedIn() { }
    public HideRelatedIn(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static HideRelatedIn() { Innovator.Client.Item.AddNullItem<HideRelatedIn>(new HideRelatedIn { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>hide_in</c> property of the item</summary>
    [ArasName("hide_in")]
    public IProperty_Text HideIn()
    {
      return this.Property("hide_in");
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