using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public interface IQueryVisitor : IExpressionVisitor, ITableVisitor
  {
    void Visit(Query query);
  }
}
