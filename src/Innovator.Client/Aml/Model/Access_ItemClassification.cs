using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Access_ItemClassification </summary>
  [ArasName("Access_ItemClassification")]
  public class Access_ItemClassification : Item, INullRelationship<Permission_ItemClassification>, IRelationship<Identity>
  {
    protected Access_ItemClassification() { }
    public Access_ItemClassification(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Access_ItemClassification() { Innovator.Client.Item.AddNullItem<Access_ItemClassification>(new Access_ItemClassification { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>can_classify</c> property of the item</summary>
    [ArasName("can_classify")]
    public IProperty_Boolean CanClassify()
    {
      return this.Property("can_classify");
    }
    /// <summary>Retrieve the <c>can_get_isclassified</c> property of the item</summary>
    [ArasName("can_get_isclassified")]
    public IProperty_Boolean CanGetIsclassified()
    {
      return this.Property("can_get_isclassified");
    }
    /// <summary>Retrieve the <c>can_unclassify</c> property of the item</summary>
    [ArasName("can_unclassify")]
    public IProperty_Boolean CanUnclassify()
    {
      return this.Property("can_unclassify");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}