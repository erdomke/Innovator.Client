using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_QueryDefinitionParameterMap </summary>
  [ArasName("rb_QueryDefinitionParameterMap")]
  public class rb_QueryDefinitionParameterMap : Item, INullRelationship<rb_TreeGridViewDefinition>
  {
    protected rb_QueryDefinitionParameterMap() { }
    public rb_QueryDefinitionParameterMap(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_QueryDefinitionParameterMap() { Innovator.Client.Item.AddNullItem<rb_QueryDefinitionParameterMap>(new rb_QueryDefinitionParameterMap { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>qd_parameter_name</c> property of the item</summary>
    [ArasName("qd_parameter_name")]
    public IProperty_Text QdParameterName()
    {
      return this.Property("qd_parameter_name");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>user_input_data_source</c> property of the item</summary>
    [ArasName("user_input_data_source")]
    public IProperty_Item<Irb_UserInputDataSource> UserInputDataSource()
    {
      return this.Property("user_input_data_source");
    }
    /// <summary>Retrieve the <c>user_input_data_type</c> property of the item</summary>
    [ArasName("user_input_data_type")]
    public IProperty_Text UserInputDataType()
    {
      return this.Property("user_input_data_type");
    }
    /// <summary>Retrieve the <c>user_input_default_value</c> property of the item</summary>
    [ArasName("user_input_default_value")]
    public IProperty_Text UserInputDefaultValue()
    {
      return this.Property("user_input_default_value");
    }
    /// <summary>Retrieve the <c>user_input_pattern</c> property of the item</summary>
    [ArasName("user_input_pattern")]
    public IProperty_Text UserInputPattern()
    {
      return this.Property("user_input_pattern");
    }
  }
}