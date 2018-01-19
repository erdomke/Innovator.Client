using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Variable </summary>
  [ArasName("Variable")]
  public class Variable : Item
  {
    protected Variable() { }
    public Variable(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Variable() { Innovator.Client.Item.AddNullItem<Variable>(new Variable { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>default_value</c> property of the item</summary>
    [ArasName("default_value")]
    public IProperty_Text DefaultValue()
    {
      return this.Property("default_value");
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
    /// <summary>Retrieve the <c>value</c> property of the item</summary>
    [ArasName("value")]
    public IProperty_Text ValueProp()
    {
      return this.Property("value");
    }
  }
}