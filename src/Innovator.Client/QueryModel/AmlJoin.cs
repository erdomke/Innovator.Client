using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal class AmlJoin
  {
    public PropertyReference CurrentProp { get; private set; }
    public PropertyReference OtherProp { get; private set; }
    public QueryItem Table { get; private set; }

    private AmlJoin() { }

    public bool IsRelationship()
    {
      return CurrentProp.Name == "id" && OtherProp.Name == "source_id";
    }

    public bool IsItemProperty()
    {
      return OtherProp.Name == "id";
    }

    public bool HasCriteria()
    {
      return Table.Where != null;
    }

    public static AmlJoin Create(QueryItem parent, Join join)
    {
      if (TryCreate(parent, join, out var result))
        return result;
      throw new NotSupportedException();
    }

    public static bool TryCreate(QueryItem parent, Join join, out AmlJoin amlJoin)
    {
      amlJoin = new AmlJoin();
      if (!(join.Condition is EqualsOperator eq))
        return false;
      var props = new[] { eq.Left, eq.Right }
        .OfType<PropertyReference>()
        .ToArray();
      if (props.Length != 2)
        return false;

      amlJoin.CurrentProp = props.Single(p => ReferenceEquals(p.Table, parent));
      amlJoin.OtherProp = props.Single(p => !ReferenceEquals(p.Table, parent));
      amlJoin.Table = amlJoin.OtherProp.Table;
      return true;
    }
  }
}
