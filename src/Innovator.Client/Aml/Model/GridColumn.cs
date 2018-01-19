using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Grid Column </summary>
  [ArasName("Grid Column")]
  public class GridColumn : Item, INullRelationship<Grid>
  {
    protected GridColumn() { }
    public GridColumn(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static GridColumn() { Innovator.Client.Item.AddNullItem<GridColumn>(new GridColumn { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>align</c> property of the item</summary>
    [ArasName("align")]
    public IProperty_Text Align()
    {
      return this.Property("align");
    }
    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>datatype</c> property of the item</summary>
    [ArasName("datatype")]
    public IProperty_Text Datatype()
    {
      return this.Property("datatype");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>property</c> property of the item</summary>
    [ArasName("property")]
    public IProperty_Text PropertyProp()
    {
      return this.Property("property");
    }
    /// <summary>Retrieve the <c>select_method</c> property of the item</summary>
    [ArasName("select_method")]
    public IProperty_Item<Method> SelectMethod()
    {
      return this.Property("select_method");
    }
    /// <summary>Retrieve the <c>select_query</c> property of the item</summary>
    [ArasName("select_query")]
    public IProperty_Text SelectQuery()
    {
      return this.Property("select_query");
    }
    /// <summary>Retrieve the <c>sort</c> property of the item</summary>
    [ArasName("sort")]
    public IProperty_Number Sort()
    {
      return this.Property("sort");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>source_form</c> property of the item</summary>
    [ArasName("source_form")]
    public IProperty_Item<Form> SourceForm()
    {
      return this.Property("source_form");
    }
    /// <summary>Retrieve the <c>starts_nested_row</c> property of the item</summary>
    [ArasName("starts_nested_row")]
    public IProperty_Boolean StartsNestedRow()
    {
      return this.Property("starts_nested_row");
    }
    /// <summary>Retrieve the <c>visible</c> property of the item</summary>
    [ArasName("visible")]
    public IProperty_Boolean Visible()
    {
      return this.Property("visible");
    }
    /// <summary>Retrieve the <c>width</c> property of the item</summary>
    [ArasName("width")]
    public IProperty_Number Width()
    {
      return this.Property("width");
    }
    /// <summary>Retrieve the <c>xpath</c> property of the item</summary>
    [ArasName("xpath")]
    public IProperty_Text Xpath()
    {
      return this.Property("xpath");
    }
  }
}