using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal class NullLiteral : ILiteral
  {
    public void Visit(IExpressionVisitor visitor)
    {
      throw new NotSupportedException();
    }

    bool? ICoercible.AsBoolean()
    {
      return null;
    }

    DateTime? ICoercible.AsDateTime()
    {
      return null;
    }

    DateTime? ICoercible.AsDateTimeUtc()
    {
      return null;
    }

    double? ICoercible.AsDouble()
    {
      return null;
    }

    Guid? ICoercible.AsGuid()
    {
      return null;
    }

    int? ICoercible.AsInt()
    {
      return null;
    }

    long? ICoercible.AsLong()
    {
      return null;
    }

    string ICoercible.AsString(string defaultValue)
    {
      return defaultValue;
    }

    public object AsClrValue()
    {
      return null;
    }
  }
}
