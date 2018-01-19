using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_TrackingRelationshipPoly </summary>
  [ArasName("ES_TrackingRelationshipPoly")]
  public class ES_TrackingRelationshipPoly : Item, IES_TrackingRelationshipPoly
  {
    protected ES_TrackingRelationshipPoly() { }
    public ES_TrackingRelationshipPoly(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_TrackingRelationshipPoly() { Innovator.Client.Item.AddNullItem<ES_TrackingRelationshipPoly>(new ES_TrackingRelationshipPoly { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}