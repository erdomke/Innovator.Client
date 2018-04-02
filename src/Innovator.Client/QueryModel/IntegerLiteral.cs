using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class IntegerLiteral : ILiteral, IEquatable<IntegerLiteral>
  {
    public long Value { get; set; }

    public IntegerLiteral(long value)
    {
      Value = value;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public override bool Equals(object obj)
    {
      if (obj is IntegerLiteral other)
        return Equals(other);
      return false;
    }

    public bool Equals(IntegerLiteral other)
    {
      return other?.Value == Value;
    }

    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }
  }
}
