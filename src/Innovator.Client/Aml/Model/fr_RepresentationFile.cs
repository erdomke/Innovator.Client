using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type fr_RepresentationFile </summary>
  [ArasName("fr_RepresentationFile")]
  public class fr_RepresentationFile : Item, IFileContainerItems, INullRelationship<fr_Representation>, IRelationship<File>
  {
    protected fr_RepresentationFile() { }
    public fr_RepresentationFile(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static fr_RepresentationFile() { Innovator.Client.Item.AddNullItem<fr_RepresentationFile>(new fr_RepresentationFile { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>kind</c> property of the item</summary>
    [ArasName("kind")]
    public IProperty_Text Kind()
    {
      return this.Property("kind");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}