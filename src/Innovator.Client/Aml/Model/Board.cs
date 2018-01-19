using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Board </summary>
  [ArasName("Board")]
  public class Board : Item
  {
    protected Board() { }
    public Board(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Board() { Innovator.Client.Item.AddNullItem<Board>(new Board { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>item_config_id</c> property of the item</summary>
    [ArasName("item_config_id")]
    public IProperty_Text ItemConfigId()
    {
      return this.Property("item_config_id");
    }
    /// <summary>Retrieve the <c>item_keyed_name</c> property of the item</summary>
    [ArasName("item_keyed_name")]
    public IProperty_Text ItemKeyedName()
    {
      return this.Property("item_keyed_name");
    }
    /// <summary>Retrieve the <c>item_type_name</c> property of the item</summary>
    [ArasName("item_type_name")]
    public IProperty_Text ItemTypeName()
    {
      return this.Property("item_type_name");
    }
  }
}