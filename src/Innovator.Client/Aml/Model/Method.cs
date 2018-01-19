using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Method </summary>
  [ArasName("Method")]
  public class Method : Item
  {
    protected Method() { }
    public Method(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Method() { Innovator.Client.Item.AddNullItem<Method>(new Method { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>comments</c> property of the item</summary>
    [ArasName("comments")]
    public IProperty_Text Comments()
    {
      return this.Property("comments");
    }
    /// <summary>Retrieve the <c>core</c> property of the item</summary>
    [ArasName("core")]
    public IProperty_Boolean Core()
    {
      return this.Property("core");
    }
    /// <summary>Retrieve the <c>effective_date</c> property of the item</summary>
    [ArasName("effective_date")]
    public IProperty_Date EffectiveDate()
    {
      return this.Property("effective_date");
    }
    /// <summary>Retrieve the <c>execution_allowed_to</c> property of the item</summary>
    [ArasName("execution_allowed_to")]
    public IProperty_Item<Identity> ExecutionAllowedTo()
    {
      return this.Property("execution_allowed_to");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>method_code</c> property of the item</summary>
    [ArasName("method_code")]
    public IProperty_Text MethodCode()
    {
      return this.Property("method_code");
    }
    /// <summary>Retrieve the <c>method_location</c> property of the item</summary>
    [ArasName("method_location")]
    public IProperty_Text MethodLocation()
    {
      return this.Property("method_location");
    }
    /// <summary>Retrieve the <c>method_type</c> property of the item</summary>
    [ArasName("method_type")]
    public IProperty_Text MethodType()
    {
      return this.Property("method_type");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>release_date</c> property of the item</summary>
    [ArasName("release_date")]
    public IProperty_Date ReleaseDate()
    {
      return this.Property("release_date");
    }
    /// <summary>Retrieve the <c>superseded_date</c> property of the item</summary>
    [ArasName("superseded_date")]
    public IProperty_Date SupersededDate()
    {
      return this.Property("superseded_date");
    }
  }
}