#if REFLECTION
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Innovator.Client.Queryable
{
  /// <summary>
  /// Class for converting a LINQ expression to an AML query
  /// </summary>
  internal class QueryTranslator : AmlVisitor
  {
    private ElementFactory _aml;
    private IItem _query;
    private IElement _curr;
    private LambdaExpression _projector;
    private QuerySettings _settings;
    private Stack<ExpressionType> _lastBinary = new Stack<ExpressionType>();

    public QueryTranslator(ElementFactory factory, QuerySettings settings)
    {
      _aml = factory;
      _settings = settings ?? new QuerySettings();
    }

    private static Expression StripQuotes(Expression e)
    {
      while (e.NodeType == ExpressionType.Quote)
        e = ((UnaryExpression)e).Operand;

      return e;
    }

    /// <summary>
    /// Translate a LINQ expression to an AML query
    /// </summary>
    internal AmlQuery Translate(Expression expression)
    {
      _query = _aml.Item(_aml.Action("get"));
      _curr = _query;
      this.Visit(expression);
      SimplifyConditionals(_query);
      return new AmlQuery() { Aml = _query, Projection = _projector };
    }

    /// <summary>
    /// Initially, a very verbose conditional structure of `and` and `or` elements are used.
    /// Take this structure and convert it to a simpler, but equivalent, structure
    /// </summary>
    private void SimplifyConditionals(IItem item)
    {
      var flattenGroup = NonItemDescendants(item).Select(CanFlattenItemChain).FirstOrDefault(g => g != null);
      while (flattenGroup != null)
      {
        var first = flattenGroup.First();
        var newCondition = first.Parent.Name == "and" ? _aml.And() : _aml.Or();

        foreach (var elem in flattenGroup)
        {
          foreach (var child in elem.Element("Item").Elements().OfType<IElement>().ToArray())
          {
            child.Remove();
            newCondition.Add(child);
          }
        }
        ((IElement)first.Element("Item")).Add(newCondition);

        foreach (var elem in flattenGroup.Skip(1).ToArray())
        {
          elem.Remove();
        }

        var parent = first.Parent;
        var grandParent = parent.Parent;
        if (parent.Elements().Count() == 1)
        {
          first.Remove();
          grandParent.Add(first);
          parent.Remove();
        }

        flattenGroup = NonItemDescendants(item).Select(CanFlattenItemChain).FirstOrDefault(g => g != null);
      }

      var toFlatten = NonItemDescendants(item).FirstOrDefault(CanFlatten);
      while (toFlatten != null)
      {
        foreach (var child in toFlatten.Elements().ToArray())
        {
          child.Remove();
          toFlatten.Parent.Add(child);
        }
        toFlatten.Remove();
        toFlatten = NonItemDescendants(item).FirstOrDefault(CanFlatten);
      }

      if (_settings.ModifyQuery != null)
        _settings.ModifyQuery(item);

      foreach (var childItem in DescendantItems(item))
      {
        SimplifyConditionals(childItem);
      }
    }

    private IEnumerable<IElement> NonItemDescendants(IElement elem)
    {
      foreach (var child in elem.Elements().Where(c => c.Name != "Item"))
      {
        yield return child;
        foreach (var descendant in NonItemDescendants(child))
          yield return descendant;
      }
    }

    private IEnumerable<IItem> DescendantItems(IElement elem)
    {
      foreach (var child in elem.Elements())
      {
        var item = child as IItem;
        if (item != null)
          yield return item;
        else
          foreach (var descendant in DescendantItems(child))
            yield return descendant;
      }
    }

    private bool CanFlatten(IElement elem)
    {
      return (elem.Name == "and" && elem.Parent.Name != "or")
        || (elem.Name == "or" && elem.Parent.Name == "or");
    }

    private IEnumerable<IElement> CanFlattenItemChain(IElement elem)
    {
      if (elem.Name != "and" && elem.Name != "or")
        return null;

      return elem.Elements()
        .GroupBy(c => c.Name)
        .FirstOrDefault(g => g.All(c => c.Element("Item").Exists) && g.Count() > 1);
    }

    protected override Expression VisitMethodCall(MethodCallExpression m)
    {
      if (VisitAmlMethod(m))
        return m;

      if (m.Method.DeclaringType == typeof(System.Linq.Queryable))
      {
        SelectNode subSelect;
        switch (m.Method.Name)
        {
          case "Where":
            this.Visit(m.Arguments[0]);
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            if (lambda.Body.NodeType == ExpressionType.Constant && ((ConstantExpression)lambda.Body).Value is bool)
            {
              if (!((bool)((ConstantExpression)lambda.Body).Value))
              {
                _curr.Attribute("id").Set("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
              }
            }
            else
            {
              this.Visit(lambda.Body);
            }
            return m;
          case "Take":
            this.Visit(m.Arguments[0]);
            var pagesize = _curr.Attribute("pagesize").AsInt();
            var take = (int)((ConstantExpression)m.Arguments[1]).Value;
            if (pagesize.HasValue && _curr.Attribute("page").AsInt(0) == 2)
            {
              if ((pagesize.Value % take) == 0)
              {
                _curr.Attribute("page").Set(pagesize.Value / take + 1);
                _curr.Attribute("pagesize").Set(take);
              }
            }
            else
            {
              _curr.Attribute("maxRecords").Set(take);
            }
            return m;
          case "Skip":
            this.Visit(m.Arguments[0]);
            var maxRecords = _curr.Attribute("maxRecords").AsInt();
            var skip = (int)((ConstantExpression)m.Arguments[1]).Value;
            if (maxRecords.HasValue && (skip % maxRecords.Value) == 0)
            {
              _curr.Attribute("maxRecords").Remove();
              _curr.Attribute("page").Set(skip / maxRecords.Value + 1);
              _curr.Attribute("pagesize").Set(maxRecords.Value);
            }
            else
            {
              _curr.Attribute("page").Set(2);
              _curr.Attribute("pagesize").Set(skip);
            }
            return m;
          case "Select":
            this.Visit(m.Arguments[0]);
            _projector = (LambdaExpression)StripQuotes(m.Arguments[1]);
            subSelect = new SelectVisitor().GetSelect(_projector);
            _curr.Attribute("select").Set(subSelect.ToString());
            return m;
          case "OrderBy":
            this.Visit(m.Arguments[0]);
            subSelect = new SelectVisitor().GetSelect(StripQuotes(m.Arguments[1]));
            _curr.Attribute("orderBy").Set(subSelect.First().ToString());
            return m;
          case "OrderByDescending":
            this.Visit(m.Arguments[0]);
            subSelect = new SelectVisitor().GetSelect(StripQuotes(m.Arguments[1]));
            _curr.Attribute("orderBy").Set(subSelect.First().ToString() + " DESC");
            return m;
          case "ThenBy":
            this.Visit(m.Arguments[0]);
            subSelect = new SelectVisitor().GetSelect(StripQuotes(m.Arguments[1]));
            _curr.Attribute("orderBy").Set(_curr.Attribute("orderBy").Value + "," + subSelect.First().ToString());
            return m;
          case "ThenByDescending":
            this.Visit(m.Arguments[0]);
            subSelect = new SelectVisitor().GetSelect(StripQuotes(m.Arguments[1]));
            _curr.Attribute("orderBy").Set(_curr.Attribute("orderBy").Value + "," + subSelect.First().ToString() + " DESC");
            return m;
        }
      }
      else if ((m.Method.DeclaringType == typeof(Enumerable) && m.Method.Name == "Contains")
        || (m.Method.DeclaringType.IsGenericType && m.Method.DeclaringType.GetGenericTypeDefinition() == typeof(HashSet<>) && m.Method.Name == "Contains"))
      {
        this.Visit(m.Arguments.Last());
        _curr.Attribute("condition").Set("in");
        var builder = new StringBuilder();
        var enumerable = (IEnumerable)((ConstantExpression)(m.Object ?? m.Arguments[0])).Value;
        IFormattable format;
        foreach (var obj in enumerable)
        {
          if (builder.Length > 0)
            builder.Append(",");
          if (ServerContext.TryCastNumber(obj, out format))
          {
            builder.Append(_aml.LocalizationContext.Format(obj));
          }
          else
          {
            builder.Append("'").Append(_aml.LocalizationContext.Format(obj)).Append("'");
          }
        }
        _curr.Add(builder.ToString());
        return m;
      }
      else if (m.Method.DeclaringType == typeof(object) && m.Method.Name == "Equals")
      {
        if (m.Object.NodeType == ExpressionType.Parameter
          && m.Arguments[0].NodeType == ExpressionType.Constant
          && ((ConstantExpression)m.Arguments[0]).Value is IReadOnlyItem)
        {
          _curr.Add(_aml.IdProp(((IReadOnlyItem)((ConstantExpression)m.Arguments[0]).Value).Id()));
        }
        else
        {
          Visit(m.Object);
          Visit(m.Arguments[0]);
        }
        return m;
      }
      else if (m.Method.DeclaringType == typeof(IReadOnlyProperty_Boolean) && m.Method.Name == "AsBoolean")
      {
        var depth = GetDepth();
        Visit(m.Object);
        _curr.Add(true);
        PopElements(depth);
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
        var depth = GetDepth();
        switch (m.Method.Name)
        {
          case "StartsWith":
            this.Visit(m.Object);
            _curr.Add(_aml.Condition(Condition.Like)
              , ((ConstantExpression)m.Arguments[0]).Value.ToString().Replace("%", "[%]") + "*");
            PopElements(depth);
            return m;
          case "EndsWith":
            this.Visit(m.Object);
            _curr.Add(_aml.Condition(Condition.Like)
              , "*" + ((ConstantExpression)m.Arguments[0]).Value.ToString().Replace("%", "[%]"));
            PopElements(depth);
            return m;
          case "Contains":
            this.Visit(m.Object);
            _curr.Add(_aml.Condition(Condition.Like)
              , "*" + ((ConstantExpression)m.Arguments[0]).Value.ToString().Replace("%", "[%]") + "*");
            PopElements(depth);
            return m;
          case "IsNullOrEmpty":
            this.Visit(m.Arguments[0]);
            _curr.Add(_aml.Condition(Condition.IsNull), "");
            PopElements(depth);
            return m;
        }
      }

      return base.VisitMethodCall(m);
    }

    protected override void VisitProperty(string name)
    {
      PushElement(_aml.Property(name));
    }

    protected override void VisitItem()
    {
      PushElement(_aml.Item(_aml.Action("get")));
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
      switch (u.NodeType)
      {
        case ExpressionType.Not:
          var op = _aml.Not();
          _curr.Add(op);
          _curr = op;
          this.Visit(u.Operand);
          _curr = _curr.Parent;
          break;
        case ExpressionType.Convert:
          this.Visit(u.Operand);

          var isComparison = (_lastBinary.Count > 0 &&
            (_lastBinary.Peek() == ExpressionType.Equal
            || _lastBinary.Peek() == ExpressionType.NotEqual
            || _lastBinary.Peek() == ExpressionType.LessThan
            || _lastBinary.Peek() == ExpressionType.LessThanOrEqual
            || _lastBinary.Peek() == ExpressionType.GreaterThan
            || _lastBinary.Peek() == ExpressionType.GreaterThanOrEqual));

          if (u.Type == typeof(bool) && !isComparison)
            SetValue(true);
          break;
        default:
          throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
      }

      return u;
    }

    private int GetDepth()
    {
      var result = 0;
      var check = _curr;
      while (check.Parent.Exists)
      {
        result++;
        check = check.Parent;
      }
      return result;
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      _lastBinary.Push(b.NodeType);

      try
      {

        var left = b.Left;
        var right = b.Right;
        if (b.Left is ConstantExpression && !(b.Right is ConstantExpression))
        {
          left = b.Right;
          right = b.Left;
        }

        var constExpr = right as ConstantExpression;
        var isNull = constExpr != null && constExpr.Value == null;
        var depth = GetDepth();

        switch (b.NodeType)
        {
          case ExpressionType.OrElse:
          case ExpressionType.AndAlso:
            var newParent = PushElement(b.NodeType == ExpressionType.OrElse ? _aml.Or() : _aml.And());
            this.Visit(left);
            this.Visit(right);
            break;
          case ExpressionType.Equal:
            if (left.NodeType == ExpressionType.Parameter && constExpr != null && constExpr.Value is IReadOnlyItem)
            {
              _curr.Add(_aml.IdProp(((IReadOnlyItem)constExpr.Value).Id()));
            }
            else if (isNull)
            {
              this.Visit(left);
              _curr.Add(_aml.Condition(Condition.IsNull));
              this.Visit(right);
            }
            else if ((left.NodeType == ExpressionType.Equal
                || left.NodeType == ExpressionType.NotEqual
                || left.NodeType == ExpressionType.LessThan
                || left.NodeType == ExpressionType.LessThanOrEqual
                || left.NodeType == ExpressionType.GreaterThan
                || left.NodeType == ExpressionType.GreaterThanOrEqual)
              && constExpr != null && constExpr.Value is bool)
            {
              // Simplify the expression that was build because there is a not followed by a parenthetical
              this.Visit(left);
            }
            else
            {
              this.Visit(left);
              this.Visit(right);
            }
            break;
          case ExpressionType.NotEqual:
            if (left.NodeType == ExpressionType.Parameter && constExpr != null && constExpr.Value is IReadOnlyItem)
            {
              _curr.Add(_aml.IdProp(_aml.Condition(Condition.NotEqual), ((IReadOnlyItem)constExpr.Value).Id()));
            }
            else
            {
              this.Visit(left);
              if (isNull)
                _curr.Add(_aml.Condition(Condition.IsNotNull));
              else
                _curr.Add(_aml.Condition(Condition.NotEqual));
              this.Visit(right);
            }
            break;
          default:
            this.Visit(left);
            switch (b.NodeType)
            {
              case ExpressionType.LessThan:
                _curr.Add(_aml.Condition(Condition.LessThan));
                break;
              case ExpressionType.LessThanOrEqual:
                _curr.Add(_aml.Condition(Condition.LessThanEqual));
                break;
              case ExpressionType.GreaterThan:
                _curr.Add(_aml.Condition(Condition.GreaterThan));
                break;
              case ExpressionType.GreaterThanOrEqual:
                _curr.Add(_aml.Condition(Condition.GreaterThanEqual));
                break;
              default:
                throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            this.Visit(right);
            break;
        }

        PopElements(depth);
        return b;
      }
      finally
      {
        _lastBinary.Pop();
      }
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      var q = c.Value as IInnovatorQuery;
      if (q != null)
      {
        _curr.Attribute("type").Set(q.ItemType);
      }
      else if (c.Value == null)
      {
        // Do nothing (already covered earlier)
      }
      else
      {
        SetValue(c.Value);
      }
      return c;
    }

    private void SetValue(object value)
    {
      var prop = _curr as IProperty;
      if (prop != null)
        prop.Set(value);
      else
        _curr.Add(value);
    }

    private void PopElements(int origDepth)
    {
      var pop = GetDepth() - origDepth;
      for (var i = 0; i < pop; i++)
        _curr = _curr.Parent;
    }
    private IElement PushElement(IElement elem)
    {
      _curr.Add(elem);
      _curr = elem;
      return _curr;
    }
  }
}
#endif
