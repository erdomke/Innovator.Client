using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  public interface IAmlXPath
  {
    object Evaluate(string expression);
    IReadOnlyElement SelectElement(string expression);
    IEnumerable<IReadOnlyElement> SelectElements(string expression);
  }
}
