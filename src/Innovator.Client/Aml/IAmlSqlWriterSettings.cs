using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  public interface IAmlSqlWriterSettings
  {
    ElementFactory AmlContext { get; }
    string IdentityList { get; }
    /// <summary>
    /// How to handle permissions with the query
    /// </summary>
    AmlSqlPermissionOption PermissionOption { get; }
    AmlSqlRenderOption RenderOption { get; }
    string UserId { get; }

    IDictionary<string, Model.Property> GetProperties(string itemType);
  }
}
