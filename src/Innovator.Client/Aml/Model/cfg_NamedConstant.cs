using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cfg_NamedConstant </summary>
  [ArasName("cfg_NamedConstant")]
  public class cfg_NamedConstant : Item, IScopeCacheDependency, INullRelationship<cfg_Enum>
  {
    protected cfg_NamedConstant() { }
    public cfg_NamedConstant(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cfg_NamedConstant() { Innovator.Client.Item.AddNullItem<cfg_NamedConstant>(new cfg_NamedConstant { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>date_value</c> property of the item</summary>
    [ArasName("date_value")]
    public IProperty_Date DateValue()
    {
      return this.Property("date_value");
    }
    /// <summary>Retrieve the <c>int_value</c> property of the item</summary>
    [ArasName("int_value")]
    public IProperty_Number IntValue()
    {
      return this.Property("int_value");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>string_value</c> property of the item</summary>
    [ArasName("string_value")]
    public IProperty_Text StringValue()
    {
      return this.Property("string_value");
    }
  }
}