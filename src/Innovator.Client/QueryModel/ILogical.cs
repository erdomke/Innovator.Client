using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public interface ILogical : IOperator
  {
    void Add(IExpression child);
  }
}
