using System.Collections;
using System.Collections.Generic;

namespace Innovator.Client.Queryable
{
  public interface IQueryResult : IReadOnlyResult, IEnumerable { }

#if NET35
  public interface IQueryResult<T>
#else
  public interface IQueryResult<out T>
#endif
    : IQueryResult, IEnumerable<T>
  {
  }
}
