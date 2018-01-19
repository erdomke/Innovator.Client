using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_AgentState </summary>
  [ArasName("ES_AgentState")]
  public class ES_AgentState : Item, INullRelationship<ES_Agent>
  {
    protected ES_AgentState() { }
    public ES_AgentState(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_AgentState() { Innovator.Client.Item.AddNullItem<ES_AgentState>(new ES_AgentState { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>last_update</c> property of the item</summary>
    [ArasName("last_update")]
    public IProperty_Date LastUpdate()
    {
      return this.Property("last_update");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}