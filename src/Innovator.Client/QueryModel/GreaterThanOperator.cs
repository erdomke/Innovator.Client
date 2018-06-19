using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class GreaterThanOperator : BinaryOperator, IBooleanOperator, INormalize
  {
    public override int Precedence => (int)PrecedenceLevel.Comparison;
    QueryItem ITableProvider.Table { get; set; }

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public IExpression Normalize()
    {
      if (Right is PropertyReference && !(Left is PropertyReference))
      {
        return new LessThanOperator()
        {
          Left = Right,
          Right = Left
        }.Normalize();
      }
      else if (Left is Functions.IndexOf_Zero indexZero && Right is IntegerLiteral intZero && intZero.Value == -1)
      {
        return new Functions.Contains()
        {
          String = indexZero.Target,
          Find = indexZero.String
        };
      }
      else if (Left is Functions.IndexOf_One indexOne && Right is IntegerLiteral intOne && intOne.Value == 0)
      {
        return new Functions.Contains()
        {
          String = indexOne.Target,
          Find = indexOne.String
        };
      }

      SetTable();
      return this;
    }
  }
}
