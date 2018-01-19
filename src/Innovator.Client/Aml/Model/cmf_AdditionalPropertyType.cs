using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cmf_AdditionalPropertyType </summary>
  [ArasName("cmf_AdditionalPropertyType")]
  public class cmf_AdditionalPropertyType : Item, INullRelationship<cmf_TabularViewColumn>
  {
    protected cmf_AdditionalPropertyType() { }
    public cmf_AdditionalPropertyType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cmf_AdditionalPropertyType() { Innovator.Client.Item.AddNullItem<cmf_AdditionalPropertyType>(new cmf_AdditionalPropertyType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>additional_property</c> property of the item</summary>
    [ArasName("additional_property")]
    public IProperty_Item<cmf_PropertyType> AdditionalProperty()
    {
      return this.Property("additional_property");
    }
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