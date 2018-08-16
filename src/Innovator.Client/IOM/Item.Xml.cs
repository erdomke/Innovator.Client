#if XMLLEGACY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Innovator.Server;

namespace Innovator.Client.IOM
{
  public partial class Item : Element, IItem, IReadOnlyResult, ISingleItemContext
  {
    /// <summary>
    /// XPath to the root <c>&lt;Fault&gt;</c> node in case the instance represents an "error" item.
    /// </summary>
    /// <remarks>
    /// It's recommended to use get\setErrorXXX() methods (e.g. <see cref="getErrorDetail"/> or <see cref="setErrorDetail(string)"/>, etc.) in order to get\set fault information.
    /// </remarks>
    /// <example>
    /// <code lang="C#"><![CDATA[var response = myitem.apply();
    /// if (response.isError())
    /// {
    ///   var faultNode = response.dom.SelectNodes(Item.XPathFault);
    ///   // Do something
    /// }]]>
    /// </code>
    /// </example>
    public static readonly string XPathFault = "/*[local-name()='Envelope' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]/*[local-name()='Body' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]/*[local-name()='Fault' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]";

    /// <summary>
    /// XPath to the <c>&lt;Result&gt;</c> tag in server response XML.
    /// </summary>
    /// <example>
    /// <code lang="C#"><![CDATA[var response = myitem.apply();
    /// if (!response.isError())
    /// {
    ///   var resultNode = response.dom.SelectSingleNode(Item.XPathResult);
    ///   // Do something
    /// }]]>
    /// </code>
    /// </example>
    public static readonly string XPathResult = "//Result";

    /// <summary>
    /// XPath to the top-level <c>&lt;Item&gt;</c> tag(s) in item's internal AML.
    /// </summary>
    /// <example>
    /// <code lang="C#"><![CDATA[var response = myitem.apply();
    /// if (!response.isError())
    /// {
    ///   var itemNodes = response.dom.SelectNodes(Item.XPathResultItem);
    ///   // Do something
    /// }]]>
    /// </code>
    /// </example>
    public static readonly string XPathResultItem = XPathResult + "/Item";

    private Innovator _innovator;

    /// <summary>
    /// Retrieve the context used for rendering primitive values
    /// </summary>
    public override ElementFactory AmlContext => _innovator;

    /// <summary>
    /// Returns <c>true</c> if this element actually exists in the underlying AML,
    /// otherwise, returns <c>false</c> to indicate that the element is just a null placeholder
    /// put in place to reduce unnecessary null reference checks
    /// </summary>
    public override bool Exists => dom != null;

    /// <summary>
    /// Local XML name of the element
    /// </summary>
    public override string Name => "Item";

    /// <summary>
    /// Retrieve the parent element
    /// </summary>
    public override IElement Parent { get; set; }

    /// <summary>
    /// String value of the element
    /// </summary>
    public override string Value
    {
      get
      {
        if (dom == null)
          return null;

        if (Xml == null)
          return dom.SelectSingleNode(XPathResult)?.InnerText;

        return Id();
      }
    }

    /// <summary>
    /// Return an exception (if there is one), otherwise, return <c>null</c>
    /// </summary>
    public ServerException Exception
    {
      get
      {
        if (dom?.SelectSingleNode(XPathFault) != null
          || dom?.SelectSingleNode("//CompileMethodResponse/Result/status")?.InnerText?.StartsWith("ERROR: ") == true)
        {
          return AmlContext.FromXml(dom).Exception;
        }
        return null;
      }
    }

    /// <summary>
    /// Get messages (such as permissions warnings) from the database
    /// </summary>
    public IReadOnlyElement Message
    {
      get
      {
        var node = dom?.SelectSingleNode("/*[local-name()='Envelope' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]/*[local-name()='Body' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]/Message") as XmlElement;
        if (node == null)
          return Client.AmlElement.NullElem;
        return new AmlElement()
        {
          Xml = node
        };
      }
    }

    IReadOnlyItem ISingleItemContext.Item => this;

    IServerConnection IContext.Conn => (IServerConnection)((Innovator)AmlContext).getConnection();

    internal Item(Innovator innovator, Element parent)
    {
      _innovator = innovator;
      Parent = parent;
    }

    internal Item(Innovator innovator, XmlElement element)
    {
      _innovator = innovator;
      node = element;
      dom = element?.OwnerDocument;
    }

    internal Item(Innovator innovator, XmlDocument document)
    {
      _innovator = innovator;
      dom = document;
      InitNodes();
    }

    private void InitNodes()
    {
      var firstItem = dom.SelectSingleNode("//Item") as XmlElement;
      if (firstItem?.ParentNode is XmlElement parent)
      {
        var xmlNodeList = parent.SelectNodes("./Item");
        if (xmlNodeList.Count == 0)
        {
          node = null;
          nodeList = null;
        }
        else if (xmlNodeList.Count > 1)
        {
          node = null;
          nodeList = xmlNodeList;
        }
        else
        {
          node = (XmlElement)xmlNodeList[0];
          nodeList = null;
        }
      }
      else
      {
        node = firstItem;
        nodeList = null;
      }
    }

    IItem IReadOnlyItem.Clone()
    {
      var writer = new ResultWriter(this.AmlContext, null, null);
      Xml.WriteTo(writer);
      return writer.Result.AssertItem();
    }

    /// <summary>
    /// The ID of the item as retrieved from either the attribute or the property
    /// </summary>
    /// <returns></returns>
    public string Id()
    {
      var attr = AssertXml().GetAttribute("id");
      if (!string.IsNullOrEmpty(attr))
        return attr;
      var prop = ItemElement("id");
      if (prop != null && !string.IsNullOrEmpty(prop.InnerText) && (prop.GetAttributeNode("condition")?.Value ?? "eq") == "eq")
        return prop.InnerText;
      return null;
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
    /// <remarks>
    /// If the property does not exist, a non-null object will be returned that has an <c>Exists</c> member which will return <c>false</c>
    /// </remarks>
    public IProperty Property(string name, string lang)
    {
      if (!Exists)
        return Client.Property.NullProp;
      var elem = GetPropertyForLanguage(name, lang);
      return elem == null ? new Property(this, name) : new Property(this, elem);
    }

    /// <summary>
    /// Returns the set of relationships associated with this item
    /// </summary>
    /// <returns></returns>
    public IRelationships Relationships()
    {
      var relElem = ItemElement("Relationships");
      return new Relationships((Innovator)AmlContext, this, relElem);
    }

    /// <summary>
    /// Returns the set of relationships associated with this item of the specified type
    /// </summary>
    /// <param name="type">Name of the ItemType for the relationships you wish to retrieve</param>
    /// <returns></returns>
    public IEnumerable<IItem> Relationships(string type)
    {
      var relElem = ItemElement("Relationships");
      return new Relationships((Innovator)AmlContext, this, relElem, type);
    }

    /// <summary>
    /// The type of the item as retrieved from either the attribute or the property
    /// </summary>
    /// <returns></returns>
    public string TypeName()
    {
      return Xml.GetAttribute("type");
    }

    IReadOnlyProperty IReadOnlyItem.Property(string name)
    {
      return Property(name, null);
    }

    IReadOnlyProperty IReadOnlyItem.Property(string name, string lang)
    {
      return Property(name, lang);
    }

    IEnumerable<IReadOnlyItem> IReadOnlyItem.Relationships()
    {
      return (IReadOnlyRelationships)Relationships();
    }

    IEnumerable<IReadOnlyItem> IReadOnlyItem.Relationships(string type)
    {
      return ((IReadOnlyItem)this).Relationships()
        .Where(i => i.TypeName() == type);
    }

    private XmlElement GetPropertyForLanguage(string name, string lang)
    {
      if (string.IsNullOrEmpty(lang))
        return ItemElement(name);

      var xpath = default(string);
      if (AmlContext.LocalizationContext.LanguageCode == lang)
        xpath = $"./*[local-name()='{name}' and (namespace-uri()='http://www.aras.com/I18N' or name()='{name}') and @xml:lang='{lang}']";
      else
        xpath = $"./*[local-name()='{name}' and namespace-uri()='http://www.aras.com/I18N' and @xml:lang='{lang}']";
      return (XmlElement)AssertXml()?.SelectSingleNode(xpath, new XmlNamespaceManager(Xml.OwnerDocument.NameTable));
    }

    private XmlElement ItemElement(string name)
    {
      return AssertXml()?.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.LocalName == name);
    }

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
      var elem = AssertXml();
      if (!string.IsNullOrEmpty(type) && elem?.GetAttribute("type") != type)
        throw new InvalidOperationException(string.Format("An item of type '{0}' was found while an item of type '{1}' was expected.", elem?.GetAttribute("type"), type));
      return this;
    }

    /// <summary>
    /// Return an enumerable of items.  Throw an exception for any error including 'No items found'
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IReadOnlyItem> AssertItems()
    {
      var items = Items();
      if (!items.Any())
        throw AmlContext.NoItemsFoundException("?", default(Command));
      return items;
    }

    /// <summary>
    /// Do nothing other than throw an exception if there is an error other than 'No Items Found'
    /// </summary>
    /// <returns>
    /// The current <see cref="IReadOnlyResult" /> for chaining additional methods
    /// </returns>
    public IReadOnlyResult AssertNoError()
    {
      var ex = Exception;
      if (ex != null)
        throw ex;
      return this;
    }

    /// <summary>
    /// Return an enumerable of items.  Throw an exception if there is an error other than 'No Items Found'
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IReadOnlyItem> Items()
    {
      if (!Exists)
        return Enumerable.Empty<IReadOnlyItem>();

      if (Xml != null)
        return Enumerable.Repeat((IReadOnlyItem)this, 1);

      if (nodeList != null)
        return nodeList.OfType<XmlElement>().Select(e => (IReadOnlyItem)Element.Factory(e, this));

      var ex = Exception;
      if (ex != null && !(ex is NoItemsFoundException))
        throw ex;

      return Enumerable.Empty<IReadOnlyItem>();
    }

    protected override XmlElement AssertXml()
    {
      if (!Exists)
        return null;

      if (Xml != null)
        return Xml;

      if (nodeList != null)
        throw new InvalidOperationException("Multiple items were found when only one was expected.");

      var ex = Exception;
      if (ex != null)
        throw ex;

      throw AmlContext.NoItemsFoundException("?", default(Command));
    }

    /// <summary>
    /// To the aml.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="settings">The settings.</param>
    public override void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      if (Xml != null)
      {
        Xml.WriteTo(writer);
        return;
      }

      if (nodeList != null)
      {
        var parentItem = nodeList[0].SelectSingleNode("ancestor::Item");
        if (parentItem != null)
        {
          nodeList[0].ParentNode.WriteTo(writer);
          return;
        }
      }

      dom.WriteTo(writer);
      return;
    }
  }
}
#endif
