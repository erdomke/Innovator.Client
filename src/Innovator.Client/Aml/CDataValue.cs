using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  public struct CDataValue
  {
    private string _value;

    public CDataValue(string value)
    {
      _value = value;
    }

    public override string ToString()
    {
      return _value;
    }

    public static implicit operator string(CDataValue value)
    {
      return value._value;
    }
  }
}
