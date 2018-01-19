using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ConversionRuleEventHandler </summary>
  [ArasName("ConversionRuleEventHandler")]
  public class ConversionRuleEventHandler : Item, INullRelationship<ConversionRule>, IRelationship<Method>
  {
    protected ConversionRuleEventHandler() { }
    public ConversionRuleEventHandler(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ConversionRuleEventHandler() { Innovator.Client.Item.AddNullItem<ConversionRuleEventHandler>(new ConversionRuleEventHandler { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>event_type</c> property of the item</summary>
    [ArasName("event_type")]
    public IProperty_Text EventType()
    {
      return this.Property("event_type");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}