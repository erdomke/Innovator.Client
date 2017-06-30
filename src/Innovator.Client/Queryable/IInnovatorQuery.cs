#if REFLECTION
using System.Linq;

namespace Innovator.Client.Queryable
{
  public interface IInnovatorQuery : IQueryable, IAmlNode
  {
    string ItemType { get; }

    IQueryResult Apply();
    IPromise<IQueryResult> ApplyAsync();
    IInnovatorQuery Include(string path);
  }
}
#endif
