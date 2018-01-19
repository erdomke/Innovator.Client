using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SavedSearch </summary>
  [ArasName("SavedSearch")]
  public class SavedSearch : Item
  {
    protected SavedSearch() { }
    public SavedSearch(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SavedSearch() { Innovator.Client.Item.AddNullItem<SavedSearch>(new SavedSearch { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>auto_saved</c> property of the item</summary>
    [ArasName("auto_saved")]
    public IProperty_Boolean AutoSaved()
    {
      return this.Property("auto_saved");
    }
    /// <summary>Retrieve the <c>criteria</c> property of the item</summary>
    [ArasName("criteria")]
    public IProperty_Text Criteria()
    {
      return this.Property("criteria");
    }
    /// <summary>Retrieve the <c>itname</c> property of the item</summary>
    [ArasName("itname")]
    public IProperty_Text Itname()
    {
      return this.Property("itname");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>location</c> property of the item</summary>
    [ArasName("location")]
    public IProperty_Text Location()
    {
      return this.Property("location");
    }
    /// <summary>Retrieve the <c>search_mode</c> property of the item</summary>
    [ArasName("search_mode")]
    public IProperty_Item<SearchMode> SearchMode()
    {
      return this.Property("search_mode");
    }
    /// <summary>Retrieve the <c>show_on_toc</c> property of the item</summary>
    [ArasName("show_on_toc")]
    public IProperty_Boolean ShowOnToc()
    {
      return this.Property("show_on_toc");
    }
  }
}