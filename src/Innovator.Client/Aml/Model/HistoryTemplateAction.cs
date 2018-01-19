using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type History Template Action </summary>
  [ArasName("History Template Action")]
  public class HistoryTemplateAction : Item, INullRelationship<HistoryTemplate>, IRelationship<HistoryAction>
  {
    protected HistoryTemplateAction() { }
    public HistoryTemplateAction(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static HistoryTemplateAction() { Innovator.Client.Item.AddNullItem<HistoryTemplateAction>(new HistoryTemplateAction { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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