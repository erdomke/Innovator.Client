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
    private object _curr;
    private LambdaExpression _projector;
    private QueryItem _query;
    private readonly Dictionary<ParameterExpression, QueryItem> _tables
      = new Dictionary<ParameterExpression, QueryItem>();

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
        query.Version = new CurrentVersion();

      if (query.Where is PropertyReference)
      {
        query.Where = new EqualsOperator()
        {
          Left = query.Where,
          Right = new BooleanLiteral(true)
        }.Normalize();
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
      if ((m.Method.DeclaringType == typeof(Enumerable) && m.Method.Name == "Contains")
        || (m.Method.DeclaringType.IsGenericType && m.Method.DeclaringType.GetGenericTypeDefinition() == typeof(HashSet<>) && m.Method.Name == "Contains"))
      {
        var inOp = new InOperator()
        {
          Left = VisitAndReturnExpr(m.Arguments.Last())
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
      else if (m.Method.DeclaringType == typeof(System.Linq.Queryable)
        || m.Method.DeclaringType == typeof(Enumerable))
      {
        IEnumerable<PropertyReference> props;
        QueryItem query;
        switch (m.Method.Name)
        {
          case "OfType":
          case "Cast":
            query = (QueryItem)VisitAndReturnAny(m.Arguments[0]);
            return m;
          case "Any":
          case "Where":
            query = (QueryItem)VisitAndReturnAny(m.Arguments[0]);
            if (m.Method.Name == "Where" && m.Arguments.Count != 2)
              throw new NotSupportedException();
            if (m.Arguments.Count == 2)
            {
              var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
              if (lambda.Body.NodeType == ExpressionType.Constant && ((ConstantExpression)lambda.Body).Value is bool)
              {
                if (!((bool)((ConstantExpression)lambda.Body).Value))
                {
                  query.Where = new BooleanLiteral(false);
                }
              }
              else
              {
                _tables.Add(lambda.Parameters[0], query);
                query.Where = VisitAndReturnExpr(lambda.Body);
                _tables.Remove(lambda.Parameters[0]);
              }
            }
            _curr = m.Method.Name == "Where" ? query : null;
            return m;
          case "Take":
            query = (QueryItem)VisitAndReturnAny(m.Arguments[0]);
            query.Fetch = (int)((ConstantExpression)m.Arguments[1]).Value;
            _curr = query;
            return m;
          case "Skip":
            query = (QueryItem)VisitAndReturnAny(m.Arguments[0]);
            query.Offset = (int)((ConstantExpression)m.Arguments[1]).Value;
            _curr = query;
            return m;
          case "Select":
            query = (QueryItem)VisitAndReturnAny(m.Arguments[0]);
            _projector = (LambdaExpression)StripQuotes(m.Arguments[1]);
            props = new SelectVisitor().GetProperties(_projector, query);
            foreach (var prop in props)
            {
              prop.Table.Select.Add(new SelectExpression()
              {
                Expression = prop
              });
            }
            _curr = query;
            return m;
          case "OrderBy":
            query = (QueryItem)VisitAndReturnAny(m.Arguments[0]);
            props = new SelectVisitor().GetProperties(StripQuotes(m.Arguments[1]), query);
            foreach (var prop in props)
            {
              prop.Table.OrderBy.Clear();
              prop.Table.OrderBy.Add(new OrderByExpression()
              {
                Ascending = true,
                Expression = prop
              });
            }
            _curr = query;
            return m;
          case "OrderByDescending":
            query = (QueryItem)VisitAndReturnAny(m.Arguments[0]);
            props = new SelectVisitor().GetProperties(StripQuotes(m.Arguments[1]), query);
            foreach (var prop in props)
            {
              prop.Table.OrderBy.Clear();
              prop.Table.OrderBy.Add(new OrderByExpression()
              {
                Ascending = false,
                Expression = prop
              });
            }
            _curr = query;
            return m;
          case "ThenBy":
            query = (QueryItem)VisitAndReturnAny(m.Arguments[0]);
            props = new SelectVisitor().GetProperties(StripQuotes(m.Arguments[1]), query);
            foreach (var prop in props)
            {
              prop.Table.OrderBy.Add(new OrderByExpression()
              {
                Ascending = true,
                Expression = prop
              });
            }
            _curr = query;
            return m;
          case "ThenByDescending":
            query = (QueryItem)VisitAndReturnAny(m.Arguments[0]);
            props = new SelectVisitor().GetProperties(StripQuotes(m.Arguments[1]), query);
            foreach (var prop in props)
            {
              prop.Table.OrderBy.Add(new OrderByExpression()
              {
                Ascending = false,
                Expression = prop
              });
            }
            _curr = query;
            return m;
          default:
            throw new NotSupportedException();
        }
      }
      else if (m.Method.DeclaringType == typeof(IReadOnlyItem) || m.Method.DeclaringType == typeof(IItem))
      {
        if (m.Method.Name == "Relationships"
          && m.Arguments.Count == 1
          && _paramStack.TrySimplify(m.Arguments[0]) is ConstantExpression con
          && con.Value is string relName)
        {
          var table = (QueryItem)VisitAndReturnAny(m.Object);
          var relation = new QueryItem(_aml.LocalizationContext)
          {
            Type = relName
          };
          table.Joins.Add(new Join()
          {
            Left = table,
            Right = relation,
            Condition = new EqualsOperator()
            {
              Left = new PropertyReference("id", table),
              Right = new PropertyReference("source_id", relation)
            }
          });
          _curr = relation;
          return m;
        }
        else
        {
          throw new NotSupportedException();
        }
      }
      else if (m.Method.DeclaringType == typeof(object) && m.Method.Name == "Equals")
      {
        var left = VisitAndReturnExpr(m.Object ?? m.Arguments[0]);
        var right = VisitAndReturnExpr(m.Arguments[m.Object == null ? 1 : 0]);
        if (right is NullLiteral)
        {
          _curr = new IsOperator()
          {
            Left = left,
            Right = IsOperand.Null
          }.Normalize();
        }
        else if (left is NullLiteral)
        {
          _curr = new IsOperator()
          {
            Left = right,
            Right = IsOperand.Null
          }.Normalize();
        }
        else
        {
          _curr = new EqualsOperator()
          {
            Left = left,
            Right = right
          }.Normalize();
        }
        return m;
      }
      else if (m.Method.DeclaringType == typeof(ItemExtensions))
      {
        switch (m.Method.Name)
        {
          case "AsBoolean":
            _curr = new EqualsOperator()
            {
              Left = VisitAndReturnExpr(m.Arguments[0]),
              Right = new BooleanLiteral(true)
            }.Normalize();
            return m;
          case "AsDateTime":
          case "AsDateTimeUtc":
          case "AsDateTimeOffset":
          case "AsDouble":
          case "AsInt":
          case "AsLong":
          case "AsGuid":
            Visit(m.Arguments[0]);
            return m;
          default:
            throw new NotSupportedException();
        }
      }
      else if (m.Method.DeclaringType == typeof(IReadOnlyProperty_Boolean))
      {
        if (m.Method.Name != "AsBoolean")
          throw new NotSupportedException();

        _curr = new EqualsOperator()
        {
          Left = VisitAndReturnExpr(m.Object),
          Right = new BooleanLiteral(true)
        }.Normalize();
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
            var left = VisitAndReturnExpr(m.Object);
            var right = VisitAndReturnExpr(m.Arguments[0]);
            _curr = LikeOperator.FromMethod(m.Method.Name, left, right);
            return m;
          case "Concat":
            if (m.Arguments.Count == 1)
            {
              _curr = VisitAndReturnExpr(m.Arguments[0]);
            }
            else if (m.Arguments.Count > 1)
            {
              _curr = new ConcatenationOperator()
              {
                Left = VisitAndReturnExpr(m.Arguments[0]),
                Right = VisitAndReturnExpr(m.Arguments[1]),
              }.Normalize();
              for (var i = 2; i < m.Arguments.Count; i++)
              {
                _curr = new ConcatenationOperator()
                {
                  Left = (IExpression)_curr,
                  Right = VisitAndReturnExpr(m.Arguments[i]),
                }.Normalize();
              }
            }
            return m;
          case "Equals":
            if (m.Object == null)
            {
              _curr = new EqualsOperator()
              {
                Left = VisitAndReturnExpr(m.Arguments[0]),
                Right = VisitAndReturnExpr(m.Arguments[1])
              }.Normalize();
            }
            else
            {
              _curr = new EqualsOperator()
              {
                Left = VisitAndReturnExpr(m.Object),
                Right = VisitAndReturnExpr(m.Arguments[0])
              }.Normalize();
            }
            return m;
          case "IndexOf":
            _curr = new QueryModel.Functions.IndexOf_One()
            {
              String = VisitAndReturnExpr(m.Object),
              Target = VisitAndReturnExpr(m.Arguments[0]),
            };
            return m;
          case "Insert":
            _curr = new ConcatenationOperator()
            {
              Left = new ConcatenationOperator()
              {
                Left = new QueryModel.Functions.Substring_One()
                {
                  String = VisitAndReturnExpr(m.Object),
                  Start = new IntegerLiteral(1),
                  Length = VisitAndReturnExpr(m.Arguments[0])
                },
                Right = VisitAndReturnExpr(m.Arguments[1])
              }.Normalize(),
              Right = new QueryModel.Functions.Substring_One()
              {
                String = VisitAndReturnExpr(m.Object),
                Start = new AdditionOperator()
                {
                  Left = VisitAndReturnExpr(m.Arguments[0]),
                  Right = new IntegerLiteral(1),
                }.Normalize(),
                Length = new SubtractionOperator()
                {
                  Left = new QueryModel.Functions.Length()
                  {
                    String = VisitAndReturnExpr(m.Object),
                  },
                  Right = VisitAndReturnExpr(m.Arguments[0])
                }.Normalize()
              }
            }.Normalize();
            return m;
          case "IsNullOrEmpty":
            _curr = new OrOperator()
            {
              Left = new IsOperator()
              {
                Left = VisitAndReturnExpr(m.Arguments[0]),
                Right = IsOperand.Null
              }.Normalize(),
              Right = new EqualsOperator()
              {
                Left = VisitAndReturnExpr(m.Arguments[0]),
                Right = new StringLiteral("")
              }
            };
            return m;
          case "Remove":
            if (m.Arguments.Count == 1)
            {
              _curr = new QueryModel.Functions.Substring_One()
              {
                String = VisitAndReturnExpr(m.Object),
                Start = new IntegerLiteral(1),
                Length = VisitAndReturnExpr(m.Arguments[0])
              };
            }
            else if (m.Arguments.Count == 2)
            {
              _curr = new ConcatenationOperator()
              {
                Left = new QueryModel.Functions.Substring_One()
                {
                  String = VisitAndReturnExpr(m.Object),
                  Start = new IntegerLiteral(1),
                  Length = VisitAndReturnExpr(m.Arguments[0])
                },
                Right = new QueryModel.Functions.Substring_One()
                {
                  String = VisitAndReturnExpr(m.Object),
                  Start = new AdditionOperator()
                  {
                    Left = new AdditionOperator()
                    {
                      Left = VisitAndReturnExpr(m.Arguments[0]),
                      Right = VisitAndReturnExpr(m.Arguments[1]),
                    }.Normalize(),
                    Right = new IntegerLiteral(1)
                  }.Normalize(),
                  Length = new SubtractionOperator()
                  {
                    Left = new SubtractionOperator()
                    {
                      Left = new QueryModel.Functions.Length()
                      {
                        String = VisitAndReturnExpr(m.Object)
                      },
                      Right = VisitAndReturnExpr(m.Arguments[0])
                    }.Normalize(),
                    Right = VisitAndReturnExpr(m.Arguments[1])
                  }.Normalize()
                }
              }.Normalize();
            }
            return m;
          case "Replace":
            _curr = new QueryModel.Functions.Replace()
            {
              String = VisitAndReturnExpr(m.Object),
              Find = VisitAndReturnExpr(m.Arguments[0]),
              Substitute = VisitAndReturnExpr(m.Arguments[1]),
            };
            return m;
          case "Substring":
            if (m.Arguments.Count == 1)
              _curr = QueryModel.Functions.Substring_One.FromZeroBased(VisitAndReturnExpr(m.Object), VisitAndReturnExpr(m.Arguments[0]));
            else if (m.Arguments.Count == 2)
              _curr = QueryModel.Functions.Substring_One.FromZeroBased(VisitAndReturnExpr(m.Object), VisitAndReturnExpr(m.Arguments[0]), VisitAndReturnExpr(m.Arguments[1]));
            return m;
          case "ToLower":
            _curr = new QueryModel.Functions.ToLower()
            {
              String = VisitAndReturnExpr(m.Object),
            };
            return m;
          case "ToUpper":
            _curr = new QueryModel.Functions.ToUpper()
            {
              String = VisitAndReturnExpr(m.Object),
            };
            return m;
          case "Trim":
            _curr = new QueryModel.Functions.Trim()
            {
              String = VisitAndReturnExpr(m.Object),
            };
            return m;
          case "TrimEnd":
            _curr = new QueryModel.Functions.RTrim()
            {
              String = VisitAndReturnExpr(m.Object),
            };
            return m;
          case "TrimStart":
            _curr = new QueryModel.Functions.LTrim()
            {
              String = VisitAndReturnExpr(m.Object),
            };
            return m;
          default:
            throw new NotSupportedException();
        }
      }
      else if (m.Method.DeclaringType.FullName == "Microsoft.VisualBasic.Strings")
      {
        switch (m.Method.Name)
        {
          case "Trim":
            _curr = new QueryModel.Functions.Trim() { String = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "LTrim":
            _curr = new QueryModel.Functions.LTrim() { String = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "RTrim":
            _curr = new QueryModel.Functions.RTrim() { String = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Len":
            _curr = new QueryModel.Functions.Length() { String = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Left":
            _curr = new QueryModel.Functions.Left()
            {
              String = VisitAndReturnExpr(m.Arguments[0]),
              Length = VisitAndReturnExpr(m.Arguments[1])
            };
            return m;
          case "Mid":
            _curr = new QueryModel.Functions.Substring_One()
            {
              String = VisitAndReturnExpr(m.Arguments[0]),
              Start = VisitAndReturnExpr(m.Arguments[1]),
              Length = VisitAndReturnExpr(m.Arguments[2])
            };
            return m;
          case "Right":
            _curr = new QueryModel.Functions.Right()
            {
              String = VisitAndReturnExpr(m.Arguments[0]),
              Length = VisitAndReturnExpr(m.Arguments[1])
            };
            return m;
          case "UCase":
            _curr = new QueryModel.Functions.ToUpper() { String = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "LCase":
            _curr = new QueryModel.Functions.ToLower() { String = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          default:
            throw new NotSupportedException();
        }
      }
      else if (m.Method.DeclaringType == typeof(DateTime) || m.Method.DeclaringType == typeof(DateTimeOffset))
      {
        switch (m.Method.Name)
        {
          case "Equals":
            if (m.Object == null)
            {
              _curr = new EqualsOperator()
              {
                Left = VisitAndReturnExpr(m.Arguments[0]),
                Right = VisitAndReturnExpr(m.Arguments[1]),
              }.Normalize();
            }
            else
            {
              _curr = new EqualsOperator()
              {
                Left = VisitAndReturnExpr(m.Object),
                Right = VisitAndReturnExpr(m.Arguments[0]),
              }.Normalize();
            }
            return m;
          case "Add":
            if (m.Arguments[0] is ConstantExpression c && c.Value is TimeSpan ts)
            {
              if (ts.TotalDays >= 1)
              {
                _curr = new QueryModel.Functions.AddDays()
                {
                  Expression = VisitAndReturnExpr(m.Object),
                  Number = new FloatLiteral(ts.TotalDays)
                };
              }
              else if (ts.TotalHours >= 1)
              {
                _curr = new QueryModel.Functions.AddHours()
                {
                  Expression = VisitAndReturnExpr(m.Object),
                  Number = new FloatLiteral(ts.TotalHours)
                };
              }
              else if (ts.TotalMinutes >= 1)
              {
                _curr = new QueryModel.Functions.AddMinutes()
                {
                  Expression = VisitAndReturnExpr(m.Object),
                  Number = new FloatLiteral(ts.TotalMinutes)
                };
              }
              else if (ts.TotalSeconds >= 1)
              {
                _curr = new QueryModel.Functions.AddSeconds()
                {
                  Expression = VisitAndReturnExpr(m.Object),
                  Number = new FloatLiteral(ts.TotalSeconds)
                };
              }
              else
              {
                _curr = new QueryModel.Functions.AddMilliseconds()
                {
                  Expression = VisitAndReturnExpr(m.Object),
                  Number = new FloatLiteral(ts.TotalMilliseconds)
                };
              }
              return m;
            }
            throw new NotSupportedException();
          case "AddDays":
            _curr = new QueryModel.Functions.AddDays()
            {
              Expression = VisitAndReturnExpr(m.Object),
              Number = VisitAndReturnExpr(m.Arguments[0])
            };
            return m;
          case "AddHours":
            _curr = new QueryModel.Functions.AddHours()
            {
              Expression = VisitAndReturnExpr(m.Object),
              Number = VisitAndReturnExpr(m.Arguments[0])
            };
            return m;
          case "AddMilliseconds":
            _curr = new QueryModel.Functions.AddMilliseconds()
            {
              Expression = VisitAndReturnExpr(m.Object),
              Number = VisitAndReturnExpr(m.Arguments[0])
            };
            return m;
          case "AddMinutes":
            _curr = new QueryModel.Functions.AddMinutes()
            {
              Expression = VisitAndReturnExpr(m.Object),
              Number = VisitAndReturnExpr(m.Arguments[0])
            };
            return m;
          case "AddMonths":
            _curr = new QueryModel.Functions.AddMonths()
            {
              Expression = VisitAndReturnExpr(m.Object),
              Number = VisitAndReturnExpr(m.Arguments[0])
            };
            return m;
          case "AddSeconds":
            _curr = new QueryModel.Functions.AddSeconds()
            {
              Expression = VisitAndReturnExpr(m.Object),
              Number = VisitAndReturnExpr(m.Arguments[0])
            };
            return m;
          case "AddYears":
            _curr = new QueryModel.Functions.AddYears()
            {
              Expression = VisitAndReturnExpr(m.Object),
              Number = VisitAndReturnExpr(m.Arguments[0])
            };
            return m;
          default:
            throw new NotSupportedException();
        }
      }
      else if (m.Method.DeclaringType.FullName == "Microsoft.VisualBasic.DateAndTime")
      {
        switch (m.Method.Name)
        {
          case "Year":
            _curr = new QueryModel.Functions.Year() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Month":
            _curr = new QueryModel.Functions.Month() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Day":
            _curr = new QueryModel.Functions.Day() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Hour":
            _curr = new QueryModel.Functions.Hour() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Minute":
            _curr = new QueryModel.Functions.Minute() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Second":
            _curr = new QueryModel.Functions.Second() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "DatePart":
            if (m.Arguments[0] is ConstantExpression c)
            {
              switch ((int)c.Value)
              {
                case 0:
                  _curr = new QueryModel.Functions.Year() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
                  return m;
                case 2:
                  _curr = new QueryModel.Functions.Month() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
                  return m;
                case 4:
                  _curr = new QueryModel.Functions.Day() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
                  return m;
                case 7:
                  _curr = new QueryModel.Functions.Hour() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
                  return m;
                case 8:
                  _curr = new QueryModel.Functions.Minute() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
                  return m;
                case 9:
                  _curr = new QueryModel.Functions.Second() { Expression = VisitAndReturnExpr(m.Arguments[0]) };
                  return m;
              }
            }
            throw new NotSupportedException();
          case "DateDiff":
            if (m.Arguments[0] is ConstantExpression c1)
            {
              switch ((int)c1.Value)
              {
                case 0:
                  _curr = new QueryModel.Functions.DiffYears()
                  {
                    StartExpression = VisitAndReturnExpr(m.Arguments[0]),
                    EndExpression = VisitAndReturnExpr(m.Arguments[1]),
                  };
                  return m;
                case 2:
                  _curr = new QueryModel.Functions.DiffMonths()
                  {
                    StartExpression = VisitAndReturnExpr(m.Arguments[0]),
                    EndExpression = VisitAndReturnExpr(m.Arguments[1]),
                  };
                  return m;
                case 4:
                  _curr = new QueryModel.Functions.DiffDays()
                  {
                    StartExpression = VisitAndReturnExpr(m.Arguments[0]),
                    EndExpression = VisitAndReturnExpr(m.Arguments[1]),
                  };
                  return m;
                case 7:
                  _curr = new QueryModel.Functions.DiffHours()
                  {
                    StartExpression = VisitAndReturnExpr(m.Arguments[0]),
                    EndExpression = VisitAndReturnExpr(m.Arguments[1]),
                  };
                  return m;
                case 8:
                  _curr = new QueryModel.Functions.DiffMinutes()
                  {
                    StartExpression = VisitAndReturnExpr(m.Arguments[0]),
                    EndExpression = VisitAndReturnExpr(m.Arguments[1]),
                  };
                  return m;
                case 9:
                  _curr = new QueryModel.Functions.DiffSeconds()
                  {
                    StartExpression = VisitAndReturnExpr(m.Arguments[0]),
                    EndExpression = VisitAndReturnExpr(m.Arguments[1]),
                  };
                  return m;
              }
            }
            throw new NotSupportedException();
          default:
            throw new NotSupportedException();
        }
      }
      else if (m.Method.DeclaringType == typeof(Guid))
      {
        switch (m.Method.Name)
        {
          case "NewGuid":
            _curr = new QueryModel.Functions.NewGuid();
            return m;
          default:
            throw new NotSupportedException();
        }
      }
      else if (m.Method.DeclaringType == typeof(Decimal))
      {
        switch (m.Method.Name)
        {
          case "Ceiling":
            _curr = new QueryModel.Functions.Ceiling() { Value = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Floor":
            _curr = new QueryModel.Functions.Floor() { Value = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Round":
            if (m.Arguments.Count > 1 && m.Arguments[1].Type == typeof(int))
            {
              _curr = new QueryModel.Functions.Round()
              {
                Value = VisitAndReturnExpr(m.Arguments[0]),
                Digits = VisitAndReturnExpr(m.Arguments[1])
              };
            }
            else
            {
              _curr = new QueryModel.Functions.Round()
              {
                Value = VisitAndReturnExpr(m.Arguments[0]),
                Digits = new IntegerLiteral(0)
              };
            }
            return m;
          default:
            throw new NotSupportedException();
        }
      }
      else if (m.Method.DeclaringType == typeof(Math))
      {
        switch (m.Method.Name)
        {
          case "Abs":
            _curr = new QueryModel.Functions.Abs() { Value = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Ceiling":
            _curr = new QueryModel.Functions.Ceiling() { Value = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Floor":
            _curr = new QueryModel.Functions.Floor() { Value = VisitAndReturnExpr(m.Arguments[0]) };
            return m;
          case "Round":
            if (m.Arguments.Count > 1 && m.Arguments[1].Type == typeof(int))
            {
              _curr = new QueryModel.Functions.Round()
              {
                Value = VisitAndReturnExpr(m.Arguments[0]),
                Digits = VisitAndReturnExpr(m.Arguments[1])
              };
            }
            else
            {
              _curr = new QueryModel.Functions.Round()
              {
                Value = VisitAndReturnExpr(m.Arguments[0]),
                Digits = new IntegerLiteral(0)
              };
            }
            return m;
          case "Power":
            _curr = new QueryModel.Functions.Power()
            {
              Value = VisitAndReturnExpr(m.Arguments[0]),
              Exponent = VisitAndReturnExpr(m.Arguments[1])
            };
            return m;
          case "Truncate":
            _curr = new QueryModel.Functions.Truncate()
            {
              Value = VisitAndReturnExpr(m.Arguments[0]),
              Digits = new IntegerLiteral(0)
            };
            return m;
          default:
            throw new NotSupportedException();
        }
      }
      else if (m.Method.DeclaringType == typeof(System.Text.RegularExpressions.Regex))
      {
        if (m.Method.Name == "IsMatch" && m.Arguments[1] is ConstantExpression c && c.Value is string str)
        {
          _curr = new LikeOperator()
          {
            Left = VisitAndReturnExpr(m.Arguments[0]),
            Right = RegexParser.Parse(str)
          };
          return m;
        }
        throw new NotSupportedException();
      }

      return base.VisitMethodCall(m);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (m.Member.DeclaringType == typeof(DateTime) || m.Member.DeclaringType == typeof(DateTimeOffset))
      {
        switch (m.Member.Name)
        {
          case "Now":
            _curr = new QueryModel.Functions.CurrentDateTime();
            return m;
          case "UtcNow":
            _curr = new QueryModel.Functions.CurrentUtcDateTime();
            return m;
          case "Date":
            _curr = new QueryModel.Functions.TruncateTime()
            {
              Expression = VisitAndReturnExpr(m.Expression)
            };
            return m;
          case "Day":
            _curr = new QueryModel.Functions.Day() { Expression = VisitAndReturnExpr(m.Expression) };
            return m;
          case "DayOfYear":
            _curr = new QueryModel.Functions.DayOfYear() { Expression = VisitAndReturnExpr(m.Expression) };
            return m;
          case "Hour":
            _curr = new QueryModel.Functions.Hour() { Expression = VisitAndReturnExpr(m.Expression) };
            return m;
          case "Millisecond":
            _curr = new QueryModel.Functions.Millisecond() { Expression = VisitAndReturnExpr(m.Expression) };
            return m;
          case "Minute":
            _curr = new QueryModel.Functions.Minute() { Expression = VisitAndReturnExpr(m.Expression) };
            return m;
          case "Month":
            _curr = new QueryModel.Functions.Month() { Expression = VisitAndReturnExpr(m.Expression) };
            return m;
          case "Second":
            _curr = new QueryModel.Functions.Second() { Expression = VisitAndReturnExpr(m.Expression) };
            return m;
          case "Year":
            _curr = new QueryModel.Functions.Year() { Expression = VisitAndReturnExpr(m.Expression) };
            return m;
        }
      }
      else if (m.Member.DeclaringType.FullName == "Microsoft.VisualBasic.DateAndTime")
      {
        switch (m.Member.Name)
        {
          case "Now":
            _curr = new QueryModel.Functions.CurrentDateTime();
            return m;
        }
      }
      else if (m.Member.DeclaringType == typeof(string))
      {
        switch (m.Member.Name)
        {
          case "Length":
            _curr = new QueryModel.Functions.Length()
            {
              String = VisitAndReturnExpr(m.Expression)
            };
            return m;
        }
      }
      else if (m.Member.DeclaringType == typeof(TimeSpan)
        && m.Expression is BinaryExpression binEx
        && binEx.NodeType == ExpressionType.Subtract)
      {
        switch (m.Member.Name)
        {
          case "TotalDays":
          case "Days":
            _curr = new QueryModel.Functions.DiffDays()
            {
              EndExpression = VisitAndReturnExpr(binEx.Left),
              StartExpression = VisitAndReturnExpr(binEx.Right),
            };
            return m;
          case "TotalHours":
            _curr = new QueryModel.Functions.DiffHours()
            {
              EndExpression = VisitAndReturnExpr(binEx.Left),
              StartExpression = VisitAndReturnExpr(binEx.Right),
            };
            return m;
          case "TotalMinutes":
            _curr = new QueryModel.Functions.DiffMinutes()
            {
              EndExpression = VisitAndReturnExpr(binEx.Left),
              StartExpression = VisitAndReturnExpr(binEx.Right),
            };
            return m;
          case "TotalSeconds":
            _curr = new QueryModel.Functions.DiffSeconds()
            {
              EndExpression = VisitAndReturnExpr(binEx.Left),
              StartExpression = VisitAndReturnExpr(binEx.Right),
            };
            return m;
          case "TotalMilliseconds":
            _curr = new QueryModel.Functions.DiffMilliseconds()
            {
              EndExpression = VisitAndReturnExpr(binEx.Left),
              StartExpression = VisitAndReturnExpr(binEx.Right),
            };
            return m;
        }
      }

      return base.VisitMemberAccess(m);
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
      switch (u.NodeType)
      {
        case ExpressionType.Not:
          _curr = new NotOperator() { Arg = VisitAndReturnExpr(u.Operand) }.Normalize();
          break;
        case ExpressionType.Convert:
          var expr = VisitAndReturnAny(_paramStack.TrySimplify(u.Operand));
          if (expr is PropertyReference && typeof(IReadOnlyItem).IsAssignableFrom(u.Type))
            VisitItem(expr);
          return u;
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
          _curr = new NegationOperator() { Arg = VisitAndReturnExpr(u.Operand) }.Normalize();
          break;
        default:
          throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
      }
      return u;
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      var left = VisitAndReturnExpr(b.Left);
      var right = VisitAndReturnExpr(b.Right);
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
      {
        _curr = new NullLiteral();
      }
      else if (Expressions.TryGetLiteral(c.Value, out var lit))
      {
        _curr = lit;
      }
      else if (c.Value is IReadOnlyItem item)
      {
        _curr = new StringLiteral(item.Id());
      }
      else if (c.Value is IInnovatorQuery table)
      {
        _query = new QueryItem(_aml.LocalizationContext)
        {
          Type = table.ItemType
        };
        _curr = _query;
      }
      else
      {
        throw new NotSupportedException($"Cannot handle literals of type {c.Type.Name}");
      }

      return c;
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      if (_tables.TryGetValue(p, out var queryItem))
        _curr = queryItem;
      return base.VisitParameter(p);
    }

    protected override object VisitProperty(Expression table, string name)
    {
      var queryItem = (QueryItem)VisitAndReturnAny(table);
      _curr = new PropertyReference(name, queryItem);
      return _curr;
    }

    protected override void VisitItem(object property)
    {
      var prop = default(PropertyReference);
      if (property is Expression expr)
        prop = (PropertyReference)VisitAndReturnAny(expr);
      else
        prop = (PropertyReference)property;
      _curr = prop.GetOrAddTable(_aml.LocalizationContext);
    }

    private IExpression VisitAndReturnExpr(Expression expr)
    {
      _curr = null;
      this.Visit(expr);
      if (_curr is QueryItem query)
        return new PropertyReference("id", query);
      return (IExpression)_curr;
    }

    private object VisitAndReturnAny(Expression expr)
    {
      _curr = null;
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
