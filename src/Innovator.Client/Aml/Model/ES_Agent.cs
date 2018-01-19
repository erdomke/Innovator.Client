using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_Agent </summary>
  [ArasName("ES_Agent")]
  public class ES_Agent : Item
  {
    protected ES_Agent() { }
    public ES_Agent(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_Agent() { Innovator.Client.Item.AddNullItem<ES_Agent>(new ES_Agent { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>agent_name</c> property of the item</summary>
    [ArasName("agent_name")]
    public IProperty_Text AgentName()
    {
      return this.Property("agent_name");
    }
    /// <summary>Retrieve the <c>crypto_pwd</c> property of the item</summary>
    [ArasName("crypto_pwd")]
    public IProperty_Text CryptoPwd()
    {
      return this.Property("crypto_pwd");
    }
  }
}