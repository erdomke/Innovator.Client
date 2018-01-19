using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Transition Distribution </summary>
  [ArasName("Transition Distribution")]
  public class TransitionDistribution : Item, INullRelationship<TransitionEMail>, IRelationship<Identity>
  {
    protected TransitionDistribution() { }
    public TransitionDistribution(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static TransitionDistribution() { Innovator.Client.Item.AddNullItem<TransitionDistribution>(new TransitionDistribution { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
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