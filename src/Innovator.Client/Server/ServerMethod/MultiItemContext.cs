using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Innovator.Client;

namespace Innovator.Server
{
  public class MultiItemContext : IMultipleItemContext
  {
    private IServerConnection _conn;
    private IEnumerable<IItem> _items;

    public IServerConnection Conn
    {
      get { return _conn; }
    }

    public IEnumerable<IItem> Items
    {
      get { return _items; }
    }

    public MultiItemContext(IServerConnection conn, IEnumerable<IItem> items)
    {
      _conn = conn;
      _items = items;
    }
  }
}
