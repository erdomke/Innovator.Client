using System;
#if SERIALIZATION
using System.Runtime.Serialization;
#endif

namespace Innovator.Client
{
  /// <summary>
  /// Represents an exception that was returned from the server as a SOAP fault 
  /// indicating that no items were found
  /// </summary>
  /// <remarks>
  /// To create a new instance of this class, use <see cref="ElementFactory.NoItemsFoundException(string)"/>
  /// or one of the other overloads
  /// </remarks>
#if SERIALIZATION
  [Serializable]
#endif
  public class NoItemsFoundException : ServerException
  {
    internal NoItemsFoundException(ElementFactory factory, string type, Command query)
      : base("No items of type " + type + " found.", "0")
    {
      var queryString = "?";
      if (query != null)
        queryString = query.ToNormalizedAml(factory.LocalizationContext);

      var detail = CreateDetailElement();
      detail.Add(new AmlElement(_fault.AmlContext, "af:legacy_faultstring", "No items of type " + type + " found using the criteria: " + queryString));
      this._query = query;
    }
    internal NoItemsFoundException(string message)
      : base(message, "0")
    {
      CreateDetailElement();
    }
    internal NoItemsFoundException(string message, Exception innerException)
      : base(message, "0", innerException, false)
    {
      CreateDetailElement();
    }
    internal NoItemsFoundException(Element fault, string database, Command query)
      : base(fault, database, query) { }
#if SERIALIZATION
    public NoItemsFoundException(SerializationInfo info, StreamingContext context)
      : base(info, context) { }
#endif

    private IElement CreateDetailElement()
    {
      var detail = _fault.ElementByName("detail") as Element;
      if (!detail.Exists || string.IsNullOrEmpty(detail.ElementByName("legacy_detail").Value))
        detail.Add(new AmlElement(_fault.AmlContext, "af:legacy_detail", this.Message));
      return detail;
    }
  }
}
