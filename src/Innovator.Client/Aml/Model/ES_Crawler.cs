using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ES_Crawler </summary>
  [ArasName("ES_Crawler")]
  public class ES_Crawler : Item
  {
    protected ES_Crawler() { }
    public ES_Crawler(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ES_Crawler() { Innovator.Client.Item.AddNullItem<ES_Crawler>(new ES_Crawler { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>crawler_agent</c> property of the item</summary>
    [ArasName("crawler_agent")]
    public IProperty_Item<ES_Agent> CrawlerAgent()
    {
      return this.Property("crawler_agent");
    }
    /// <summary>Retrieve the <c>crawler_paging</c> property of the item</summary>
    [ArasName("crawler_paging")]
    public IProperty_Text CrawlerPaging()
    {
      return this.Property("crawler_paging");
    }
    /// <summary>Retrieve the <c>crawler_parameters</c> property of the item</summary>
    [ArasName("crawler_parameters")]
    public IProperty_Text CrawlerParameters()
    {
      return this.Property("crawler_parameters");
    }
    /// <summary>Retrieve the <c>crawler_period</c> property of the item</summary>
    [ArasName("crawler_period")]
    public IProperty_Number CrawlerPeriod()
    {
      return this.Property("crawler_period");
    }
    /// <summary>Retrieve the <c>crawler_state</c> property of the item</summary>
    [ArasName("crawler_state")]
    public IProperty_Text CrawlerState()
    {
      return this.Property("crawler_state");
    }
    /// <summary>Retrieve the <c>crawler_threads</c> property of the item</summary>
    [ArasName("crawler_threads")]
    public IProperty_Number CrawlerThreads()
    {
      return this.Property("crawler_threads");
    }
    /// <summary>Retrieve the <c>crawler_type</c> property of the item</summary>
    [ArasName("crawler_type")]
    public IProperty_Text CrawlerType()
    {
      return this.Property("crawler_type");
    }
  }
}