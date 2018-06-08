using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class ParameterReference : ILiteral
  {
    public object DefaultValue { get; set; }
    public bool IsRaw { get; private set; }
    public string Name { get; internal set; }

    internal ParameterReference() { }

    public ParameterReference(string name, bool isRaw)
    {
      Name = name;
      IsRaw = IsRaw;
    }

    public static ParameterReference TryCreate(string value)
    {
      if (value == null || value.Length < 2) return null;

      var start = 0;
      var closingBracket = false;
      var param = new ParameterReference();

      switch (value[0])
      {
        case '@':
          start = 1;
          if (value[start] == '{')
          {
            start++;
            closingBracket = true;
          }
          break;
        case '$':
          if (value[1] != '{')
            return null;
          start = 2;
          closingBracket = true;
          break;
        case '{':
          start = 1;
          if (value[start] == '@')
            start++;
          closingBracket = true;
          break;
        default:
          return null;
      }

      var end = value.Length;
      if (value[value.Length - 1] == '!')
      {
        param.IsRaw = true;
        end--;
      }

      if (closingBracket)
      {
        if (value[end - 1] != '}')
          return null;
        end--;
      }

      for (var i = start; i < end; i++)
      {
        if (!char.IsLetterOrDigit(value[i]) && value[i] != '_') return null;
      }
      param.Name = value.Substring(start, end - start);
      return param;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    bool? ICoercible.AsBoolean()
    {
      throw new NotSupportedException();
    }

    DateTime? ICoercible.AsDateTime()
    {
      throw new NotSupportedException();
    }

    DateTime? ICoercible.AsDateTimeUtc()
    {
      throw new NotSupportedException();
    }

    double? ICoercible.AsDouble()
    {
      throw new NotSupportedException();
    }

    Guid? ICoercible.AsGuid()
    {
      throw new NotSupportedException();
    }

    int? ICoercible.AsInt()
    {
      throw new NotSupportedException();
    }

    long? ICoercible.AsLong()
    {
      throw new NotSupportedException();
    }

    string ICoercible.AsString(string defaultValue)
    {
      throw new NotSupportedException();
    }
  }
}
