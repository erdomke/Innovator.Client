using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cfg_Scope </summary>
  [ArasName("cfg_Scope")]
  public class cfg_Scope : Item, IScopeCacheDependency
  {
    protected cfg_Scope() { }
    public cfg_Scope(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cfg_Scope() { Innovator.Client.Item.AddNullItem<cfg_Scope>(new cfg_Scope { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>builder_method_id</c> property of the item</summary>
    [ArasName("builder_method_id")]
    public IProperty_Item<Method> BuilderMethodId()
    {
      return this.Property("builder_method_id");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
  }
}