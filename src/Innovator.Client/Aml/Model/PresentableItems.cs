using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type PresentableItems </summary>
  [ArasName("PresentableItems")]
  public class PresentableItems : Item, IPresentableItems
  {
    protected PresentableItems() { }
    public PresentableItems(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static PresentableItems() { Innovator.Client.Item.AddNullItem<PresentableItems>(new PresentableItems { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}