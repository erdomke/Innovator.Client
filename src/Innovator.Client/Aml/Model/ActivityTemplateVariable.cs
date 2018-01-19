using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Activity Template Variable </summary>
  [ArasName("Activity Template Variable")]
  public class ActivityTemplateVariable : Item, INullRelationship<ActivityTemplate>
  {
    protected ActivityTemplateVariable() { }
    public ActivityTemplateVariable(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ActivityTemplateVariable() { Innovator.Client.Item.AddNullItem<ActivityTemplateVariable>(new ActivityTemplateVariable { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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
    /// <summary>Retrieve the <c>default_value</c> property of the item</summary>
    [ArasName("default_value")]
    public IProperty_Text DefaultValue()
    {
      return this.Property("default_value");
    }
    /// <summary>Retrieve the <c>is_hidden</c> property of the item</summary>
    [ArasName("is_hidden")]
    public IProperty_Boolean IsHidden()
    {
      return this.Property("is_hidden");
    }
    /// <summary>Retrieve the <c>is_required</c> property of the item</summary>
    [ArasName("is_required")]
    public IProperty_Boolean IsRequired()
    {
      return this.Property("is_required");
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
    /// <summary>Retrieve the <c>sequence</c> property of the item</summary>
    [ArasName("sequence")]
    public IProperty_Number Sequence()
    {
      return this.Property("sequence");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>source</c> property of the item</summary>
    [ArasName("source")]
    public IProperty_Item<List> Source()
    {
      return this.Property("source");
    }
  }
}