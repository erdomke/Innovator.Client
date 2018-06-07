using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class OracleSqlVisitor : SqlServerVisitor
  {
    private static readonly PatternParser Oracle = new PatternParser('%', '_', '\0', '\0');

    public OracleSqlVisitor(System.IO.TextWriter writer, IQueryWriterSettings settings) : base(writer, settings)
    {
    }

    public OracleSqlVisitor(System.IO.TextWriter writer, IQueryWriterSettings settings, IServerContext context) : base(writer, settings, context)
    {
    }

    protected override void VisitTopRecords(QueryItem query)
    {
      // Do nothing
    }

    protected override void VisitOffsetClause(QueryItem query)
    {
      if (query.Fetch > 0)
      {
        Writer.Write(" offset ");
        Writer.Write(query.Offset ?? 0);
        Writer.Write(" rows fetch next ");
        Writer.Write(query.Fetch);
        Writer.Write(" rows only");
      }
    }

    protected override bool NeedsQuotes(string identifier)
    {
      if (string.IsNullOrEmpty(identifier))
        return true;

      if (!char.IsLetter(identifier[0]))
        return true;

      for (var i = 1; i < identifier.Length; i++)
      {
        if (char.IsLetterOrDigit(identifier[i])
          || identifier[i] == '_'
          || identifier[i] == '$'
          || identifier[i] == '#')
        {
          // Do nothing
        }
        else
        {
          return true;
        }
      }

      switch (identifier.ToUpperInvariant())
      {
        case "ACCESS":
        case "ADD":
        case "ALL":
        case "ALTER":
        case "AND":
        case "ANY":
        case "AS":
        case "ASC":
        case "AUDIT":
        case "BETWEEN":
        case "BY":
        case "CHAR":
        case "CHECK":
        case "CLUSTER":
        case "COLUMN":
        case "COMMENT":
        case "COMPRESS":
        case "CONNECT":
        case "CREATE":
        case "CURRENT":
        case "DATE":
        case "DECIMAL":
        case "DEFAULT":
        case "DELETE":
        case "DESC":
        case "DISTINCT":
        case "DROP":
        case "ELSE":
        case "EXCLUSIVE":
        case "EXISTS":
        case "FILE":
        case "FLOAT":
        case "FOR":
        case "FROM":
        case "GRANT":
        case "GROUP":
        case "HAVING":
        case "IDENTIFIED":
        case "IMMEDIATE":
        case "IN":
        case "INCREMENT":
        case "INDEX":
        case "INITIAL":
        case "INSERT":
        case "INTEGER":
        case "INTERSECT":
        case "INTO":
        case "IS":
        case "LEVEL":
        case "LIKE":
        case "LOCK":
        case "LONG":
        case "MAXEXTENTS":
        case "MINUS":
        case "MLSLABEL":
        case "MODE":
        case "MODIFY":
        case "NOAUDIT":
        case "NOCOMPRESS":
        case "NOT":
        case "NOWAIT":
        case "NULL":
        case "NUMBER":
        case "OF":
        case "OFFLINE":
        case "ON":
        case "ONLINE":
        case "OPTION":
        case "OR":
        case "ORDER":
        case "PCTFREE":
        case "PRIOR":
        case "PRIVILEGES":
        case "PUBLIC":
        case "RAW":
        case "RENAME":
        case "RESOURCE":
        case "REVOKE":
        case "ROW":
        case "ROWID":
        case "ROWNUM":
        case "ROWS":
        case "SELECT":
        case "SESSION":
        case "SET":
        case "SHARE":
        case "SIZE":
        case "SMALLINT":
        case "START":
        case "SUCCESSFUL":
        case "SYNONYM":
        case "SYSDATE":
        case "TABLE":
        case "THEN":
        case "TO":
        case "TRIGGER":
        case "UID":
        case "UNION":
        case "UNIQUE":
        case "UPDATE":
        case "USER":
        case "VALIDATE":
        case "VALUES":
        case "VARCHAR":
        case "VARCHAR2":
        case "VIEW":
        case "WHENEVER":
        case "WHERE":
        case "WITH":
          return true;
      }
      return false;
    }

    public override void Visit(ConcatenationOperator op)
    {
      AddParenthesesIfNeeded(op, () =>
      {
        op.Left.Visit(this);
        Writer.Write(" || ");
        op.Right.Visit(this);
      });
    }

    public override void Visit(DateTimeLiteral op)
    {
      if (op.Value.TimeOfDay.TotalMilliseconds > 0)
      {
        Writer.Write("TIMESTAMP'");
        Writer.Write(Context.Format(op.Value).Replace('T', ' '));
        Writer.Write('\'');
      }
      else
      {
        Writer.Write("DATE'");
        Writer.Write(Context.Format(op.Value).Substring(0, 10));
        Writer.Write('\'');
      }
    }

    protected override void Visit(LikeOperator op, bool not)
    {
      if (op.Right is PatternList pat)
      {
        var pattern = default(StringLiteral);
        var isRegex = false;
        try
        {
          pattern = new StringLiteral(Oracle.Render(pat));
        }
        catch (NotSupportedException)
        {
          var writer = new RegexWriter();
          pat.Visit(writer);
          pattern = new StringLiteral(writer.ToString());
          isRegex = true;
        }

        AddParenthesesIfNeeded(op, () =>
        {
          if (isRegex)
          {
            if (not)
              Writer.Write(" not ");
            Writer.Write("regexp_like(");
            op.Left.Visit(this);
            Writer.Write(",");
            pattern.Visit(this);
            Writer.Write(", 'i')");
          }
          else
          {
            op.Left.Visit(this);
            Writer.Write((not ? " not" : "") + " like ");
            pattern.Visit(this);
          }
        });
      }
      else
      {
        base.Visit(op, not);
      }
    }

    public override void Visit(ModulusOperator op)
    {
      Writer.Write("mod(");
      op.Left.Visit(this);
      Writer.Write(",");
      op.Right.Visit(this);
      Writer.Write(")");
    }

    public override void Visit(ParameterReference op)
    {
      Writer.Write(':');
      Writer.Write(op.Name);
    }
  }
}
