using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xClass </summary>
  [ArasName("xClass")]
  public class xClass : Item, INullRelationship<xClassificationTree>
  {
    protected xClass() { }
    public xClass(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xClass() { Innovator.Client.Item.AddNullItem<xClass>(new xClass { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>ref_id</c> property of the item</summary>
    [ArasName("ref_id")]
    public IProperty_Text RefId()
    {
      return this.Property("ref_id");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>xproperties_sort_order</c> property of the item</summary>
    [ArasName("xproperties_sort_order")]
    public IProperty_Text XpropertiesSortOrder()
    {
      return this.Property("xproperties_sort_order");
    }
  }
}