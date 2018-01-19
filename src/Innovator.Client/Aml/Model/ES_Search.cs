using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_Search </summary>
  [ArasName("ES_Search")]
  public class ES_Search : Item
  {
    protected ES_Search() { }
    public ES_Search(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_Search() { Innovator.Client.Item.AddNullItem<ES_Search>(new ES_Search { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}