using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class ObjectLiteral : ILiteral
  {
    public IServerContext Context { get; }
    public PropertyReference TypeProvider { get; }
    public object Value { get; }

    public ObjectLiteral(object value, PropertyReference prop, IServerContext context)
    {
      Context = context;
      TypeProvider = prop;
      Value = value;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public ILiteral Normalize(IQueryWriterSettings settings)
    {
      if (!(Value is string))
      {
        if (Expressions.TryGetLiteral(Value, out var lit))
          return lit;
        else
          throw new NotSupportedException();
      }

      var dataType = default(string);
      if (!string.IsNullOrEmpty(TypeProvider?.Table.Type))
      {
        var props = settings.GetProperties(TypeProvider?.Table.Type);
        if (props != null && props.TryGetValue(TypeProvider.Name, out var propDefn))
        {
          dataType = propDefn.DataType().Value;
          if (dataType == "foreign")
            dataType = null;
        }
      }

      var str = (string)Value;
      if (dataType == "boolean")
      {
        return new BooleanLiteral(str == "1");
      }
      else if ((dataType == null || dataType == "date")
        && Context.TryParseDateTime(str, out var date)
        && date.HasValue)
      {
        return new DateTimeLiteral(date.Value.LocalDateTime);
      }
      else if ((dataType == null || dataType == "integer")
        && long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long lng))
      {
        return new IntegerLiteral(lng);
      }
      else if ((dataType == null || dataType == "float" || dataType == "decimal")
        && double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double dbl))
      {
        return new FloatLiteral(dbl);
      }
      else
      {
        return new StringLiteral(str);
      }
    }

    public override string ToString()
    {
      return ElementFactory.Local.LocalizationContext.Format(Value);
    }

    public bool? AsBoolean()
    {
      return ElementFactory.Local.LocalizationContext.AsBoolean(Value);
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
      return ElementFactory.Local.LocalizationContext.AsDouble(Value);
    }

    public Guid? AsGuid()
    {
      if (Value == null) return null;
      return new Guid(Value.ToString());
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
