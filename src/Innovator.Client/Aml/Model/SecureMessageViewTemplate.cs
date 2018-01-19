using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SecureMessageViewTemplate </summary>
  [ArasName("SecureMessageViewTemplate")]
  public class SecureMessageViewTemplate : Item
  {
    protected SecureMessageViewTemplate() { }
    public SecureMessageViewTemplate(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SecureMessageViewTemplate() { Innovator.Client.Item.AddNullItem<SecureMessageViewTemplate>(new SecureMessageViewTemplate { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>sketch_tooltip_template</c> property of the item</summary>
    [ArasName("sketch_tooltip_template")]
    public IProperty_Text SketchTooltipTemplate()
    {
      return this.Property("sketch_tooltip_template");
    }
    /// <summary>Retrieve the <c>style</c> property of the item</summary>
    [ArasName("style")]
    public IProperty_Text Style()
    {
      return this.Property("style");
    }
    /// <summary>Retrieve the <c>template</c> property of the item</summary>
    [ArasName("template")]
    public IProperty_Text Template()
    {
      return this.Property("template");
    }
    /// <summary>Retrieve the <c>thumbnail_tooltip_template</c> property of the item</summary>
    [ArasName("thumbnail_tooltip_template")]
    public IProperty_Text ThumbnailTooltipTemplate()
    {
      return this.Property("thumbnail_tooltip_template");
    }
  }
}