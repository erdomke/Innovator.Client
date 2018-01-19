using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_IndexedProperty </summary>
  [ArasName("ES_IndexedProperty")]
  public class ES_IndexedProperty : Item, INullRelationship<ES_IndexedType>
  {
    protected ES_IndexedProperty() { }
    public ES_IndexedProperty(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_IndexedProperty() { Innovator.Client.Item.AddNullItem<ES_IndexedProperty>(new ES_IndexedProperty { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>is_facet</c> property of the item</summary>
    [ArasName("is_facet")]
    public IProperty_Boolean IsFacet()
    {
      return this.Property("is_facet");
    }
    /// <summary>Retrieve the <c>is_search</c> property of the item</summary>
    [ArasName("is_search")]
    public IProperty_Boolean IsSearch()
    {
      return this.Property("is_search");
    }
    /// <summary>Retrieve the <c>is_store_only</c> property of the item</summary>
    [ArasName("is_store_only")]
    public IProperty_Boolean IsStoreOnly()
    {
      return this.Property("is_store_only");
    }
    /// <summary>Retrieve the <c>is_ui</c> property of the item</summary>
    [ArasName("is_ui")]
    public IProperty_Boolean IsUi()
    {
      return this.Property("is_ui");
    }
    /// <summary>Retrieve the <c>property_name</c> property of the item</summary>
    [ArasName("property_name")]
    public IProperty_Text PropertyName()
    {
      return this.Property("property_name");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}