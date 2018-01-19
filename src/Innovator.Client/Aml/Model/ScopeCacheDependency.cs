using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ScopeCacheDependency </summary>
  [ArasName("ScopeCacheDependency")]
  public class ScopeCacheDependency : Item, IScopeCacheDependency
  {
    protected ScopeCacheDependency() { }
    public ScopeCacheDependency(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ScopeCacheDependency() { Innovator.Client.Item.AddNullItem<ScopeCacheDependency>(new ScopeCacheDependency { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}