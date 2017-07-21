using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Innovator.Client.Model;

namespace Innovator.Client
{
  public class ConnectedAmlSqlWriterSettings : IAmlSqlWriterSettings
  {
    private Dictionary<string, Dictionary<string, Model.Property>> _props
      = new Dictionary<string, Dictionary<string, Model.Property>>();
    private IConnection _conn;
    private string _identList;

    public ElementFactory AmlContext { get { return _conn.AmlContext; } }
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
    public AmlSqlPermissionOption PermissionOption { get; set; }
    public AmlSqlRenderOption RenderOption { get; set; }
    public string UserId { get { return _conn.UserId; } }

    public ConnectedAmlSqlWriterSettings(IConnection conn)
    {
      _conn = conn;
    }

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
