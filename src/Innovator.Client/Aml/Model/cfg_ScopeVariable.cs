using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cfg_ScopeVariable </summary>
  [ArasName("cfg_ScopeVariable")]
  public class cfg_ScopeVariable : Item, INullRelationship<cfg_Scope>, IRelationship<cfg_Variable>
  {
    protected cfg_ScopeVariable() { }
    public cfg_ScopeVariable(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cfg_ScopeVariable() { Innovator.Client.Item.AddNullItem<cfg_ScopeVariable>(new cfg_ScopeVariable { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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