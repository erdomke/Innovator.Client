using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Innovator.Client.Model;

namespace Innovator.Client
{
  /// <summary>
  /// Settings and metadata used to generate a SQL statement from AML.  Metadata
  /// will be queried using the specified <see cref="IConnection"/>
  /// </summary>
  /// <seealso cref="Innovator.Client.IAmlSqlWriterSettings" />
  public class ConnectedAmlSqlWriterSettings : IAmlSqlWriterSettings
  {
    private Dictionary<string, Dictionary<string, Model.Property>> _props
      = new Dictionary<string, Dictionary<string, Model.Property>>();
    private IConnection _conn;
    private string _identList;

    /// <summary>
    /// Gets the aml context used for serializing/deserializing values
    /// </summary>
    public ElementFactory AmlContext { get { return _conn.AmlContext; } }
    /// <summary>
    /// Gets the identity list for the current user
    /// </summary>
    public string IdentityList
    {
      get
      {
        if (_identList == null)
        {
          _identList = _conn.Apply(new Command("<Item/>").WithAction(CommandAction.GetIdentityList)).Value ?? "";
        }
        return _identList;
      }
    }
    /// <summary>
    /// How to handle permissions with the query
    /// </summary>
    public AmlSqlPermissionOption PermissionOption { get; set; }
    /// <summary>
    /// What portion of the SQL query to render
    /// </summary>
    public AmlSqlRenderOption RenderOption { get; set; }
    /// <summary>
    /// ID of the current user
    /// </summary>
    public string UserId { get { return _conn.UserId; } }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectedAmlSqlWriterSettings"/> class for the specified
    /// <see cref="IConnection"/>.
    /// </summary>
    /// <param name="conn">The connection.</param>
    public ConnectedAmlSqlWriterSettings(IConnection conn)
    {
      _conn = conn;
    }

    /// <summary>
    /// Gets the property metadata for an itemtype by name.
    /// </summary>
    /// <param name="itemType">Name of the itemtype</param>
    public IDictionary<string, Model.Property> GetProperties(string itemType)
    {
      Dictionary<string, Model.Property> result;
      if (!_props.TryGetValue(itemType, out result))
      {
        result = _conn.Apply(@"<Item type='Property' action='get'>
  <source_id>
    <Item type='ItemType' action='get'>
      <name>@0</name>
    </Item>
  </source_id>
</Item>", itemType)
          .Items()
          .OfType<Model.Property>()
          .ToDictionary(p => p.NameProp().Value);
        _props[itemType] = result;
      }

      return result;
    }
  }
}
