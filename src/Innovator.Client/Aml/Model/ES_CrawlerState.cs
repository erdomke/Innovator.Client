using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_CrawlerState </summary>
  [ArasName("ES_CrawlerState")]
  public class ES_CrawlerState : Item, INullRelationship<ES_Crawler>
  {
    protected ES_CrawlerState() { }
    public ES_CrawlerState(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_CrawlerState() { Innovator.Client.Item.AddNullItem<ES_CrawlerState>(new ES_CrawlerState { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    [ArasName("behavior")]
    public IProperty_Text Behavior()
    {
      return this.Property("behavior");
    }
    /// <summary>Retrieve the <c>ca_finish</c> property of the item</summary>
    [ArasName("ca_finish")]
    public IProperty_Date CaFinish()
    {
      return this.Property("ca_finish");
    }
    /// <summary>Retrieve the <c>ca_start</c> property of the item</summary>
    [ArasName("ca_start")]
    public IProperty_Date CaStart()
    {
      return this.Property("ca_start");
    }
    /// <summary>Retrieve the <c>current_action</c> property of the item</summary>
    [ArasName("current_action")]
    public IProperty_Text CurrentAction()
    {
      return this.Property("current_action");
    }
    /// <summary>Retrieve the <c>currently_processed</c> property of the item</summary>
    [ArasName("currently_processed")]
    public IProperty_Number CurrentlyProcessed()
    {
      return this.Property("currently_processed");
    }
    /// <summary>Retrieve the <c>errors</c> property of the item</summary>
    [ArasName("errors")]
    public IProperty_Text Errors()
    {
      return this.Property("errors");
    }
    /// <summary>Retrieve the <c>has_errors</c> property of the item</summary>
    [ArasName("has_errors")]
    public IProperty_Boolean HasErrors()
    {
      return this.Property("has_errors");
    }
    /// <summary>Retrieve the <c>is_iteration_finished</c> property of the item</summary>
    [ArasName("is_iteration_finished")]
    public IProperty_Boolean IsIterationFinished()
    {
      return this.Property("is_iteration_finished");
    }
    /// <summary>Retrieve the <c>last_update</c> property of the item</summary>
    [ArasName("last_update")]
    public IProperty_Date LastUpdate()
    {
      return this.Property("last_update");
    }
    /// <summary>Retrieve the <c>next_action</c> property of the item</summary>
    [ArasName("next_action")]
    public IProperty_Text NextAction()
    {
      return this.Property("next_action");
    }
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    [ArasName("sort_order")]
    public IProperty_Number SortOrder()
    {
      return this.Property("sort_order");
    }
    /// <summary>Retrieve the <c>total_to_process</c> property of the item</summary>
    [ArasName("total_to_process")]
    public IProperty_Number TotalToProcess()
    {
      return this.Property("total_to_process");
    }
  }
}