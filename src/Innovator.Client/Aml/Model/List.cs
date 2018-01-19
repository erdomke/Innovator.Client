using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type List </summary>
  [ArasName("List")]
  public class List : Item, Irb_UserInputDataSource
  {
    protected List() { }
    public List(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static List() { Innovator.Client.Item.AddNullItem<List>(new List { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>core</c> property of the item</summary>
    [ArasName("core")]
    public IProperty_Boolean Core()
    {
      return this.Property("core");
    }
    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
  }
}