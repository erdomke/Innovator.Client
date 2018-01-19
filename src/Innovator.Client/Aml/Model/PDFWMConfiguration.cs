using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type PDFWMConfiguration </summary>
  [ArasName("PDFWMConfiguration")]
  public class PDFWMConfiguration : Item, IWMConfiguration
  {
    protected PDFWMConfiguration() { }
    public PDFWMConfiguration(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static PDFWMConfiguration() { Innovator.Client.Item.AddNullItem<PDFWMConfiguration>(new PDFWMConfiguration { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>watermark_context</c> property of the item</summary>
    [ArasName("watermark_context")]
    public IProperty_Text WatermarkContext()
    {
      return this.Property("watermark_context");
    }
  }
}