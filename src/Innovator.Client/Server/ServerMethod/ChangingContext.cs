using Innovator.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Innovator.Server
{
  /// <inheritdoc cref="IChangingContext"/>
  public class ChangingContext : IChangingContext
  {
    private IReadOnlyItem _existing;
    private bool _existingLoaded;
    private readonly IResult _result;

    /// <inheritdoc cref="IChangingContext.Item"/>
    public IItem Item { get; }

    /// <inheritdoc cref="IChangingContext.Conn"/>
    public IServerConnection Conn { get; }

    /// <inheritdoc cref="IChangingContext.Existing"/>
    public IReadOnlyItem Existing
    {
      get
      {
        EnsureExisting();
        return _existing;
      }
    }

    /// <inheritdoc cref="IChangingContext.IsNew"/>
    public bool IsNew
    {
      get
      {
        EnsureExisting();
        return _existing?.Exists != true;
      }
    }

    /// <inheritdoc cref="IChangingContext.QueryDefaults"/>
    public Action<IItem> QueryDefaults { get; set; }

    /// <inheritdoc cref="IChangingContext.Merged"/>
    public IReadOnlyItem Merged
    {
      get
      {
        if (this.IsNew) return Item;
        var merges = _existing.Clone();
        var names = new HashSet<string>(Item.Elements().Select(e => e.Name));
        var toRemove = merges.Elements().Where(e => names.Contains(e.Name)).ToList();
        foreach (var elem in toRemove)
        {
          elem.Remove();
        }

        foreach (var elem in Item.Elements())
        {
          merges.Add(elem);
        }
        return merges;
      }
    }

    /// <inheritdoc cref="IChangingContext.IsBeingSetNull(string)"/>
    public bool IsBeingSetNull(string name)
    {
      var isNew = IsNew;
      var prop = Item.Property(name);
      return (isNew && !prop.HasValue())
        || (!isNew && prop.IsNull().AsBoolean(false));
    }

    /// <inheritdoc cref="IChangingContext.IsBeingSetNullOrEmpty(string)"/>
    public bool IsBeingSetNullOrEmpty(string name)
    {
      var prop = Item.Property(name);
      return IsBeingSetNull(name) || (prop.Exists && string.IsNullOrEmpty(prop.Value));
    }

    /// <inheritdoc cref="IChangingContext.IsChanging"/>
    public bool IsChanging(params string[] names)
    {
      // Are any of the properties in the changing item?
      var existingProperties = names.Where(n => Item.Property(n).Exists);
      if (!existingProperties.Any()) return false;

      // Is this new?
      if (IsNew) return true;

      // Check only the changing properties against their existing values
      return existingProperties.Any(n => Item.Property(n).Value != _existing.Property(n).Value);
    }

    /// <inheritdoc cref="IChangingContext.NewOrExisting"/>
    public IReadOnlyProperty NewOrExisting(string name)
    {
      var result = Item.Property(name);
      if (result.Exists) return result;

      if (this.IsNew)
      {
        var item = Item as Item;
        if (item == null) return Property.NullProp;
        return item.Property(name);
      }

      return _existing.Property(name);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangingContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="changes">The changes.</param>
    public ChangingContext(IServerConnection conn, IItem changes)
    {
      Item = changes;
      Conn = conn;
      _result = Conn.AmlContext.Result();
      _result.ErrorContext(Item);
    }

    private void EnsureExisting()
    {
      if (!_existingLoaded)
      {
        _existingLoaded = true;
        if (!string.IsNullOrEmpty(Item.Id()))
        {
          var aml = Conn.AmlContext;
          var query = aml.Item(Item.Type(), aml.Id(Item.Id()), aml.Action("get"));
          if (QueryDefaults != null) QueryDefaults.Invoke(query);
          var items = query.Apply(Conn).Items();
          if (items.Any())
            _existing = items.Single();
          else
            _existing = Client.Item.GetNullItem<IReadOnlyItem>();
        }
        else
        {
          _existing = Client.Item.GetNullItem<IReadOnlyItem>();
        }
      }
    }
  }
}
