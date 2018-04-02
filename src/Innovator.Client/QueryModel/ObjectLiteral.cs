using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class ObjectLiteral : ILiteral
  {
    public string Value { get; }
    public PropertyReference TypeProvider { get; }

    public ObjectLiteral(string value, PropertyReference prop)
    {
      Value = value;
      TypeProvider = prop;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
