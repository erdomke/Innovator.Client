using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal static class Expression
  {
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
