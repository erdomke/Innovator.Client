using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_TreeGridViewDefinition </summary>
  [ArasName("rb_TreeGridViewDefinition")]
  public class rb_TreeGridViewDefinition : Item, IPresentableItems
  {
    protected rb_TreeGridViewDefinition() { }
    public rb_TreeGridViewDefinition(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_TreeGridViewDefinition() { Innovator.Client.Item.AddNullItem<rb_TreeGridViewDefinition>(new rb_TreeGridViewDefinition { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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
    /// <summary>Retrieve the <c>query_definition</c> property of the item</summary>
    [ArasName("query_definition")]
    public IProperty_Item<qry_QueryDefinition> QueryDefinition()
    {
      return this.Property("query_definition");
    }
  }
}