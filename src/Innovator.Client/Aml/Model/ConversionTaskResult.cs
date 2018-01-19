using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ConversionTaskResult </summary>
  [ArasName("ConversionTaskResult")]
  public class ConversionTaskResult : Item, INullRelationship<ConversionTask>
  {
    protected ConversionTaskResult() { }
    public ConversionTaskResult(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ConversionTaskResult() { Innovator.Client.Item.AddNullItem<ConversionTaskResult>(new ConversionTaskResult { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>file_id</c> property of the item</summary>
    [ArasName("file_id")]
    public IProperty_Text FileId()
    {
      return this.Property("file_id");
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