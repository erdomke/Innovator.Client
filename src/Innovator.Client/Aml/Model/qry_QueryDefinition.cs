using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type qry_QueryDefinition </summary>
  [ArasName("qry_QueryDefinition")]
  public class qry_QueryDefinition : Item
  {
    protected qry_QueryDefinition() { }
    public qry_QueryDefinition(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static qry_QueryDefinition() { Innovator.Client.Item.AddNullItem<qry_QueryDefinition>(new qry_QueryDefinition { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
  }
}