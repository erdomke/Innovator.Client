using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type Vault </summary>
  [ArasName("Vault")]
  public class Vault : Item
  {
    protected Vault() { }
    public Vault(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static Vault() { Innovator.Client.Item.AddNullItem<Vault>(new Vault { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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
    /// <summary>Retrieve the <c>vault_url</c> property of the item</summary>
    [ArasName("vault_url")]
    public IProperty_Text VaultUrl()
    {
      return this.Property("vault_url");
    }
    /// <summary>Retrieve the <c>vault_url_pattern</c> property of the item</summary>
    [ArasName("vault_url_pattern")]
    public IProperty_Text VaultUrlPattern()
    {
      return this.Property("vault_url_pattern");
    }
  }
}