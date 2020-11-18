using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class SqlServerVisitor : IQueryVisitor
  {
    private readonly Stack<IOperator> _operators = new Stack<IOperator>();
    private bool _hasFromOrSelect = false;
    private readonly Dictionary<string, QueryItem> _aliases = new Dictionary<string, QueryItem>();

    public SqlRenderOption RenderOption { get; set; }

    protected IServerContext Context { get; } = ElementFactory.Utc.LocalizationContext;
    protected IQueryWriterSettings Settings { get; }
    protected TextWriter Writer { get; }
    protected SqlRenderOption WriteState { get; set; } = SqlRenderOption.Default;

    public SqlServerVisitor(TextWriter writer, IQueryWriterSettings settings)
    {
      Writer = writer;
      Settings = settings;
      RenderOption = (settings as IAmlSqlWriterSettings)?.RenderOption ?? SqlRenderOption.Default;
    }

    public SqlServerVisitor(TextWriter writer, IQueryWriterSettings settings, IServerContext context)
      : this(writer, settings)
    {
      Context = context;
    }

    public virtual void Visit(QueryItem query)
    {
      if (RenderOption == SqlRenderOption.Default)
        RenderOption = SqlRenderOption.SelectQuery;

      if ((RenderOption & SqlRenderOption.SelectClause) != 0)
        VisitSelect(query);
      if ((RenderOption & SqlRenderOption.FromClause) != 0)
        VisitFrom(query);
      if ((RenderOption & SqlRenderOption.WhereClause) != 0)
        VisitWhere(query);
      if ((RenderOption & SqlRenderOption.OrderByClause) != 0)
        VisitOrderBy(query);
      if ((RenderOption & SqlRenderOption.OffsetClause) != 0)
        VisitOffsetClause(query);
    }

    protected virtual void VisitSelect(QueryItem query)
    {
      var prevState = WriteState;
      try
      {
        WriteState = SqlRenderOption.SelectClause;
        _hasFromOrSelect = true;
        Writer.Write("select ");

        VisitTopRecords(query);

        if (query.Select.Count == 0)
        {
          WriteAlias(query);
          Writer.Write("*");
        }
        else
        {
          var first = true;
          foreach (var prop in query.Select)
          {
            if (!first)
              Writer.Write(", ");
            first = false;
            prop.Expression.Visit(this);
            if (!string.IsNullOrEmpty(prop.Alias))
            {
              Writer.Write(" as ");
              WriteIdentifier(prop.Alias);
            }
          }
        }
      }
      finally
      {
        WriteState = prevState;
      }
    }

    protected virtual void VisitTopRecords(QueryItem query)
    {
      if ((RenderOption & SqlRenderOption.OffsetClause) != 0
        && query.Fetch > 0
        && (query.Offset ?? 0) < 1)
      {
        Writer.Write("top ");
        Writer.Write(query.Fetch);
        Writer.Write(' ');
      }
    }

    protected void TryFillName(QueryItem item)
    {
      item.TryFillName(Settings);

      if (string.IsNullOrEmpty(item.Alias))
      {
        if (!string.IsNullOrEmpty(item.Type))
        {
          var alias = item.Type;
          var i = 1;
          while (_aliases.TryGetValue(alias, out var table) && !object.ReferenceEquals(table, item))
            alias = item.Type + (++i).ToString();
          _aliases[alias] = item;
          if (i > 1)
            item.Alias = alias;
        }
      }
      else if (_aliases.TryGetValue(item.Alias, out var table)
        && !object.ReferenceEquals(table, item))
      {
        throw new InvalidOperationException("Two tables cannot have the same alias in a given query");
      }
      else
      {
        _aliases[item.Alias] = item;
      }
    }

    protected virtual void WriteAlias(QueryItem item)
    {
      TryFillName(item);
      if (!string.IsNullOrEmpty(item.Alias) || !string.IsNullOrEmpty(item.Type))
      {
        if (string.IsNullOrEmpty(item.Alias))
          WriteTableName(item);
        else
          WriteIdentifier(item.Alias);
        Writer.Write('.');
      }
    }

    protected virtual void WriteIdentifier(string identifier)
    {
      if (NeedsQuotes(identifier))
      {
        Writer.Write('[');
        Writer.Write(identifier);
        Writer.Write(']');
      }
      else
      {
        Writer.Write(identifier);
      }
    }

    protected virtual bool NeedsQuotes(string identifier)
    {
      if (string.IsNullOrEmpty(identifier))
        return true;

      if (!(char.IsLetter(identifier[0]) || identifier[0] == '_'))
        return true;

      for (var i = 1; i < identifier.Length; i++)
      {
        if (char.IsLetterOrDigit(identifier[i])
          || identifier[i] == '_'
          || identifier[i] == '$'
          || identifier[i] == '@'
          || identifier[i] == '#')
        {
          // Do nothing
        }
        else
        {
          return true;
        }
      }

      return SqlTokenizer.IsKeyword(identifier);
    }

    protected virtual void VisitFrom(QueryItem item)
    {
      var prevState = WriteState;
      try
      {
        WriteState = SqlRenderOption.FromClause;

        if (_hasFromOrSelect)
          Writer.Write(" ");
        _hasFromOrSelect = true;
        Writer.Write("from ");
        WriteTableDefinition(item);
        foreach (var join in item.Joins.Where(j => j.GetCardinality() == Cardinality.OneToOne))
        {
          VisitJoin(join);
        }
      }
      finally
      {
        WriteState = prevState;
      }
    }

    protected virtual void WriteTableDefinition(QueryItem item)
    {
      TryFillName(item);
      WriteIdentifier(item.Type);
    }

    protected virtual void WriteTableName(QueryItem item)
    {
      TryFillName(item);
      WriteIdentifier(item.Type);
    }

    protected virtual void VisitJoin(Join join)
    {
      if (join.Type == JoinType.Inner)
        Writer.Write(" inner join ");
      else
        Writer.Write(" left join ");

      WriteTableDefinition(join.Right);
      Writer.Write(" on ");
      join.Condition.Visit(this);

      foreach (var otherJoin in join.Right.Joins.Where(j => j.GetCardinality() == Cardinality.OneToOne))
      {
        VisitJoin(otherJoin);
      }
    }

    protected void VisitWhere(QueryItem query, IEnumerable<IExpression> extraCriteria = null, bool skipIdWhereClause = false)
    {
      var prevState = WriteState;
      try
      {
        WriteState = SqlRenderOption.WhereClause;

        var criteria = new List<IExpression>();
        var clause = GetWhereClause(query, skipIdWhereClause);
        if (clause != null)
          criteria.Add(clause);
        if (extraCriteria != null)
          criteria.AddRange(extraCriteria);
        AddJoinsToCriteria(query, criteria);
        VisitWhere(criteria);
      }
      finally
      {
        WriteState = prevState;
      }
    }

    protected virtual IExpression GetWhereClause(QueryItem query, bool skipIdWhereClause = false)
    {
      return query.Where;
    }

    private void VisitWhere(List<IExpression> criteria)
    {
      if (criteria.Count > 0)
      {
        var expr = criteria[0];
        foreach (var otherExpr in criteria.Skip(1))
        {
          expr = new AndOperator()
          {
            Left = expr,
            Right = otherExpr
          };
        }

        if (_hasFromOrSelect)
          Writer.Write(" where ");
        expr.Visit(this);
      }
    }

    private void AddJoinsToCriteria(QueryItem query, IList<IExpression> criteria)
    {
      foreach (var join in query.Joins)
      {
        if (join.GetCardinality() == Cardinality.OneToOne)
        {
          var clause = GetWhereClause(join.Right);
          if (clause != null)
          {
            TryFillName(join.Right);
            criteria.Add(clause);
          }
          AddJoinsToCriteria(join.Right, criteria);
        }
        else if (join.Type == JoinType.Inner)
        {
          TryFillName(join.Right);
          criteria.Add(new SubQueryExists(join));
        }
      }
    }

    protected virtual void VisitOrderBy(QueryItem query)
    {
      var prevState = WriteState;
      try
      {
        WriteState = SqlRenderOption.OrderByClause;

        var orderBy = GetOrderBy(query);
        if (orderBy.Any())
        {
          Writer.Write(" order by ");
          var first = true;
          foreach (var prop in orderBy)
          {
            if (!first)
              Writer.Write(", ");
            first = false;
            Visit(prop);
          }
        }
      }
      finally
      {
        WriteState = prevState;
      }
    }

    protected virtual IEnumerable<OrderByExpression> GetOrderBy(QueryItem query)
    {
      if (query.OrderBy.Count > 0)
        return query.OrderBy;

      return Enumerable.Empty<OrderByExpression>();
    }

    private void Visit(OrderByExpression op)
    {
      op.Expression.Visit(this);
      if (!op.Ascending)
        Writer.Write(" desc");
    }

    protected virtual void VisitOffsetClause(QueryItem query)
    {
      var prevState = WriteState;
      try
      {
        WriteState = SqlRenderOption.OffsetClause;

        if (query.Fetch > 0 && query.Offset > 0)
        {
          Writer.Write(" offset ");
          Writer.Write(query.Offset);
          Writer.Write(" rows fetch next ");
          Writer.Write(query.Fetch);
          Writer.Write(" rows only");
        }
      }
      finally
      {
        WriteState = prevState;
      }
    }

    protected void AddParenthesesIfNeeded(IOperator op, Action render)
    {
      var paren = _operators.Count > 0 && _operators.Peek().Precedence > op.Precedence;

      if (paren)
        Writer.Write('(');
      _operators.Push(op);

      render();

      _operators.Pop();
      if (paren)
        Writer.Write(')');
    }

    public virtual void Visit(AndOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" and ");
        op.Right.Visit(this);
      });
    }

    public void Visit(BetweenOperator op)
    {
      Visit(op, false);
    }

    protected virtual void Visit(BetweenOperator op, bool not)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write((not ? " not" : "") + " between ");
        op.Min.Visit(this);
        Writer.Write(" and ");
        op.Max.Visit(this);
      });
    }

    public virtual void Visit(BooleanLiteral op)
    {
      Writer.Write('\'');
      Writer.Write(Context.Format(op.Value));
      Writer.Write('\'');
    }

    public virtual void Visit(DateTimeLiteral op)
    {
      Writer.Write('\'');
      Writer.Write(Context.Format(op.Value));
      Writer.Write('\'');
    }

    public virtual void Visit(EqualsOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" = ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(FloatLiteral op)
    {
      var str = Context.Format(op.Value);
      if (str.IndexOf('.') < 0)
        str += ".0";
      Writer.Write(str);
    }

    public virtual void Visit(FunctionExpression op)
    {
      if (op is Functions.CurrentDateTime || op is Functions.CurrentUtcDateTime)
      {
        Writer.Write("getutcdate()");
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    public virtual void Visit(GreaterThanOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" > ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(GreaterThanOrEqualsOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" >= ");
        op.Right.Visit(this);
      });
    }

    public void Visit(InOperator op)
    {
      Visit(op, false);
    }

    protected virtual void Visit(InOperator op, bool not)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write((not ? " not" : "") + " in ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(IntegerLiteral op)
    {
      Writer.Write(Context.Format(op.Value));
    }

    public virtual void Visit(IsOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" is ");
        switch (op.Right)
        {
          case IsOperand.Null:
          case IsOperand.NotDefined:
            Writer.Write("null");
            break;
          default:
            Writer.Write("not null");
            break;
        }
      });
    }

    public virtual void Visit(LessThanOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" < ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(LessThanOrEqualsOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" <= ");
        op.Right.Visit(this);
      });
    }

    public void Visit(LikeOperator op)
    {
      Visit(op, false);
    }

    protected virtual void Visit(LikeOperator op, bool not)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write((not ? " not" : "") + " like ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(ListExpression op)
    {
      Writer.Write('(');
      var first = true;
      foreach (var arg in op.Values)
      {
        if (!first)
          Writer.Write(", ");
        first = false;
        arg.Visit(this);
      }
      Writer.Write(')');
    }

    public virtual void Visit(NotEqualsOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" <> ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(NotOperator op)
    {
      if (op.Arg is BetweenOperator btw)
      {
        Visit(btw, true);
      }
      else if (op.Arg is InOperator inOp)
      {
        Visit(inOp, true);
      }
      else if (op.Arg is LikeOperator like)
      {
        Visit(like, true);
      }
      else
      {
        AddParenthesesIfNeeded(op, () =>
        {
          Writer.Write(" not ");
          op.Arg.Visit(this);
        });
      }
    }

    public virtual void Visit(ObjectLiteral op)
    {
      op.Normalize(Settings).Visit(this);
    }

    public virtual void Visit(OrOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" or ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(PropertyReference op)
    {
      WriteAlias(op.Table);
      WriteIdentifier(op.Name);
    }

    public virtual void Visit(StringLiteral op)
    {
      Writer.Write("'");
      Writer.Write(op.Value.Replace("'", "''"));
      Writer.Write('\'');
    }

    public virtual void Visit(MultiplicationOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" * ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(DivisionOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" / ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(ModulusOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" % ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(AdditionOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" + ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(SubtractionOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" - ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(NegationOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        Writer.Write(" -");
        op.Arg.Visit(this);
      });
    }

    public virtual void Visit(ConcatenationOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" + ");
        op.Right.Visit(this);
      });
    }

    public virtual void Visit(ParameterReference op)
    {
      Writer.Write('@');
      Writer.Write(op.Name);
    }

    public virtual void Visit(AllProperties op)
    {
      if (op.XProperties)
        throw new NotSupportedException();
      WriteAlias(op.Table);
      Writer.Write("*");
    }

    public virtual void Visit(PatternList op)
    {
      Visit(new StringLiteral(PatternParser.SqlServer.Render(op)));
    }

    public virtual void Visit(CountAggregate op)
    {
      if (WriteState == SqlRenderOption.SelectClause)
      {
        Writer.Write("count(*)");
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    protected virtual void Visit(SubQueryExists subQuery)
    {
      var query = subQuery.Join.Right;
      Writer.Write("exists (select null");
      _hasFromOrSelect = true;
      VisitFrom(query);
      VisitWhere(query, new[] { subQuery.Join.Condition });
      Writer.Write(")");
    }

    protected class SubQueryExists : IExpression
    {
      public Join Join { get; }

      public SubQueryExists(Join join)
      {
        Join = join;
      }

      public void Visit(IExpressionVisitor visitor)
      {
        ((SqlServerVisitor)visitor).Visit(this);
      }
    }
  }
}
