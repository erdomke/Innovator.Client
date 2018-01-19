using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type mp_PolicyAccessItem </summary>
  [ArasName("mp_PolicyAccessItem")]
  public class mp_PolicyAccessItem : Item, Imp_PolicyAccessItem
  {
    protected mp_PolicyAccessItem() { }
    public mp_PolicyAccessItem(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static mp_PolicyAccessItem() { Innovator.Client.Item.AddNullItem<mp_PolicyAccessItem>(new mp_PolicyAccessItem { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}