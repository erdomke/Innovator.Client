using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Relationship Grid Event </summary>
  [ArasName("Relationship Grid Event")]
  public class RelationshipGridEvent : Item, INullRelationship<RelationshipType>, IRelationship<Method>
  {
    protected RelationshipGridEvent() { }
    public RelationshipGridEvent(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static RelationshipGridEvent() { Innovator.Client.Item.AddNullItem<RelationshipGridEvent>(new RelationshipGridEvent { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>grid_event</c> property of the item</summary>
    [ArasName("grid_event")]
    public IProperty_Text GridEvent()
    {
      return this.Property("grid_event");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}