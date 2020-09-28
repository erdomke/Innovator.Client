using System;
using System.Collections.Generic;
using System.Linq;
using Innovator.Client;

namespace Innovator.Server
{
  /// <summary>
  /// Context for a server method which is used on before add
  /// or update of an item.
  /// </summary>
  public class ValidationContext : IValidationContext
  {
    private IReadOnlyItem _existing;
    private bool _existingLoaded;
    private readonly IResult _result;

    /// <summary>
    /// The changes given to the database.  This object should be modified to make any additional
    /// changes
    /// </summary>
    public IItem Item { get; }

    /// <summary>
    /// Connection to the database
    /// </summary>
    public IServerConnection Conn { get; }

    /// <summary>
    /// Error builder which captures any errors which are encountered
    /// </summary>
    public IErrorBuilder ErrorBuilder
    {
      get { return _result; }
    }

    /// <summary>
    /// Get the exception object created for any errors that have happened so far.
    /// </summary>
    public Exception Exception
    {
      get { return _result.Exception; }
    }

    /// <summary>
    /// Get the existing item in the database
    /// </summary>
    public IReadOnlyItem Existing
    {
      get
      {
        EnsureExisting();
        return _existing;
      }
    }

    /// <summary>
    /// Indicates if the argument is new (not in the database)
    /// </summary>
    public bool IsNew
    {
      get
      {
        EnsureExisting();
        return _existing?.Exists != true;
      }
    }

    /// <summary>
    /// Method for modifying the query to get existing items
    /// </summary>
    public Action<IItem> QueryDefaults { get; set; }

    /// <summary>
    /// Gets an item which represents the new item after the changes are applied
    /// </summary>
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

    /// <inheritdoc cref="IValidationContext.IsBeingSetNull(string)"/>
    public bool IsBeingSetNull(string name)
    {
      var isNew = IsNew;
      var prop = Item.Property(name);
      return (isNew && !prop.HasValue())
        || (!isNew && prop.IsNull().AsBoolean(false));
    }

    /// <inheritdoc cref="IValidationContext.IsBeingSetNullOrEmpty(string)"/>
    public bool IsBeingSetNullOrEmpty(string name)
    {
      var prop = Item.Property(name);
      return IsBeingSetNull(name) || (prop.Exists && string.IsNullOrEmpty(prop.Value));
    }

    /// <summary>
    /// Indicates if one or more properties in the list are changing
    /// </summary>
    /// <param name="names">Property name(s)</param>
    /// <returns>
    /// <c>true</c> if at least one of the properties is being changed with
    /// this query, <c>false</c> otherwise
    /// </returns>
    public bool IsChanging(params string[] names)
    {
      // Are any of the properties in the changing item?
      var propExists = names.Any(n => Item.Property(n).Exists);
      if (!propExists) return false;

      // Is this new?
      if (IsNew) return true;

      return names.Any(n => Item.Property(n).Value != _existing.Property(n).Value);
    }

    /// <summary>
    /// Gets a property from the <see cref="Item" /> item (if it exists).  Otherwise, the property
    /// from <see cref="Existing" /> is returned
    /// </summary>
    /// <param name="name">Name of the property to retrieve</param>
    /// <returns>
    /// The <see cref="IReadOnlyProperty" /> from the incoming query (if defined).  Otherwise,
    /// the <see cref="IReadOnlyProperty" /> from the database data
    /// </returns>
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
    /// Initializes a new instance of the <see cref="ValidationContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="changes">The changes.</param>
    public ValidationContext(IServerConnection conn, IItem changes)
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
