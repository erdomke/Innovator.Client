using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Access_ExplicitDefine </summary>
  [ArasName("Access_ExplicitDefine")]
  public class Access_ExplicitDefine : Item, INullRelationship<Permission_ExplicitDefine>, IRelationship<Identity>
  {
    protected Access_ExplicitDefine() { }
    public Access_ExplicitDefine(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Access_ExplicitDefine() { Innovator.Client.Item.AddNullItem<Access_ExplicitDefine>(new Access_ExplicitDefine { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>can_define_explicitly</c> property of the item</summary>
    [ArasName("can_define_explicitly")]
    public IProperty_Boolean CanDefineExplicitly()
    {
      return this.Property("can_define_explicitly");
    }
    /// <summary>Retrieve the <c>can_get_isdefined_explicitly</c> property of the item</summary>
    [ArasName("can_get_isdefined_explicitly")]
    public IProperty_Boolean CanGetIsdefinedExplicitly()
    {
      return this.Property("can_get_isdefined_explicitly");
    }
    /// <summary>Retrieve the <c>can_undefine_explicitly</c> property of the item</summary>
    [ArasName("can_undefine_explicitly")]
    public IProperty_Boolean CanUndefineExplicitly()
    {
      return this.Property("can_undefine_explicitly");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
  }
}