using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class CloneVisitor : IQueryVisitor
  {
    private IExpression _clone;
    private QueryItem _query;
    private readonly Dictionary<QueryItem, QueryItem> _clones = new Dictionary<QueryItem, QueryItem>();

    public void Visit(QueryItem query)
    {
      var newQuery = new QueryItem(query.Context)
      {
        Alias = query.Alias,
        Fetch = query.Fetch,
        Offset = query.Offset,
        Type = query.Type,
        TypeProvider = query.TypeProvider
      };
      _clones[query] = newQuery;

      foreach (var join in query.Joins)
      {
        newQuery.Joins.Add(new Join()
        {
          Type = join.Type,
          Left = Clone(join.Left),
          Right = Clone(join.Right),
          Condition = Clone(join.Condition),
        });
      }

      foreach (var orderBy in query.OrderBy)
      {
        newQuery.OrderBy.Add(new OrderByExpression()
        {
          Ascending = orderBy.Ascending,
          Expression = Clone(orderBy.Expression)
        });
      }

      foreach (var select in query.Select)
      {
        newQuery.Select.Add(new SelectExpression()
        {
          Alias = select.Alias,
          Expression = Clone(select.Expression),
          OnlyReturnNonNull = select.OnlyReturnNonNull
        });
      }

      newQuery.Where = Clone(query.Where);
      _query = newQuery;
    }

    public void Visit(AndOperator op)
    {
      _clone = new AndOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(BetweenOperator op)
    {
      _clone = new BetweenOperator()
      {
        Left = Clone(op.Left),
        Min = Clone(op.Min),
        Max = Clone(op.Max)
      };
    }

    public void Visit(BooleanLiteral op)
    {
      _clone = new BooleanLiteral(op.Value);
    }

    public void Visit(DateTimeLiteral op)
    {
      _clone = new DateTimeLiteral(op.Value);
    }

    public void Visit(EqualsOperator op)
    {
      _clone = new EqualsOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(FloatLiteral op)
    {
      _clone = new FloatLiteral(op.Value);
    }

    public void Visit(FunctionExpression op)
    {
      var clone = new FunctionExpression()
      {
        Name = op.Name
      };
      foreach (var arg in op.Args)
        clone.Args.Add(Clone(arg));
      _clone = clone;
    }

    public void Visit(GreaterThanOperator op)
    {
      _clone = new GreaterThanOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      _clone = new GreaterThanOrEqualsOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(InOperator op)
    {
      _clone = new InOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(IntegerLiteral op)
    {
      _clone = new IntegerLiteral(op.Value);
    }

    public void Visit(IsOperator op)
    {
      _clone = new IsOperator()
      {
        Left = Clone(op.Left),
        Right = op.Right
      };
    }

    public void Visit(LessThanOperator op)
    {
      _clone = new LessThanOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      _clone = new LessThanOrEqualsOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(LikeOperator op)
    {
      _clone = new LikeOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(ListExpression op)
    {
      var clone = new ListExpression();
      foreach (var value in op.Values)
        clone.Values.Add(Clone(value));
      _clone = clone;
    }

    public void Visit(NotBetweenOperator op)
    {
      _clone = new NotBetweenOperator()
      {
        Left = Clone(op.Left),
        Min = Clone(op.Min),
        Max = Clone(op.Max)
      };
    }

    public void Visit(NotEqualsOperator op)
    {
      _clone = new NotEqualsOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(NotInOperator op)
    {
      _clone = new NotInOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(NotLikeOperator op)
    {
      _clone = new NotLikeOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(NotOperator op)
    {
      _clone = new NotOperator()
      {
        Arg = Clone(op.Arg)
      };
    }

    public void Visit(ObjectLiteral op)
    {
      _clone = new ObjectLiteral(op.Value, op.TypeProvider);
    }

    public void Visit(OrOperator op)
    {
      _clone = new OrOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(PropertyReference op)
    {
      _clone = new PropertyReference(op.Name, GetTable(op.Table));
    }

    public void Visit(StringLiteral op)
    {
      _clone = new StringLiteral(op.Value);
    }

    public void Visit(MultiplicationOperator op)
    {
      _clone = new MultiplicationOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(DivisionOperator op)
    {
      _clone = new DivisionOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(ModulusOperator op)
    {
      _clone = new ModulusOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(AdditionOperator op)
    {
      _clone = new AdditionOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(SubtractionOperator op)
    {
      _clone = new SubtractionOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(NegationOperator op)
    {
      _clone = new NegationOperator()
      {
        Arg = Clone(op.Arg)
      };
    }

    public void Visit(ConcatenationOperator op)
    {
      _clone = new ConcatenationOperator()
      {
        Left = Clone(op.Left),
        Right = Clone(op.Right)
      };
    }

    public void Visit(ParameterReference op)
    {
      _clone = new ParameterReference(op.Name, op.IsRaw);
    }

    public void Visit(AllProperties op)
    {
      _clone = new AllProperties(op.Table) { XProperties = op.XProperties };
    }

    private QueryItem GetTable(QueryItem orig)
    {
      if (_clones.TryGetValue(orig, out var result))
        return result;
      return orig;
    }

    private T Clone<T>(T expr) where T : IExpression
    {
      expr.Visit(this);
      return (T)_clone;
    }

    private QueryItem Clone(QueryItem query)
    {
      if (_clones.TryGetValue(query, out var result))
        return result;
      Visit(query);
      return _query;
    }
  }
}
