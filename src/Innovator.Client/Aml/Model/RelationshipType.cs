using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type RelationshipType </summary>
  [ArasName("RelationshipType")]
  public class RelationshipType : Item, INullRelationship<ItemType>, IRelationship<ItemType>
  {
    protected RelationshipType() { }
    public RelationshipType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static RelationshipType() { Innovator.Client.Item.AddNullItem<RelationshipType>(new RelationshipType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>auto_search</c> property of the item</summary>
    [ArasName("auto_search")]
    public IProperty_Boolean AutoSearch()
    {
      return this.Property("auto_search");
    }
    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>copy_permissions</c> property of the item</summary>
    [ArasName("copy_permissions")]
    public IProperty_Boolean CopyPermissions()
    {
      return this.Property("copy_permissions");
    }
    /// <summary>Retrieve the <c>core</c> property of the item</summary>
    [ArasName("core")]
    public IProperty_Boolean Core()
    {
      return this.Property("core");
    }
    /// <summary>Retrieve the <c>create_related</c> property of the item</summary>
    [ArasName("create_related")]
    public IProperty_Boolean CreateRelated()
    {
      return this.Property("create_related");
    }
    /// <summary>Retrieve the <c>default_page_size</c> property of the item</summary>
    [ArasName("default_page_size")]
    public IProperty_Number DefaultPageSize()
    {
      return this.Property("default_page_size");
    }
    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>grid_view</c> property of the item</summary>
    [ArasName("grid_view")]
    public IProperty_Text GridView()
    {
      return this.Property("grid_view");
    }
    /// <summary>Retrieve the <c>help_item</c> property of the item</summary>
    [ArasName("help_item")]
    public IProperty_Item<Help> HelpItem()
    {
      return this.Property("help_item");
    }
    /// <summary>Retrieve the <c>help_url</c> property of the item</summary>
    [ArasName("help_url")]
    public IProperty_Text HelpUrl()
    {
      return this.Property("help_url");
    }
    /// <summary>Retrieve the <c>hide_in_all</c> property of the item</summary>
    [ArasName("hide_in_all")]
    public IProperty_Boolean HideInAll()
    {
      return this.Property("hide_in_all");
    }
    /// <summary>Retrieve the <c>inc_rel_key_name</c> property of the item</summary>
    [ArasName("inc_rel_key_name")]
    public IProperty_Boolean IncRelKeyName()
    {
      return this.Property("inc_rel_key_name");
    }
    /// <summary>Retrieve the <c>inc_related_key_name</c> property of the item</summary>
    [ArasName("inc_related_key_name")]
    public IProperty_Boolean IncRelatedKeyName()
    {
      return this.Property("inc_related_key_name");
    }
    /// <summary>Retrieve the <c>is_list_type</c> property of the item</summary>
    [ArasName("is_list_type")]
    public IProperty_Boolean IsListType()
    {
      return this.Property("is_list_type");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>max_occurs</c> property of the item</summary>
    [ArasName("max_occurs")]
    public IProperty_Number MaxOccurs()
    {
      return this.Property("max_occurs");
    }
    /// <summary>Retrieve the <c>min_occurs</c> property of the item</summary>
    [ArasName("min_occurs")]
    public IProperty_Number MinOccurs()
    {
      return this.Property("min_occurs");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>new_show_related</c> property of the item</summary>
    [ArasName("new_show_related")]
    public IProperty_Boolean NewShowRelated()
    {
      return this.Property("new_show_related");
    }
    /// <summary>Retrieve the <c>related_notnull</c> property of the item</summary>
    [ArasName("related_notnull")]
    public IProperty_Boolean RelatedNotnull()
    {
      return this.Property("related_notnull");
    }
    /// <summary>Retrieve the <c>related_option</c> property of the item</summary>
    [ArasName("related_option")]
    public IProperty_Text RelatedOption()
    {
      return this.Property("related_option");
    }
    /// <summary>Retrieve the <c>relationship_id</c> property of the item</summary>
    [ArasName("relationship_id")]
    public IProperty_Item<ItemType> RelationshipId()
    {
      return this.Property("relationship_id");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}