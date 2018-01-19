using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SSVCEmailDigestEntry </summary>
  [ArasName("SSVCEmailDigestEntry")]
  public class SSVCEmailDigestEntry : Item
  {
    protected SSVCEmailDigestEntry() { }
    public SSVCEmailDigestEntry(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SSVCEmailDigestEntry() { Innovator.Client.Item.AddNullItem<SSVCEmailDigestEntry>(new SSVCEmailDigestEntry { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>item_config_id</c> property of the item</summary>
    [ArasName("item_config_id")]
    public IProperty_Text ItemConfigId()
    {
      return this.Property("item_config_id");
    }
    /// <summary>Retrieve the <c>item_id</c> property of the item</summary>
    [ArasName("item_id")]
    public IProperty_Text ItemId()
    {
      return this.Property("item_id");
    }
    /// <summary>Retrieve the <c>item_keyed_name</c> property of the item</summary>
    [ArasName("item_keyed_name")]
    public IProperty_Text ItemKeyedName()
    {
      return this.Property("item_keyed_name");
    }
    /// <summary>Retrieve the <c>item_major_rev</c> property of the item</summary>
    [ArasName("item_major_rev")]
    public IProperty_Text ItemMajorRev()
    {
      return this.Property("item_major_rev");
    }
    /// <summary>Retrieve the <c>item_state</c> property of the item</summary>
    [ArasName("item_state")]
    public IProperty_Text ItemState()
    {
      return this.Property("item_state");
    }
    /// <summary>Retrieve the <c>item_type_id</c> property of the item</summary>
    [ArasName("item_type_id")]
    public IProperty_Text ItemTypeId()
    {
      return this.Property("item_type_id");
    }
    /// <summary>Retrieve the <c>item_version</c> property of the item</summary>
    [ArasName("item_version")]
    public IProperty_Number ItemVersion()
    {
      return this.Property("item_version");
    }
    /// <summary>Retrieve the <c>message_classification</c> property of the item</summary>
    [ArasName("message_classification")]
    public IProperty_Text MessageClassification()
    {
      return this.Property("message_classification");
    }
    /// <summary>Retrieve the <c>message_comments</c> property of the item</summary>
    [ArasName("message_comments")]
    public IProperty_Text MessageComments()
    {
      return this.Property("message_comments");
    }
    /// <summary>Retrieve the <c>message_created_by_id</c> property of the item</summary>
    [ArasName("message_created_by_id")]
    public IProperty_Item<User> MessageCreatedById()
    {
      return this.Property("message_created_by_id");
    }
    /// <summary>Retrieve the <c>message_created_on</c> property of the item</summary>
    [ArasName("message_created_on")]
    public IProperty_Date MessageCreatedOn()
    {
      return this.Property("message_created_on");
    }
    /// <summary>Retrieve the <c>message_created_on_tick</c> property of the item</summary>
    [ArasName("message_created_on_tick")]
    public IProperty_Number MessageCreatedOnTick()
    {
      return this.Property("message_created_on_tick");
    }
    /// <summary>Retrieve the <c>message_id</c> property of the item</summary>
    [ArasName("message_id")]
    public IProperty_Item<SecureMessage> MessageId()
    {
      return this.Property("message_id");
    }
    /// <summary>Retrieve the <c>message_is_reply</c> property of the item</summary>
    [ArasName("message_is_reply")]
    public IProperty_Boolean MessageIsReply()
    {
      return this.Property("message_is_reply");
    }
    /// <summary>Retrieve the <c>notification_user_id</c> property of the item</summary>
    [ArasName("notification_user_id")]
    public IProperty_Item<User> NotificationUserId()
    {
      return this.Property("notification_user_id");
    }
  }
}