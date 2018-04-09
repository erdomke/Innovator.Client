using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class StringLiteral : ILiteral, IEquatable<StringLiteral>
  {
    public string Value { get; set; }

    public StringLiteral(string value)
    {
      Value = value;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public override bool Equals(object obj)
    {
      if (obj is StringLiteral other)
        return Equals(other);
      return false;
    }

    public bool Equals(StringLiteral other)
    {
      return other?.Value == Value;
    }

    public override int GetHashCode()
    {
      return Value?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
      return Value;
    }
  }
}
