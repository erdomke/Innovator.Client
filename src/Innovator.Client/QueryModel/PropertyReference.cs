using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class PropertyReference : IOperand
  {
    public string Name { get; }
    public Table Table { get; }

    public PropertyReference(string name, Table table)
    {
      Name = name;
      Table = table;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
