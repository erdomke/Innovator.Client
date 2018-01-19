using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_UserMapping </summary>
  [ArasName("ES_UserMapping")]
  public class ES_UserMapping : Item
  {
    protected ES_UserMapping() { }
    public ES_UserMapping(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_UserMapping() { Innovator.Client.Item.AddNullItem<ES_UserMapping>(new ES_UserMapping { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>user_id</c> property of the item</summary>
    [ArasName("user_id")]
    public IProperty_Text UserId()
    {
      return this.Property("user_id");
    }
    /// <summary>Retrieve the <c>user_index</c> property of the item</summary>
    [ArasName("user_index")]
    public IProperty_Text UserIndex()
    {
      return this.Property("user_index");
    }
  }
}