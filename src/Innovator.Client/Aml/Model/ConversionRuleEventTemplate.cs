using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ConversionRuleEventTemplate </summary>
  [ArasName("ConversionRuleEventTemplate")]
  public class ConversionRuleEventTemplate : Item, INullRelationship<ConversionRule>, IRelationship<Method>
  {
    protected ConversionRuleEventTemplate() { }
    public ConversionRuleEventTemplate(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ConversionRuleEventTemplate() { Innovator.Client.Item.AddNullItem<ConversionRuleEventTemplate>(new ConversionRuleEventTemplate { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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
    /// <summary>Retrieve the <c>lock_dependencies</c> property of the item</summary>
    [ArasName("lock_dependencies")]
    public IProperty_Boolean LockDependencies()
    {
      return this.Property("lock_dependencies");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}