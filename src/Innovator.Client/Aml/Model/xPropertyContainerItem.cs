using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xPropertyContainerItem </summary>
  [ArasName("xPropertyContainerItem")]
  public class xPropertyContainerItem : Item, IxPropertyContainerItem
  {
    protected xPropertyContainerItem() { }
    public xPropertyContainerItem(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xPropertyContainerItem() { Innovator.Client.Item.AddNullItem<xPropertyContainerItem>(new xPropertyContainerItem { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}