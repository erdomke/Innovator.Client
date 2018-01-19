using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Viewer </summary>
  [ArasName("Viewer")]
  public class Viewer : Item
  {
    protected Viewer() { }
    public Viewer(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Viewer() { Innovator.Client.Item.AddNullItem<Viewer>(new Viewer { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
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
    /// <summary>Retrieve the <c>viewer_url</c> property of the item</summary>
    [ArasName("viewer_url")]
    public IProperty_Text ViewerUrl()
    {
      return this.Property("viewer_url");
    }
  }
}