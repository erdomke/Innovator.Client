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
          var left = VisitAndReturn(m.Object ?? m.Arguments[0]);
          var right = VisitAndReturn(m.Arguments[m.Object == null ? 1 : 0]);
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
            _curr = LikeOperator.FromMethod(m.Method.Name, left, right);
            return m;
          case "Concat":
            if (m.Arguments.Count == 1)
            {
              _curr = VisitAndReturn(m.Arguments[0]);
            }
            else if (m.Arguments.Count > 1)
            {
              _curr = new ConcatenationOperator()
              {
                Left = VisitAndReturn(m.Arguments[0]),
                Right = VisitAndReturn(m.Arguments[1]),
              }.Normalize();
              for (var i = 2; i < m.Arguments.Count; i++)
              {
                _curr = new ConcatenationOperator()
                {
                  Left = _curr,
                  Right = VisitAndReturn(m.Arguments[i]),
                }.Normalize();
              }
            }
            return m;
          case "Equals":
            if (m.Object == null)
            {
              _curr = new EqualsOperator()
              {
                Left = VisitAndReturn(m.Arguments[0]),
                Right = VisitAndReturn(m.Arguments[1])
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
          case "IndexOf":
            _curr = new QueryModel.Functions.IndexOf_One()
            {
              String = VisitAndReturn(m.Object),
              Target = VisitAndReturn(m.Arguments[0]),
            };
            return m;
          case "Insert":
            _curr = new ConcatenationOperator()
            {
              Left = new ConcatenationOperator()
              {
                Left = new QueryModel.Functions.Substring_One()
                {
                  String = VisitAndReturn(m.Object),
                  Start = new IntegerLiteral(1),
                  Length = VisitAndReturn(m.Arguments[0])
                },
                Right = VisitAndReturn(m.Arguments[1])
              }.Normalize(),
              Right = new QueryModel.Functions.Substring_One()
              {
                String = VisitAndReturn(m.Object),
                Start = new AdditionOperator()
                {
                  Left = VisitAndReturn(m.Arguments[0]),
                  Right = new IntegerLiteral(1),
                }.Normalize(),
                Length = new SubtractionOperator()
                {
                  Left = new QueryModel.Functions.Length()
                  {
                    String = VisitAndReturn(m.Object),
                  },
                  Right = VisitAndReturn(m.Arguments[0])
                }.Normalize()
              }
            }.Normalize();
            return m;
          case "IsNullOrEmpty":
            _curr = new OrOperator()
            {
              Left = new IsOperator()
              {
                Left = VisitAndReturn(m.Arguments[0]),
                Right = IsOperand.Null
              }.Normalize(),
              Right = new EqualsOperator()
              {
                Left = VisitAndReturn(m.Arguments[0]),
                Right = new StringLiteral("")
              }
            };
            return m;
          case "Remove":
            if (m.Arguments.Count == 1)
            {
              _curr = new QueryModel.Functions.Substring_One()
              {
                String = VisitAndReturn(m.Object),
                Start = new IntegerLiteral(1),
                Length = VisitAndReturn(m.Arguments[0])
              };
            }
            else if (m.Arguments.Count == 2)
            {
              _curr = new ConcatenationOperator()
              {
                Left = new QueryModel.Functions.Substring_One()
                {
                  String = VisitAndReturn(m.Object),
                  Start = new IntegerLiteral(1),
                  Length = VisitAndReturn(m.Arguments[0])
                },
                Right = new QueryModel.Functions.Substring_One()
                {
                  String = VisitAndReturn(m.Object),
                  Start = new AdditionOperator()
                  {
                    Left = new AdditionOperator()
                    {
                      Left = VisitAndReturn(m.Arguments[0]),
                      Right = VisitAndReturn(m.Arguments[1]),
                    }.Normalize(),
                    Right = new IntegerLiteral(1)
                  }.Normalize(),
                  Length = new SubtractionOperator()
                  {
                    Left = new SubtractionOperator()
                    {
                      Left = new QueryModel.Functions.Length()
                      {
                        String = VisitAndReturn(m.Object)
                      },
                      Right = VisitAndReturn(m.Arguments[0])
                    }.Normalize(),
                    Right = VisitAndReturn(m.Arguments[1])
                  }.Normalize()
                }
              }.Normalize();
            }
            return m;
          case "Replace":
            _curr = new QueryModel.Functions.Replace()
            {
              String = VisitAndReturn(m.Object),
              Find = VisitAndReturn(m.Arguments[0]),
              Substitute = VisitAndReturn(m.Arguments[1]),
            };
            return m;
          case "Substring":
            if (m.Arguments.Count == 1)
              _curr = QueryModel.Functions.Substring_One.FromZeroBased(VisitAndReturn(m.Object), VisitAndReturn(m.Arguments[0]));
            else if (m.Arguments.Count == 2)
              _curr = QueryModel.Functions.Substring_One.FromZeroBased(VisitAndReturn(m.Object), VisitAndReturn(m.Arguments[0]), VisitAndReturn(m.Arguments[1]));
            return m;
          case "ToLower":
            _curr = new QueryModel.Functions.ToLower()
            {
              String = VisitAndReturn(m.Object),
            };
            return m;
          case "ToUpper":
            _curr = new QueryModel.Functions.ToUpper()
            {
              String = VisitAndReturn(m.Object),
            };
            return m;
          case "Trim":
            _curr = new QueryModel.Functions.Trim()
            {
              String = VisitAndReturn(m.Object),
            };
            return m;
          case "TrimEnd":
            _curr = new QueryModel.Functions.RTrim()
            {
              String = VisitAndReturn(m.Object),
            };
            return m;
          case "TrimStart":
            _curr = new QueryModel.Functions.LTrim()
            {
              String = VisitAndReturn(m.Object),
            };
            return m;
        }
      }
      else if (m.Method.DeclaringType.FullName == "Microsoft.VisualBasic.Strings")
      {
        switch (m.Method.Name)
        {
          case "Trim":
            _curr = new QueryModel.Functions.Trim() { String = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "LTrim":
            _curr = new QueryModel.Functions.LTrim() { String = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "RTrim":
            _curr = new QueryModel.Functions.RTrim() { String = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Len":
            _curr = new QueryModel.Functions.Length() { String = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Left":
            _curr = new QueryModel.Functions.Left()
            {
              String = VisitAndReturn(m.Arguments[0]),
              Length = VisitAndReturn(m.Arguments[1])
            };
            return m;
          case "Mid":
            _curr = new QueryModel.Functions.Substring_One()
            {
              String = VisitAndReturn(m.Arguments[0]),
              Start = VisitAndReturn(m.Arguments[1]),
              Length = VisitAndReturn(m.Arguments[2])
            };
            return m;
          case "Right":
            _curr = new QueryModel.Functions.Right()
            {
              String = VisitAndReturn(m.Arguments[0]),
              Length = VisitAndReturn(m.Arguments[1])
            };
            return m;
          case "UCase":
            _curr = new QueryModel.Functions.ToUpper() { String = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "LCase":
            _curr = new QueryModel.Functions.ToLower() { String = VisitAndReturn(m.Arguments[0]) };
            return m;
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
                Left = VisitAndReturn(m.Arguments[0]),
                Right = VisitAndReturn(m.Arguments[1]),
              }.Normalize();
            }
            else
            {
              _curr = new EqualsOperator()
              {
                Left = VisitAndReturn(m.Object),
                Right = VisitAndReturn(m.Arguments[0]),
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
                  Expression = VisitAndReturn(m.Object),
                  Number = new FloatLiteral(ts.TotalDays)
                };
              }
              else if (ts.TotalHours >= 1)
              {
                _curr = new QueryModel.Functions.AddHours()
                {
                  Expression = VisitAndReturn(m.Object),
                  Number = new FloatLiteral(ts.TotalHours)
                };
              }
              else if (ts.TotalMinutes >= 1)
              {
                _curr = new QueryModel.Functions.AddMinutes()
                {
                  Expression = VisitAndReturn(m.Object),
                  Number = new FloatLiteral(ts.TotalMinutes)
                };
              }
              else if (ts.TotalSeconds >= 1)
              {
                _curr = new QueryModel.Functions.AddSeconds()
                {
                  Expression = VisitAndReturn(m.Object),
                  Number = new FloatLiteral(ts.TotalSeconds)
                };
              }
              else
              {
                _curr = new QueryModel.Functions.AddMilliseconds()
                {
                  Expression = VisitAndReturn(m.Object),
                  Number = new FloatLiteral(ts.TotalMilliseconds)
                };
              }
              return m;
            }
            break;
          case "AddDays":
            _curr = new QueryModel.Functions.AddDays()
            {
              Expression = VisitAndReturn(m.Object),
              Number = VisitAndReturn(m.Arguments[0])
            };
            return m;
          case "AddHours":
            _curr = new QueryModel.Functions.AddHours()
            {
              Expression = VisitAndReturn(m.Object),
              Number = VisitAndReturn(m.Arguments[0])
            };
            return m;
          case "AddMilliseconds":
            _curr = new QueryModel.Functions.AddMilliseconds()
            {
              Expression = VisitAndReturn(m.Object),
              Number = VisitAndReturn(m.Arguments[0])
            };
            return m;
          case "AddMinutes":
            _curr = new QueryModel.Functions.AddMinutes()
            {
              Expression = VisitAndReturn(m.Object),
              Number = VisitAndReturn(m.Arguments[0])
            };
            return m;
          case "AddMonths":
            _curr = new QueryModel.Functions.AddMonths()
            {
              Expression = VisitAndReturn(m.Object),
              Number = VisitAndReturn(m.Arguments[0])
            };
            return m;
          case "AddSeconds":
            _curr = new QueryModel.Functions.AddSeconds()
            {
              Expression = VisitAndReturn(m.Object),
              Number = VisitAndReturn(m.Arguments[0])
            };
            return m;
          case "AddYears":
            _curr = new QueryModel.Functions.AddYears()
            {
              Expression = VisitAndReturn(m.Object),
              Number = VisitAndReturn(m.Arguments[0])
            };
            return m;
        }
      }
      else if (m.Method.DeclaringType.FullName == "Microsoft.VisualBasic.DateAndTime")
      {
        switch (m.Method.Name)
        {
          case "Year":
            _curr = new QueryModel.Functions.Year() { Expression = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Month":
            _curr = new QueryModel.Functions.Month() { Expression = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Day":
            _curr = new QueryModel.Functions.Day() { Expression = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Hour":
            _curr = new QueryModel.Functions.Hour() { Expression = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Minute":
            _curr = new QueryModel.Functions.Minute() { Expression = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Second":
            _curr = new QueryModel.Functions.Second() { Expression = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "DatePart":
            if (m.Arguments[0] is ConstantExpression c)
            {
              switch ((int)c.Value)
              {
                case 0:
                  _curr = new QueryModel.Functions.Year() { Expression = VisitAndReturn(m.Arguments[0]) };
                  return m;
                case 2:
                  _curr = new QueryModel.Functions.Month() { Expression = VisitAndReturn(m.Arguments[0]) };
                  return m;
                case 4:
                  _curr = new QueryModel.Functions.Day() { Expression = VisitAndReturn(m.Arguments[0]) };
                  return m;
                case 7:
                  _curr = new QueryModel.Functions.Hour() { Expression = VisitAndReturn(m.Arguments[0]) };
                  return m;
                case 8:
                  _curr = new QueryModel.Functions.Minute() { Expression = VisitAndReturn(m.Arguments[0]) };
                  return m;
                case 9:
                  _curr = new QueryModel.Functions.Second() { Expression = VisitAndReturn(m.Arguments[0]) };
                  return m;
              }
            }
            break;
          case "DateDiff":
            if (m.Arguments[0] is ConstantExpression c1)
            {
              switch ((int)c1.Value)
              {
                case 0:
                  _curr = new QueryModel.Functions.DiffYears()
                  {
                    StartExpression = VisitAndReturn(m.Arguments[0]),
                    EndExpression = VisitAndReturn(m.Arguments[1]),
                  };
                  return m;
                case 2:
                  _curr = new QueryModel.Functions.DiffMonths()
                  {
                    StartExpression = VisitAndReturn(m.Arguments[0]),
                    EndExpression = VisitAndReturn(m.Arguments[1]),
                  };
                  return m;
                case 4:
                  _curr = new QueryModel.Functions.DiffDays()
                  {
                    StartExpression = VisitAndReturn(m.Arguments[0]),
                    EndExpression = VisitAndReturn(m.Arguments[1]),
                  };
                  return m;
                case 7:
                  _curr = new QueryModel.Functions.DiffHours()
                  {
                    StartExpression = VisitAndReturn(m.Arguments[0]),
                    EndExpression = VisitAndReturn(m.Arguments[1]),
                  };
                  return m;
                case 8:
                  _curr = new QueryModel.Functions.DiffMinutes()
                  {
                    StartExpression = VisitAndReturn(m.Arguments[0]),
                    EndExpression = VisitAndReturn(m.Arguments[1]),
                  };
                  return m;
                case 9:
                  _curr = new QueryModel.Functions.DiffSeconds()
                  {
                    StartExpression = VisitAndReturn(m.Arguments[0]),
                    EndExpression = VisitAndReturn(m.Arguments[1]),
                  };
                  return m;
              }
            }
            break;
        }
      }
      else if (m.Method.DeclaringType == typeof(Guid))
      {
        switch (m.Method.Name)
        {
          case "NewGuid":
            _curr = new QueryModel.Functions.NewGuid();
            return m;
        }
      }
      else if (m.Method.DeclaringType == typeof(Decimal))
      {
        switch (m.Method.Name)
        {
          case "Ceiling":
            _curr = new QueryModel.Functions.Ceiling() { Value = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Floor":
            _curr = new QueryModel.Functions.Floor() { Value = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Round":
            if (m.Arguments.Count > 1 && m.Arguments[1].Type == typeof(int))
            {
              _curr = new QueryModel.Functions.Round()
              {
                Value = VisitAndReturn(m.Arguments[0]),
                Digits = VisitAndReturn(m.Arguments[1])
              };
            }
            else
            {
              _curr = new QueryModel.Functions.Round()
              {
                Value = VisitAndReturn(m.Arguments[0]),
                Digits = new IntegerLiteral(0)
              };
            }
            return m;
        }
      }
      else if (m.Method.DeclaringType == typeof(Math))
      {
        switch (m.Method.Name)
        {
          case "Abs":
            _curr = new QueryModel.Functions.Abs() { Value = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Ceiling":
            _curr = new QueryModel.Functions.Ceiling() { Value = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Floor":
            _curr = new QueryModel.Functions.Floor() { Value = VisitAndReturn(m.Arguments[0]) };
            return m;
          case "Round":
            if (m.Arguments.Count > 1 && m.Arguments[1].Type == typeof(int))
            {
              _curr = new QueryModel.Functions.Round()
              {
                Value = VisitAndReturn(m.Arguments[0]),
                Digits = VisitAndReturn(m.Arguments[1])
              };
            }
            else
            {
              _curr = new QueryModel.Functions.Round()
              {
                Value = VisitAndReturn(m.Arguments[0]),
                Digits = new IntegerLiteral(0)
              };
            }
            return m;
          case "Power":
            _curr = new QueryModel.Functions.Power()
            {
              Value = VisitAndReturn(m.Arguments[0]),
              Exponent = VisitAndReturn(m.Arguments[1])
            };
            return m;
          case "Truncate":
            _curr = new QueryModel.Functions.Truncate()
            {
              Value = VisitAndReturn(m.Arguments[0]),
              Digits = new IntegerLiteral(0)
            };
            return m;
        }
      }
      else if (m.Method.DeclaringType == typeof(System.Text.RegularExpressions.Regex)
        && m.Method.Name == "IsMatch" && m.Arguments[1] is ConstantExpression c && c.Value is string str)
      {
        _curr = new LikeOperator()
        {
          Left = VisitAndReturn(m.Arguments[0]),
          Right = RegexParser.Parse(str)
        };
        return m;
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
              Expression = VisitAndReturn(m.Expression)
            };
            return m;
          case "Day":
            _curr = new QueryModel.Functions.Day() { Expression = VisitAndReturn(m.Expression) };
            return m;
          case "DayOfYear":
            _curr = new QueryModel.Functions.DayOfYear() { Expression = VisitAndReturn(m.Expression) };
            return m;
          case "Hour":
            _curr = new QueryModel.Functions.Hour() { Expression = VisitAndReturn(m.Expression) };
            return m;
          case "Millisecond":
            _curr = new QueryModel.Functions.Millisecond() { Expression = VisitAndReturn(m.Expression) };
            return m;
          case "Minute":
            _curr = new QueryModel.Functions.Minute() { Expression = VisitAndReturn(m.Expression) };
            return m;
          case "Month":
            _curr = new QueryModel.Functions.Month() { Expression = VisitAndReturn(m.Expression) };
            return m;
          case "Second":
            _curr = new QueryModel.Functions.Second() { Expression = VisitAndReturn(m.Expression) };
            return m;
          case "Year":
            _curr = new QueryModel.Functions.Year() { Expression = VisitAndReturn(m.Expression) };
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
              String = VisitAndReturn(m.Expression)
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
              EndExpression = VisitAndReturn(binEx.Left),
              StartExpression = VisitAndReturn(binEx.Right),
            };
            return m;
          case "TotalHours":
            _curr = new QueryModel.Functions.DiffHours()
            {
              EndExpression = VisitAndReturn(binEx.Left),
              StartExpression = VisitAndReturn(binEx.Right),
            };
            return m;
          case "TotalMinutes":
            _curr = new QueryModel.Functions.DiffMinutes()
            {
              EndExpression = VisitAndReturn(binEx.Left),
              StartExpression = VisitAndReturn(binEx.Right),
            };
            return m;
          case "TotalSeconds":
            _curr = new QueryModel.Functions.DiffSeconds()
            {
              EndExpression = VisitAndReturn(binEx.Left),
              StartExpression = VisitAndReturn(binEx.Right),
            };
            return m;
          case "TotalMilliseconds":
            _curr = new QueryModel.Functions.DiffMilliseconds()
            {
              EndExpression = VisitAndReturn(binEx.Left),
              StartExpression = VisitAndReturn(binEx.Right),
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
