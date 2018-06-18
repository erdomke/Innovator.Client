using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal class SimpleSearchVisitor : IExpressionVisitor
  {
    private SimpleSearchParser _parser;
    private List<Criteria> _criteria = new List<Criteria>();

    public IList<Criteria> Criteria { get { return _criteria; } }

    public SimpleSearchVisitor(SimpleSearchParser parser)
    {
      _parser = parser;
    }

    public void Visit(AndOperator op)
    {
      if (Criteria.Count > 0)
        throw new NotSupportedException();
      foreach (var criteria in BinaryOperator.Flatten(op))
      {
        criteria.Visit(this);
      }
    }

    public void Visit(BetweenOperator op)
    {
      Visit(op, false);
    }

    private void Visit(BetweenOperator op, bool not)
    {
      if (!(op.Left is PropertyReference prop
        && TryGetLiteralValue(op.Min, out var min)
        && TryGetLiteralValue(op.Max, out var max)))
      {
        throw new NotSupportedException();
      }

      var condition = not ? Condition.NotBetween : Condition.Between;

      if (min is long minLng && max is long maxLng)
      {
        if (minLng <= int.MaxValue && minLng >= int.MinValue
          && maxLng <= int.MaxValue && maxLng >= int.MinValue)
        {
          var minInt = (int)minLng;
          var maxInt = (int)maxLng;
          AddCriteria(prop, condition, new Range<int>(minInt, maxInt));
        }
        else
        {
          AddCriteria(prop, condition, new Range<long>(minLng, maxLng));
        }
      }
      else if (min is double minDbl && max is double maxDbl)
      {
        AddCriteria(prop, condition, new Range<double>(minDbl, maxDbl));
      }
      else if (min is ZonedDateTime minDt && max is ZonedDateTime maxDt)
      {
        AddCriteria(prop, condition, new Range<ZonedDateTime>(minDt, maxDt));
      }
      else if (min is DateOffset minDo && max is DateOffset maxDo)
      {
        AddCriteria(prop, condition, new Range<DateOffset>(minDo, maxDo));
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    public void Visit(BooleanLiteral op)
    {
      throw new InvalidOperationException();
    }

    public void Visit(DateTimeLiteral op)
    {
      throw new InvalidOperationException();
    }

    public void Visit(EqualsOperator op)
    {
      HandleBinary(op, Condition.Equal);
    }

    public void Visit(FloatLiteral op)
    {
      throw new NotSupportedException();
    }

    public void Visit(FunctionExpression op)
    {
      throw new NotSupportedException();
    }

    public void Visit(GreaterThanOperator op)
    {
      HandleBinary(op, Condition.GreaterThan);
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      HandleBinary(op, Condition.GreaterThanEqual);
    }

    public void Visit(InOperator op)
    {
      Visit(op, false);
    }

    private void Visit(InOperator op, bool not)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      var values = new List<object>();

      foreach (var valueExpr in op.Right.Values)
      {
        if (!TryGetLiteralValue(valueExpr, out var value))
          throw new NotSupportedException();
        values.Add(value);
      }

      AddCriteria(prop, not ? Condition.NotEqual : Condition.Equal, values.ToArray());
    }

    public void Visit(IntegerLiteral op)
    {
      throw new NotSupportedException();
    }

    public void Visit(IsOperator op)
    {
      if (!(op.Left is PropertyReference prop))
        throw new NotSupportedException();
      switch (op.Right)
      {
        case IsOperand.NotNull:
        case IsOperand.Defined:
          AddCriteria(prop, Condition.IsNotNull, null);
          break;
        default:
          AddCriteria(prop, Condition.IsNull, null);
          break;
      }
    }

    public void Visit(LessThanOperator op)
    {
      HandleBinary(op, Condition.LessThan);
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      HandleBinary(op, Condition.LessThanEqual);
    }

    public void Visit(LikeOperator op)
    {
      HandleBinary(op, Condition.Like);
    }

    public void Visit(ListExpression op)
    {
      throw new NotSupportedException();
    }

    public void Visit(NotEqualsOperator op)
    {
      HandleBinary(op, Condition.NotEqual);
    }

    public void Visit(NotOperator op)
    {
      if (op.Arg is InOperator inOp)
        Visit(inOp, true);
      else if (op.Arg is BetweenOperator btwOp)
        Visit(btwOp, true);
      else if (op.Arg is LikeOperator likeOp)
        HandleBinary(likeOp, Condition.NotLike);
      else if (op.Arg is EqualsOperator eqOp)
        HandleBinary(eqOp, Condition.NotEqual);
      else if (op.Arg is OrOperator orOp)
        Visit(orOp, true);
      else
        throw new NotSupportedException();
    }

    public void Visit(ObjectLiteral op)
    {
      throw new NotSupportedException();
    }

    public void Visit(OrOperator op)
    {
      Visit(op, false);
    }

    private void Visit(OrOperator op, bool not)
    {
      var props = new HashSet<PropertyReference>();
      var origList = _criteria;
      var orCriteria = new List<Criteria>();
      _criteria = orCriteria;

      try
      {

        foreach (var crit in BinaryOperator.Flatten(op))
        {
          if (crit is BinaryOperator bin && bin.Left is PropertyReference prop)
          {
            props.Add(prop);
            bin.Visit(this);
          }
          else if (crit is InOperator inOp && inOp.Left is PropertyReference inProp)
          {
            props.Add(inProp);
            foreach (var value in inOp.Right.Values)
            {
              new EqualsOperator()
              {
                Left = inProp,
                Right = value
              }.Normalize().Visit(this);
            }
          }
          else
          {
            throw new NotSupportedException();
          }
        }

        if (props.Count != 1)
          throw new NotSupportedException();
      }
      finally
      {
        _criteria = origList;
      }

      var conditions = orCriteria.Select(c => c.Condition).Distinct().ToList();
      if (conditions.Count == 2
        && conditions.Contains(Condition.Like)
        && conditions.Contains(Condition.Equal))
      {
        var values = new List<object>();
        foreach (var criteria in orCriteria)
        {
          if (criteria.Condition == Condition.Equal)
          {
            values.Add(_parser.String.Render(new PatternList()
            {
              Patterns =
              {
                new Pattern()
                {
                  Matches = { new StringMatch(criteria.Value.ToString()) }
                }
              }
            }));
          }
          else
          {
            values.Add(criteria.Value);
          }
        }
        AddCriteria(props.Single(), not ? Condition.NotLike : Condition.Like, values.ToArray());
      }
      else if (conditions.Count == 1)
      {
        AddCriteria(props.Single(), not ? Negate(conditions[0]) : conditions[0], orCriteria.Select(c => c.Value).ToArray());
      }
    }

    private Condition Negate(Condition condition)
    {
      switch (condition)
      {
        case Condition.Between:
          return Condition.NotBetween;
        case Condition.Equal:
          return Condition.NotEqual;
        case Condition.GreaterThan:
          return Condition.LessThanEqual;
        case Condition.GreaterThanEqual:
          return Condition.LessThan;
        case Condition.In:
          return Condition.NotIn;
        case Condition.IsNotNull:
          return Condition.IsNull;
        case Condition.IsNull:
          return Condition.IsNotNull;
        case Condition.LessThan:
          return Condition.GreaterThanEqual;
        case Condition.LessThanEqual:
          return Condition.GreaterThan;
        case Condition.Like:
          return Condition.NotLike;
        case Condition.NotBetween:
          return Condition.Between;
        case Condition.NotEqual:
          return Condition.Equal;
        case Condition.NotIn:
          return Condition.In;
        case Condition.NotLike:
          return Condition.Like;
        default:
          throw new NotSupportedException();
      }
    }

    public void Visit(PropertyReference op)
    {
      throw new NotSupportedException();
    }

    public void Visit(StringLiteral op)
    {
      throw new NotSupportedException();
    }

    public void Visit(MultiplicationOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(DivisionOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(ModulusOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(AdditionOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(SubtractionOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(NegationOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(ConcatenationOperator op)
    {
      throw new NotSupportedException();
    }

    public void Visit(ParameterReference op)
    {
      throw new NotSupportedException();
    }

    public void Visit(AllProperties op)
    {
      throw new NotSupportedException();
    }

    public void Visit(PatternList op)
    {
      throw new NotSupportedException();
    }

    private void HandleBinary(BinaryOperator op, Condition condition)
    {
      if (!(op.Left is PropertyReference prop
        && TryGetLiteralValue(op.Right, out var value)))
      {
        throw new NotSupportedException();
      }

      AddCriteria(prop, condition, value);
    }

    private bool TryGetLiteralValue(IExpression expression, out object value)
    {
      if (expression is FunctionExpression function)
        expression = function.Evaluate();

      if (expression is PatternList pattern)
      {
        value = _parser.String.Render(pattern);
        return true;
      }
      else if (expression is DateTimeLiteral dateLiteral)
      {
        value = new ZonedDateTime(dateLiteral.Value, _parser.Context.GetTimeZone());
        return true;
      }
      else if (expression is ILiteral literal)
      {
        value = literal.AsClrValue();
        return true;
      }
      else
      {
        value = null;
        return false;
      }
    }

    private void AddCriteria(PropertyReference prop, Condition condition, object value)
    {
      var name = prop.Name;
      if (name == "keyed_name" && prop.Table.TypeProvider != null)
        name = prop.Table.TypeProvider.Name;

      Criteria.Add(new Criteria(name, condition, value, _parser));
    }

    public void Visit(CountAggregate op)
    {
      throw new NotSupportedException();
    }
  }
}
