using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type View With </summary>
  [ArasName("View With")]
  public class ViewWith : Item, INullRelationship<FileType>, IRelationship<Viewer>
  {
    protected ViewWith() { }
    public ViewWith(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ViewWith() { Innovator.Client.Item.AddNullItem<ViewWith>(new ViewWith { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>application</c> property of the item</summary>
    [ArasName("application")]
    public IProperty_Text Application()
    {
      return this.Property("application");
    }
    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>client</c> property of the item</summary>
    [ArasName("client")]
    public IProperty_Text Client()
    {
      return this.Property("client");
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