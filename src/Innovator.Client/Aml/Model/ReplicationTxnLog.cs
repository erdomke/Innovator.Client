using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ReplicationTxnLog </summary>
  [ArasName("ReplicationTxnLog")]
  public class ReplicationTxnLog : Item
  {
    protected ReplicationTxnLog() { }
    public ReplicationTxnLog(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ReplicationTxnLog() { Innovator.Client.Item.AddNullItem<ReplicationTxnLog>(new ReplicationTxnLog { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

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
    public IProperty_Text FromVault()
    {
      return this.Property("from_vault");
    }
    /// <summary>Retrieve the <c>replication_rule</c> property of the item</summary>
    [ArasName("replication_rule")]
    public IProperty_Text ReplicationRule()
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
    public IProperty_Text ToVault()
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