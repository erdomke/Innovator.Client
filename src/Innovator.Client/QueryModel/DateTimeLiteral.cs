using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class DateTimeLiteral : ILiteral
  {
    public virtual DateTime Value { get; set; }

    protected DateTimeLiteral() { }
    public DateTimeLiteral(DateTime value)
    {
      Value = value;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
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
      return ElementFactory.Local.LocalizationContext.AsDateTime(Value);
    }

    public DateTime? AsDateTimeUtc()
    {
      return ElementFactory.Local.LocalizationContext.AsDateTimeUtc(Value);
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
