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
    public static readonly string XPathFault = "/*[local-name()='Envelope' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]/*[local-name()='Body' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]/*[local-name()='Fault' and (namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/' or namespace-uri()='')]";
    public static readonly string XPathResult = "//Result";
    public static readonly string XPathResultItem = XPathResult + "/Item";

    private Innovator _innovator;

    public override ElementFactory AmlContext => _innovator;
    public override bool Exists => dom != null;
    public override string Name => "Item";
    public override IElement Parent { get; set; }
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

    public IProperty Property(string name)
    {
      return Property(name, null);
    }

    public IProperty Property(string name, string lang)
    {
      if (!Exists)
        return Client.Property.NullProp;
      var elem = GetPropertyForLanguage(name, lang);
      return elem == null ? new Property(this, name) : new Property(this, elem);
    }

    public IRelationships Relationships()
    {
      var relElem = ItemElement("Relationships");
      return new Relationships((Innovator)AmlContext, this, relElem);
    }

    public IEnumerable<IItem> Relationships(string type)
    {
      var relElem = ItemElement("Relationships");
      return new Relationships((Innovator)AmlContext, this, relElem, type);
    }

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

    public IReadOnlyItem AssertItem(string type = null)
    {
      var elem = AssertXml();
      if (!string.IsNullOrEmpty(type) && elem?.GetAttribute("type") != type)
        throw new InvalidOperationException(string.Format("An item of type '{0}' was found while an item of type '{1}' was expected.", elem?.GetAttribute("type"), type));
      return this;
    }

    public IEnumerable<IReadOnlyItem> AssertItems()
    {
      var items = Items();
      if (!items.Any())
        throw AmlContext.NoItemsFoundException("?", default(Command));
      return items;
    }

    public IReadOnlyResult AssertNoError()
    {
      var ex = Exception;
      if (ex != null)
        throw ex;
      return this;
    }

    public IEnumerable<IReadOnlyItem> Items()
    {
      if (!Exists)
        return Enumerable.Empty<IReadOnlyItem>();

      if (Xml != null)
        return Enumerable.Repeat((IReadOnlyItem)this, 1);

      if (nodeList != null)
        return nodeList.OfType<XmlElement>().Select(e => (IReadOnlyItem)new Item((Innovator)AmlContext, e));

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
