using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// Wraps an Aras item so that additional functionality can be easily provided
  /// </summary>
  [DebuggerTypeProxy(typeof(ItemDebugView))]
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public abstract class ItemWrapper : IReadOnlyItem
  {
    private readonly IReadOnlyItem _item;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemWrapper"/> class.
    /// </summary>
    /// <param name="item">The item to wrap.</param>
    public ItemWrapper(IReadOnlyItem item)
    {
      _item = item;
    }

    private string DebuggerDisplay
    {
      get
      {
        if (!_item.Exists)
          return "{null:" + _item.GetType().Name + "}";
        return _item.ToAml();
      }
    }

    /// <inheritdoc/>
    public IItem Clone()
    {
      return _item.Clone();
    }
    /// <inheritdoc/>
    public string Id()
    {
      return _item.Id();
    }
    /// <inheritdoc/>
    public virtual IReadOnlyProperty Property(string name)
    {
      return _item.Property(name);
    }
    /// <inheritdoc/>
    public virtual IReadOnlyProperty Property(string name, string lang)
    {
      return _item.Property(name, lang);
    }
    /// <inheritdoc/>
    public IEnumerable<IReadOnlyItem> Relationships()
    {
      return _item.Relationships();
    }
    /// <inheritdoc/>
    public IEnumerable<IReadOnlyItem> Relationships(string type)
    {
      return _item.Relationships(type);
    }
    /// <inheritdoc/>
    public IReadOnlyAttribute Attribute(string name)
    {
      return _item.Attribute(name);
    }
    /// <inheritdoc/>
    public IEnumerable<IReadOnlyAttribute> Attributes()
    {
      return _item.Attributes();
    }
    /// <inheritdoc/>
    public IEnumerable<IReadOnlyElement> Elements()
    {
      return _item.Elements();
    }
    /// <inheritdoc/>
    public ElementFactory AmlContext
    {
      get { return _item.AmlContext; }
    }
    /// <inheritdoc/>
    public bool Exists
    {
      get { return _item.Exists; }
    }
    /// <inheritdoc/>
    public string Name
    {
      get { return _item.Name; }
    }
    /// <inheritdoc/>
    public IReadOnlyElement Parent
    {
      get { return _item.Parent; }
    }
    /// <inheritdoc/>
    public string Prefix
    {
      get { return _item.Prefix; }
    }
    /// <inheritdoc/>
    public string Value
    {
      get { return _item.Value; }
    }
    /// <inheritdoc/>
    public string ToAml()
    {
      return _item.ToAml();
    }
    /// <inheritdoc/>
    public void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      _item.ToAml(writer, settings);
    }
    /// <inheritdoc/>
    public string TypeName()
    {
      return _item.TypeName();
    }

    private class ItemDebugView
    {
      private readonly IReadOnlyItem _item;

      public ElementFactory AmlContext { get { return _item.AmlContext; } }
      public IEnumerable<IReadOnlyAttribute> Attributes { get { return _item.Attributes().ToArray(); } }
      public IEnumerable<IReadOnlyElement> Elements { get { return _item.Elements().ToArray(); } }
      public bool Exists { get { return _item.Exists; } }
      public string Name { get { return _item.Name; } }
      public IReadOnlyElement Parent { get { return _item.Parent; } }
      public string Value { get { return _item.Value; } }

      public ItemDebugView(ItemWrapper item)
      {
        _item = item._item;
      }
    }
  }
}
