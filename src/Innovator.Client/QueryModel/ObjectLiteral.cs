using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class ObjectLiteral : ILiteral
  {
    public object Value { get; }
    public PropertyReference TypeProvider { get; }

    public ObjectLiteral(object value, PropertyReference prop)
    {
      Value = value;
      TypeProvider = prop;
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
        && DateTime.TryParse(str, out DateTime date))
      {
        return new DateTimeLiteral(date);
      }
      else if ((dataType == null || dataType == "integer")
        && long.TryParse(str, out long lng))
      {
        return new IntegerLiteral(lng);
      }
      else if ((dataType == null || dataType == "float" || dataType == "decimal")
        && double.TryParse(str, out double dbl))
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
      return Value?.ToString();
    }
  }
}
