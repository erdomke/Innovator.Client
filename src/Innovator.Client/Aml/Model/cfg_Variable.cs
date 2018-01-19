using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cfg_Variable </summary>
  [ArasName("cfg_Variable")]
  public class cfg_Variable : Item, IScopeCacheDependency
  {
    protected cfg_Variable() { }
    public cfg_Variable(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cfg_Variable() { Innovator.Client.Item.AddNullItem<cfg_Variable>(new cfg_Variable { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>data_type</c> property of the item</summary>
    [ArasName("data_type")]
    public IProperty_Text DataType()
    {
      return this.Property("data_type");
    }
    /// <summary>Retrieve the <c>enum_id</c> property of the item</summary>
    [ArasName("enum_id")]
    public IProperty_Item<cfg_Enum> EnumId()
    {
      return this.Property("enum_id");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
  }
}