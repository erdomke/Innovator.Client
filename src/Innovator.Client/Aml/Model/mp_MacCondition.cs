using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type mp_MacCondition </summary>
  [ArasName("mp_MacCondition")]
  public class mp_MacCondition : Item, INullRelationship<mp_MacPolicy>
  {
    protected mp_MacCondition() { }
    public mp_MacCondition(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static mp_MacCondition() { Innovator.Client.Item.AddNullItem<mp_MacCondition>(new mp_MacCondition { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>condition_xml</c> property of the item</summary>
    [ArasName("condition_xml")]
    public IProperty_Text ConditionXml()
    {
      return this.Property("condition_xml");
    }
    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>ref_id</c> property of the item</summary>
    [ArasName("ref_id")]
    public IProperty_Text RefId()
    {
      return this.Property("ref_id");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}