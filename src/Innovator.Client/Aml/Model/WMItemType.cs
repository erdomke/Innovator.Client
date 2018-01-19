using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type WMItemType </summary>
  [ArasName("WMItemType")]
  public class WMItemType : Item, IWMItemType
  {
    protected WMItemType() { }
    public WMItemType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static WMItemType() { Innovator.Client.Item.AddNullItem<WMItemType>(new WMItemType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}