using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type WMType </summary>
  [ArasName("WMType")]
  public class WMType : Item
  {
    protected WMType() { }
    public WMType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static WMType() { Innovator.Client.Item.AddNullItem<WMType>(new WMType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>conversion_task_method</c> property of the item</summary>
    [ArasName("conversion_task_method")]
    public IProperty_Item<Method> ConversionTaskMethod()
    {
      return this.Property("conversion_task_method");
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
  }
}