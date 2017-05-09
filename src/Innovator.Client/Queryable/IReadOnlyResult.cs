using System.Collections.Generic;

namespace Innovator.Client.Queryable
{
#if NET35
  public interface IReadOnlyResult<T> 
#else
	public interface IReadOnlyResult<out T> 
#endif
		: IReadOnlyResult, IEnumerable<T>
  {
  }
}
