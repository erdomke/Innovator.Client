using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type FileExchangePackageFile </summary>
  [ArasName("FileExchangePackageFile")]
  public class FileExchangePackageFile : Item, IFileContainerItems, INullRelationship<FileExchangePackage>, IRelationship<File>
  {
    protected FileExchangePackageFile() { }
    public FileExchangePackageFile(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static FileExchangePackageFile() { Innovator.Client.Item.AddNullItem<FileExchangePackageFile>(new FileExchangePackageFile { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>parental_id</c> property of the item</summary>
    [ArasName("parental_id")]
    public IProperty_Text ParentalId()
    {
      return this.Property("parental_id");
    }
    /// <summary>Retrieve the <c>parental_property</c> property of the item</summary>
    [ArasName("parental_property")]
    public IProperty_Text ParentalProperty()
    {
      return this.Property("parental_property");
    }
    /// <summary>Retrieve the <c>parental_type</c> property of the item</summary>
    [ArasName("parental_type")]
    public IProperty_Text ParentalType()
    {
      return this.Property("parental_type");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}