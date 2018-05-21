using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class AllProperties : IOperand
  {
    public QueryItem Table { get; }
    public bool XProperties { get; set; }

    public AllProperties(QueryItem table)
    {
      Table = table;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
