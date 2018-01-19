using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Team </summary>
  [ArasName("Team")]
  public class Team : Item
  {
    protected Team() { }
    public Team(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Team() { Innovator.Client.Item.AddNullItem<Team>(new Team { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
  }
}