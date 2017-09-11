using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Innovator.Server;
using Innovator.Client;

#if XMLLEGACY
namespace Innovator.Server
{
  public class IomServerConnection : Client.IOM.IomConnection, IServerConnection
  {
    public IomServerConnection(object iomConnection) : this(iomConnection, Factory.DefaultItemFactory) { }
    public IomServerConnection(object iomConnection, IItemFactory itemFactory) : base(iomConnection, itemFactory)
    {
    }

    public IServerCache ApplicationCache
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public string OriginalRequest
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public IServerPermissions Permissions
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public string RequestUrl
    {
      get
      {
        LazyLoadCreds();
        return _requestUrl.ToString();
      }
    }

    public IServerCache SessionCache
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public string GetHeader(string name)
    {
      throw new NotImplementedException();
    }

    protected override void LazyLoadCreds()
    {
      if (string.IsNullOrEmpty(_httpUsername))
      {
        var type = _iomConnection.GetType();
        var noArgs = new object[] { };

        // Try to get information from a server-side connection
        try
        {
          var ccoProp = type.GetProperty("CCO");
          if (ccoProp != null)
          {
            var cco = ccoProp.GetValue(_iomConnection, null);
            var vars = cco.GetType().GetProperty("Variables").GetValue(cco, null);
            _httpPassword = (string)vars.GetType().GetMethod("GetUserPassword").Invoke(vars, noArgs);
            _httpUsername = (string)vars.GetType().GetMethod("GetLoginName").Invoke(vars, noArgs);
            var request = cco.GetType().GetProperty("Request").GetValue(cco, null);
            _requestUrl = (Uri)request.GetType().GetProperty("Url").GetValue(request, null);
            _innovatorClientBin = new Uri(_requestUrl, "../Client/cbin/");
          }
        }
        catch (Exception) { }
      }
    }
  }
}
#endif
