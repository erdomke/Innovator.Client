using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_LocalStyle </summary>
  [ArasName("rb_LocalStyle")]
  public class rb_LocalStyle : Item, INullRelationship<rb_TreeGridViewDefinition>
  {
    protected rb_LocalStyle() { }
    public rb_LocalStyle(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_LocalStyle() { Innovator.Client.Item.AddNullItem<rb_LocalStyle>(new rb_LocalStyle { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
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
  }
}