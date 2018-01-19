using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type fr_SupportedFileType </summary>
  [ArasName("fr_SupportedFileType")]
  public class fr_SupportedFileType : Item, INullRelationship<fr_RepresentationType>, IRelationship<FileType>
  {
    protected fr_SupportedFileType() { }
    public fr_SupportedFileType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static fr_SupportedFileType() { Innovator.Client.Item.AddNullItem<fr_SupportedFileType>(new fr_SupportedFileType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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