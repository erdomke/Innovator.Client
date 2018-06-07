using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class BooleanLiteral : ILiteral, IEquatable<BooleanLiteral>
  {
    public bool Value { get; set; }

    public BooleanLiteral(bool value)
    {
      Value = value;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public override bool Equals(object obj)
    {
      if (obj is BooleanLiteral other)
        return Equals(other);
      return false;
    }

    public bool Equals(BooleanLiteral other)
    {
      return other?.Value == Value;
    }

    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }

    public override string ToString()
    {
      return ElementFactory.Local.LocalizationContext.Format(Value);
    }

    public bool? AsBoolean()
    {
      return Value;
    }

    public DateTime? AsDateTime()
    {
      throw new InvalidCastException();
    }

    public DateTime? AsDateTimeUtc()
    {
      throw new InvalidCastException();
    }

    public double? AsDouble()
    {
      throw new InvalidCastException();
    }

    public Guid? AsGuid()
    {
      throw new InvalidCastException();
    }

    public int? AsInt()
    {
      throw new InvalidCastException();
    }

    public long? AsLong()
    {
      throw new InvalidCastException();
    }

    public string AsString(string defaultValue)
    {
      return ElementFactory.Local.LocalizationContext.Format(Value) ?? defaultValue;
    }
  }
}
