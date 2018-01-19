using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SelfServiceReport </summary>
  [ArasName("SelfServiceReport")]
  public class SelfServiceReport : Item, ICuiDependency, IFileContainerItems
  {
    protected SelfServiceReport() { }
    public SelfServiceReport(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SelfServiceReport() { Innovator.Client.Item.AddNullItem<SelfServiceReport>(new SelfServiceReport { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>base_item_type</c> property of the item</summary>
    [ArasName("base_item_type")]
    public IProperty_Item<ItemType> BaseItemType()
    {
      return this.Property("base_item_type");
    }
    /// <summary>Retrieve the <c>definition</c> property of the item</summary>
    [ArasName("definition")]
    public IProperty_Text Definition()
    {
      return this.Property("definition");
    }
    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>extension</c> property of the item</summary>
    [ArasName("extension")]
    public IProperty_Text Extension()
    {
      return this.Property("extension");
    }
    /// <summary>Retrieve the <c>format</c> property of the item</summary>
    [ArasName("format")]
    public IProperty_Text Format()
    {
      return this.Property("format");
    }
    /// <summary>Retrieve the <c>hidden</c> property of the item</summary>
    [ArasName("hidden")]
    public IProperty_Boolean Hidden()
    {
      return this.Property("hidden");
    }
    /// <summary>Retrieve the <c>itemtype_classification</c> property of the item</summary>
    [ArasName("itemtype_classification")]
    public IProperty_Text ItemtypeClassification()
    {
      return this.Property("itemtype_classification");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>thumbnail</c> property of the item</summary>
    [ArasName("thumbnail")]
    public IProperty_Item<File> Thumbnail()
    {
      return this.Property("thumbnail");
    }
    /// <summary>Retrieve the <c>visibility</c> property of the item</summary>
    [ArasName("visibility")]
    public IProperty_Text Visibility()
    {
      return this.Property("visibility");
    }
  }
}