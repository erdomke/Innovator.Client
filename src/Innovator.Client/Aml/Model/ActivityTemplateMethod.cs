using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Activity Template Method </summary>
  [ArasName("Activity Template Method")]
  public class ActivityTemplateMethod : Item, INullRelationship<ActivityTemplate>, IRelationship<Method>
  {
    protected ActivityTemplateMethod() { }
    public ActivityTemplateMethod(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ActivityTemplateMethod() { Innovator.Client.Item.AddNullItem<ActivityTemplateMethod>(new ActivityTemplateMethod { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>event</c> property of the item</summary>
    [ArasName("event")]
    public IProperty_Text Event()
    {
      return this.Property("event");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}