using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  /// <summary>
  /// Settings and metadata used to generate a SQL statement from AML.  Metadata
  /// will be queried using the specified <see cref="IConnection"/>
  /// </summary>
  public interface IAmlSqlWriterSettings
  {
    /// <summary>
    /// Gets the aml context used for serializing/deserializing values
    /// </summary>
    ElementFactory AmlContext { get; }
    /// <summary>
    /// Gets the identity list for the current user
    /// </summary>
    string IdentityList { get; }
    /// <summary>
    /// How to handle permissions with the query
    /// </summary>
    AmlSqlPermissionOption PermissionOption { get; }
    /// <summary>
    /// What portion of the SQL query to render
    /// </summary>
    AmlSqlRenderOption RenderOption { get; }
    /// <summary>
    /// ID of the current user
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// Gets the property metadata for an itemtype by name.
    /// </summary>
    /// <param name="itemType">Name of the itemtype</param>
    IDictionary<string, Model.Property> GetProperties(string itemType);
  }
}
