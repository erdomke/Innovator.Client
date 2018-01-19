using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SystemEventHandler </summary>
  [ArasName("SystemEventHandler")]
  public class SystemEventHandler : Item, INullRelationship<SystemEvent>, IRelationship<Method>
  {
    protected SystemEventHandler() { }
    public SystemEventHandler(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SystemEventHandler() { Innovator.Client.Item.AddNullItem<SystemEventHandler>(new SystemEventHandler { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>is_enabled</c> property of the item</summary>
    [ArasName("is_enabled")]
    public IProperty_Boolean IsEnabled()
    {
      return this.Property("is_enabled");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}