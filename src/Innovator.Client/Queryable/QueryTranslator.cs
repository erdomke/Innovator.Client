#if REFLECTION
using Innovator.Client.QueryModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Queryable
{
  internal class QueryTranslator : AmlVisitor
  {
    private ElementFactory _aml;
    private IExpression _curr;
    private PropertyReference _lastProp;
    private LambdaExpression _projector;
    private QueryItem _query;

    public QueryTranslator(ElementFactory factory)
    {
      _aml = factory;
    }

    /// <summary>
    /// Translate a LINQ expression to an AML query
    /// </summary>
    internal AmlQuery Translate(Expression expression)
    {
      _query = new QueryItem(_aml.LocalizationContext);
      this.Visit(expression);
      Normalize(_query);
      return new AmlQuery(_query, _aml, _projector);
    }

    private void Normalize(QueryItem query)
    {
      var visitor = new NormalizeVisitor();
      query.Where?.Visit(visitor);
      if (!visitor.LatestQuery)
      {
        if (query.Where == null)
        {
          query.Where = new EqualsOperator()
          {
            Left = new PropertyReference("is_current", query),
            Right = new BooleanLiteral(true)
          }.Normalize();
        }
        else
        {
          query.Where = new AndOperator()
          {
            Left = query.Where,
            Right = new EqualsOperator()
            {
              Left = new PropertyReference("is_current", query),
              Right = new BooleanLiteral(true)
            }.Normalize()
          }.Normalize();
        }
      }
    }

    private class NormalizeVisitor : SimpleVisitor
    {
      public bool LatestQuery { get; set; }

      public override void Visit(PropertyReference op)
      {
        if (op.Name == "generation" || op.Name == "is_current" || op.Name == "is_active_rev" || op.Name == "id")
          LatestQuery = true;
      }
    }

    private static Expression StripQuotes(Expression e)
    {
      while (e.NodeType == ExpressionType.Quote)
        e = ((UnaryExpression)e).Operand;

      return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression m)
    {
      if (VisitAmlMethod(m))
        return m;
      if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
      {
        IEnumerable<PropertyReference> props;
        switch (m.Method.Name)
        {
          case "Where":
            this.Visit(m.Arguments[0]);
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            if (lambda.Body.NodeType == ExpressionType.Constant && ((ConstantExpression)lambda.Body).Value is bool)
            {
              if (!((bool)((ConstantExpression)lambda.Body).Value))
              {
                _query.Where = new BooleanLiteral(false);
              }
            }
            else
            {
              _query.Where = VisitAndReturn(lambda.Body);
            }
            return m;
          case "Take":
            this.Visit(m.Arguments[0]);
            _query.Fetch = (int)((ConstantExpression)m.Arguments[1]).Value;
            return m;
          case "Skip":
            this.Visit(m.Arguments[0]);
            _query.Offset = (int)((ConstantExpression)m.Arguments[1]).Value;
            return m;
          case "Select":
            this.Visit(m.Arguments[0]);
            _projector = (LambdaExpression)StripQuotes(m.Arguments[1]);
            props = new SelectVisitor().GetProperties(_projector, _query);
            foreach (var prop in props)
            {
              prop.Table.Select.Add(new SelectExpression()
              {
                Expression = prop
              });
            }
            return m;
          case "OrderBy":
            this.Visit(m.Arguments[0]);
            props = new SelectVisitor().GetProperties(StripQuotes(m.Arguments[1]), _query);
            foreach (var prop in props)
            {
              prop.Table.OrderBy.Clear();
              prop.Table.OrderBy.Add(new OrderByExpression()
              {
                Ascending = true,
                Expression = prop
              });
            }
            return m;
          case "OrderByDescending":
            this.Visit(m.Arguments[0]);
            props = new SelectVisitor().GetProperties(StripQuotes(m.Arguments[1]), _query);
            foreach (var prop in props)
            {
              prop.Table.OrderBy.Clear();
              prop.Table.OrderBy.Add(new OrderByExpression()
              {
                Ascending = false,
                Expression = prop
              });
            }
            return m;
          case "ThenBy":
            this.Visit(m.Arguments[0]);
            props = new SelectVisitor().GetProperties(StripQuotes(m.Arguments[1]), _query);
            foreach (var prop in props)
            {
              prop.Table.OrderBy.Add(new OrderByExpression()
              {
                Ascending = true,
                Expression = prop
              });
            }
            return m;
          case "ThenByDescending":
            this.Visit(m.Arguments[0]);
            props = new SelectVisitor().GetProperties(StripQuotes(m.Arguments[1]), _query);
            foreach (var prop in props)
            {
              prop.Table.OrderBy.Add(new OrderByExpression()
              {
                Ascending = false,
                Expression = prop
              });
            }
            return m;
        }
      }
      else if ((m.Method.DeclaringType == typeof(Enumerable) && m.Method.Name == "Contains")
        || (m.Method.DeclaringType.IsGenericType && m.Method.DeclaringType.GetGenericTypeDefinition() == typeof(HashSet<>) && m.Method.Name == "Contains"))
      {
        var inOp = new InOperator()
        {
          Left = VisitAndReturn(m.Arguments.Last())
        };

        var enumerable = (IEnumerable)((ConstantExpression)(m.Object ?? m.Arguments[0])).Value;
        var list = new ListExpression();
        foreach (var obj in enumerable)
        {
          if (Expressions.TryGetLiteral(obj, out var lit))
            list.Values.Add(lit);
          else
            throw new NotSupportedException($"Cannot handle literals of type {obj.GetType().Name}");
        }
        inOp.Right = list;
        _curr = inOp.Normalize();
        return m;
      }
      else if (m.Method.DeclaringType == typeof(object) && m.Method.Name == "Equals")
      {
        if (m.Object.NodeType == ExpressionType.Parameter
          && m.Arguments[0].NodeType == ExpressionType.Constant
          && ((ConstantExpression)m.Arguments[0]).Value is IReadOnlyItem)
        {
          _curr = new EqualsOperator()
          {
            Left = new PropertyReference("id", _query),
            Right = new StringLiteral(((IReadOnlyItem)((ConstantExpression)m.Arguments[0]).Value).Id())
          }.Normalize();
        }
        else
        {
          _curr = new EqualsOperator()
          {
            Left = VisitAndReturn(m.Object),
            Right = VisitAndReturn(m.Arguments[0])
          }.Normalize();
        }
        return m;
      }
      else if (m.Method.DeclaringType == typeof(ItemExtensions) && m.Method.Name == "AsBoolean")
      {
        _curr = new EqualsOperator()
        {
          Left = VisitAndReturn(m.Arguments[0]),
          Right = new BooleanLiteral(true)
        }.Normalize();
        return m;
      }
      else if (m.Method.DeclaringType == typeof(IReadOnlyProperty_Boolean) && m.Method.Name == "AsBoolean")
      {
        _curr = new EqualsOperator()
        {
          Left = VisitAndReturn(m.Object),
          Right = new BooleanLiteral(true)
        }.Normalize();
        return m;
      }
      else if (m.Method.DeclaringType == typeof(ItemExtensions)
        && (m.Method.Name == "AsDateTime" || m.Method.Name == "AsDateTimeUtc"
        || m.Method.Name == "AsDateTimeOffset"
        || m.Method.Name == "AsDouble"
        || m.Method.Name == "AsInt"
        || m.Method.Name == "AsLong"
        || m.Method.Name == "AsGuid"))
      {
        Visit(m.Arguments[0]);
        return m;
      }
      else if ((m.Method.DeclaringType == typeof(IReadOnlyProperty_Date) && m.Method.Name == "AsDateTime")
        || (m.Method.DeclaringType == typeof(IReadOnlyProperty_Date) && m.Method.Name == "AsDateTimeUtc")
        || (m.Method.DeclaringType == typeof(IReadOnlyProperty_Date) && m.Method.Name == "AsDateTimeOffset")
        || (m.Method.DeclaringType == typeof(IReadOnlyProperty_Number) && m.Method.Name == "AsDouble")
        || (m.Method.DeclaringType == typeof(IReadOnlyProperty_Number) && m.Method.Name == "AsInt")
        || (m.Method.DeclaringType == typeof(IReadOnlyProperty_Number) && m.Method.Name == "AsLong")
        || (m.Method.DeclaringType == typeof(IReadOnlyProperty_Text) && m.Method.Name == "AsString")
        || (m.Method.DeclaringType.IsGenericType && m.Method.DeclaringType.GetGenericTypeDefinition() == typeof(IReadOnlyProperty_Item<>) && m.Method.Name == "AsGuid"))
      {
        Visit(m.Object);
        return m;
      }
      else if (m.Method.DeclaringType == typeof(string))
      {
        switch (m.Method.Name)
        {
          case "StartsWith":
          case "EndsWith":
          case "Contains":
            var left = VisitAndReturn(m.Object);
            var right = VisitAndReturn(m.Arguments[0]);

            if (right is StringLiteral str)
            {
              switch (m.Method.Name)
              {
                case "StartsWith":
                  str.Value = str.Value.Replace("%", "[%]") + "%";
                  break;
                case "EndsWith":
                  str.Value = "%" + str.Value.Replace("%", "[%]");
                  break;
                case "Contains":
                  str.Value = "%" + str.Value.Replace("%", "[%]") + "%";
                  break;
              }
            }
            else
            {
              switch (m.Method.Name)
              {
                case "StartsWith":
                  right = new ConcatenationOperator()
                  {
                    Left = right,
                    Right = new StringLiteral("%")
                  }.Normalize();
                  break;
                case "EndsWith":
                  right = new ConcatenationOperator()
                  {
                    Left = new StringLiteral("%"),
                    Right = right
                  }.Normalize();
                  break;
                case "Contains":
                  right = new ConcatenationOperator()
                  {
                    Left = new StringLiteral("%"),
                    Right = new ConcatenationOperator()
                    {
                      Left = right,
                      Right = new StringLiteral("%")
                    }.Normalize()
                  }.Normalize();
                  break;
              }
            }

            _curr = new LikeOperator() { Left = left, Right = right }.Normalize();
            return m;
          case "IsNullOrEmpty":
            _curr = new IsOperator()
            {
              Left = VisitAndReturn(m.Arguments[0]),
              Right = IsOperand.Null
            }.Normalize();
            return m;
        }
      }

      return base.VisitMethodCall(m);
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
      switch (u.NodeType)
      {
        case ExpressionType.Not:
          _curr = new NotOperator() { Arg = VisitAndReturn(u.Operand) }.Normalize();
          break;
        case ExpressionType.Convert:
          return base.VisitUnary(u);
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
          _curr = new NegationOperator() { Arg = VisitAndReturn(u.Operand) }.Normalize();
          break;
        default:
          throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
      }
      return u;
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      var left = VisitAndReturn(b.Left);
      var right = VisitAndReturn(b.Right);
      switch (b.NodeType)
      {
        case ExpressionType.Add:
          _curr = new AdditionOperator() { Left = left, Right = right }.Normalize();
          break;
        case ExpressionType.AndAlso:
          _curr = new AndOperator() { Left = left, Right = right }.Normalize();
          break;
        case ExpressionType.Divide:
          _curr = new DivisionOperator() { Left = left, Right = right }.Normalize();
          break;
        case ExpressionType.Equal:
          if (left == null && b.Left.NodeType == ExpressionType.Parameter)
          {
            VisitProperty("id");
            left = _curr;
          }
          _curr = new EqualsOperator() { Left = left, Right = right }.Normalize();
          if (right is NullLiteral)
            _curr = new IsOperator() { Left = left, Right = IsOperand.Null }.Normalize();
          break;
        case ExpressionType.GreaterThan:
          _curr = new GreaterThanOperator() { Left = left, Right = right }.Normalize();
          break;
        case ExpressionType.GreaterThanOrEqual:
          _curr = new GreaterThanOrEqualsOperator() { Left = left, Right = right }.Normalize();
          break;
        case ExpressionType.LessThan:
          _curr = new LessThanOperator() { Left = left, Right = right }.Normalize();
          break;
        case ExpressionType.LessThanOrEqual:
          _curr = new LessThanOrEqualsOperator() { Left = left, Right = right }.Normalize();
          break;
        case ExpressionType.Modulo:
          _curr = new ModulusOperator() { Left = left, Right = right }.Normalize();
          break;
        case ExpressionType.Multiply:
          _curr = new MultiplicationOperator() { Left = left, Right = right }.Normalize();
          break;
        case ExpressionType.NotEqual:
          if (left == null && b.Left.NodeType == ExpressionType.Parameter)
          {
            VisitProperty("id");
            left = _curr;
          }
          _curr = new NotEqualsOperator() { Left = left, Right = right }.Normalize();
          if (right is NullLiteral)
            _curr = new IsOperator() { Left = left, Right = IsOperand.NotNull }.Normalize();
          break;
        case ExpressionType.OrElse:
          _curr = new OrOperator() { Left = left, Right = right }.Normalize();
          break;
        case ExpressionType.Subtract:
          _curr = new SubtractionOperator() { Left = left, Right = right }.Normalize();
          break;
        default:
          throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
      }
      return b;
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value == null)
        _curr = new NullLiteral();
      else if (Expressions.TryGetLiteral(c.Value, out var lit))
        _curr = lit;
      else if (c.Value is IReadOnlyItem item)
        _curr = new StringLiteral(item.Id());
      else if (c.Value is IInnovatorQuery query)
        _query.Type = query.ItemType;
      else
        throw new NotSupportedException($"Cannot handle literals of type {c.Type.Name}");
      return c;
    }

    protected override void VisitProperty(string name)
    {
      if (_lastProp != null)
        _curr = _lastProp.Table.GetProperty(new[] { _lastProp.Name, name });
      else
        _curr = new PropertyReference(name, _query);
      _lastProp = (PropertyReference)_curr;
    }

    private IExpression VisitAndReturn(Expression expr)
    {
      _curr = null;
      _lastProp = null;
      this.Visit(expr);
      return _curr;
    }

    private class NullLiteral : IExpression
    {
      public void Visit(IExpressionVisitor visitor)
      {
        throw new NotSupportedException();
      }
    }

  }
}
#endif
