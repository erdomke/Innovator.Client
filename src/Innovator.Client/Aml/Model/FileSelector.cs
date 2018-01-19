using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type FileSelector </summary>
  [ArasName("FileSelector")]
  public class FileSelector : Item, INullRelationship<Feed>
  {
    protected FileSelector() { }
    public FileSelector(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static FileSelector() { Innovator.Client.Item.AddNullItem<FileSelector>(new FileSelector { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>filter</c> property of the item</summary>
    [ArasName("filter")]
    public IProperty_Text Filter()
    {
      return this.Property("filter");
    }
    /// <summary>Retrieve the <c>reference</c> property of the item</summary>
    [ArasName("reference")]
    public IProperty_Text Reference()
    {
      return this.Property("reference");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>tooltip_template</c> property of the item</summary>
    [ArasName("tooltip_template")]
    public IProperty_Text TooltipTemplate()
    {
      return this.Property("tooltip_template");
    }
  }
}