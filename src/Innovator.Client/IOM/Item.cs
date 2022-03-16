#if !XMLLEGACY
using Innovator.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Innovator.Client.IOM
{
  /// <summary>
  /// Wraps an AML object with a API which is compatible with 
  /// <a href="http://www.aras.com/support/documentation/DocumentView.aspx?file=./11.0%20SP9/Other%20Documentation/On-Line%20.NET%20API%20Guide.html">Aras's IOM</a>
  /// </summary>
  public partial class Item : IReadOnlyResult, IItem, Server.ISingleItemContext
  {
    public static readonly string XPathFault = "/*[local-name()='Envelope' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]/*[local-name()='Body' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]/*[local-name()='Fault' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]";
    public static readonly string XPathResult = "//Result";
    public static readonly string XPathResultItem = XPathResult + "/Item";

    private readonly IConnection _conn;
    private object _content;
    private readonly IElement _parent;

    internal Item(IConnection conn, params object[] args)
    {
      _conn = conn;
      var aml = _conn.AmlContext;
      var item = aml.Item();
      if (args.Length > 0)
        item.Add(args);
      item.Add(aml.Attribute("isNew", true), aml.Attribute("isTemp", true));
      _content = new List<IReadOnlyItem>() { item };
    }

    internal Item(IConnection conn, string result)
    {
      _conn = conn;
      _content = result;
    }

    internal Item(IConnection conn, ServerException ex)
    {
      _conn = conn;
      _content = ex;
    }

    internal Item(IConnection conn, IList<IReadOnlyItem> result, IElement parent)
    {
      _conn = conn;
      _content = result;
      _parent = parent;
    }

    internal Item(IConnection conn, IReadOnlyItem result)
    {
      _conn = conn;
      _content = new List<IReadOnlyItem>() { result };
    }

    internal Item(IConnection conn, IReadOnlyResult result)
    {
      _conn = conn;
      loadAML(result);
    }

    #region IReadOnlyResult    
    /// <summary>
    /// Return an exception (if there is one), otherwise, return <c>null</c>
    /// </summary>
    public ServerException Exception { get { return _content as ServerException; } }

    /// <summary>
    /// Get messages (such as permissions warnings) from the database
    /// </summary>
    public IReadOnlyElement Message { get { return AmlElement.NullElem; } }

    /// <summary>
    /// Return the string value of the result
    /// </summary>
    public string Value { get { return _content as string; } }

    /// <summary>
    /// Return a single item.  If that is not possible, throw an appropriate
    /// exception (e.g. the exception returned by the server where possible)
    /// </summary>
    /// <param name="type">If specified, throw an exception if the item doesn't have the specified type</param>
    /// <returns>
    /// A single <see cref="IReadOnlyItem" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IReadOnlyItem AssertItem(string type = null)
    {
      var exception = this.Exception;
      if (exception != null) throw exception;
      var items = _content as IEnumerable<IReadOnlyItem>;
      if (items == null)
        throw _conn.AmlContext.NoItemsFoundException("?", "");
      var item = items.Single(i => true, i => i < 1
          ? (Exception)NewNoItemsException()
          : new InvalidOperationException("Multiple items were found when only one was expected."));
      if (item != null && (string.IsNullOrEmpty(type) || item.TypeName() == type))
        return item;
      throw new InvalidOperationException(string.Format("An item of type '{0}' was found while an item of type '{1}' was expected.", item.Type().Value, type));
    }

    /// <summary>
    /// Return an enumerable of items.  Throw an exception for any error including 'No items found'
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IReadOnlyItem> AssertItems()
    {
      var exception = this.Exception;
      if (exception != null) throw exception;
      var items = _content as IEnumerable<IReadOnlyItem>;
      if (items == null)
        throw _conn.AmlContext.NoItemsFoundException("?", "");
      return items;
    }

    /// <summary>
    /// Do nothing other than throw an exception if present
    /// </summary>
    public IReadOnlyResult AssertNoError()
    {
      this.AssertNoError(false);
      return this;
    }

    /// <summary>
    /// Do nothing other than throw an exception if present, optionally ignoring <see cref="NoItemsFoundException"/>
    /// </summary>
    public IReadOnlyResult AssertNoError(bool ignoreNoItemsFound)
    {
      var exception = this.Exception;
      if (exception is NoItemsFoundException && ignoreNoItemsFound) return this;
      if (exception != null) throw exception;
      return this;
    }

    /// <summary>
    /// Return an enumerable of items.  Throw an exception if there is an error other than 'No Items Found'
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IReadOnlyItem> Items()
    {
      var exception = this.Exception;
      if (exception is NoItemsFoundException) return Enumerable.Empty<IReadOnlyItem>();
      if (exception != null) throw exception;
      var items = _content as IEnumerable<IReadOnlyItem>;
      return items ?? Enumerable.Empty<IReadOnlyItem>();
    }

    /// <summary>
    /// Write the node to the specified <see cref="XmlWriter" /> as AML
    /// </summary>
    /// <param name="writer"><see cref="XmlWriter" /> to write the node to</param>
    /// <param name="settings">Settings controlling how the node is written</param>
    public void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      var elems = (_content as IEnumerable)?.OfType<IReadOnlyElement>()?.ToArray();

      if (elems?.Length == 1)
      {
        elems[0].ToAml(writer, settings);
        return;
      }

      var ex = _content as ServerException;
      if (ex != null)
      {
        ex.ToAml(writer, settings);
      }
      else
      {
        writer.WriteStartElement("SOAP-ENV", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
        writer.WriteStartElement("SOAP-ENV", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
        writer.WriteStartElement("Result");
        var items = _content as IEnumerable<IReadOnlyItem>;
        if (items != null)
        {
          foreach (var item in items)
          {
            item.ToAml(writer, settings);
          }
        }
        else if (_content != null)
        {
          writer.WriteString(_conn.AmlContext.LocalizationContext.Format(_content));
        }
        writer.WriteEndElement();
        if (Message.Exists)
        {
          Message.ToAml(writer, settings);
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
      }
    }

    private ServerException NewNoItemsException()
    {
      return (this.Exception as NoItemsFoundException) ??
        _conn.AmlContext.NoItemsFoundException("?", "");
    }

    private T AssertSingle<T>()
    {
      var exception = this.Exception;
      if (exception != null) throw exception;
      var elems = (_content as IEnumerable)?.OfType<T>();
      if (elems == null || !elems.Any())
        throw _conn.AmlContext.NoItemsFoundException("?", "");
      return elems.Single(i => true, i => i < 1
        ? (Exception)NewNoItemsException()
        : new InvalidOperationException("Multiple items were found when only one was expected."));
    }
    #endregion

    #region IItem

    /// <summary>
    /// Retrieve the parent element
    /// </summary>
    public IElement Parent
    {
      get
      {
        var elems = (_content as IEnumerable)?.OfType<IElement>()?.ToArray();
        if (elems?.Length == 1)
          return elems[0].Parent;
        return AmlElement.NullElem;
      }
    }

    /// <summary>
    /// Retrieve the context used for rendering primitive values
    /// </summary>
    public ElementFactory AmlContext { get { return _conn.AmlContext; } }

    /// <summary>
    /// Returns <c>true</c> if this element actually exists in the underlying AML,
    /// otherwise, returns <c>false</c> to indicate that the element is just a null placeholder
    /// put in place to reduce unnecessary null reference checks
    /// </summary>
    public bool Exists
    {
      get
      {
        var exception = this.Exception;
        if (exception != null) throw exception;
        var elems = (_content as IEnumerable)?.Cast<IReadOnlyElement>();
        if (elems == null)
          return _content != null;
        return elems.All(e => e.Exists);
      }
    }

    /// <inheritdoc/>
    public string Name { get { return AssertSingle<IReadOnlyElement>().Name; } }

    /// <inheritdoc/>
    public string Prefix { get { return AssertSingle<IReadOnlyElement>().Prefix; } }

    /// <summary>
    /// Retrieve the parent element
    /// </summary>
    IReadOnlyElement IReadOnlyElement.Parent
    {
      get
      {
        var elems = (_content as IEnumerable)?.OfType<IReadOnlyElement>()?.ToArray();
        if (elems?.Length == 1)
          return elems[0].Parent;
        return AmlElement.NullElem;
      }
    }

    /// <summary>
    /// Returns a reference to the property with the specified name
    /// </summary>
    /// <param name="name">Name of the property</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item>
    ///     <description>If the property exists, a valid <see cref="IProperty" /> will be returned</description>
    ///   </item>
    ///   <item>
    ///     <description>If the property does not exists, a "null" <see cref="IProperty" /> will be returned where <see cref="IReadOnlyElement.Exists" /> = <c>false</c></description>
    ///   </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// If the property does not exist, a non-null object will be returned that has an <c>Exists</c> member which will return <c>false</c>
    /// </remarks>
    public IProperty Property(string name)
    {
      return Property(name, null);
    }

    /// <summary>
    /// Returns a reference to the property with the specified name and language
    /// </summary>
    /// <param name="name">Name of the property</param>
    /// <param name="lang">Language of the (multilingual) property</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item>
    ///     <description>If the property exists, a valid <see cref="IProperty" /> will be returned</description>
    ///   </item>
    ///   <item>
    ///     <description>If the property does not exists, a "null" <see cref="IProperty" /> will be returned where <see cref="IReadOnlyElement.Exists" /> = <c>false</c></description>
    ///   </item>
    /// </list>
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <remarks>
    /// If the property does not exist, a non-null object will be returned that has an <c>Exists</c> member which will return <c>false</c>
    /// </remarks>
    public IProperty Property(string name, string lang)
    {
      var elem = AssertSingle<IElement>();
      var item = elem as IItem;
      if (item != null)
        return item.Property(name, lang);

      var logical = elem as ILogical;
      if (logical == null)
        throw new InvalidOperationException(string.Format("Cannot get a property from an item of type '{0}'", elem.GetType().Name));

      return logical.Property(name, lang);
    }

    /// <summary>
    /// Returns the set of relationships associated with this item
    /// </summary>
    public IRelationships Relationships()
    {
      return AssertSingle<IItem>().Relationships();
    }

    /// <summary>
    /// Returns the set of relationships associated with this item of the specified type
    /// </summary>
    /// <param name="type">Name of the ItemType for the relationships you wish to retrieve</param>
    public IEnumerable<IItem> Relationships(string type)
    {
      return AssertSingle<IItem>().Relationships(type);
    }

    /// <summary>
    /// Retrieve the attribute with the specified name
    /// </summary>
    /// <param name="name">Name of the XML attribute</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item>
    ///     <description>If the property exists, a valid <see cref="IAttribute" /> will be returned</description>
    ///   </item>
    ///   <item>
    ///     <description>If the property does not exists, a "null" <see cref="IAttribute" /> will be returned where <see cref="IReadOnlyAttribute.Exists" /> = <c>false</c></description>
    ///   </item>
    /// </list>
    /// </returns>
    public IAttribute Attribute(string name)
    {
      return AssertSingle<IElement>().Attribute(name);
    }

    /// <summary>
    /// Retrieve all attributes specified for the element
    /// </summary>
    public IEnumerable<IAttribute> Attributes()
    {
      return AssertSingle<IElement>().Attributes();
    }

    /// <summary>
    /// Retrieve all child elements
    /// </summary>
    public IEnumerable<IElement> Elements()
    {
      return AssertSingle<IElement>().Elements();
    }

    /// <summary>
    /// Add new content to the element
    /// </summary>
    /// <param name="content"><see cref="IElement" />, <see cref="IAttribute" />, or <see cref="object" /> to add as a child of the element</param>
    /// <returns>
    /// The current element for chaining additional calls
    /// </returns>
    public IElement Add(object content)
    {
      return AssertSingle<IElement>().Add(content);
    }

    /// <summary>
    /// Remove the element from its parent
    /// </summary>
    public void Remove()
    {
      AssertSingle<IElement>().Remove();
    }

    /// <summary>
    /// Remove attributes from the element
    /// </summary>
    public void RemoveAttributes()
    {
      AssertSingle<IElement>().RemoveAttributes();
    }

    /// <summary>
    /// Remove child nodes from the element
    /// </summary>
    public void RemoveNodes()
    {
      AssertSingle<IElement>().RemoveNodes();
    }

    /// <summary>
    /// Creates a duplicate of the item object.  All properties (including the ID) are preserved
    /// </summary>
    /// <returns>
    /// A mutable copy of the current item
    /// </returns>
    public IItem Clone()
    {
      return AssertSingle<IReadOnlyItem>().Clone();
    }

    IReadOnlyProperty IReadOnlyItem.Property(string name)
    {
      return ((IReadOnlyItem)this).Property(name, null);
    }

    IReadOnlyProperty IReadOnlyItem.Property(string name, string lang)
    {
      var elem = AssertSingle<IReadOnlyElement>();
      var item = elem as IReadOnlyItem;
      if (item != null)
        return item.Property(name, lang);

      var logical = elem as IReadOnlyLogical;
      if (logical == null)
        throw new InvalidOperationException(string.Format("Cannot get a property from an item of type '{0}'", elem.GetType().Name));

      return logical.Property(name, lang);
    }

    IEnumerable<IReadOnlyItem> IReadOnlyItem.Relationships()
    {
      return AssertSingle<IReadOnlyItem>().Relationships();
    }

    IEnumerable<IReadOnlyItem> IReadOnlyItem.Relationships(string type)
    {
      return AssertSingle<IReadOnlyItem>().Relationships(type);
    }

    IReadOnlyAttribute IReadOnlyElement.Attribute(string name)
    {
      return AssertSingle<IReadOnlyElement>().Attribute(name);
    }

    IEnumerable<IReadOnlyAttribute> IReadOnlyElement.Attributes()
    {
      return AssertSingle<IReadOnlyElement>().Attributes();
    }

    IEnumerable<IReadOnlyElement> IReadOnlyElement.Elements()
    {
      return AssertSingle<IReadOnlyElement>().Elements();
    }

    /// <summary>
    /// The ID of the item as retrieved from either the attribute or the property
    /// </summary>
    public string Id()
    {
      return AssertItem().Id();
    }

    /// <summary>
    /// The type of the item as retrieved from either the attribute or the property
    /// </summary>
    public string TypeName()
    {
      return AssertItem().TypeName();
    }
    #endregion

    #region ISingleItemContext
    IReadOnlyItem ISingleItemContext.Item { get { return AssertItem(); } }
    IServerConnection IContext.Conn { get { return (IServerConnection)_conn; } }
    #endregion

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return this.ToAml();
    }

    /// <summary>
    /// Returns the AML of the root parent
    /// </summary>
    /// <returns>The AML of the root parent</returns>
    public string ToAmlRoot()
    {
      return this.ParentsAndSelf().Last().ToAml();
    }
  }
}
#endif
