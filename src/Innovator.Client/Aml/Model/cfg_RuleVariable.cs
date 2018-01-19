using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cfg_RuleVariable </summary>
  [ArasName("cfg_RuleVariable")]
  public class cfg_RuleVariable : Item, INullRelationship<cfg_Rule>, IRelationship<cfg_Variable>
  {
    protected cfg_RuleVariable() { }
    public cfg_RuleVariable(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cfg_RuleVariable() { Innovator.Client.Item.AddNullItem<cfg_RuleVariable>(new cfg_RuleVariable { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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