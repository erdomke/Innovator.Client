using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_VersionedPropertyPoly </summary>
  [ArasName("ES_VersionedPropertyPoly")]
  public class ES_VersionedPropertyPoly : Item, IES_VersionedPropertyPoly
  {
    protected ES_VersionedPropertyPoly() { }
    public ES_VersionedPropertyPoly(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_VersionedPropertyPoly() { Innovator.Client.Item.AddNullItem<ES_VersionedPropertyPoly>(new ES_VersionedPropertyPoly { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}