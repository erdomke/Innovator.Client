#if REFLECTION
using System.Linq;

namespace Innovator.Client.Queryable
{
  internal interface IInnovatorQuery : IQueryable
  {
    string Type { get; set; }
  }
}
#endif