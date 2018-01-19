using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type SSVC_Preferences </summary>
  [ArasName("SSVC_Preferences")]
  public class SSVC_Preferences : Item, INullRelationship<Preference>
  {
    protected SSVC_Preferences() { }
    public SSVC_Preferences(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static SSVC_Preferences() { Innovator.Client.Item.AddNullItem<SSVC_Preferences>(new SSVC_Preferences { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>default_bookmark</c> property of the item</summary>
    [ArasName("default_bookmark")]
    public IProperty_Text DefaultBookmark()
    {
      return this.Property("default_bookmark");
    }
    /// <summary>Retrieve the <c>default_flagged_by_number</c> property of the item</summary>
    [ArasName("default_flagged_by_number")]
    public IProperty_Number DefaultFlaggedByNumber()
    {
      return this.Property("default_flagged_by_number");
    }
    /// <summary>Retrieve the <c>default_replies_number</c> property of the item</summary>
    [ArasName("default_replies_number")]
    public IProperty_Number DefaultRepliesNumber()
    {
      return this.Property("default_replies_number");
    }
    /// <summary>Retrieve the <c>desc_digest_notifications</c> property of the item</summary>
    [ArasName("desc_digest_notifications")]
    public IProperty_Text DescDigestNotifications()
    {
      return this.Property("desc_digest_notifications");
    }
    /// <summary>Retrieve the <c>desc_single_notifications</c> property of the item</summary>
    [ArasName("desc_single_notifications")]
    public IProperty_Text DescSingleNotifications()
    {
      return this.Property("desc_single_notifications");
    }
    /// <summary>Retrieve the <c>enable_email_digest_notification</c> property of the item</summary>
    [ArasName("enable_email_digest_notification")]
    public IProperty_Boolean EnableEmailDigestNotification()
    {
      return this.Property("enable_email_digest_notification");
    }
    /// <summary>Retrieve the <c>enable_immediate_notifications</c> property of the item</summary>
    [ArasName("enable_immediate_notifications")]
    public IProperty_Boolean EnableImmediateNotifications()
    {
      return this.Property("enable_immediate_notifications");
    }
    /// <summary>Retrieve the <c>enable_in_app_notifications</c> property of the item</summary>
    [ArasName("enable_in_app_notifications")]
    public IProperty_Boolean EnableInAppNotifications()
    {
      return this.Property("enable_in_app_notifications");
    }
    /// <summary>Retrieve the <c>last_scheduled_digest_time_date</c> property of the item</summary>
    [ArasName("last_scheduled_digest_time_date")]
    public IProperty_Date LastScheduledDigestTimeDate()
    {
      return this.Property("last_scheduled_digest_time_date");
    }
    /// <summary>Retrieve the <c>messages_max_lines</c> property of the item</summary>
    [ArasName("messages_max_lines")]
    public IProperty_Number MessagesMaxLines()
    {
      return this.Property("messages_max_lines");
    }
    /// <summary>Retrieve the <c>prefix_for_highlight_text_markup</c> property of the item</summary>
    [ArasName("prefix_for_highlight_text_markup")]
    public IProperty_Text PrefixForHighlightTextMarkup()
    {
      return this.Property("prefix_for_highlight_text_markup");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>subject_digest_notifications</c> property of the item</summary>
    [ArasName("subject_digest_notifications")]
    public IProperty_Text SubjectDigestNotifications()
    {
      return this.Property("subject_digest_notifications");
    }
    /// <summary>Retrieve the <c>subject_single_notifications</c> property of the item</summary>
    [ArasName("subject_single_notifications")]
    public IProperty_Text SubjectSingleNotifications()
    {
      return this.Property("subject_single_notifications");
    }
    /// <summary>Retrieve the <c>time_digest_interval</c> property of the item</summary>
    [ArasName("time_digest_interval")]
    public IProperty_Number TimeDigestInterval()
    {
      return this.Property("time_digest_interval");
    }
    /// <summary>Retrieve the <c>use_legacy_3d_view_files</c> property of the item</summary>
    [ArasName("use_legacy_3d_view_files")]
    public IProperty_Boolean UseLegacy3dViewFiles()
    {
      return this.Property("use_legacy_3d_view_files");
    }
    /// <summary>Retrieve the <c>use_standard_toolbar_for_viewers</c> property of the item</summary>
    [ArasName("use_standard_toolbar_for_viewers")]
    public IProperty_Boolean UseStandardToolbarForViewers()
    {
      return this.Property("use_standard_toolbar_for_viewers");
    }
  }
}