using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type ServiceProvider </summary>
  [ArasName("ServiceProvider")]
  public class ServiceProvider : Item
  {
    protected ServiceProvider() { }
    public ServiceProvider(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static ServiceProvider() { Innovator.Client.Item.AddNullItem<ServiceProvider>(new ServiceProvider { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>active</c> property of the item</summary>
    [ArasName("active")]
    public IProperty_Boolean Active()
    {
      return this.Property("active");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>service_type</c> property of the item</summary>
    [ArasName("service_type")]
    public IProperty_Text ServiceType()
    {
      return this.Property("service_type");
    }
    /// <summary>Retrieve the <c>url</c> property of the item</summary>
    [ArasName("url")]
    public IProperty_Text Url()
    {
      return this.Property("url");
    }
  }
}