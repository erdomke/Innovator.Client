using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class PropertyReference : IOperand, ITableProvider, IEquatable<PropertyReference>
  {
    public string Name { get; }
    public QueryItem Table { get; set; }

    public PropertyReference(string name, QueryItem table)
    {
      Name = name;
      Table = table;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public override string ToString()
    {
      return this.ToSqlString();
    }

    internal QueryItem GetOrAddTable(IServerContext context)
    {
      var join = Table.Joins.FirstOrDefault(j => j.Condition is EqualsOperator eq
        && new[] { eq.Left, eq.Right }.OfType<PropertyReference>()
          .Any(p => p.Table == Table && p.Name == Name));
      if (join != null)
        return join.Right;

      var newTable = new QueryItem(context)
      {
        TypeProvider = this
      };
      Table.Joins.Add(new Join()
      {
        Left = Table,
        Right = newTable,
        Condition = new EqualsOperator()
        {
          Left = this,
          Right = new PropertyReference("id", newTable)
        },
        Type = JoinType.LeftOuter
      });
      return newTable;
    }

    public override bool Equals(object obj)
    {
      if (obj is PropertyReference prop)
        return Equals(prop);
      return false;
    }

    public bool Equals(PropertyReference other)
    {
      if (other == null)
        return false;
      return string.Equals(other.Name, Name, StringComparison.OrdinalIgnoreCase)
        && other.Table == Table;
    }

    public override int GetHashCode()
    {
      return (Name?.GetHashCode() ?? 0)
        ^ (Table?.GetHashCode() ?? 0);
    }

    public static bool operator ==(PropertyReference left, PropertyReference right)
    {
      if (ReferenceEquals(left, right))
        return true;

      if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
        return false;

      return left.Equals(right);
    }

    public static bool operator !=(PropertyReference left, PropertyReference right)
    {
      return !(left == right);
    }
  }
}
