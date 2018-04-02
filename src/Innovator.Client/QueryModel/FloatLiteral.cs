using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class FloatLiteral : ILiteral, IEquatable<FloatLiteral>
  {
    public double Value { get; set; }

    public FloatLiteral(double value)
    {
      Value = value;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public override bool Equals(object obj)
    {
      if (obj is FloatLiteral other)
        return Equals(other);
      return false;
    }

    public bool Equals(FloatLiteral other)
    {
      return other?.Value == Value;
    }

    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }

    public static bool TryGetNumberLiteral(string value, out ILiteral literal)
    {
      if (long.TryParse(value, out long lng))
      {
        literal = new IntegerLiteral(lng);
        return true;
      }
      else if (double.TryParse(value, out double dbl))
      {
        literal = new FloatLiteral(dbl);
        return true;
      }
      literal = null;
      return false;
    }
  }
}
