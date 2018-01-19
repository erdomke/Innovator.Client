using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Business Calendar Exception </summary>
  [ArasName("Business Calendar Exception")]
  public class BusinessCalendarException : Item, INullRelationship<BusinessCalendarYear>
  {
    protected BusinessCalendarException() { }
    public BusinessCalendarException(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static BusinessCalendarException() { Innovator.Client.Item.AddNullItem<BusinessCalendarException>(new BusinessCalendarException { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>day_date</c> property of the item</summary>
    [ArasName("day_date")]
    public IProperty_Date DayDate()
    {
      return this.Property("day_date");
    }
    /// <summary>Retrieve the <c>day_off</c> property of the item</summary>
    [ArasName("day_off")]
    public IProperty_Boolean DayOff()
    {
      return this.Property("day_off");
    }
    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}