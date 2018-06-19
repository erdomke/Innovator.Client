using System.Diagnostics;
using System.Linq;

namespace Innovator.Client.QueryModel
{
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public class Join
  {
    public QueryItem Left { get; set; }
    public QueryItem Right { get; set; }
    public JoinType Type { get; set; }
    public IExpression Condition { get; set; }

    private string DebuggerDisplay
    {
      get
      {
        using (var writer = new System.IO.StringWriter())
        {
          if (Type == JoinType.Inner)
            writer.Write("inner join ");
          else
            writer.Write("left join ");

          var visitor = new SqlServerVisitor(writer, new NullAmlSqlWriterSettings());
          Condition?.Visit(visitor);
          writer.Flush();
          return writer.ToString();
        }
      }
    }

    public Cardinality GetCardinality()
    {
      if (Condition is EqualsOperator eq)
      {
        var rightPropRef = new[] { eq.Left, eq.Right }
          .OfType<PropertyReference>()
          .FirstOrDefault(p => p.Table == Right);
        if (rightPropRef?.Name == "id")
          return Cardinality.OneToOne;
      }

      return Cardinality.OneToMany;
    }
  }
}
