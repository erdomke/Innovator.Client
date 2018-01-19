using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Filter Value </summary>
  [ArasName("Filter Value")]
  public class FilterValue : Item, INullRelationship<List>
  {
    protected FilterValue() { }
    public FilterValue(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static FilterValue() { Innovator.Client.Item.AddNullItem<FilterValue>(new FilterValue { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>filter</c> property of the item</summary>
    [ArasName("filter")]
    public IProperty_Text Filter()
    {
      return this.Property("filter");
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
    /// <summary>Retrieve the <c>value</c> property of the item</summary>
    [ArasName("value")]
    public IProperty_Text ValueProp()
    {
      return this.Property("value");
    }
  }
}