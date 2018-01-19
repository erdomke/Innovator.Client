using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type xClassificationTree </summary>
  [ArasName("xClassificationTree")]
  public class xClassificationTree : Item
  {
    protected xClassificationTree() { }
    public xClassificationTree(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static xClassificationTree() { Innovator.Client.Item.AddNullItem<xClassificationTree>(new xClassificationTree { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>classification_hierarchy</c> property of the item</summary>
    [ArasName("classification_hierarchy")]
    public IProperty_Text ClassificationHierarchy()
    {
      return this.Property("classification_hierarchy");
    }
    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>item_number</c> property of the item</summary>
    [ArasName("item_number")]
    public IProperty_Text ItemNumber()
    {
      return this.Property("item_number");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>select_only_leaf_class</c> property of the item</summary>
    [ArasName("select_only_leaf_class")]
    public IProperty_Boolean SelectOnlyLeafClass()
    {
      return this.Property("select_only_leaf_class");
    }
    /// <summary>Retrieve the <c>select_only_single_class</c> property of the item</summary>
    [ArasName("select_only_single_class")]
    public IProperty_Boolean SelectOnlySingleClass()
    {
      return this.Property("select_only_single_class");
    }
  }
}