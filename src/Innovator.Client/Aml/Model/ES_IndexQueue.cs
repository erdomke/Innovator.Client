using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_IndexQueue </summary>
  [ArasName("ES_IndexQueue")]
  public class ES_IndexQueue : Item
  {
    protected ES_IndexQueue() { }
    public ES_IndexQueue(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_IndexQueue() { Innovator.Client.Item.AddNullItem<ES_IndexQueue>(new ES_IndexQueue { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>indexed_id</c> property of the item</summary>
    [ArasName("indexed_id")]
    public IProperty_Text IndexedId()
    {
      return this.Property("indexed_id");
    }
    /// <summary>Retrieve the <c>indexed_indexed_on</c> property of the item</summary>
    [ArasName("indexed_indexed_on")]
    public IProperty_Date IndexedIndexedOn()
    {
      return this.Property("indexed_indexed_on");
    }
    /// <summary>Retrieve the <c>indexed_modified_on</c> property of the item</summary>
    [ArasName("indexed_modified_on")]
    public IProperty_Date IndexedModifiedOn()
    {
      return this.Property("indexed_modified_on");
    }
    /// <summary>Retrieve the <c>indexed_type</c> property of the item</summary>
    [ArasName("indexed_type")]
    public IProperty_Text IndexedType()
    {
      return this.Property("indexed_type");
    }
    /// <summary>Retrieve the <c>indexing_started_on</c> property of the item</summary>
    [ArasName("indexing_started_on")]
    public IProperty_Date IndexingStartedOn()
    {
      return this.Property("indexing_started_on");
    }
    /// <summary>Retrieve the <c>operation_type</c> property of the item</summary>
    [ArasName("operation_type")]
    public IProperty_Text OperationType()
    {
      return this.Property("operation_type");
    }
    /// <summary>Retrieve the <c>root_id</c> property of the item</summary>
    [ArasName("root_id")]
    public IProperty_Text RootId()
    {
      return this.Property("root_id");
    }
    /// <summary>Retrieve the <c>root_type</c> property of the item</summary>
    [ArasName("root_type")]
    public IProperty_Text RootType()
    {
      return this.Property("root_type");
    }
    /// <summary>Retrieve the <c>status</c> property of the item</summary>
    [ArasName("status")]
    public IProperty_Text Status()
    {
      return this.Property("status");
    }
  }
}