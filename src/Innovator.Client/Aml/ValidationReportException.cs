using System;
using System.Collections.Generic;
using System.Linq;
#if SERIALIZATION
using System.Runtime.Serialization;
#endif
using System.Text;
using System.Xml;

namespace Innovator.Client
{
#if SERIALIZATION
  [Serializable]
#endif
  public class ValidationReportException : ServerException
  {
    public IReadOnlyItem Item
    {
      get
      {
        var item = _fault.ElementByName("detail").ElementByName("item");
        if (!item.Exists)
          return Innovator.Client.Item.GetNullItem<IReadOnlyItem>();
        var aml = _fault.AmlContext;
        return aml.Item(aml.Type(item.Attribute("type").Value), aml.Id(item.Attribute("id").Value));
      }
    }
    public string Report
    {
      get { return _fault.ElementByName("detail").ElementByName("error_resolution_report").Value; }
    }

    internal ValidationReportException(string message
      , IReadOnlyItem item, string report)
      : base(message, 1001)
    {
      CreateDetailElement(item, report);
    }
    internal ValidationReportException(string message, Exception innerException
      , IReadOnlyItem item, string report)
      : base(message, 1001, innerException)
    {
      CreateDetailElement(item, report);
    }
#if SERIALIZATION
    public ValidationReportException(SerializationInfo info, StreamingContext context)
      : base(info, context) { }
#endif
    internal ValidationReportException(Element fault, string database, Command query)
      : base(fault, database, query) { }

    private IElement CreateDetailElement(IReadOnlyItem item, string report)
    {
      var detail = _fault.ElementByName("detail");
      if (item != null)
      {
        detail.Add(new AmlElement(_fault.AmlContext, "item"
        , new Attribute("type", item.Type().Value)
        , new Attribute("id", item.Id())));
      }
      detail.Add(new AmlElement(_fault.AmlContext, "error_resolution_report", report));
      if (!detail.Exists)
        _fault.Add(detail);
      return detail;
    }
  }
}
