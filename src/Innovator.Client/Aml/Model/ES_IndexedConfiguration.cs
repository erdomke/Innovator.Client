using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_IndexedConfiguration </summary>
  [ArasName("ES_IndexedConfiguration")]
  public class ES_IndexedConfiguration : Item
  {
    protected ES_IndexedConfiguration() { }
    public ES_IndexedConfiguration(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_IndexedConfiguration() { Innovator.Client.Item.AddNullItem<ES_IndexedConfiguration>(new ES_IndexedConfiguration { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>is_enabled</c> property of the item</summary>
    [ArasName("is_enabled")]
    public IProperty_Boolean IsEnabled()
    {
      return this.Property("is_enabled");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>reset_on</c> property of the item</summary>
    [ArasName("reset_on")]
    public IProperty_Date ResetOn()
    {
      return this.Property("reset_on");
    }
    /// <summary>Retrieve the <c>root_type</c> property of the item</summary>
    [ArasName("root_type")]
    public IProperty_Item<ItemType> RootType()
    {
      return this.Property("root_type");
    }
  }
}