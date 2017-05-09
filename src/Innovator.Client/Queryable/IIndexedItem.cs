using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Queryable
{
  public interface IIndexedItem : IReadOnlyItem
  {
    object this[string name] { get; }
  }
}
