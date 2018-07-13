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
      var inBrackets = false;
      var param = new ParameterReference();

      switch (value[0])
      {
        case '@':
          start = 1;
          if (value[start] == '{')
          {
            start++;
            inBrackets = true;
          }
          break;
        case '$':
          if (value[1] != '{')
            return null;
          start = 2;
          inBrackets = true;
          break;
        case '{':
          start = 1;
          inBrackets = true;
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

      if (inBrackets)
      {
        if (value[end - 1] != '}')
          return null;
        end--;
      }

      param.Name = value.Substring(start, end - start);
      if (int.TryParse(param.Name, out var integer))
        return param;

      if (inBrackets)
      {
        // Handle possible XPath and XML characters
        if (param.Name.StartsWith("Item/") && param.Name.Length > 5)
          param.Name = param.Name.Substring(5);

        start = 0;
        if (param.Name[0] == '@' && param.Name.Length > 1)
          start++;

        if (!char.IsLetter(param.Name[start]) && param.Name[start] != '_')
          return null;
        start++;
        for (var i = start; i < param.Name.Length; i++)
        {
          if (!char.IsLetterOrDigit(param.Name[i])
            && param.Name[i] != '_'
            && param.Name[i] != '-'
            && param.Name[i] != '.')
          {
            return null;
          }
        }
      }
      else
      {
        start = 0;
        if (!char.IsLetter(param.Name[start]) && param.Name[start] != '_')
          return null;
        start++;
        for (var i = start; i < param.Name.Length; i++)
        {
          if (!char.IsLetterOrDigit(param.Name[i]) && param.Name[i] != '_')
            return null;
        }
      }
      return param;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public bool? AsBoolean()
    {
      return ElementFactory.Local.LocalizationContext.AsBoolean(DefaultValue);
    }

    public DateTime? AsDateTime()
    {
      return ElementFactory.Local.LocalizationContext.AsDateTime(DefaultValue);
    }

    public DateTime? AsDateTimeUtc()
    {
      return ElementFactory.Local.LocalizationContext.AsDateTimeUtc(DefaultValue);
    }

    public double? AsDouble()
    {
      return ElementFactory.Local.LocalizationContext.AsDouble(DefaultValue);
    }

    public Guid? AsGuid()
    {
      if (DefaultValue == null) return null;
      return new Guid(DefaultValue.ToString());
    }

    public int? AsInt()
    {
      return ElementFactory.Local.LocalizationContext.AsInt(DefaultValue);
    }

    public long? AsLong()
    {
      return ElementFactory.Local.LocalizationContext.AsLong(DefaultValue);
    }

    public string AsString(string defaultValue)
    {
      return ElementFactory.Local.LocalizationContext.Format(DefaultValue) ?? defaultValue;
    }

    public object AsClrValue()
    {
      return DefaultValue;
    }
  }
}
