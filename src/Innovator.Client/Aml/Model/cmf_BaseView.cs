using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type cmf_BaseView </summary>
  [ArasName("cmf_BaseView")]
  public class cmf_BaseView : Item, Icmf_BaseView
  {
    protected cmf_BaseView() { }
    public cmf_BaseView(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static cmf_BaseView() { Innovator.Client.Item.AddNullItem<cmf_BaseView>(new cmf_BaseView { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
  }
}