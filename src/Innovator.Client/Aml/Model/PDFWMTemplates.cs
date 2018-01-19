using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type PDFWMTemplates </summary>
  [ArasName("PDFWMTemplates")]
  public class PDFWMTemplates : Item, IFileContainerItems, INullRelationship<PDFWMConfiguration>
  {
    protected PDFWMTemplates() { }
    public PDFWMTemplates(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static PDFWMTemplates() { Innovator.Client.Item.AddNullItem<PDFWMTemplates>(new PDFWMTemplates { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>page_type</c> property of the item</summary>
    [ArasName("page_type")]
    public IProperty_Text PageType()
    {
      return this.Property("page_type");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>source_type</c> property of the item</summary>
    [ArasName("source_type")]
    public IProperty_Item<ItemType> SourceType()
    {
      return this.Property("source_type");
    }
    /// <summary>Retrieve the <c>template_file</c> property of the item</summary>
    [ArasName("template_file")]
    public IProperty_Item<File> TemplateFile()
    {
      return this.Property("template_file");
    }
    /// <summary>Retrieve the <c>usage_order</c> property of the item</summary>
    [ArasName("usage_order")]
    public IProperty_Number UsageOrder()
    {
      return this.Property("usage_order");
    }
    /// <summary>Retrieve the <c>usage_type</c> property of the item</summary>
    [ArasName("usage_type")]
    public IProperty_Text UsageType()
    {
      return this.Property("usage_type");
    }
  }
}