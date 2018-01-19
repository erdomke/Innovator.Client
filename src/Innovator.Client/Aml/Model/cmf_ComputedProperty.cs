using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cmf_ComputedProperty </summary>
  [ArasName("cmf_ComputedProperty")]
  public class cmf_ComputedProperty : Item, INullRelationship<cmf_PropertyType>
  {
    protected cmf_ComputedProperty() { }
    public cmf_ComputedProperty(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cmf_ComputedProperty() { Innovator.Client.Item.AddNullItem<cmf_ComputedProperty>(new cmf_ComputedProperty { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>on_client_compute_method</c> property of the item</summary>
    [ArasName("on_client_compute_method")]
    public IProperty_Item<Method> OnClientComputeMethod()
    {
      return this.Property("on_client_compute_method");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}