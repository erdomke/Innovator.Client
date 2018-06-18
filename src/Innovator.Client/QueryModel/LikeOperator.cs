using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class LikeOperator : BinaryOperator, IBooleanOperator, INormalize
  {
    public override int Precedence => (int)PrecedenceLevel.Comparison;
    QueryItem ITableProvider.Table { get; set; }

    public virtual IExpression Normalize()
    {
      if (Right is PatternList pattern
        && pattern.Patterns.Count == 1
        && pattern.Patterns[0].Matches.Count == 3
        && pattern.Patterns[0].Matches[0] is Anchor start
        && start.Type == AnchorType.Start_Absolute
        && pattern.Patterns[0].Matches[1] is StringMatch str
        && pattern.Patterns[0].Matches[2] is Anchor end
        && end.Type == AnchorType.End_Absolute)
      {
        return new EqualsOperator()
        {
          Left = Left,
          Right = new StringLiteral(str.ToString())
        };
      }

      SetTable();
      return this;
    }

    public override void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal static IExpression FromMethod(string name, IExpression left, IExpression right)
    {
      if (right is StringLiteral str)
      {
        switch (name.ToLowerInvariant())
        {
          case "startswith":
            right = new PatternList()
            {
              Patterns =
              {
                new Pattern() {
                  Matches =
                  {
                    new Anchor() { Type = AnchorType.Start_Absolute },
                    new StringMatch(str.Value)
                  }
                }
              }
            };
            break;
          case "endswith":
            right = new PatternList()
            {
              Patterns =
              {
                new Pattern() {
                  Matches =
                  {
                    new StringMatch(str.Value),
                    new Anchor() { Type = AnchorType.End_Absolute }
                  }
                }
              }
            };
            break;
          case "contains":
            right = new PatternList()
            {
              Patterns =
              {
                new Pattern() {
                  Matches = { new StringMatch(str.Value) }
                }
              }
            };
            break;
          default:
            throw new NotSupportedException();
        }

        return new LikeOperator() { Left = left, Right = right }.Normalize();
      }
      else
      {
        switch (name.ToLowerInvariant())
        {
          case "startswith":
            return new Functions.StartsWith()
            {
              String = left,
              Find = right
            };
          case "endswith":
            return new Functions.EndsWith()
            {
              String = left,
              Find = right
            };
          case "contains":
            return new Functions.Contains()
            {
              String = left,
              Find = right
            };
          default:
            throw new NotSupportedException();
        }
      }
    }
  }
}
