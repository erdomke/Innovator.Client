using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public interface ITableVisitor
  {
    void Visit(Join op);
    void Visit(Table op);
  }
}
