using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type fr_Representation </summary>
  [ArasName("fr_Representation")]
  public class fr_Representation : Item, INullRelationship<File>
  {
    protected fr_Representation() { }
    public fr_Representation(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static fr_Representation() { Innovator.Client.Item.AddNullItem<fr_Representation>(new fr_Representation { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>representation_type_id</c> property of the item</summary>
    [ArasName("representation_type_id")]
    public IProperty_Item<fr_RepresentationType> RepresentationTypeId()
    {
      return this.Property("representation_type_id");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}