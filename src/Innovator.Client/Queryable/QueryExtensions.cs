#if REFLECTION
using Innovator.Client.Model;
using Innovator.Client.Queryable;
using System;
using System.Linq;

namespace Innovator.Client
{
  public static class QueryExtensions
  {
    public static InnovatorQuery<IReadOnlyItem> Queryable(this IConnection conn, string itemType, QuerySettings settings = null)
    {
      return new InnovatorQuery<IReadOnlyItem>(new InnovatorQueryProvider(conn, settings), itemType);
    }

    public static InnovatorQuery<T> Queryable<T>(this IConnection conn, string itemType = null, QuerySettings settings = null)
    {
      if (string.IsNullOrEmpty(itemType))
      {
        var nameAttr = typeof(T).GetCustomAttributes(false).OfType<ArasNameAttribute>().FirstOrDefault();
        if (nameAttr == null)
          throw new ArgumentNullException(nameof(itemType), "The name of the item type must be specified");
        itemType = nameAttr.Name;
      }
      return new InnovatorQuery<T>(new InnovatorQueryProvider(conn, settings), itemType);
    }
  }
}
#endif
