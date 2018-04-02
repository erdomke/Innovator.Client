using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class Table : ITableOperand
  {
    private List<IExpression> _args = new List<IExpression>();

    public string Alias { get; set; }
    public IList<IExpression> Args { get { return _args; } }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public PropertyReference TypeProvider { get; set; }

    public void Visit(ITableVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
