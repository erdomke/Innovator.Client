using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type qry_QueryParameter </summary>
  [ArasName("qry_QueryParameter")]
  public class qry_QueryParameter : Item, INullRelationship<qry_QueryDefinition>
  {
    protected qry_QueryParameter() { }
    public qry_QueryParameter(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static qry_QueryParameter() { Innovator.Client.Item.AddNullItem<qry_QueryParameter>(new qry_QueryParameter { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
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
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>value</c> property of the item</summary>
    [ArasName("value")]
    public IProperty_Text ValueProp()
    {
      return this.Property("value");
    }
  }
}