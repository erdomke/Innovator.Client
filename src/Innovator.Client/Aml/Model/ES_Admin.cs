using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_Admin </summary>
  [ArasName("ES_Admin")]
  public class ES_Admin : Item
  {
    protected ES_Admin() { }
    public ES_Admin(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_Admin() { Innovator.Client.Item.AddNullItem<ES_Admin>(new ES_Admin { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}