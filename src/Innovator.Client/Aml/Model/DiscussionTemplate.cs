using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type DiscussionTemplate </summary>
  [ArasName("DiscussionTemplate")]
  public class DiscussionTemplate : Item, INullRelationship<ItemType>
  {
    protected DiscussionTemplate() { }
    public DiscussionTemplate(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static DiscussionTemplate() { Innovator.Client.Item.AddNullItem<DiscussionTemplate>(new DiscussionTemplate { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>class_path</c> property of the item</summary>
    [ArasName("class_path")]
    public IProperty_Text ClassPath()
    {
      return this.Property("class_path");
    }
    /// <summary>Retrieve the <c>default_visibility_identity</c> property of the item</summary>
    [ArasName("default_visibility_identity")]
    public IProperty_Item<Identity> DefaultVisibilityIdentity()
    {
      return this.Property("default_visibility_identity");
    }
    /// <summary>Retrieve the <c>file_selection_depth</c> property of the item</summary>
    [ArasName("file_selection_depth")]
    public IProperty_Number FileSelectionDepth()
    {
      return this.Property("file_selection_depth");
    }
    /// <summary>Retrieve the <c>item_selection_depth</c> property of the item</summary>
    [ArasName("item_selection_depth")]
    public IProperty_Number ItemSelectionDepth()
    {
      return this.Property("item_selection_depth");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>visibility_supported</c> property of the item</summary>
    [ArasName("visibility_supported")]
    public IProperty_Boolean VisibilitySupported()
    {
      return this.Property("visibility_supported");
    }
  }
}