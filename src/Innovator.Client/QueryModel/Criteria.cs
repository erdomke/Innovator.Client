using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class Criteria
  {
    private readonly SimpleSearchParser _parser;

    public PropertyReference Property { get; }
    public Condition Condition { get; }
    public object Value { get; }

    public Criteria(PropertyReference property, Condition condition, object value, SimpleSearchParser parser)
    {
      Property = property;
      Condition = condition;
      Value = value;
      _parser = parser;
    }

  }
}
