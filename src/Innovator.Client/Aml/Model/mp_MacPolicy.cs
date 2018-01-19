using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type mp_MacPolicy </summary>
  [ArasName("mp_MacPolicy")]
  public class mp_MacPolicy : Item
  {
    protected mp_MacPolicy() { }
    public mp_MacPolicy(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static mp_MacPolicy() { Innovator.Client.Item.AddNullItem<mp_MacPolicy>(new mp_MacPolicy { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>can_delete</c> property of the item</summary>
    [ArasName("can_delete")]
    public IProperty_Text CanDelete()
    {
      return this.Property("can_delete");
    }
    /// <summary>Retrieve the <c>can_discover</c> property of the item</summary>
    [ArasName("can_discover")]
    public IProperty_Text CanDiscover()
    {
      return this.Property("can_discover");
    }
    /// <summary>Retrieve the <c>can_get</c> property of the item</summary>
    [ArasName("can_get")]
    public IProperty_Text CanGet()
    {
      return this.Property("can_get");
    }
    /// <summary>Retrieve the <c>can_update</c> property of the item</summary>
    [ArasName("can_update")]
    public IProperty_Text CanUpdate()
    {
      return this.Property("can_update");
    }
    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>effective_date</c> property of the item</summary>
    [ArasName("effective_date")]
    public IProperty_Date EffectiveDate()
    {
      return this.Property("effective_date");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>release_date</c> property of the item</summary>
    [ArasName("release_date")]
    public IProperty_Date ReleaseDate()
    {
      return this.Property("release_date");
    }
    /// <summary>Retrieve the <c>show_permissions_warning</c> property of the item</summary>
    [ArasName("show_permissions_warning")]
    public IProperty_Text ShowPermissionsWarning()
    {
      return this.Property("show_permissions_warning");
    }
    /// <summary>Retrieve the <c>superseded_date</c> property of the item</summary>
    [ArasName("superseded_date")]
    public IProperty_Date SupersededDate()
    {
      return this.Property("superseded_date");
    }
  }
}