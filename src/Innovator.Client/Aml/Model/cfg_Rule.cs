using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cfg_Rule </summary>
  [ArasName("cfg_Rule")]
  public class cfg_Rule : Item, IScopeCacheDependency
  {
    protected cfg_Rule() { }
    public cfg_Rule(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cfg_Rule() { Innovator.Client.Item.AddNullItem<cfg_Rule>(new cfg_Rule { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>definition</c> property of the item</summary>
    [ArasName("definition")]
    public IProperty_Text Definition()
    {
      return this.Property("definition");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
  }
}