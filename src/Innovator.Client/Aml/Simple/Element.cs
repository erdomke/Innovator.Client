using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// Represents an XML element in an AML structure.  This element could be an Item, property, result tag, or something else
  /// </summary>
  [DebuggerTypeProxy(typeof(ElementDebugView))]
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public abstract class Element : IElement, ILinkedElement
  {
    /// <summary>
    /// Field containing a reference to the element attributes
    /// </summary>
    protected ElementAttributes _attr;
    /// <summary>
    /// Field containing the element content (either a child <see cref="IElement"/> or the value)
    /// </summary>
    protected object _content;
    private ILinkedAnnotation _lastAttr;

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
    public virtual bool Exists { get { return Next != null; } }

    /// <summary>Local XML name of the element</summary>
    public abstract string Name { get; }

    /// <summary>
    /// The next sibling AML element (if any)
    /// </summary>
    public abstract ILinkedElement Next { get; set; }

    /// <summary>Retrieve the parent element</summary>
    public abstract IElement Parent { get; set; }

    /// <summary>Retrieve the parent element</summary>
    IReadOnlyElement IReadOnlyElement.Parent { get { return this.Parent; } }

    /// <summary>Retrieve the parent element</summary>
    IReadOnlyElement ILinkedElement.Parent
    {
      get { return this.Parent; }
      set { this.Parent = (IElement)value; }
    }

    /// <inheritdoc/>
    public abstract string Prefix { get; }

    /// <summary>String value of the element</summary>
    public virtual string Value
    {
      get
      {
        if (_content != null
          && AmlContext != null
          && !(_content is ILinkedElement && !(_content is IReadOnlyItem)))
          return AmlContext.LocalizationContext.Format(_content);
        return _content as string;
      }
      set
      {
        AssertModifiable();
        _content = value;
      }
    }

    private string DebuggerDisplay
    {
      get
      {
        if (!this.Exists)
          return "{null:" + this.GetType().Name + "}";
        return this.ToAml();
      }
    }

    /// <summary>
    /// Indicates if the element is read only
    /// </summary>
    public bool ReadOnly
    {
      get
      {
        return string.IsNullOrEmpty(Name)
          || (_attr & ElementAttributes.ReadOnly) != 0;
      }
      set
      {
        if (value)
          _attr = _attr | ElementAttributes.ReadOnly;
        else
          _attr = _attr & ~ElementAttributes.ReadOnly;
      }
    }

    internal bool PreferCData
    {
      get
      {
        return (_attr & ElementAttributes.PreferCdata) != 0;
      }
      set
      {
        if (value)
          _attr = _attr | ElementAttributes.PreferCdata;
        else
          _attr = _attr & ~ElementAttributes.PreferCdata;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Element"/> class.
    /// </summary>
    public Element() { }

    /// <summary>
    /// Clones the element, specifying a new parent.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    /// <returns>A copy of the element</returns>
    protected virtual Element Clone(IElement newParent)
    {
      return new AmlElement(newParent, this);
    }

    /// <summary>
    /// Copies data from <paramref name="elem"/> into this instance.
    /// </summary>
    /// <param name="elem">The elem to copy from.</param>
    protected void CopyData(IReadOnlyElement elem)
    {
      Add(elem.Attributes());
      var elements = elem.Elements();
      if (elements.Any())
        Add(elements);
      else if (elem.Value != null)
        Add(elem.Value); // Only set the value if there are no elements as it will overwrite the elements
    }

    /// <summary>Add new content to the element</summary>
    public virtual IElement Add(object content)
    {
      if (content == null)
        return this;

      AssertModifiable();

      var id = content as IdAnnotation;
      if (id != null)
      {
        QuickAddAttribute(id);
        return this;
      }

      var elem = TryGet(content, this);
      if (elem != null)
      {
        QuickAddElement(elem);
        return this;
      }

      if (content is string)
      {
        _content = content;
        return this;
      }

      var attr = Innovator.Client.Attribute.TryGet(content, this);
      if (attr != null)
      {
        QuickAddAttribute(attr);
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

      if (content is CDataValue cData)
      {
        _content = cData.ToString();
        PreferCData = true;
      }
      else
      {
        _content = content;
      }
      return this;
    }

    internal void QuickAddElement(ILinkedElement elem)
    {
      _content = LinkedListOps.Add(_content as ILinkedElement, elem);
    }

    internal void QuickAddAttribute(ILinkedAnnotation attr)
    {
      LinkedListOps.Add(ref _lastAttr, attr);
    }

    private static ILinkedElement TryGet(object value, Element newParent)
    {
      var impl = value as ILinkedElement;
      if (impl != null)
      {
        if (impl.Parent == null || !impl.Parent.Exists || impl.Parent == newParent)
        {
          impl.Parent = newParent;
          return impl;
        }

        var item = value as Item;
        if (item != null)
          return (ILinkedElement)item.Clone();

        var el = value as Element;
        if (el != null)
          return el.Clone(newParent);
        return new AmlElement(newParent, (IReadOnlyElement)impl);
      }

      var elem = value as IReadOnlyElement;
      if (elem != null)
        return new AmlElement(newParent, elem);

      return null;
    }

    /// <summary>Retrieve the attribute with the specified name</summary>
    public IAttribute Attribute(string name)
    {
      return (((IReadOnlyElement)this).Attribute(name) as IAttribute)
        ?? Client.Attribute.NullAttr;
    }

    /// <summary>Retrieve all attributes specified for the element</summary>
    public IEnumerable<IAttribute> Attributes()
    {
      return LinkedListOps.Enumerate(_lastAttr).OfType<IAttribute>();
    }

    /// <summary>Retrieve all child elements</summary>
    public IEnumerable<IElement> Elements()
    {
      return ReadOnlyElements().OfType<IElement>();
    }

    protected virtual IEnumerable<IReadOnlyElement> ReadOnlyElements()
    {
      return LinkedListOps.Enumerate(_content as ILinkedElement).OfType<IReadOnlyElement>();
    }

    internal Element ElementByName(string name)
    {
      var elem = _content as ILinkedElement;
      if (elem != null)
      {
        var result = (LinkedListOps.Find(elem, name) as Element);
        if (result != null)
          return result;
      }
      return Elements().OfType<Element>().FirstOrDefault(e => e.Name == name) ?? new AmlElement(this, name);
    }

    /// <summary>Remove the element from its parent</summary>
    public void Remove()
    {
      if (Exists)
      {
        var elem = this.Parent as Element;
        if (elem != null && elem.Exists)
        {
          elem.RemoveNode(this);
        }
        this.Parent = AmlElement.NullElem;
      }
    }
    internal void RemoveAttribute(Attribute attr)
    {
      AssertModifiable();
      LinkedListOps.Remove(ref _lastAttr, attr);
    }

    /// <summary>Remove attributes from the element</summary>
    public void RemoveAttributes()
    {
      AssertModifiable();
      _lastAttr = null;
    }

    internal void RemoveNode(ILinkedElement elem)
    {
      AssertModifiable();
      var lastElem = _content as ILinkedElement;
      if (lastElem == null)
        return;
      _content = LinkedListOps.Remove(lastElem, elem);
    }

    /// <summary>Remove child nodes from the element</summary>
    public void RemoveNodes()
    {
      AssertModifiable();
      _content = null;
    }

    /// <summary>
    /// Write the node to the specified <see cref="XmlWriter"/>
    /// </summary>
    /// <param name="writer"><see cref="XmlWriter"/> to write the node to</param>
    /// <param name="settings">Settings controlling how the node is written</param>
    public void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      var name = Name;
      var prefix = Prefix;
      var i = name.IndexOf(':');
      var attrs = LinkedListOps.Enumerate(_lastAttr).OfType<IReadOnlyAttribute>().ToArray();
      if (string.IsNullOrEmpty(prefix) && i > 0)
      {
        prefix = name.Substring(0, i);
        name = name.Substring(i + 1);
      }
      if (string.IsNullOrEmpty(prefix))
      {
        writer.WriteStartElement(name);
      }
      else
      {
        var ns = AmlReader.NamespaceFromPrefix(prefix);
        writer.WriteStartElement(prefix, name, ns);
      }

      foreach (var attr in attrs)
      {
        i = attr.Name.IndexOf(':');
        if (i > 0)
        {
          var attributePrefix = attr.Name.Substring(0, i);
          if (attributePrefix == "xmlns")
            continue;
          name = attr.Name.Substring(i + 1);
          var ns = AmlReader.NamespaceFromPrefix(attributePrefix);
          writer.WriteAttributeString(attributePrefix, name, ns, attr.Value);
        }
        else
        {
          writer.WriteAttributeString(attr.Name, attr.Value);
        }
      }
      if (!(_content is ILinkedElement))
      {
        if (PreferCData)
          writer.WriteCData(this.Value);
        else
          writer.WriteString(this.Value);
      }
      else
      {
        var elems = Elements().ToArray();
        var item = elems.OfType<IReadOnlyItem>().FirstOrDefault();
        if (this is IReadOnlyProperty
          && !settings.ExpandPropertyItems
          && item?.Attribute("action").Exists == false)
        {
          writer.WriteAttributeString("type", item.TypeName());
          var keyedName = item.KeyedName().Value ?? item.Property("id").KeyedName().Value;
          if (!string.IsNullOrEmpty(keyedName))
            writer.WriteAttributeString("keyed_name", keyedName);
          writer.WriteString(item.Id());
        }
        else
        {
          foreach (var e in elems)
          {
            e.ToAml(writer, settings);
          }
        }
      }
      writer.WriteEndElement();
    }

    IReadOnlyAttribute IReadOnlyElement.Attribute(string name)
    {
      return (LinkedListOps.Find(_lastAttr, name) as IReadOnlyAttribute)
        ?? new Attribute(this, name);
    }

    IEnumerable<IReadOnlyAttribute> IReadOnlyElement.Attributes()
    {
      return LinkedListOps.Enumerate(_lastAttr).OfType<IReadOnlyAttribute>();
    }

    IEnumerable<IReadOnlyElement> IReadOnlyElement.Elements()
    {
      return ReadOnlyElements();
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that has the AML of the element.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that has the AML of the element.
    /// </returns>
    public override string ToString()
    {
      return this.ToAml();
    }

    /// <summary>
    /// Asserts that the element is modifiable modifiable.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the user attempts to modify a read only element</exception>
    protected void AssertModifiable()
    {
      if (this.ReadOnly)
        throw new InvalidOperationException("Cannot modify a read only element");
    }

    private class ElementDebugView
    {
      private Element _elem;

      public ElementFactory AmlContext { get { return _elem.AmlContext; } }
      public IEnumerable<IAttribute> Attributes { get { return _elem.Attributes().ToArray(); } }
      public IEnumerable<IElement> Elements { get { return _elem.Elements().ToArray(); } }
      public bool Exists { get { return _elem.Exists; } }
      public string Name { get { return _elem.Name; } }
      public IElement Parent { get { return _elem.Parent; } }
      public string Prefix { get { return _elem.Prefix; } }
      public bool ReadOnly { get { return _elem.ReadOnly; } }
      public string Value { get { return _elem.Value; } }

      public ElementDebugView(Element elem)
      {
        _elem = elem;
      }
    }
  }
}
