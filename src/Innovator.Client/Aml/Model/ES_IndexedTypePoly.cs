using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_IndexedTypePoly </summary>
  [ArasName("ES_IndexedTypePoly")]
  public class ES_IndexedTypePoly : Item, IES_IndexedTypePoly
  {
    protected ES_IndexedTypePoly() { }
    public ES_IndexedTypePoly(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_IndexedTypePoly() { Innovator.Client.Item.AddNullItem<ES_IndexedTypePoly>(new ES_IndexedTypePoly { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}