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

    public override string ToString()
    {
      return ElementFactory.Local.LocalizationContext.Format(Value);
    }

    public bool? AsBoolean()
    {
      throw new InvalidCastException();
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
      return ElementFactory.Local.LocalizationContext.AsDouble(Value);
    }

    public Guid? AsGuid()
    {
      throw new InvalidCastException();
    }

    public int? AsInt()
    {
      return ElementFactory.Local.LocalizationContext.AsInt(Value);
    }

    public long? AsLong()
    {
      return ElementFactory.Local.LocalizationContext.AsLong(Value);
    }

    public string AsString(string defaultValue)
    {
      return ElementFactory.Local.LocalizationContext.Format(Value) ?? defaultValue;
    }
  }
}
