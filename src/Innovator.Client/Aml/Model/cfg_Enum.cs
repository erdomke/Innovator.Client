using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cfg_Enum </summary>
  [ArasName("cfg_Enum")]
  public class cfg_Enum : Item, IScopeCacheDependency
  {
    protected cfg_Enum() { }
    public cfg_Enum(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cfg_Enum() { Innovator.Client.Item.AddNullItem<cfg_Enum>(new cfg_Enum { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>data_type</c> property of the item</summary>
    [ArasName("data_type")]
    public IProperty_Text DataType()
    {
      return this.Property("data_type");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
  }
}