using System;
using System.Collections.Generic;
using System.Linq;

namespace Innovator.Client
{
  /// <summary>
  /// Represents an XML <c>Item</c> element in an AML structure.
  /// </summary>
  public class Item : Element, IItem, IEquatable<IItemRef>
  {
    private ElementFactory _amlContext;
    private IElement _parent = AmlElement.NullElem;

    /// <summary>Retrieve the context used for rendering primitive values</summary>
    public override ElementFactory AmlContext { get { return _amlContext; } }

    /// <summary>Returns <c>true</c> if this element actually exists in the underlying AML,
    /// otherwise, returns <c>false</c> to indicate that the element is just a null placeholder
    /// put in place to reduce unnecessary null reference checks</summary>
    public override bool Exists { get { return (_attr & ElementAttributes.Null) == 0; } }

    internal bool IsNull
    {
      get { return (_attr & ElementAttributes.Null) != 0; }
      set
      {
        if (value)
          _attr = _attr | ElementAttributes.Null;
        else
          _attr = _attr & ~ElementAttributes.Null;
      }
    }

    /// <summary>Local XML name of the element</summary>
    public override string Name { get { return "Item"; } }

    /// <summary>
    /// The next sibling AML element (if any)
    /// </summary>
    public override ILinkedElement Next { get; set; }

    /// <summary>Retrieve the parent element</summary>
    public override IElement Parent
    {
      get { return _parent; }
      set { _parent = value ?? AmlElement.NullElem; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Item"/> class.
    /// </summary>
    protected Item() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Item"/> class.
    /// </summary>
    /// <param name="amlContext">The aml context.</param>
    /// <param name="content">The content.</param>
    public Item(ElementFactory amlContext, params object[] content)
    {
      _amlContext = amlContext;
      if (content.Length > 0)
        Add(content);
    }

    private static Dictionary<Type, IReadOnlyItem> _nullItems = new Dictionary<Type, IReadOnlyItem>()
    {
      { typeof(Item), new Item(){ _attr = ElementAttributes.ReadOnly | ElementAttributes.Null } }
    };

    /// <summary>
    /// Add a strongly-typed 'null' model to list of null models by type
    /// </summary>
    /// <typeparam name="T">Type of item model</typeparam>
    /// <param name="value">'null' model instance</param>
    public static void AddNullItem<T>(T value) where T : IReadOnlyItem
    {
      _nullItems[typeof(T)] = value;
    }

    /// <summary>
    /// Retrieve a strongly-typed 'null' model to the list of null models by type
    /// </summary>
    /// <remarks>
    /// If you are looking to create a new item, see <see cref="ElementFactory.Item(object[])"/> or 
    /// <see cref="ElementFactory.FromXml(string, object[])"/>.
    /// </remarks>
    /// <typeparam name="T">Type of item model</typeparam>
    /// <example>
    /// Say you have a nullable <see cref="IReadOnlyItem"/> variable and you want to make sure it 
    /// is never null.
    /// <code lang="C#">
    /// IReadOnlyItem parameter = null;
    /// var item = parameter ?? Item.GetNullItem&lt;IReadOnlyItem&gt;()
    /// </code>
    /// </example>
    public static T GetNullItem<T>() where T : IReadOnlyItem
    {
      return (T)(GetNullItem(typeof(T)) ?? default(T));
    }

    public static object GetNullItem(Type type)
    {
      if (_nullItems.TryGetValue(type, out IReadOnlyItem result))
        return result;

      switch (type.FullName)
      {
        case "Innovator.Client.IReadOnlyItem":
        case "Innovator.Client.IItem":
          return _nullItems[typeof(Item)];
      }

      // Force a call to the static constructor
      try
      {
        var temp = Activator.CreateInstance(type, ElementFactory.Local);
        if (_nullItems.TryGetValue(type, out result))
          return result;
      }
      catch (Exception) { }

      return null;
    }

    internal Item SetFlag(ElementAttributes attr)
    {
      _attr |= attr;
      return this;
    }

    public override IElement Add(object content)
    {
      if (!Exists && !ReadOnly)
      {
        IsNull = false;
        Parent.Add(this);
      }

      return base.Add(content);
    }

    /// <summary>Returns a reference to the property with the specified name</summary>
    /// <remarks>If the property does not exist, a non-null object will be returned that has an <c>Exists</c> member which will return <c>false</c></remarks>
    /// <param name="name">Name of the property</param>
    public IProperty Property(string name)
    {
      return (((IReadOnlyItem)this).Property(name) as IProperty)
        ?? Client.Property.NullProp;
    }

    /// <summary>Returns a reference to the property with the specified name and language</summary>
    /// <remarks>If the property does not exist, a non-null object will be returned that has an <c>Exists</c> member which will return <c>false</c></remarks>
    /// <param name="name">Name of the property</param>
    /// <param name="lang">Language of the (multilingual) property</param>
    public IProperty Property(string name, string lang)
    {
      return (((IReadOnlyItem)this).Property(name, lang) as IProperty)
        ?? Client.Property.NullProp;
    }

    /// <summary>Returns the set of relationships associated with this item</summary>
    public IRelationships Relationships()
    {
      if (Exists)
      {
        var elem = _content as ILinkedElement;
        if (elem != null)
        {
          var rel = LinkedListOps.FindAll(elem, "Relationships")
            .OfType<Relationships>()
            .OrderByDescending(r => r.Elements().OfType<IReadOnlyItem>().Count())
            .FirstOrDefault();
          if (rel != null)
            return rel;
        }
      }
      return new Relationships(this);
    }

    /// <summary>Returns the set of relationships associated with this item of the specified type</summary>
    /// <param name="type">Name of the ItemType for the relationships you wish to retrieve</param>
    public IEnumerable<IItem> Relationships(string type)
    {
      return Relationships()
        .Elements().OfType<IItem>()
        .Where(i => i.TypeName() == type);
    }

    /// <summary>Creates a duplicate of the item object.  All properties (including the ID) are preserved</summary>
    public IItem Clone()
    {
      var writer = new ResultWriter(this.AmlContext, null, null);
      ToAml(writer, new AmlWriterSettings());
      var clone = writer.Result.AssertItem();
      // Make sure to transfer the null status to the new item
      if (IsNull && clone is Item item)
      {
        item.IsNull = true;
      }
      return clone;
    }

    protected override IEnumerable<IReadOnlyElement> ReadOnlyElements()
    {
      if ((_attr & ElementAttributes.ItemDefaultAny) != 0)
        return GetDefaultProperties().Concat(base.ReadOnlyElements());
      return base.ReadOnlyElements();
    }

    private IEnumerable<IReadOnlyElement> GetDefaultProperties()
    {
      if ((_attr & ElementAttributes.ItemDefaultGeneration) != 0)
        yield return Client.Property.DefaultGeneration;
      if ((_attr & ElementAttributes.ItemDefaultIsCurrent) != 0)
        yield return Client.Property.DefaultIsCurrent;
      if ((_attr & ElementAttributes.ItemDefaultIsReleased) != 0)
        yield return Client.Property.DefaultIsReleased;
      if ((_attr & ElementAttributes.ItemDefaultMajorRev) != 0)
        yield return Client.Property.DefaultMajorRev;
      if ((_attr & ElementAttributes.ItemDefaultNewVersion) != 0)
        yield return Client.Property.DefaultNewVersion;
      if ((_attr & ElementAttributes.ItemDefaultNotLockable) != 0)
        yield return Client.Property.DefaultNotLockable;
    }

    IReadOnlyProperty IReadOnlyItem.Property(string name)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException(name);

      if (Exists || _parent != AmlElement.NullElem)
      {
        var elem = _content as ILinkedElement;
        if (elem != null)
        {
          var prop = LinkedListOps.Find(elem, name) as IReadOnlyProperty;
          // The first one should be in the user's language if multiple languages
          // exist
          if (prop != null && prop.Attribute("xml:lang").Exists)
          {
            prop = LinkedListOps.FindAll(elem, name)
              .OfType<IReadOnlyProperty>()
              .FirstOrDefault(p => p.Attribute("xml:lang").Value == AmlContext.LocalizationContext.LanguageCode);

          }

          if (prop != null)
            return prop;
        }

        if (name == "generation" && (_attr & ElementAttributes.ItemDefaultGeneration) > 0)
          return Client.Property.DefaultGeneration;
        if (name == "is_current" && (_attr & ElementAttributes.ItemDefaultIsCurrent) > 0)
          return Client.Property.DefaultIsCurrent;
        if (name == "is_released" && (_attr & ElementAttributes.ItemDefaultIsReleased) > 0)
          return Client.Property.DefaultIsReleased;
        if (name == "major_rev" && (_attr & ElementAttributes.ItemDefaultMajorRev) > 0)
          return Client.Property.DefaultMajorRev;
        if (name == "new_version" && (_attr & ElementAttributes.ItemDefaultNewVersion) > 0)
          return Client.Property.DefaultNewVersion;
        if (name == "not_lockable" && (_attr & ElementAttributes.ItemDefaultNotLockable) > 0)
          return Client.Property.DefaultNotLockable;
        return new Property(this, name);
      }
      return Innovator.Client.Property.NullProp;
    }

    IReadOnlyProperty IReadOnlyItem.Property(string name, string lang)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException(name);
      if (string.IsNullOrEmpty(lang))
        return ((IReadOnlyItem)this).Property(name);

      if (Exists)
      {
        var elem = _content as ILinkedElement;
        if (elem != null)
        {
          var prop = LinkedListOps.FindAll(elem, name)
            .OfType<IReadOnlyProperty>()
            .FirstOrDefault(p => p.Attribute("xml:lang").Value == lang);

          if (prop != null)
            return prop;
        }
        var result = new Property(this, name, new Attribute("xml:lang", lang));
        return result;
      }
      return Innovator.Client.Property.NullProp;
    }

    IEnumerable<IReadOnlyItem> IReadOnlyItem.Relationships()
    {
      return Relationships().OfType<IReadOnlyItem>();
    }

    IEnumerable<IReadOnlyItem> IReadOnlyItem.Relationships(string type)
    {
      return Relationships(type).OfType<IReadOnlyItem>();
    }

    /// <summary>The ID of the item as retrieved from either the attribute or the property</summary>
    public string Id()
    {
      var attr = ((IReadOnlyElement)this).Attribute("id");
      if (attr.HasValue())
        return attr.Value;
      var prop = this.IdProp();
      if (prop.HasValue() && (!prop.Condition().HasValue() || prop.Condition().Value == "eq"))
        return prop.Value;
      return null;
    }

    /// <summary>The type of the item as retrieved from either the attribute or the property</summary>
    public virtual string TypeName()
    {
      return ((IReadOnlyElement)this).Attribute("type").Value;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      var result = 0;

      var buffer = Id();
      if (!string.IsNullOrEmpty(buffer))
        result ^= new Guid(buffer).GetHashCode();

      buffer = TypeName();
      if (string.IsNullOrEmpty(buffer))
        buffer = ((IReadOnlyElement)this).Attribute("typeId").Value;
      if (!string.IsNullOrEmpty(buffer))
        result ^= buffer.GetHashCode();

      return result;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(IItemRef other)
    {
      if (object.ReferenceEquals(this, other))
        return true;

      var cId = Id();
      var oId = other.Id();
      if (string.IsNullOrEmpty(cId)
        || string.IsNullOrEmpty(cId))
        return false;
      var cType = TypeName();
      var oType = other.TypeName();
      if (string.IsNullOrEmpty(cType)
        || string.IsNullOrEmpty(oType))
        return false;

      return cId == oId && cType == oType;
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
      var item = obj as IItemRef;
      if (item == null)
        return false;
      return Equals(item);
    }
  }
}
