using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class Join : ITableOperand
  {
    public ITableOperand Left { get; set; }
    public ITableOperand Right { get; set; }
    public JoinType Type { get; set; }
    public IExpression Condition { get; set; }

    public void Visit(ITableVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
