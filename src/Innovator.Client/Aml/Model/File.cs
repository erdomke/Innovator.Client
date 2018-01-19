using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type File </summary>
  [ArasName("File")]
  public class File : Item
  {
    protected File() { }
    public File(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static File() { Innovator.Client.Item.AddNullItem<File>(new File { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>checkedout_path</c> property of the item</summary>
    [ArasName("checkedout_path")]
    public IProperty_Text CheckedoutPath()
    {
      return this.Property("checkedout_path");
    }
    /// <summary>Retrieve the <c>checksum</c> property of the item</summary>
    [ArasName("checksum")]
    public IProperty_Text Checksum()
    {
      return this.Property("checksum");
    }
    /// <summary>Retrieve the <c>comments</c> property of the item</summary>
    [ArasName("comments")]
    public IProperty_Text Comments()
    {
      return this.Property("comments");
    }
    /// <summary>Retrieve the <c>file_size</c> property of the item</summary>
    [ArasName("file_size")]
    public IProperty_Number FileSize()
    {
      return this.Property("file_size");
    }
    /// <summary>Retrieve the <c>file_type</c> property of the item</summary>
    [ArasName("file_type")]
    public IProperty_Item<FileType> FileType()
    {
      return this.Property("file_type");
    }
    /// <summary>Retrieve the <c>filename</c> property of the item</summary>
    [ArasName("filename")]
    public IProperty_Text Filename()
    {
      return this.Property("filename");
    }
    /// <summary>Retrieve the <c>indexed_on</c> property of the item</summary>
    [ArasName("indexed_on")]
    public IProperty_Date IndexedOn()
    {
      return this.Property("indexed_on");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>mimetype</c> property of the item</summary>
    [ArasName("mimetype")]
    public IProperty_Text Mimetype()
    {
      return this.Property("mimetype");
    }
  }
}