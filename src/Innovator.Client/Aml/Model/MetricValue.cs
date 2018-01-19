using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Metric Value </summary>
  [ArasName("Metric Value")]
  public class MetricValue : Item, INullRelationship<Metric>
  {
    protected MetricValue() { }
    public MetricValue(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static MetricValue() { Innovator.Client.Item.AddNullItem<MetricValue>(new MetricValue { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>calculate</c> property of the item</summary>
    [ArasName("calculate")]
    public IProperty_Boolean Calculate()
    {
      return this.Property("calculate");
    }
    /// <summary>Retrieve the <c>color</c> property of the item</summary>
    [ArasName("color")]
    public IProperty_Text Color()
    {
      return this.Property("color");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>link</c> property of the item</summary>
    [ArasName("link")]
    public IProperty_Text Link()
    {
      return this.Property("link");
    }
    /// <summary>Retrieve the <c>query</c> property of the item</summary>
    [ArasName("query")]
    public IProperty_Text Query()
    {
      return this.Property("query");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>value</c> property of the item</summary>
    [ArasName("value")]
    public IProperty_Number ValueProp()
    {
      return this.Property("value");
    }
  }
}