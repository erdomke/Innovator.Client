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
    private TextWriter _writer;
    private IAmlSqlWriterSettings _settings;
    private Stack<ILogical> _logicals = new Stack<ILogical>();
    private IServerContext _context = ElementFactory.Utc.LocalizationContext;
    private bool _hasFromOrSelect = false;
    private AmlSqlRenderOption _renderOption;

    public SqlServerVisitor(TextWriter writer, IAmlSqlWriterSettings settings)
    {
      _writer = writer;
      _settings = settings;
      _renderOption = _settings.RenderOption;
      if (_renderOption == AmlSqlRenderOption.Default)
        _renderOption = AmlSqlRenderOption.SelectQuery;
    }

    public void Visit(QueryItem query)
    {
      if ((_renderOption & AmlSqlRenderOption.SelectClause) != 0)
        VisitSelect(query);
      if ((_renderOption & AmlSqlRenderOption.FromClause) != 0)
        VisitFrom(query);
      if ((_renderOption & AmlSqlRenderOption.WhereClause) != 0)
        VisitWhere(query);
      if ((_renderOption & AmlSqlRenderOption.OrderByClause) != 0)
        VisitOrderBy(query);

      if ((_renderOption & AmlSqlRenderOption.OffsetClause) != 0)
      {
        if (query.Fetch > 0 && query.Offset > 0)
        {
          _writer.Write(" offset ");
          _writer.Write(query.Offset);
          _writer.Write(" rows fetch next ");
          _writer.Write(query.Fetch);
          _writer.Write(" rows only");
        }
      }
    }

    private void VisitSelect(QueryItem query)
    {
      _hasFromOrSelect = true;
      _writer.Write("select ");

      if ((_renderOption & AmlSqlRenderOption.OffsetClause) != 0
        && query.Fetch > 0
        && (query.Offset ?? 0) < 1)
      {
        _writer.Write("top ");
        _writer.Write(query.Fetch);
        _writer.Write(' ');
      }

      if (!query.Select.Any())
      {
        WriteAlias(query);
        _writer.Write(".*");
      }
      else
      {
        var first = true;
        foreach (var prop in query.Select)
        {
          if (!first)
            _writer.Write(", ");
          first = false;
          prop.Expression.Visit(this);
          if (!string.IsNullOrEmpty(prop.Alias))
          {
            _writer.Write(" as [");
            _writer.Write(prop.Alias);
            _writer.Write("]");
          }
        }
      }
    }

    private void TryFillName(QueryItem item)
    {
      if (string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.TypeProvider?.Table.Name))
      {
        var props = _settings.GetProperties(item.TypeProvider.Table.Name);
        if (props != null && props.TryGetValue(item.TypeProvider.Name, out var propDefn))
        {
          item.Name = propDefn.DataSource().KeyedName().Value;
        }
      }
    }

    private void WriteAlias(QueryItem item)
    {
      TryFillName(item);

      _writer.Write('[');
      if (!string.IsNullOrEmpty(item.Alias))
        _writer.Write(item.Alias);
      else
        _writer.Write(item.Name.Replace(' ', '_'));
      _writer.Write(']');
    }

    private void VisitFrom(QueryItem item)
    {
      if (_hasFromOrSelect)
        _writer.Write(" ");
      _hasFromOrSelect = true;
      _writer.Write("from ");
      VisitTableName(item);
      foreach (var join in item.Joins.Where(j => j.GetCardinality() == Cardinality.OneToOne))
      {
        VisitJoin(join);
      }
    }

    private void VisitTableName(QueryItem item)
    {
      _writer.Write("[innovator].[");
      var sqlName = item.Name.Replace(' ', '_');
      _writer.Write(sqlName);
      _writer.Write("]");
      if (!string.IsNullOrEmpty(item.Alias) && !string.Equals(item.Alias, sqlName, StringComparison.OrdinalIgnoreCase))
      {
        _writer.Write(" as [");
        _writer.Write(item.Alias);
        _writer.Write("]");
      }
    }

    private void VisitJoin(Join join)
    {
      if (join.Type == JoinType.Inner)
        _writer.Write(" inner join ");
      else
        _writer.Write(" left join ");

      VisitTableName(join.Right);
      _writer.Write(" on ");
      join.Condition.Visit(this);

      foreach (var otherJoin in join.Right.Joins.Where(j => j.GetCardinality() == Cardinality.OneToOne))
      {
        VisitJoin(otherJoin);
      }
    }

    private void VisitWhere(QueryItem query)
    {
      var criteria = new List<IExpression>();
      if (query.Where != null)
      {
        var clause = query.Where;
        if (_settings.PermissionOption == AmlSqlPermissionOption.LegacyFunction)
        {
          clause = new AndOperator()
          {
            Left = clause,
            Right = new LegacyPermissionFunction(query)
          };
        }
        criteria.Add(clause);
      }
      AddJoinsToCriteria(query, criteria);

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
          _writer.Write(" where ");
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
          var clause = join.Right.Where;
          if (_settings.PermissionOption == AmlSqlPermissionOption.LegacyFunction)
          {
            clause = new AndOperator()
            {
              Left = clause,
              Right = new LegacyPermissionFunction(join.Right)
            };
          }
          criteria.Add(clause);
        }
        AddJoinsToCriteria(join.Right, criteria);
      }
    }

    private void VisitOrderBy(QueryItem query)
    {
      if (query.OrderBy.Any())
      {
        _writer.Write(" order by ");
        var first = true;
        foreach (var prop in query.OrderBy)
        {
          if (!first)
            _writer.Write(", ");
          first = false;
          Visit(prop);
        }
      }
    }

    private void Visit(OrderByExpression op)
    {
      op.Expression.Visit(this);
      if (!op.Ascending)
        _writer.Write(" DESC");
    }

    public void Visit(AndOperator op)
    {
      var paren = _logicals.Count > 0 && _logicals.Peek() is NotOperator;

      if (paren)
        _writer.Write('(');
      _logicals.Push(op);

      op.Left.Visit(this);
      _writer.Write(" and ");
      op.Right.Visit(this);

      _logicals.Pop();
      if (paren)
        _writer.Write(')');
    }

    public void Visit(BetweenOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" between ");
      op.Min.Visit(this);
      _writer.Write(" and ");
      op.Max.Visit(this);
    }

    public void Visit(BooleanLiteral op)
    {
      _writer.Write('\'');
      _writer.Write(_context.Format(op.Value));
      _writer.Write('\'');
    }

    public void Visit(DateTimeLiteral op)
    {
      _writer.Write('\'');
      _writer.Write(_context.Format(op.Value));
      _writer.Write('\'');
    }

    public void Visit(EqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" = ");
      op.Right.Visit(this);
    }

    public void Visit(FloatLiteral op)
    {
      _writer.Write(_context.Format(op.Value));
    }

    public void Visit(FunctionExpression op)
    {
      var name = op.Name;
      switch (name.ToLowerInvariant())
      {
        case "getdate":
          name = "GetUtcDate";
          break;
      }
      _writer.Write(name);
      _writer.Write('(');
      var first = true;
      foreach (var arg in op.Args)
      {
        if (!first)
          _writer.Write(", ");
        first = false;
        arg.Visit(this);
      }
      _writer.Write(')');
    }

    public void Visit(GreaterThanOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" > ");
      op.Right.Visit(this);
    }

    public void Visit(GreaterThanOrEqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" >= ");
      op.Right.Visit(this);
    }

    public void Visit(InOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" in ");
      op.Right.Visit(this);
    }

    public void Visit(IntegerLiteral op)
    {
      _writer.Write(_context.Format(op.Value));
    }

    public void Visit(IsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" is ");
      switch (op.Right)
      {
        case IsOperand.@null:
        case IsOperand.notDefined:
          _writer.Write(" null");
          break;
        default:
          _writer.Write(" not null");
          break;
      }
    }

    public void Visit(LessThanOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" < ");
      op.Right.Visit(this);
    }

    public void Visit(LessThanOrEqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" <= ");
      op.Right.Visit(this);
    }

    public void Visit(LikeOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" like ");
      op.Right.Visit(this);
    }

    public void Visit(ListExpression op)
    {
      _writer.Write('(');
      var first = true;
      foreach (var arg in op.Values)
      {
        if (!first)
          _writer.Write(", ");
        first = false;
        arg.Visit(this);
      }
      _writer.Write(')');
    }

    public void Visit(NotBetweenOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" not between ");
      op.Min.Visit(this);
      _writer.Write(" and ");
      op.Max.Visit(this);
    }

    public void Visit(NotEqualsOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" <> ");
      op.Right.Visit(this);
    }

    public void Visit(NotInOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" not in ");
      op.Right.Visit(this);
    }

    public void Visit(NotLikeOperator op)
    {
      op.Left.Visit(this);
      _writer.Write(" not like ");
      op.Right.Visit(this);
    }

    public void Visit(NotOperator op)
    {
      _logicals.Push(op);
      op.Arg.Visit(this);
      _logicals.Pop();
    }

    public void Visit(ObjectLiteral op)
    {
      var dataType = default(string);
      if (!string.IsNullOrEmpty(op.TypeProvider?.Table.Name))
      {
        var props = _settings.GetProperties(op.TypeProvider?.Table.Name);
        if (props != null && props.TryGetValue(op.TypeProvider.Name, out var propDefn))
        {
          dataType = propDefn.DataType().Value;
          if (dataType == "foreign")
            dataType = null;
        }
      }

      if (dataType == "boolean")
      {
        Visit(new BooleanLiteral(op.Value == "1"));
      }
      else if ((dataType == null || dataType == "date")
        && DateTime.TryParse(op.Value, out DateTime date))
      {
        Visit(new DateTimeLiteral(date));
      }
      else if ((dataType == null || dataType == "integer")
        && long.TryParse(op.Value, out long lng))
      {
        Visit(new IntegerLiteral(lng));
      }
      else if ((dataType == null || dataType == "float" || dataType == "decimal")
        && double.TryParse(op.Value, out double dbl))
      {
        Visit(new FloatLiteral(dbl));
      }
      else
      {
        if (dataType == "item" || dataType == "md5" || op.Value.IsGuid())
          _writer.Write('\'');
        else
          _writer.Write("N'");
        _writer.Write(op.Value.Replace("'", "''"));
        _writer.Write('\'');
      }
    }

    public void Visit(OrOperator op)
    {
      var paren = _logicals.Count > 0 && !(_logicals.Peek() is OrOperator);

      if (paren)
        _writer.Write('(');
      _logicals.Push(op);

      op.Left.Visit(this);
      _writer.Write(" or ");
      op.Right.Visit(this);

      _logicals.Pop();
      if (paren)
        _writer.Write(')');
    }

    public void Visit(PropertyReference op)
    {
      WriteAlias(op.Table);
      _writer.Write(".[");
      _writer.Write(op.Name);
      _writer.Write(']');
    }

    public void Visit(StringLiteral op)
    {
      if (op.Value.IsGuid())
        _writer.Write('\'');
      else
        _writer.Write("N'");
      _writer.Write(op.Value.Replace("'", "''"));
      _writer.Write('\'');
    }

    private void Visit(LegacyPermissionFunction perm)
    {

      _writer.Write("( SELECT p FROM innovator.[");
      _writer.Write(_settings.PermissionOption == AmlSqlPermissionOption.LegacyFunction ? "GetDiscoverPermissions" : "EvaluatePermissions");
      _writer.Write("] ('can_get', ");
      WriteAlias(perm.Table);
      _writer.Write(".permission_id, ");
      WriteAlias(perm.Table);
      _writer.Write(".created_by_id, ");
      WriteAlias(perm.Table);
      _writer.Write(".managed_by_id, ");
      WriteAlias(perm.Table);
      _writer.Write(".owned_by_id, ");
      WriteAlias(perm.Table);
      _writer.Write(".team_id, '");
      _writer.Write(_settings.IdentityList);
      _writer.Write("', null, '");
      _writer.Write(_settings.UserId);
      _writer.Write("', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0");
    }

    private class LegacyPermissionFunction : IExpression
    {
      public QueryItem Table { get; }

      public LegacyPermissionFunction(QueryItem table)
      {
        Table = table;
      }

      public void Visit(IExpressionVisitor visitor)
      {
        ((SqlServerVisitor)visitor).Visit(this);
      }
    }
  }
}
