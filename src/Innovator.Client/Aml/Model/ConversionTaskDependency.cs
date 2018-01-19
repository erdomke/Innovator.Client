using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ConversionTaskDependency </summary>
  [ArasName("ConversionTaskDependency")]
  public class ConversionTaskDependency : Item, INullRelationship<ConversionTask>
  {
    protected ConversionTaskDependency() { }
    public ConversionTaskDependency(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ConversionTaskDependency() { Innovator.Client.Item.AddNullItem<ConversionTaskDependency>(new ConversionTaskDependency { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>dependency_id</c> property of the item</summary>
    [ArasName("dependency_id")]
    public IProperty_Text DependencyId()
    {
      return this.Property("dependency_id");
    }
    /// <summary>Retrieve the <c>dependency_type</c> property of the item</summary>
    [ArasName("dependency_type")]
    public IProperty_Text DependencyType()
    {
      return this.Property("dependency_type");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}