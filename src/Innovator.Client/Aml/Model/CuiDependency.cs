using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type CuiDependency </summary>
  [ArasName("CuiDependency")]
  public class CuiDependency : Item, ICuiDependency
  {
    protected CuiDependency() { }
    public CuiDependency(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static CuiDependency() { Innovator.Client.Item.AddNullItem<CuiDependency>(new CuiDependency { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}