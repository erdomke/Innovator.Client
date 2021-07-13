#if XMLLEGACY
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client.IOM
{
  /// <summary>
  /// Represents an XML element in an AML structure.  This element could be an Item, property, result tag, or something else
  /// </summary>
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public abstract class Element : IElement
  {
    /// <summary>Retrieve the context used for rendering primitive values</summary>
    public virtual ElementFactory AmlContext
    {
      get
      {
        if (this.Parent != null)
          return this.Parent.AmlContext;
        return null;
      }
    }

    /// <summary>Returns <c>true</c> if this element actually exists in the underlying AML,
    /// otherwise, returns <c>false</c> to indicate that the element is just a null placeholder
    /// put in place to reduce unnecessary null reference checks</summary>
    public virtual bool Exists => Xml != null;

    /// <summary>Local XML name of the element</summary>
    public virtual string Name => Xml?.LocalName;

    /// <summary>Retrieve the parent element</summary>
    public abstract IElement Parent { get; set; }

    /// <summary>Retrieve the parent element</summary>
    IReadOnlyElement IReadOnlyElement.Parent => this.Parent;

    /// <summary>Namespace prefix of the element name</summary>
    public virtual string Prefix => Xml.Prefix;

    /// <summary>String value of the element</summary>
    public virtual string Value { get { return Xml?.ChildNodes.OfType<XmlElement>().Any() != false ? null : Xml.InnerText; } }

    internal XmlElement Xml { get; set; }

    private string DebuggerDisplay
    {
      get
      {
        if (!this.Exists)
          return "{null:" + this.GetType().Name + "}";
        return this.ToAml();
      }
    }

    public virtual IElement Add(object content)
    {
      if (content == null
#if DBDATA
        || content == DBNull.Value
#endif
        )
        return this;

      if (content is Element elem && elem.Xml != null)
      {
        Xml.AppendChild(Xml.OwnerDocument.ImportNode(elem.Xml, true));
        return this;
      }

      if (content is Item item && item.nodeList != null)
      {
        foreach (var node in item.nodeList.OfType<XmlNode>())
          Xml.AppendChild(Xml.OwnerDocument.ImportNode(node, true));
        return this;
      }

      if (content is IAmlNode aml)
      {
        using (var writer = Xml.CreateNavigator().AppendChild())
          aml.ToAml(writer);
        return this;
      }

      if (content is IReadOnlyAttribute attr)
      {
        Xml.SetAttribute(attr.Name, attr.Value);
        return this;
      }

      if (content is string str)
      {
        Xml.InnerText = str;
        return this;
      }

      var enumerable = content as IEnumerable;
      if (enumerable != null)
      {
        foreach (var curr in enumerable)
        {
          Add(curr);
        }
        return this;
      }

      if (content is XmlElement xElem)
      {
        var imported = Xml.OwnerDocument.ImportNode(xElem, true);
        Xml.AppendChild(imported);
        return this;
      }

      Xml.InnerText = AmlContext.LocalizationContext.Format(content);
      return this;
    }

    public IAttribute Attribute(string name)
    {
      var elem = AssertXml();
      if (elem == null)
        return Client.Attribute.NullAttr;
      var node = elem.GetAttributeNode(name);
      return node == null ? new Attribute(this, name) : new Attribute(this, node);
    }

    public IEnumerable<IAttribute> Attributes()
    {
      var elem = AssertXml();
      if (elem == null)
        return Enumerable.Empty<IAttribute>();
      return Xml.Attributes.OfType<XmlAttribute>().Select(a => (IAttribute)new Attribute(this, a));
    }

    public IEnumerable<IElement> Elements()
    {
      return Xml.ChildNodes.OfType<XmlElement>().Select(e => (IElement)Factory(e, this));
    }

    public void Remove()
    {
      AssertXml()?.ParentNode.RemoveChild(Xml);
    }

    public void RemoveAttributes()
    {
      AssertXml()?.RemoveAllAttributes();
    }

    public void RemoveNodes()
    {
      var children = AssertXml()?.ChildNodes.OfType<XmlNode>().ToList() ?? Enumerable.Empty<XmlNode>();
      foreach (var child in children)
        Xml.RemoveChild(child);
    }

    public virtual void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      AssertXml().WriteTo(writer);
    }

    public override string ToString()
    {
      return this.ToAml();
    }

    IReadOnlyAttribute IReadOnlyElement.Attribute(string name)
    {
      return Attribute(name);
    }

    IEnumerable<IReadOnlyAttribute> IReadOnlyElement.Attributes()
    {
      return Attributes().OfType<IReadOnlyAttribute>();
    }

    IEnumerable<IReadOnlyElement> IReadOnlyElement.Elements()
    {
      return Elements().OfType<IReadOnlyElement>();
    }

    protected virtual XmlElement AssertXml()
    {
      return Xml;
    }

    internal static Element Factory(XmlElement elem, Element parent)
    {
      switch (elem.LocalName)
      {
        case "Item":
          return new Item((Innovator)parent.AmlContext, elem) { Parent = parent };
        case "Relationships":
          return new Relationships((Innovator)parent.AmlContext, parent, elem);
        case "and":
        case "AND":
        case "or":
        case "OR":
        case "not":
        case "NOT":
          return new Logical((Innovator)parent.AmlContext, parent, elem);
        default:
          return new Property(parent, elem);
      }
    }
  }
}
#endif
