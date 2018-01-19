using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cmf_PropertyBinding </summary>
  [ArasName("cmf_PropertyBinding")]
  public class cmf_PropertyBinding : Item, INullRelationship<cmf_ElementBinding>
  {
    protected cmf_PropertyBinding() { }
    public cmf_PropertyBinding(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cmf_PropertyBinding() { Innovator.Client.Item.AddNullItem<cmf_PropertyBinding>(new cmf_PropertyBinding { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>property</c> property of the item</summary>
    [ArasName("property")]
    public IProperty_Item<cmf_PropertyType> PropertyProp()
    {
      return this.Property("property");
    }
    /// <summary>Retrieve the <c>read_only</c> property of the item</summary>
    [ArasName("read_only")]
    public IProperty_Boolean ReadOnlyProp()
    {
      return this.Property("read_only");
    }
    /// <summary>Retrieve the <c>reference_type_property_id</c> property of the item</summary>
    [ArasName("reference_type_property_id")]
    public IProperty_Item<Property> ReferenceTypePropertyId()
    {
      return this.Property("reference_type_property_id");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}