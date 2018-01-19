using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Action </summary>
  [ArasName("Action")]
  public class Action : Item, ICuiDependency
  {
    protected Action() { }
    public Action(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Action() { Innovator.Client.Item.AddNullItem<Action>(new Action { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>body</c> property of the item</summary>
    [ArasName("body")]
    public IProperty_Text Body()
    {
      return this.Property("body");
    }
    /// <summary>Retrieve the <c>can_execute</c> property of the item</summary>
    [ArasName("can_execute")]
    public IProperty_Item<Method> CanExecute()
    {
      return this.Property("can_execute");
    }
    /// <summary>Retrieve the <c>item_query</c> property of the item</summary>
    [ArasName("item_query")]
    public IProperty_Text ItemQuery()
    {
      return this.Property("item_query");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>location</c> property of the item</summary>
    [ArasName("location")]
    public IProperty_Text Location()
    {
      return this.Property("location");
    }
    /// <summary>Retrieve the <c>method</c> property of the item</summary>
    [ArasName("method")]
    public IProperty_Item<Method> Method()
    {
      return this.Property("method");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>on_complete</c> property of the item</summary>
    [ArasName("on_complete")]
    public IProperty_Item<Method> OnComplete()
    {
      return this.Property("on_complete");
    }
    /// <summary>Retrieve the <c>target</c> property of the item</summary>
    [ArasName("target")]
    public IProperty_Text Target()
    {
      return this.Property("target");
    }
    /// <summary>Retrieve the <c>type</c> property of the item</summary>
    [ArasName("type")]
    public IProperty_Text Type()
    {
      return this.Property("type");
    }
  }
}