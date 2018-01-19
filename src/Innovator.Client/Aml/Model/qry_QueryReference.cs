using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type qry_QueryReference </summary>
  [ArasName("qry_QueryReference")]
  public class qry_QueryReference : Item, INullRelationship<qry_QueryDefinition>
  {
    protected qry_QueryReference() { }
    public qry_QueryReference(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static qry_QueryReference() { Innovator.Client.Item.AddNullItem<qry_QueryReference>(new qry_QueryReference { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>child_ref_id</c> property of the item</summary>
    [ArasName("child_ref_id")]
    public IProperty_Text ChildRefId()
    {
      return this.Property("child_ref_id");
    }
    /// <summary>Retrieve the <c>filter_xml</c> property of the item</summary>
    [ArasName("filter_xml")]
    public IProperty_Text FilterXml()
    {
      return this.Property("filter_xml");
    }
    /// <summary>Retrieve the <c>parent_ref_id</c> property of the item</summary>
    [ArasName("parent_ref_id")]
    public IProperty_Text ParentRefId()
    {
      return this.Property("parent_ref_id");
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
    /// <summary>Retrieve the <c>start_query_reference_path</c> property of the item</summary>
    [ArasName("start_query_reference_path")]
    public IProperty_Text StartQueryReferencePath()
    {
      return this.Property("start_query_reference_path");
    }
  }
}