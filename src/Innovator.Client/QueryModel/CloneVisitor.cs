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
    private PropertyReference _prop;
    private QueryItem _query;
    private readonly Dictionary<QueryItem, QueryItem> _clones = new Dictionary<QueryItem, QueryItem>();
    private Func<PropertyReference, IExpression> _propMapper;
    private Func<PropertyReference, ILiteral, IExpression> _valueMapper;

    public CloneVisitor WithPropertyMapper(Func<PropertyReference, IExpression> propMapper)
    {
      _propMapper = propMapper;
      return this;
    }

    public CloneVisitor WithValueMapper(Func<PropertyReference, ILiteral, IExpression> valueMapper)
    {
      _valueMapper = valueMapper;
      return this;
    }

    public void Visit(QueryItem query)
    {
      var newQuery = new QueryItem(query.Context)
      {
        Alias = query.Alias,
        Fetch = query.Fetch,
        Offset = query.Offset,
        Type = query.Type,
      };
      _clones[query] = newQuery;

      if (query.Version is CurrentVersion)
        newQuery.Version = new CurrentVersion();
      else if (query.Version is LatestMatch latest)
        newQuery.Version = new LatestMatch() { AsOf = latest.AsOf };
      else if (query.Version is VersionCriteria crit)
        newQuery.Version = new VersionCriteria() { Condition = CloneAndReturn(crit.Condition) };
      else if (query.Version is LastVersionOfId last)
        newQuery.Version = new LastVersionOfId() { Id = last.Id };

      foreach (var attr in query.Attributes)
        newQuery.Attributes.Add(attr);

      if (query.TypeProvider != null)
        newQuery.TypeProvider = Clone(query.TypeProvider) as PropertyReference;

      foreach (var join in query.Joins
        .Select(Clone)
        .Where(j => j != null && !(j.Condition is IgnoreNode)))
      {
        newQuery.Joins.Add(join);
      }

      foreach (var orderBy in query.OrderBy
        .Select(Clone)
        .Where(o => o != null && !(o.Expression is IgnoreNode)))
      {
        newQuery.OrderBy.Add(orderBy);
      }

      foreach (var select in query.Select
        .Select(Clone)
        .Where(s => s != null && !(s.Expression is IgnoreNode)))
      {
        newQuery.Select.Add(select);
      }

      if (query.Where != null)
        newQuery.Where = CloneAndReturn(query.Where);
      _query = newQuery;
    }

    public virtual Join Clone(Join join)
    {
      return new Join()
      {
        Type = join.Type,
        Left = Clone(join.Left),
        Right = Clone(join.Right),
        Condition = CloneAndReturn(join.Condition),
      };
    }

    public virtual OrderByExpression Clone(OrderByExpression orderBy)
    {
      return new OrderByExpression()
      {
        Ascending = orderBy.Ascending,
        Expression = CloneAndReturn(orderBy.Expression)
      };
    }

    public virtual SelectExpression Clone(SelectExpression select)
    {
      return new SelectExpression()
      {
        Alias = select.Alias,
        Expression = CloneAndReturn(select.Expression),
        OnlyReturnNonNull = select.OnlyReturnNonNull
      };
    }

    void IExpressionVisitor.Visit(AndOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(AndOperator op)
    {
      return new AndOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneAndReturn(op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(BetweenOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(BetweenOperator op)
    {
      return new BetweenOperator()
      {
        Left = CloneAndReturn(op.Left),
        Min = CloneValue(op.Left, op.Min),
        Max = CloneValue(op.Left, op.Max)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(BooleanLiteral op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(BooleanLiteral op)
    {
      return new BooleanLiteral(op.Value);
    }

    void IExpressionVisitor.Visit(CountAggregate op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(CountAggregate op)
    {
      var result = new CountAggregate();
      foreach (var table in op.TablePath)
        result.TablePath.Add(GetTable(table));
      return result;
    }

    void IExpressionVisitor.Visit(DateTimeLiteral op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(DateTimeLiteral op)
    {
      return new DateTimeLiteral(op.Value);
    }

    void IExpressionVisitor.Visit(EqualsOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(EqualsOperator op)
    {
      return new EqualsOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(FloatLiteral op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(FloatLiteral op)
    {
      return new FloatLiteral(op.Value);
    }

    void IExpressionVisitor.Visit(FunctionExpression op)
    {
      _clone = op.Clone(CloneAndReturn);
    }

    void IExpressionVisitor.Visit(GreaterThanOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(GreaterThanOperator op)
    {
      return new GreaterThanOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(GreaterThanOrEqualsOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(GreaterThanOrEqualsOperator op)
    {
      return new GreaterThanOrEqualsOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(InOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(InOperator op)
    {
      _prop = op.Left as PropertyReference;
      return new InOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneAndReturn(op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(IntegerLiteral op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(IntegerLiteral op)
    {
      return new IntegerLiteral(op.Value);
    }

    void IExpressionVisitor.Visit(IsOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(IsOperator op)
    {
      return new IsOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = op.Right
      }.Normalize();
    }

    void IExpressionVisitor.Visit(LessThanOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(LessThanOperator op)
    {
      return new LessThanOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(LessThanOrEqualsOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(LessThanOrEqualsOperator op)
    {
      return new LessThanOrEqualsOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(LikeOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(LikeOperator op)
    {
      return new LikeOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(ListExpression op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(ListExpression op)
    {
      var clone = new ListExpression();
      foreach (var value in op.Values)
        clone.Values.Add((IOperand)CloneValue(_prop, value));
      return clone;
    }

    void IExpressionVisitor.Visit(NotEqualsOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(NotEqualsOperator op)
    {
      return new NotEqualsOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(NotOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(NotOperator op)
    {
      return new NotOperator()
      {
        Arg = CloneAndReturn(op.Arg)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(ObjectLiteral op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(ObjectLiteral op)
    {
      return new ObjectLiteral(op.Value, Clone(op.TypeProvider) as PropertyReference, op.Context);
    }

    void IExpressionVisitor.Visit(OrOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(OrOperator op)
    {
      return new OrOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneAndReturn(op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(PropertyReference op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(PropertyReference op)
    {
      var result = new PropertyReference(op.Name, GetTable(op.Table));
      if (_propMapper != null)
        return _propMapper(result);
      return result;
    }

    void IExpressionVisitor.Visit(StringLiteral op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(StringLiteral op)
    {
      return new StringLiteral(op.Value);
    }

    void IExpressionVisitor.Visit(MultiplicationOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(MultiplicationOperator op)
    {
      return new MultiplicationOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(DivisionOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(DivisionOperator op)
    {
      return new DivisionOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(ModulusOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(ModulusOperator op)
    {
      return new ModulusOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(AdditionOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(AdditionOperator op)
    {
      return new AdditionOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(SubtractionOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(SubtractionOperator op)
    {
      return new SubtractionOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(NegationOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(NegationOperator op)
    {
      return new NegationOperator()
      {
        Arg = CloneAndReturn(op.Arg)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(ConcatenationOperator op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(ConcatenationOperator op)
    {
      return new ConcatenationOperator()
      {
        Left = CloneAndReturn(op.Left),
        Right = CloneValue(op.Left, op.Right)
      }.Normalize();
    }

    void IExpressionVisitor.Visit(ParameterReference op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(ParameterReference op)
    {
      return new ParameterReference(op.Name, op.IsRaw);
    }

    void IExpressionVisitor.Visit(AllProperties op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(AllProperties op)
    {
      return new AllProperties(GetTable(op.Table))
      {
        XProperties = op.XProperties
      };
    }

    void IExpressionVisitor.Visit(PatternList op)
    {
      _clone = Clone(op);
    }

    public virtual IExpression Clone(PatternList op)
    {
      return op.Clone();
    }

    private QueryItem GetTable(QueryItem orig)
    {
      if (_clones.TryGetValue(orig, out var result))
        return result;
      return orig;
    }

    private IExpression CloneValue(IExpression left, IExpression right)
    {
      if (left is PropertyReference prop && right is ILiteral literal)
        return ClonePropertyValue(prop, literal);
      else
        return CloneAndReturn(right);
    }

    protected virtual IExpression ClonePropertyValue(PropertyReference prop, ILiteral literal)
    {
      var result = CloneAndReturn(literal);
      if (_valueMapper != null)
        return _valueMapper(prop, result);
      return result;
    }

    private T CloneAndReturn<T>(T expr) where T : IExpression
    {
      expr.Visit(this);
      return (T)_clone;
    }

    public QueryItem Clone(QueryItem query)
    {
      if (_clones.TryGetValue(query, out var result))
        return result;
      Visit(query);
      return _query;
    }

  }
}
