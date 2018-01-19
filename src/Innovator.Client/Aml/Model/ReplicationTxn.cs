using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ReplicationTxn </summary>
  [ArasName("ReplicationTxn")]
  public class ReplicationTxn : Item
  {
    protected ReplicationTxn() { }
    public ReplicationTxn(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ReplicationTxn() { Innovator.Client.Item.AddNullItem<ReplicationTxn>(new ReplicationTxn { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>end_time</c> property of the item</summary>
    [ArasName("end_time")]
    public IProperty_Date EndTime()
    {
      return this.Property("end_time");
    }
    /// <summary>Retrieve the <c>error_msg</c> property of the item</summary>
    [ArasName("error_msg")]
    public IProperty_Text ErrorMsg()
    {
      return this.Property("error_msg");
    }
    /// <summary>Retrieve the <c>execution_attempt</c> property of the item</summary>
    [ArasName("execution_attempt")]
    public IProperty_Number ExecutionAttempt()
    {
      return this.Property("execution_attempt");
    }
    /// <summary>Retrieve the <c>file_id</c> property of the item</summary>
    [ArasName("file_id")]
    public IProperty_Text FileId()
    {
      return this.Property("file_id");
    }
    /// <summary>Retrieve the <c>from_vault</c> property of the item</summary>
    [ArasName("from_vault")]
    public IProperty_Item<Vault> FromVault()
    {
      return this.Property("from_vault");
    }
    /// <summary>Retrieve the <c>not_before</c> property of the item</summary>
    [ArasName("not_before")]
    public IProperty_Date NotBefore()
    {
      return this.Property("not_before");
    }
    /// <summary>Retrieve the <c>replication_rule</c> property of the item</summary>
    [ArasName("replication_rule")]
    public IProperty_Item<ReplicationRule> ReplicationRule()
    {
      return this.Property("replication_rule");
    }
    /// <summary>Retrieve the <c>replication_status</c> property of the item</summary>
    [ArasName("replication_status")]
    public IProperty_Text ReplicationStatus()
    {
      return this.Property("replication_status");
    }
    /// <summary>Retrieve the <c>start_time</c> property of the item</summary>
    [ArasName("start_time")]
    public IProperty_Date StartTime()
    {
      return this.Property("start_time");
    }
    /// <summary>Retrieve the <c>to_vault</c> property of the item</summary>
    [ArasName("to_vault")]
    public IProperty_Item<Vault> ToVault()
    {
      return this.Property("to_vault");
    }
    /// <summary>Retrieve the <c>user_id</c> property of the item</summary>
    [ArasName("user_id")]
    public IProperty_Item<User> UserId()
    {
      return this.Property("user_id");
    }
  }
}