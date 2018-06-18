using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class ArasSqlServerVisitor : SqlServerVisitor
  {
    public ArasSqlServerVisitor(TextWriter writer, IAmlSqlWriterSettings settings) : base(writer, settings)
    {
    }

    public ArasSqlServerVisitor(TextWriter writer, IAmlSqlWriterSettings settings, IServerContext context) : base(writer, settings, context)
    {
    }

    public override void Visit(QueryItem query)
    {
      if (RenderOption == SqlRenderOption.Default)
      {
        if (query.Attributes.TryGetValue("offsetId", out var offsetId))
        {
          RenderOption = SqlRenderOption.OffsetQuery;
        }
        else if (query.Select.Count == 1
          && query.Select[0].Expression is CountAggregate cnt
          && cnt.TablePath.Count == 1
          && cnt.TablePath[0] == query)
        {
          RenderOption = SqlRenderOption.CountQuery;
        }
        else
        {
          RenderOption = SqlRenderOption.SelectQuery;
        }
      }

      switch (RenderOption)
      {
        case SqlRenderOption.CountQuery:
          Writer.Write("select isnull(sum(cnt), 0) count from (select ");
          WritePermissionFields(query);
          Writer.Write(", count(*) cnt ");
          VisitFrom(query);
          VisitWhere(query);
          Writer.Write(" group by ");
          WritePermissionFields(query);
          Writer.Write(") perm where ");
          WritePermissionCheck(() => Writer.Write("perm."));
          break;
        case SqlRenderOption.OffsetQuery:
          Writer.Write("select isnull(sum(cnt), 0) offset from ( select ");
          WritePermissionFields(query);
          Writer.Write(", count(*) cnt from ");
          WriteTableDefinition(query);
          Writer.Write(" inner join ( select ");
          var first = true;
          foreach (var orderBy in GetOrderBy(query))
          {
            if (!first)
              Writer.Write(", ");
            first = false;
            orderBy.Expression.Visit(this);
          }
          Writer.Write(" ");
          VisitFrom(query);
          VisitWhere(query);
          Writer.Write(") offset on ");

          var cols = GetOrderBy(query).ToArray();
          for (var i = 0; i < cols.Length; i++)
          {
            if (i > 0)
              Writer.Write(" or ");
            Writer.Write('(');
            for (var j = 0; j < i; j++)
            {
              WriteAlias(query);
              Writer.Append("[");
              Writer.Write(((PropertyReference)cols[j].Expression).Name);
              Writer.Write("] = offset.[");
              Writer.Write(((PropertyReference)cols[j].Expression).Name);
              Writer.Write("] and ");
            }
            WriteAlias(query);
            Writer.Append("[");
            Writer.Write(((PropertyReference)cols[i].Expression).Name);
            Writer.Write(cols[i].Ascending ? "] < " : "] > ");
            Writer.Write("offset.[");
            Writer.Write(((PropertyReference)cols[i].Expression).Name);
            Writer.Write("])");
          }

          var genVisitor = new GenerationCriteriaVisitor();
          query.Where.Visit(genVisitor);
          if (query.Version is CurrentVersion)
          {
            genVisitor.Criteria = new EqualsOperator()
            {
              Left = new PropertyReference("is_current", query),
              Right = new BooleanLiteral(true)
            };
          }

          if (genVisitor.Criteria != null)
          {
            Writer.Write(" where ");
            genVisitor.Criteria.Visit(this);
          }

          Writer.Write(" group by ");
          WritePermissionFields(query);
          Writer.Write(" ) perm where ");
          WritePermissionCheck(() => Writer.Write("perm."));
          break;
        default:
          base.Visit(query);
          break;
      }
    }

    private class GenerationCriteriaVisitor : SimpleVisitor
    {
      public IExpression Criteria { get; set; }

      public override void Visit(EqualsOperator op)
      {
        if (op.Left is PropertyReference prop
          && op.Right is BooleanLiteral boolLit
          && (prop.Name == "is_current" || prop.Name == "is_active_rev"))
        {
          Criteria = op;
        }
      }
    }

    private void WritePermissionFields(QueryItem query)
    {
      WriteAlias(query);
      Writer.Write("permission_id, ");
      WriteAlias(query);
      Writer.Write("created_by_id, ");
      WriteAlias(query);
      Writer.Write("managed_by_id, ");
      WriteAlias(query);
      Writer.Write("owned_by_id, ");
      WriteAlias(query);
      Writer.Write("team_id");
    }

    protected override void WriteTableDefinition(QueryItem item)
    {
      TryFillName(item);
      var _settings = (IAmlSqlWriterSettings)Settings;
      var secured = _settings.PermissionOption == AmlSqlPermissionOption.SecuredFunction
        || _settings.PermissionOption == AmlSqlPermissionOption.SecuredFunctionEnviron;
      if (RenderOption == SqlRenderOption.CountQuery
        || RenderOption == SqlRenderOption.OffsetQuery)
      {
        secured = false;
      }

      Writer.Write(secured ? "secured." : "innovator.");
      var sqlName = item.Type.Replace(' ', '_');
      WriteIdentifier(sqlName);

      if (secured)
      {
        Writer.Append("('can_get','")
          .Append(_settings.IdentityList)
          .Append("',null,'")
          .Append(_settings.UserId)
          .Append("',null");
        if (_settings.PermissionOption == AmlSqlPermissionOption.SecuredFunctionEnviron)
          Writer.Append(",null");
        Writer.Append(")");
      }

      if (!string.IsNullOrEmpty(item.Alias) && !string.Equals(item.Alias, sqlName, StringComparison.OrdinalIgnoreCase))
      {
        Writer.Write(" as ");
        WriteIdentifier(item.Alias);
      }
    }

    protected override void WriteTableName(QueryItem item)
    {
      TryFillName(item);
      WriteIdentifier(item.Type.Replace(' ', '_'));
    }

    protected override void VisitWhere(QueryItem query)
    {
      var criteria = new List<IExpression>();
      var clause = AddPermissionCheck(query);

      if (RenderOption == SqlRenderOption.OffsetQuery)
      {
        if (!query.Attributes.TryGetValue("offsetId", out var offsetId))
          throw new InvalidOperationException("No `offsetId` attribute was specified");

        clause = AppendCriteria(clause, new EqualsOperator()
        {
          Left = new PropertyReference("id", query),
          Right = new StringLiteral(offsetId)
        });
      }

      if (clause != null)
        criteria.Add(clause);
      AddJoinsToCriteria(query, criteria);
      VisitWhere(query, criteria);
    }

    private IExpression AddPermissionCheck(QueryItem query)
    {
      var clause = query.Where;
      if (query.Version is CurrentVersion)
      {
        clause = AppendCriteria(clause, new EqualsOperator()
        {
          Left = new PropertyReference("is_current", query),
          Right = new BooleanLiteral(true)
        }.Normalize());
      }

      if (((IAmlSqlWriterSettings)Settings).PermissionOption == AmlSqlPermissionOption.LegacyFunction
          && RenderOption != SqlRenderOption.CountQuery
          && RenderOption != SqlRenderOption.OffsetQuery)
      {
        clause = AppendCriteria(clause, new LegacyPermissionFunction(query));
      }
      return clause;
    }

    private IExpression AppendCriteria(IExpression orig, IExpression additional)
    {
      if (orig == null)
        return additional;
      return new AndOperator()
      {
        Left = orig,
        Right = additional
      };
    }

    private void AddJoinsToCriteria(QueryItem query, IList<IExpression> criteria)
    {
      foreach (var join in query.Joins.Where(j => j.GetCardinality() == Cardinality.OneToOne))
      {
        if (join.Right.Where != null
          || (join.Type == JoinType.Inner
            && ((IAmlSqlWriterSettings)Settings).PermissionOption == AmlSqlPermissionOption.LegacyFunction
            && RenderOption != SqlRenderOption.CountQuery
            && RenderOption != SqlRenderOption.OffsetQuery))
        {
          TryFillName(join.Right);
          criteria.Add(AddPermissionCheck(join.Right));
        }
        AddJoinsToCriteria(join.Right, criteria);
      }
    }

    protected override IEnumerable<OrderByExpression> GetOrderBy(QueryItem query)
    {
      if (query.OrderBy.Count > 0)
        return query.OrderBy;

      var props = Settings.GetProperties(query.Type).Values;
      var orderProps = props
          .OfType<Model.Property>()
          .Where(p => p.OrderBy().HasValue())
          .OrderBy(p => p.OrderBy().AsInt(int.MaxValue))
          .Select(p => new OrderByExpression()
          {
            Expression = new PropertyReference(p.NameProp().Value, query)
          })
          .ToArray();

      if (orderProps.Length > 0)
        return orderProps;

      return new[]
      {
        new OrderByExpression()
        {
          Expression = new PropertyReference("id", query)
        }
      };
    }

    public override void Visit(PropertyReference op)
    {
      if (op.Name.StartsWith("xp-"))
        throw new NotSupportedException();
      base.Visit(op);
    }

    public override void Visit(StringLiteral op)
    {
      if (op.Value.IsGuid())
        Writer.Write('\'');
      else
        Writer.Write("N'");
      Writer.Write(op.Value.Replace("'", "''"));
      Writer.Write('\'');
    }

    private void Visit(LegacyPermissionFunction perm)
    {
      WritePermissionCheck(() => WriteAlias(perm.Table));
    }

    private void WritePermissionCheck(Action writeAlias)
    {
      var settings = (IAmlSqlWriterSettings)Settings;
      Writer.Write("( SELECT p FROM innovator.[");
      Writer.Write(settings.PermissionOption == AmlSqlPermissionOption.LegacyFunction ? "GetDiscoverPermissions" : "EvaluatePermissions");
      Writer.Write("] ('can_get', ");
      writeAlias();
      Writer.Write("permission_id, ");
      writeAlias();
      Writer.Write("created_by_id, ");
      writeAlias();
      Writer.Write("managed_by_id, ");
      writeAlias();
      Writer.Write("owned_by_id, ");
      writeAlias();
      Writer.Write("team_id, '");
      Writer.Write(settings.IdentityList);
      Writer.Write("', null, '");
      Writer.Write(settings.UserId);
      Writer.Write("', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0");
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
        ((ArasSqlServerVisitor)visitor).Visit(this);
      }
    }
  }
}
