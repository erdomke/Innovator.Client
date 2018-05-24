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
    private readonly IServerContext _context = ElementFactory.Utc.LocalizationContext;
    private bool _hasFromOrSelect = false;

    public SqlRenderOption RenderOption { get; set; }
    protected IQueryWriterSettings Settings { get; }
    protected TextWriter Writer { get; }

    public SqlServerVisitor(TextWriter writer, IQueryWriterSettings settings)
    {
      Writer = writer;
      Settings = settings;
      RenderOption = (settings as IAmlSqlWriterSettings)?.RenderOption ?? SqlRenderOption.Default;
    }

    public SqlServerVisitor(TextWriter writer, IQueryWriterSettings settings, IServerContext context)
      : this(writer, settings)
    {
      _context = context;
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
      {
        if (query.Fetch > 0 && query.Offset > 0)
        {
          Writer.Write(" offset ");
          Writer.Write(query.Offset);
          Writer.Write(" rows fetch next ");
          Writer.Write(query.Fetch);
          Writer.Write(" rows only");
        }
      }
    }

    protected virtual void VisitSelect(QueryItem query)
    {
      _hasFromOrSelect = true;
      Writer.Write("select ");

      if ((RenderOption & SqlRenderOption.OffsetClause) != 0
        && query.Fetch > 0
        && (query.Offset ?? 0) < 1)
      {
        Writer.Write("top ");
        Writer.Write(query.Fetch);
        Writer.Write(' ');
      }

      if (query.Select.Count == 0)
      {
        WriteAlias(query);
        Writer.Write(".*");
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

    protected void TryFillName(QueryItem item)
    {
      if (string.IsNullOrEmpty(item.Type) && !string.IsNullOrEmpty(item.TypeProvider?.Table.Type))
      {
        var props = Settings.GetProperties(item.TypeProvider.Table.Type);
        if (props != null && props.TryGetValue(item.TypeProvider.Name, out var propDefn))
        {
          item.Type = propDefn.DataSource().KeyedName().Value;
        }
      }
    }

    protected virtual void WriteAlias(QueryItem item)
    {
      TryFillName(item);
      if (string.IsNullOrEmpty(item.Alias))
        WriteTableName(item);
      else
        WriteIdentifier(item.Alias);
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

    protected virtual void WriteTableDefinition(QueryItem item)
    {
      WriteIdentifier(item.Type);
    }

    protected virtual void WriteTableName(QueryItem item)
    {
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

    protected virtual void VisitWhere(QueryItem query)
    {
      var criteria = new List<IExpression>();
      AddJoinsToCriteria(query, criteria);
      VisitWhere(query, criteria);
    }

    protected void VisitWhere(QueryItem query, List<IExpression> criteria)
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
      foreach (var join in query.Joins.Where(j => j.GetCardinality() == Cardinality.OneToOne))
      {
        if (join.Right.Where != null)
        {
          TryFillName(join.Right);
          criteria.Add(join.Right.Where);
        }
        AddJoinsToCriteria(join.Right, criteria);
      }
    }

    protected virtual void VisitOrderBy(QueryItem query)
    {
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

    private void AddParenthesesIfNeeded(IOperator op, Action render)
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

    public virtual void Visit(BetweenOperator op)
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
      Writer.Write(_context.Format(op.Value));
      Writer.Write('\'');
    }

    public virtual void Visit(DateTimeLiteral op)
    {
      Writer.Write('\'');
      Writer.Write(_context.Format(op.Value));
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
      Writer.Write(_context.Format(op.Value));
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

    public virtual void Visit(InOperator op)
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
      Writer.Write(_context.Format(op.Value));
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

    public virtual void Visit(LikeOperator op)
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
      Writer.Write(".");
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
      Writer.Write(".*");
    }

    public virtual void Visit(PatternList op)
    {
      var writer = new SqlPatternWriter(PatternParser.SqlServer);
      op.Visit(writer);
      Visit(new StringLiteral(writer.ToString()));
    }
  }
}
