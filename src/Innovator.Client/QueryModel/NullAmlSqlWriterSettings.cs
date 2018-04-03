using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Innovator.Client.Model;

namespace Innovator.Client.QueryModel
{
  internal class NullAmlSqlWriterSettings : IAmlSqlWriterSettings
  {
    public string IdentityList => null;

    public AmlSqlPermissionOption PermissionOption => AmlSqlPermissionOption.None;

    public AmlSqlRenderOption RenderOption => AmlSqlRenderOption.Default;

    public string UserId => null;

    public IDictionary<string, IPropertyDefinition> GetProperties(string itemType)
    {
      return new Dictionary<string, IPropertyDefinition>();
    }
  }
}
