using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type fr_RepresentationType </summary>
  [ArasName("fr_RepresentationType")]
  public class fr_RepresentationType : Item, INullRelationship<FileType>
  {
    protected fr_RepresentationType() { }
    public fr_RepresentationType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static fr_RepresentationType() { Innovator.Client.Item.AddNullItem<fr_RepresentationType>(new fr_RepresentationType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>is_active</c> property of the item</summary>
    [ArasName("is_active")]
    public IProperty_Boolean IsActive()
    {
      return this.Property("is_active");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}