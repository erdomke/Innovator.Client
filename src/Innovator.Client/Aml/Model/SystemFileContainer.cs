using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SystemFileContainer </summary>
  [ArasName("SystemFileContainer")]
  public class SystemFileContainer : Item, IFileContainerItems
  {
    protected SystemFileContainer() { }
    public SystemFileContainer(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SystemFileContainer() { Innovator.Client.Item.AddNullItem<SystemFileContainer>(new SystemFileContainer { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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