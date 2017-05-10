using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Queryable
{
  internal class QueryableItem : ItemWrapper, IIndexedItem
  {
    public QueryableItem(IReadOnlyItem item) : base(item) { }

    public object this[string name] { get { return base.Property(name.ToLowerInvariant()).Value; } }
  }
}
