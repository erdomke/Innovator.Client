using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal static class Expressions
  {
    public static bool TryGetLiteral(object value, out ILiteral literal)
    {
      literal = null;
      if (value is bool b)
        literal = new BooleanLiteral(b);
      else if (value is byte by)
        literal = new IntegerLiteral(by);
      else if (value is sbyte sb)
        literal = new IntegerLiteral(sb);
      else if (value is short sh)
        literal = new IntegerLiteral(sh);
      else if (value is ushort us)
        literal = new IntegerLiteral(us);
      else if (value is int i)
        literal = new IntegerLiteral(i);
      else if (value is uint ui)
        literal = new IntegerLiteral(ui);
      else if (value is long l)
        literal = new IntegerLiteral(l);
      else if (value is ulong ul)
        literal = new IntegerLiteral((long)ul);
      else if (value is float f)
        literal = new FloatLiteral(f);
      else if (value is double d)
        literal = new FloatLiteral(d);
      else if (value is DateTime dt)
        literal = new DateTimeLiteral(dt);
      else if (value is Guid g)
        literal = new StringLiteral(g.ToArasId());
      else if (value is string str)
        literal = new StringLiteral(str);
      else
        return false;

      return true;
    }

    public static bool TryGetNumberLiteral(string value, out ILiteral literal)
    {
      if (long.TryParse(value, out long lng))
      {
        literal = new IntegerLiteral(lng);
        return true;
      }
      else if (double.TryParse(value, out double dbl))
      {
        literal = new FloatLiteral(dbl);
        return true;
      }
      literal = null;
      return false;
    }

    public static bool TryGetExpression(SqlToken sql, out IExpression expr)
    {
      switch (sql.Type)
      {
        case SqlType.Number:
          var result = TryGetNumberLiteral(sql.Text, out ILiteral literal);
          expr = literal;
          return result;
        case SqlType.String:
          var value = sql.Text.Substring(1, sql.Text.Length - 2).Replace("''", "'");
          if (DateTime.TryParse(value, out var date))
            expr = new DateTimeLiteral(date);
          else
            expr = new StringLiteral(value);
          return true;
        default:
          expr = null;
          return false;
      }
    }

    public static string ToSqlString(this IExpression expr)
    {
      using (var writer = new System.IO.StringWriter())
      {
        var visitor = new SqlServerVisitor(writer, new NullAmlSqlWriterSettings());
        expr.Visit(visitor);
        writer.Flush();
        return writer.ToString();
      }
    }
  }
}
