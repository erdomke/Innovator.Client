using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_Settings </summary>
  [ArasName("ES_Settings")]
  public class ES_Settings : Item, INullRelationship<Preference>
  {
    protected ES_Settings() { }
    public ES_Settings(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_Settings() { Innovator.Client.Item.AddNullItem<ES_Settings>(new ES_Settings { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>max_analyzed_chars</c> property of the item</summary>
    [ArasName("max_analyzed_chars")]
    public IProperty_Number MaxAnalyzedChars()
    {
      return this.Property("max_analyzed_chars");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}