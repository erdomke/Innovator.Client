using System;
using System.Collections.Generic;
using System.Linq;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// Settings and metadata used to generate a SQL statement from AML.  Metadata
  /// will be queried using the specified <see cref="IConnection"/>
  /// </summary>
  /// <seealso cref="Innovator.Client.QueryModel.IAmlSqlWriterSettings" />
  public class ConnectedAmlSqlWriterSettings : IAmlSqlWriterSettings
  {
    private readonly Dictionary<string, Dictionary<string, Model.IPropertyDefinition>> _props
      = new Dictionary<string, Dictionary<string, Model.IPropertyDefinition>>();
    private readonly IConnection _conn;
    private string _identList;

    /// <summary>
    /// Gets the identity list for the current user
    /// </summary>
    public string IdentityList
    {
      get
      {
        return _identList ?? (_identList = _conn.Apply(new Command("<Item/>").WithAction(CommandAction.GetIdentityList)).Value ?? "");
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
    /// Fluent API to set the permission option to the correct value based on the Aras version
    /// </summary>
    /// <param name="arasVersion">The Aras version.</param>
    /// <returns>The current instance of <see cref="ConnectedAmlSqlWriterSettings"/></returns>
    public ConnectedAmlSqlWriterSettings WithPermissions(Version arasVersion)
    {
      if (arasVersion == null)
        return this;

      var num = int.Parse(arasVersion.Major.ToString("D2") + arasVersion.Minor.ToString("D2") + arasVersion.ServicePack().Value.ToString("D2"));
      if (num < 110005)
        PermissionOption = AmlSqlPermissionOption.LegacyFunction;
      else if (num < 110012)
        PermissionOption = AmlSqlPermissionOption.SecuredFunction;
      else
        PermissionOption = AmlSqlPermissionOption.SecuredFunctionEnviron;
      return this;
    }

    /// <summary>
    /// Gets the property metadata for an itemtype by name.
    /// </summary>
    /// <param name="itemType">Name of the itemtype</param>
    public IDictionary<string, Model.IPropertyDefinition> GetProperties(string itemType)
    {
      Dictionary<string, Model.IPropertyDefinition> result;
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
          .OfType<Model.IPropertyDefinition>()
          .ToDictionary(p => p.NameProp().Value);
        _props[itemType] = result;
      }

      return result;
    }
  }
}
