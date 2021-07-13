using System;
#if SERIALIZATION
using System.Runtime.Serialization;
#endif
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// Represents an exception that was returned from the server as a SOAP fault.
  /// </summary>
  /// <remarks>
  /// To create a new instance of this class, use <see cref="ElementFactory.ServerException(string)"/>
  /// or one of the other overloads
  /// </remarks>
#if SERIALIZATION
  [Serializable]
#endif
  public class ServerException : Exception, IAmlNode
  {
    /// <summary>
    /// The name of the database where the excepton originated
    /// </summary>
    protected string _database;

    /// <summary>
    /// The AML fault element from the SOAP message
    /// </summary>
    protected Element _fault;

    /// <summary>
    /// The query which was executed when the error was returned
    /// </summary>
    protected Command _query;

    private string _className;

    /// <summary>
    /// Gets the name of the database where the excepton originated
    /// </summary>
    public string Database { get { return _database; } }

    /// <summary>
    /// Gets the AML fault element from the SOAP message
    /// </summary>
    public IElement Fault
    {
      get { return _fault; }
    }

    /// <summary>
    /// Gets or sets the (generally numeric) fault code.
    /// </summary>
    public string FaultCode
    {
      get { return _fault.ElementByName("faultcode").Value; }
      set { _fault.ElementByName("faultcode").Add(value); }
    }

    /// <summary>
    /// Gets the query which was executed when the error was returned
    /// </summary>
    public string Query
    {
      get
      {
        if (_query == null)
          return null;
        if (_fault == null)
          return _query.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
        return _query.ToNormalizedAml(_fault.AmlContext.LocalizationContext);
      }
    }

    internal ServerException(string message)
      : this(message, "1") { }
    internal ServerException(string message, Exception innerException)
      : this(message ?? innerException.Message, "1", innerException, false) { }
    internal ServerException(Exception innerException, bool serializeStackTrace)
      : this(innerException.Message, innerException.GetType().FullName, innerException, serializeStackTrace) { }

#if SERIALIZATION
    private const string FaultNodeEntry = "``FaultNode";
    private const string DatabaseEntry = "``Database";
    private const string QueryEntry = "``Query";

    public ServerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      var result = ElementFactory.Local.FromXml(info.GetString(FaultNodeEntry));
      _fault = result.Exception._fault;
      _database = info.GetString(DatabaseEntry);
      _query = info.GetString(QueryEntry);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      if (_fault != null)
      {
        if (_fault.Parent.Parent.Exists)
        {
          info.AddValue(FaultNodeEntry, _fault.ToAml());
        }
        else
        {
          info.AddValue(FaultNodeEntry, _fault.ToAml());
        }
      }
      info.AddValue(DatabaseEntry, this.Database);
      info.AddValue(QueryEntry, this.Query);
    }
#endif    
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerException"/> class.
    /// </summary>
    /// <param name="fault">The fault element.</param>
    /// <param name="database">The database where the exception originated.</param>
    /// <param name="query">The query which was executed when the error was returned.</param>
    public ServerException(Element fault, string database, Command query)
      : base(fault.ElementByName("faultstring").Value ?? "An unexpected error occurred")
    {
      _fault = fault;
      _database = database;
      _query = query;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="code">The fault code.</param>
    protected ServerException(string message, string code)
      : base(message)
    {
      CreateXml(message, code);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="code">The fault code.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="serializeStackTrace">Whether to serialize the stack trace</param>
    protected ServerException(string message, string code, Exception innerException, bool serializeStackTrace)
      : base(message, innerException)
    {
      CreateXml(message, code);
      if (serializeStackTrace)
      {
        var aml = ElementFactory.Local;
        _fault.Add(aml.Element("detail", aml.Element("af:legacy_faultactor", innerException.StackTrace)));
      }
    }

    internal ServerException SetDetails(string database, Command query)
    {
      _database = database;
      _query = query;
      return this;
    }

    /// <summary>
    /// Creates an <see cref="XmlReader"/> for reading through the exception SOAP data
    /// </summary>
    public XmlReader CreateReader()
    {
      return new AmlReader(this);
    }

    private void CreateXml(string message, string code)
    {
      var aml = ElementFactory.Local;
      _fault = aml.Element("SOAP-ENV:Fault", aml.Element("faultcode", code), aml.Element("faultstring", message)) as Element;
    }

    /// <summary>
    /// Renders the exception as an AML string
    /// </summary>
    /// <returns>An AML string</returns>
    public string ToAml()
    {
      using (var writer = new System.IO.StringWriter())
      using (var xml = XmlWriter.Create(writer, new XmlWriterSettings() { OmitXmlDeclaration = true }))
      {
        ToAml(xml, new AmlWriterSettings());
        xml.Flush();
        return writer.ToString();
      }
    }

    /// <summary>
    /// Write the node to the specified <see cref="XmlWriter" /> as AML
    /// </summary>
    /// <param name="writer"><see cref="XmlWriter" /> to write the node to</param>
    /// <param name="settings">Settings controlling how the node is written</param>
    public void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      writer.WriteStartElement("SOAP-ENV", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
      writer.WriteStartElement("SOAP-ENV", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
      writer.WriteStartElement("SOAP-ENV", "Fault", "http://schemas.xmlsoap.org/soap/envelope/");
      writer.WriteAttributeString("xmlns", "af", null, "http://www.aras.com/InnovatorFault");
      foreach (var elem in _fault.Elements())
      {
        elem.ToAml(writer, settings);
      }
      writer.WriteEndElement();
      writer.WriteEndElement();
      writer.WriteEndElement();
    }

    /// <summary>
    /// Creates a new result composed of this exception
    /// </summary>
    public IReadOnlyResult AsResult()
    {
      return new Result(ElementFactory.Local) { Exception = this };
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance consisting
    /// of the exception message and full stack trace
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      var result = base.ToString();

      var serverStack = _fault.Element("detail").Element("legacy_faultactor").Value;
      if (!string.IsNullOrEmpty(serverStack))
        result += Environment.NewLine + "[Server]" + Environment.NewLine + serverStack;

      return result;
    }

    private string GetClassName()
    {
      if (this._className == null)
      {
        this._className = this.GetType().ToString();
      }
      return this._className;
    }

#if XMLLEGACY    
    /// <summary>
    /// Returns a navigator for executing XPath against the element
    /// </summary>
    public IAmlXPath XPath(IReadOnlyResult elem)
    {
      return new AmlNavigator(elem);
    }
#endif
  }
}
