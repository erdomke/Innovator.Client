using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class Join
  {
    public QueryItem Left { get; set; }
    public QueryItem Right { get; set; }
    public JoinType Type { get; set; }
    public IExpression Condition { get; set; }

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
