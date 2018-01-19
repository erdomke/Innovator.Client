using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cfg_ScopeRule </summary>
  [ArasName("cfg_ScopeRule")]
  public class cfg_ScopeRule : Item, INullRelationship<cfg_Scope>, IRelationship<cfg_Rule>
  {
    protected cfg_ScopeRule() { }
    public cfg_ScopeRule(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cfg_ScopeRule() { Innovator.Client.Item.AddNullItem<cfg_ScopeRule>(new cfg_ScopeRule { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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