using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class PropertyReference : IOperand, ITableProvider
  {
    public string Name { get; }
    public QueryItem Table { get; set; }

    public PropertyReference(string name, QueryItem table)
    {
      Name = name;
      Table = table;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public override string ToString()
    {
      return this.ToSqlString();
    }
  }
}
