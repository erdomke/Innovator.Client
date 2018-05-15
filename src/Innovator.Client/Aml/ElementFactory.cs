using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// Class for generating mutable AML objects
  /// </summary>
  /// <remarks>
  /// <para>An <see cref="ElementFactory"/> is used to create AML objects (Items, properties, 
  /// attributes, and more).  A 
  /// <a href="https://en.wikipedia.org/wiki/Factory_method_pattern">factory</a> is needed to 
  /// create these objects because the objects need to know how to serialize and deserialize base 
  /// .Net types (e.g. <see cref="DateTime"/>) according to the time zone and context of a 
  /// particular server connection.  You can get a <see cref="ElementFactory"/> from a 
  /// <see cref="IConnection"/> via the <see cref="IConnection.AmlContext"/> property.  
  /// Alternatively, if you are processing AML without an Aras connection (e.g. reading an AML 
  /// export file), you can easily get an <see cref="ElementFactory"/> in from the 
  /// <see cref="ElementFactory.Local"/> or the <see cref="ElementFactory.Utc"/> static properties
  /// for a factory using either the local or the UTC time zone respectively.</para>
  /// <para>To create an AML structure (that you can easily modify) with an 
  /// <see cref="ElementFactory"/>, you can either start with a parameter substituted AML string,
  /// or by creating the AML objects directly</para>
  /// <code lang="C#">
  /// var aml = conn.AmlContext;
  /// var query = aml.FromXml(
  /// @"&lt;Item type='Part' action='get'&gt;
  ///   &lt;classification&gt;@0&lt;/classification&gt;
  ///   &lt;created_on condition='lt'&gt;@1&lt;/created_on&gt;
  /// &lt;/Item&gt;", classification, DateTime.Now.AddMinutes(-20)).AssertItem();
  /// query.State().Set("Preliminary");
  /// </code>
  /// <para>- OR -</para>
  /// <code lang="C#">
  /// var aml = conn.AmlContext;
  /// var query = aml.Item(aml.Type("Part"), aml.Action("get")
  ///   , aml.Classification(classification)
  ///   , aml.CreatedOn(aml.Condition(Condition.LessThan), DateTime.Now.AddMinutes(-20))
  ///   , aml.Property("state", "Preliminary")
  /// );
  /// return conn.Apply(query.ToAml()).Items();
  /// </code>
  /// <para>When used in the second manner, the API should feel very similar to creating 
  /// LINQ-to-XML objects.  However, using <see cref="ElementFactory"/> should be preferred as, 
  /// unlike LINQ, it properly serializes base types and handles time zone conversions</para>
  /// <code lang="C#">
  /// var query = new XElement(new XAttribute("type", "Part"), new XAttribute("action", "get")
  ///   , new XElement("classification", classification)
  ///   , new XElement("created_on", new XAttribute("condition", "lt"), DateTime.Now.AddMinutes(-20).ToString("s")
  ///   , new XElement("state", "Preliminary")
  /// );
  /// </code>
  /// </remarks>
  /// <example>
  /// <code lang="C#">
  /// var aml = conn.AmlContext;
  /// // --- OR ---
  /// var aml = ElementFactory.Local;
  ///
  /// IItem myItem = aml.Item(aml.Type(myType), aml.Action(myAction));
  /// IResult myResult = aml.Result(resultText);
  /// ServerException = aml.ServerException(errorMessage);
  /// </code>
  /// </example>
  public class ElementFactory
  {

    /// <summary>
    /// Context for serializing/deserializing native types (e.g. <c>DateTime</c>, <c>double</c>, <c>boolean</c>, etc.)
    /// </summary>
    public IServerContext LocalizationContext { get; }

    /// <summary>
    /// Gets the factory for creating strongly-typed items.
    /// </summary>
    /// <value>
    /// The factory for creating strongly-typed items.
    /// </value>
    public IItemFactory ItemFactory { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementFactory"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="itemFactory">The item factory.</param>
    public ElementFactory(IServerContext context, IItemFactory itemFactory = null)
    {
      LocalizationContext = context;
      ItemFactory = itemFactory ?? Factory.DefaultItemFactory;
    }

    /// <summary>
    /// Formats an AML string by substituting the @0 style parameters with the
    /// arguments specified.
    /// </summary>
    /// <param name="format">Query to format</param>
    /// <param name="args">Arguments to substitute into the query</param>
    public string FormatAml(string format, params object[] args)
    {
      var sub = new ParameterSubstitution();
      sub.AddIndexedParameters(args);
      return sub.Substitute(format, LocalizationContext);
    }

    /// <summary>
    /// Return a result from an AML string by substituting the @0 style parameters
    /// with the arguments specified.
    /// </summary>
    /// <param name="xml">Query to format</param>
    /// <param name="args">Arguments to substitute into the query</param>
    public IResult FromXml(string xml, params object[] args)
    {
      using (var writer = new ResultWriter(this, null, null))
      {
        var sub = new ParameterSubstitution();
        sub.AddIndexedParameters(args);
        sub.Substitute(xml, LocalizationContext, writer);
        return writer.Result;
      }
    }

    /// <summary>Return a result from a <see cref="Command"/></summary>
    /// <param name="aml">XML data</param>
    public IResult FromXml(Command aml)
    {
      using (var writer = new ResultWriter(this, null, null))
      {
        aml.ToNormalizedAml(LocalizationContext, writer);
        return writer.Result;
      }
    }

    /// <summary>
    /// Return a result from a method which writes to an <see cref="XmlWriter"/>
    /// </summary>
    /// <param name="writer">The callback to write to a writer.</param>
    public IResult FromXml(Action<XmlWriter> writer)
    {
      using (var w = new ResultWriter(this, null, null))
      {
        writer(w);
        return w.Result;
      }
    }

    /// <summary>Return a result from a stream</summary>
    /// <param name="xml">XML data</param>
    public IResult FromXml(Stream xml)
    {
      var xmlStream = xml as IXmlStream;
      using (var xmlReader = (xmlStream == null ? XmlReader.Create(xml) : xmlStream.CreateReader()))
      {
        return FromXml(xmlReader);
      }
    }

    /// <summary>Return a result from an AML text reader</summary>
    /// <param name="xml">XML data</param>
    public IResult FromXml(TextReader xml)
    {
      using (var xmlReader = XmlReader.Create(xml))
      {
        return FromXml(xmlReader);
      }
    }

    /// <summary>Return a result from an XML reader</summary>
    /// <param name="xml">XML data</param>
    public IResult FromXml(XmlReader xml)
    {
      return (IResult)FromXml(xml, null, null);
    }

    /// <summary>Return a result from an AML string indicating that it is the result of a query performed on a specific connection</summary>
    /// <param name="xml">XML data</param>
    /// <param name="query">The original query which produced the <paramref name="xml"/> response</param>
    /// <param name="conn">The connection which was queried</param>
    public IReadOnlyResult FromXml(string xml, Command query, IConnection conn)
    {
      using (var strReader = new StringReader(xml))
      using (var xmlReader = XmlReader.Create(strReader))
      {
        return FromXml(xmlReader, query, conn == null ? null : conn.Database);
      }
    }

    /// <summary>Return a result from an AML stream indicating that it is the result of a query performed on a specific connection</summary>
    /// <param name="xml">XML data</param>
    /// <param name="query">The original query which produced the <paramref name="xml"/> response</param>
    /// <param name="conn">The connection which was queried</param>
    public IReadOnlyResult FromXml(Stream xml, Command query, IConnection conn)
    {
      var xmlStream = xml as IXmlStream;
      using (var xmlReader = (xmlStream == null ? XmlReader.Create(xml) : xmlStream.CreateReader()))
      {
        return FromXml(xmlReader, query, conn?.Database);
      }
    }

    /// <summary>Return a result from an XmlReader indicating that it is the result of a query performed on a specific connection</summary>
    /// <param name="xml">XML data</param>
    /// <param name="query">The original query which produced the <paramref name="xml"/> response</param>
    /// <param name="database">The name of the database which was queried</param>
    public IReadOnlyResult FromXml(XmlReader xml, Command query, string database)
    {
      var writer = new ResultWriter(this, database, query);
      xml.CopyTo(writer);
      return writer.Result;
    }

    /// <summary>Return a result from an XmlReader indicating that it is the result of a query performed on a specific connection</summary>
    /// <param name="xml">XML data</param>
    /// <param name="query">The original query which produced the <paramref name="xml"/> response</param>
    /// <param name="database">The name of the database which was queried</param>
    /// <param name="readOnly">Whether or not the data coming back should be read-only</param>
    internal IReadOnlyResult FromXml(XmlReader xml, Command query, string database, bool readOnly)
    {
      var writer = new ResultWriter(this, database, query, readOnly);
      xml.CopyTo(writer);
      return writer.Result;
    }

    /// <summary>Create a new action attribute tag</summary>
    /// <remarks>action [String] The name of the Method (or Built in Action Method) to apply to the Item.</remarks>
    public IAttribute Action(string value)
    {
      return new Attribute("action", value);
    }
    /// <summary>Create a new <c>AML</c> tag (for use with the ApplyAML method)</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IElement Aml(params object[] content)
    {
      return new AmlElement(this, "AML", content);
    }
    /// <summary>Create a logical <c>or</c> AML tag used with 'get' queries</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public ILogical And(params object[] content)
    {
      return new Logical(this, "and", content);
    }
    /// <summary>Create a new attribute tag with the specified name</summary>
    public IAttribute Attribute(string name)
    {
      return new Attribute(name);
    }
    /// <summary>Create a new attribute tag with the specified name and value</summary>
    public IAttribute Attribute(string name, object value)
    {
      return new Attribute(name, value);
    }
    /// <summary>Create a new <c>classification</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty Classification(params object[] content)
    {
      return new Property("classification", content);
    }
    /// <summary>Create a new <c>condition</c> attribute</summary>
    public IAttribute Condition(Condition value)
    {
      return new Attribute("condition", value);
    }
    /// <summary>Create a new <c>config_id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty ConfigId(params object[] content)
    {
      return new Property("config_id", content);
    }
    /// <summary>Create a new <c>created_by_id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty CreatedById(params object[] content)
    {
      return new Property("created_by_id", content);
    }
    /// <summary>Create a new <c>created_on</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty CreatedOn(params object[] content)
    {
      return new Property("created_on", content);
    }
    /// <summary>Create a new <c>css</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty Css(params object[] content)
    {
      return new Property("css", content);
    }
    /// <summary>Create a new <c>current_state</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty CurrentState(params object[] content)
    {
      return new Property("current_state", content);
    }
    /// <summary>Create a new doGetItem attribute tag</summary>
    /// <remarks>doGetItem [Boolean] If 0 then do not perform a final get action on the Item after the server performed that action as defined by the action attribute. Default is 1.</remarks>
    public IAttribute DoGetItem(bool value)
    {
      return new Attribute("doGetItem", value);
    }
    /// <summary>Create a generic AML tag given a name and the content</summary>
    /// <param name="name">The local name of the element</param>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IElement Element(string name, params object[] content)
    {
      return new AmlElement(this, name, content);
    }
    /// <summary>Create a new <c>generation</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty Generation(params object[] content)
    {
      return new Property("generation", content);
    }
    /// <summary>Create a new where <c>id</c> attribute</summary>
    public IAttribute Id(string value)
    {
      return new Attribute("id", value);
    }
    /// <summary>Create a new where <c>id</c> attribute</summary>
    public IAttribute Id(Guid? value)
    {
      return new Attribute("id", value);
    }
    /// <summary>Create a new <c>idlist</c> attribute</summary>
    public IAttribute IdList(string value)
    {
      return new Attribute("idlist", value);
    }
    /// <summary>Create a new <c>idlist</c> attribute</summary>
    public IAttribute IdList(IEnumerable<string> values)
    {
      return new Attribute("idlist", values.GroupConcat(","));
    }
    /// <summary>Create a new <c>idlist</c> attribute</summary>
    public IAttribute IdList(IEnumerable<Guid> values)
    {
      return new Attribute("idlist", values.GroupConcat(",", i => i.ToArasId()));
    }
    /// <summary>Create a new <c>id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty IdProp(params object[] content)
    {
      return new Property("id", content);
    }
    /// <summary>Create a new <c>is_current</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty IsCurrent(params object[] content)
    {
      return new Property("is_current", content);
    }
    /// <summary>Create a new <c>is_null</c> attribute</summary>
    public IAttribute IsNull(bool value)
    {
      return new Attribute("is_null", value);
    }
    /// <summary>Create a new <c>is_released</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty IsReleased(params object[] content)
    {
      return new Property("is_released", content);
    }
    /// <summary>Create a new <c>Item</c> AML tag</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IItem Item(params object[] content)
    {
      var type = content
        .OfType<IReadOnlyAttribute>()
        .FirstOrDefault(a => a.Name == "type");
      if (type != null)
      {
        var result = ItemFactory.NewItem(this, type.Value);
        if (result != null)
        {
          result.Add(content);
          return result;
        }
      }

      return new Item(this, content);
    }
    /// <summary>Create a new <c>keyed_name</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty KeyedName(params object[] content)
    {
      return new Property("keyed_name", content);
    }
    /// <summary>Create a new <c>locked_by_id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty LockedById(params object[] content)
    {
      return new Property("locked_by_id", content);
    }
    /// <summary>Create a new <c>major_rev</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty MajorRev(params object[] content)
    {
      return new Property("major_rev", content);
    }
    /// <summary>Create a new <c>managed_by_id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty ManagedById(params object[] content)
    {
      return new Property("managed_by_id", content);
    }
    /// <summary>Create a new <c>maxRecords</c> attribute</summary>
    public IAttribute MaxRecords(int value)
    {
      return new Attribute("maxRecords", value);
    }
    /// <summary>Create a new <c>minor_rev</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty MinorRev(params object[] content)
    {
      return new Property("minor_rev", content);
    }
    /// <summary>Create a new <c>modified_by_id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty ModifiedById(params object[] content)
    {
      return new Property("modified_by_id", content);
    }
    /// <summary>Create a new <c>modified_on</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty ModifiedOn(params object[] content)
    {
      return new Property("modified_on", content);
    }
    /// <summary>Create a new <c>new_version</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty NewVersion(params object[] content)
    {
      return new Property("new_version", content);
    }
    /// <summary>Creates a 'No items found' exception</summary>
    /// <param name="type">The ItemType name for the item which couldn't be found</param>
    /// <param name="query">The AML query which didn't return any results</param>
    public NoItemsFoundException NoItemsFoundException(string type, Command query)
    {
      return new NoItemsFoundException(this, type, query);
    }
    /// <summary>Creates a 'No items found' exception</summary>
    public NoItemsFoundException NoItemsFoundException(string message)
    {
      return new NoItemsFoundException(message);
    }
    /// <summary>Creates a 'No items found' exception</summary>
    public NoItemsFoundException NoItemsFoundException(string message, Exception innerException)
    {
      return new NoItemsFoundException(message, innerException);
    }
    /// <summary>Create a logical <c>not</c> AML tag used with 'get' queries</summary>
    public ILogical Not(params object[] content)
    {
      return new Logical(this, "not", content);
    }
    /// <summary>Create a new <c>not_lockable</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty NotLockable(params object[] content)
    {
      return new Property("not_lockable", content);
    }
    /// <summary>Create a logical <c>or</c> AML tag used with 'get' queries</summary>
    public ILogical Or(params object[] content)
    {
      return new Logical(this, "or", content);
    }
    /// <summary>Create a new <c>orderBy</c> attribute</summary>
    public IAttribute OrderBy(string value)
    {
      return new Attribute("orderBy", value);
    }
    /// <summary>Create a new <c>owned_by_id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty OwnedById(params object[] content)
    {
      return new Property("owned_by_id", content);
    }
    /// <summary>Create a new <c>page</c> attribute</summary>
    public IAttribute Page(int value)
    {
      return new Attribute("page", value);
    }
    /// <summary>Create a new <c>pagesize</c> attribute</summary>
    public IAttribute PageSize(int value)
    {
      return new Attribute("pagesize", value);
    }
    /// <summary>Create a new <c>permission_id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty PermissionId(params object[] content)
    {
      return new Property("permission_id", content);
    }
    /// <summary>Create a new property tag with the specified name</summary>
    /// <param name="name">Name of the property to create</param>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty Property(string name, params object[] content)
    {
      return new Property(name, content);
    }
    /// <summary>Create a new <c>queryDate</c> attribute</summary>
    public IAttribute QueryDate(DateTime value)
    {
      return new Attribute("queryDate", value);
    }
    /// <summary>Create a new <c>queryType</c> attribute</summary>
    public IAttribute QueryType(QueryType value)
    {
      return new Attribute("queryType", value);
    }
    /// <summary>Create a new <c>related_expand</c> attribute</summary>
    public IAttribute RelatedExpand(bool value)
    {
      return new Attribute("related_expand", value);
    }
    /// <summary>Create a new <c>related_id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty RelatedId(params object[] content)
    {
      return new Property("related_id", content);
    }
    /// <summary>Create a new <c>Relationships</c> tag</summary>
    public IRelationships Relationships(params object[] content)
    {
      return new Relationships(content);
    }
    /// <summary>Create a new <c>Result</c> AML tag</summary>
    public IResult Result(params object[] content)
    {
      var result = new Result(this);
      if (content?.Length < 1)
      {
        // do nothing
      }
      else if (content.Length == 1 && content[0] is ServerException)
      {
        result.Exception = (ServerException)content[0];
      }
      else if (content.OfType<IReadOnlyItem>().Any())
      {
        foreach (var item in content.OfType<IReadOnlyItem>())
        {
          result.AddReadOnly(item);
        }
      }
      else if (content.OfType<IEnumerable>().Any(e => e.OfType<IReadOnlyItem>().Any()))
      {
        foreach (var item in content.OfType<IEnumerable>().SelectMany(e => e.OfType<IReadOnlyItem>()))
        {
          result.AddReadOnly(item);
        }
      }
      else if (content.Length == 1)
      {
        result.Value = LocalizationContext.Format(content[0]);
      }
      else
      {
        throw new NotSupportedException();
      }
      return result;
    }
    /// <summary>Create a new <c>select</c> attribute</summary>
    public IAttribute Select(string value)
    {
      return new Attribute("select", value);
    }
    /// <summary>Create a new <c>select</c> attribute</summary>
    public IAttribute Select(params SelectNode[] properties)
    {
      return new Attribute("select", SelectNode.ToString(properties));
    }
    /// <summary>Create a new <c>serverEvents</c> attribute</summary>
    public IAttribute ServerEvents(bool value)
    {
      return new Attribute("serverEvents", value);
    }
    /// <summary>Create a new server exception</summary>
    public ServerException ServerException(string message)
    {
      return new ServerException(message);
    }
    /// <summary>Create a new server exception</summary>
    public ServerException ServerException(Exception exception, bool serializeStackTrace = false)
    {
      return new ServerException(exception, serializeStackTrace);
    }
    /// <summary>Create a new server exception</summary>
    public ServerException ServerException(string message, Exception innerException)
    {
      return new ServerException(message, innerException);
    }
    /// <summary>Create a new <c>state</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty State(params object[] content)
    {
      return new Property("state", content);
    }
    /// <summary>Create a new <c>source_id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty SourceId(params object[] content)
    {
      return new Property("source_id", content);
    }
    /// <summary>Create a new <c>team_id</c> property</summary>
    /// <param name="content">The initial content of the elment (attributes, elements, or values)</param>
    public IProperty TeamId(params object[] content)
    {
      return new Property("team_id", content);
    }
    /// <summary>Create a new <c>type</c> attribute</summary>
    /// <remarks>type [String] The ItemType name for which the Item is an instance.</remarks>
    public IAttribute Type(string value)
    {
      return new Attribute("type", value);
    }
    /// <summary>Create a new <c>typeId</c> attribute</summary>
    public IAttribute TypeId(string value)
    {
      return new Attribute("typeId", value);
    }
    /// <summary>Create a new validation exception</summary>
    /// <param name="message">Exception message</param>
    /// <param name="item">Item being validated</param>
    /// <param name="properties">Properties with errors</param>
    public ValidationException ValidationException(string message, IReadOnlyItem item, params string[] properties)
    {
      return new ValidationException(message, item, properties);
    }
    /// <summary>Create a new validation exception</summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    /// <param name="item">Item being validated</param>
    /// <param name="properties">Properties with errors</param>
    public ValidationException ValidationException(string message, Exception innerException, IReadOnlyItem item, params string[] properties)
    {
      return new ValidationException(message, innerException, item, properties);
    }
    /// <summary>Create a new validation exception</summary>
    /// <param name="message">Exception message</param>
    /// <param name="item">Item being validated</param>
    /// <param name="report">HTML report to present to the user</param>
    public ValidationReportException ValidationException(string message, IReadOnlyItem item, string report)
    {
      return new ValidationReportException(message, item, report);
    }
    /// <summary>Create a new validation exception</summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    /// <param name="item">Item being validated</param>
    /// <param name="report">HTML report to present to the user</param>
    public ValidationReportException ValidationException(string message, Exception innerException, IReadOnlyItem item, string report)
    {
      return new ValidationReportException(message, innerException, item, report);
    }
    /// <summary>Create a new <c>where</c> attribute</summary>
    /// <remarks>where [String] Used instead of the id attribute to specify the WHERE clause for the search criteria. Include the table name with the column name using the dot notation: where="[user].first_name like 'Tom%'"</remarks>
    public IAttribute Where(string value)
    {
      return new Attribute("where", value);
    }

    /// <summary>
    /// Calculate the MD5 checksum of the securely stored value
    /// </summary>
    public string CalcSha256(SecureToken value)
    {
      return value.UseBytes((ref byte[] b) => Sha256.ComputeHash(b)).ToLowerInvariant();
    }

    /// <summary>
    /// Calculate the Sha256 checksum of the <c>string</c> value
    /// </summary>
    public string CalcSha256(string value)
    {
      return Sha256.ComputeHash(Utils.AsciiGetBytes(value)).ToLowerInvariant();
    }

    /// <summary>
    /// Calculate the MD5 checksum of the securely stored value
    /// </summary>
    public string CalcMd5(SecureToken value)
    {
      return value.UseBytes((ref byte[] b) => MD5.ComputeHash(b)).ToLowerInvariant();
    }

    /// <summary>
    /// Calculate the MD5 checksum of the <c>string</c> value
    /// </summary>
    public string CalcMd5(string value)
    {
      return MD5.ComputeHash(Utils.AsciiGetBytes(value)).ToLowerInvariant();
    }

    /// <summary>Generate a new GUID id</summary>
    public string NewId()
    {
      return Guid.NewGuid().ToString("N").ToUpperInvariant();
    }

    private static readonly ElementFactory _local = new ElementFactory(new ServerContext(false));
    private static readonly ElementFactory _utc = new ElementFactory(new ServerContext(true));

    /// <summary>Return an <see cref="ElementFactory"/> using the local time zone and culture</summary>
    public static ElementFactory Local { get { return _local; } }

    /// <summary>Return an <see cref="ElementFactory"/> using the UTC time zone and current culture</summary>
    public static ElementFactory Utc { get { return _utc; } }
  }
}
