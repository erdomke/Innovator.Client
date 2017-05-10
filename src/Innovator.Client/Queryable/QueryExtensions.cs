#if REFLECTION
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Innovator.Client.Queryable;
using Innovator.Client.Model;

namespace Innovator.Client
{
  public static class QueryExtensions
  {
    public static IQueryable<IReadOnlyItem> Queryable(this IConnection conn, string itemType, QuerySettings settings = null)
    {
      return new InnovatorQuery<IReadOnlyItem>(new InnovatorQueryProvider(conn, settings)) { Type = itemType };
    }

    public static IQueryable<T> Queryable<T>(this IConnection conn, string itemType = null, QuerySettings settings = null)
    {
      if (string.IsNullOrEmpty(itemType))
      {
        var nameAttr = typeof(T).GetCustomAttributes(false).OfType<ArasNameAttribute>().FirstOrDefault();
        if (nameAttr == null)
          throw new ArgumentNullException("itemType", "The name of the item type must be specified");
        itemType = nameAttr.Name;
      }
      return new InnovatorQuery<T>(new InnovatorQueryProvider(conn, settings)) { Type = itemType };
    }

    public static IReadOnlyResult<T> Apply<T>(this IQueryable<T> query)
    {
      var innQuery = query as InnovatorQuery<T>;
      if (innQuery == null)
        return new QueryWrapper<T>(query);
      return innQuery.Result;
    }
    public static IPromise<IReadOnlyResult<T>> ApplyAsync<T>(this IQueryable<T> query)
    {
      var innQuery = query as InnovatorQuery<T>;
      if (innQuery == null)
        return Promises.Resolved<IReadOnlyResult<T>>(new QueryWrapper<T>(query));
      return innQuery.GetResultAsync();
    }

    private class QueryWrapper<T> : IReadOnlyResult<T>
    {
      private IEnumerable<T> _query;

      public ServerException Exception { get { throw new NotSupportedException(); } }
      public IReadOnlyElement Message { get { throw new NotSupportedException(); } }
      public string Value { get { throw new NotSupportedException(); } }

      public QueryWrapper(IEnumerable<T> query)
      {
        _query = query;
      }

      public IReadOnlyItem AssertItem(string type = null)
      {
        throw new NotSupportedException();
      }

      public IEnumerable<IReadOnlyItem> AssertItems()
      {
        throw new NotSupportedException();
      }

      public IReadOnlyResult AssertNoError()
      {
        throw new NotSupportedException();
      }

      public IEnumerator<T> GetEnumerator()
      {
        return _query.GetEnumerator();
      }

      public IEnumerable<IReadOnlyItem> Items()
      {
        throw new NotSupportedException();
      }

      public void ToAml(XmlWriter writer, AmlWriterSettings settings)
      {
        throw new NotSupportedException();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }
    }
  }
}
#endif
