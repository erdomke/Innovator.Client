using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_IndexedType </summary>
  [ArasName("ES_IndexedType")]
  public class ES_IndexedType : Item
  {
    protected ES_IndexedType() { }
    public ES_IndexedType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_IndexedType() { Innovator.Client.Item.AddNullItem<ES_IndexedType>(new ES_IndexedType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>indexed_type</c> property of the item</summary>
    [ArasName("indexed_type")]
    public IProperty_Item<ItemType> IndexedType()
    {
      return this.Property("indexed_type");
    }
  }
}