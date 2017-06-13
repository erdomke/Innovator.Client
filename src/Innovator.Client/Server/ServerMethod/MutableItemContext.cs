using Innovator.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Server
{
  public class MutableItemContext : IMutableItemContext
  {
    private IServerConnection _conn;
    private IItem _item;

    public IServerConnection Conn
    {
      get { return _conn; }
    }

    public IItem Item
    {
      get { return _item; }
    }

    public MutableItemContext(IServerConnection conn, IItem item)
    {
      _conn = conn;
      _item = item;
    }
  }
}
