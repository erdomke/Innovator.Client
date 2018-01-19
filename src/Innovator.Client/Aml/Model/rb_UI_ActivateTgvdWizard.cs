using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_UI_ActivateTgvdWizard </summary>
  [ArasName("rb_UI_ActivateTgvdWizard")]
  public class rb_UI_ActivateTgvdWizard : Item
  {
    protected rb_UI_ActivateTgvdWizard() { }
    public rb_UI_ActivateTgvdWizard(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_UI_ActivateTgvdWizard() { Innovator.Client.Item.AddNullItem<rb_UI_ActivateTgvdWizard>(new rb_UI_ActivateTgvdWizard { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>access_identity</c> property of the item</summary>
    [ArasName("access_identity")]
    public IProperty_Item<Identity> AccessIdentity()
    {
      return this.Property("access_identity");
    }
    /// <summary>Retrieve the <c>is_startcondition</c> property of the item</summary>
    [ArasName("is_startcondition")]
    public IProperty_Boolean IsStartcondition()
    {
      return this.Property("is_startcondition");
    }
    /// <summary>Retrieve the <c>item_type</c> property of the item</summary>
    [ArasName("item_type")]
    public IProperty_Item<ItemType> ItemType()
    {
      return this.Property("item_type");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>new_exist_list</c> property of the item</summary>
    [ArasName("new_exist_list")]
    public IProperty_Text NewExistList()
    {
      return this.Property("new_exist_list");
    }
    /// <summary>Retrieve the <c>relationship_name</c> property of the item</summary>
    [ArasName("relationship_name")]
    public IProperty_Text RelationshipName()
    {
      return this.Property("relationship_name");
    }
    /// <summary>Retrieve the <c>relationship_names</c> property of the item</summary>
    [ArasName("relationship_names")]
    public IProperty_Text RelationshipNames()
    {
      return this.Property("relationship_names");
    }
    /// <summary>Retrieve the <c>target_usage</c> property of the item</summary>
    [ArasName("target_usage")]
    public IProperty_Text TargetUsage()
    {
      return this.Property("target_usage");
    }
  }
}