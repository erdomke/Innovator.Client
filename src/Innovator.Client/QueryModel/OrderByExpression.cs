using System.Diagnostics;

namespace Innovator.Client.QueryModel
{
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public class OrderByExpression
  {
    public IExpression Expression { get; set; }
    public bool Ascending { get; set; } = true;

    private string DebuggerDisplay
    {
      get
      {
        using (var writer = new System.IO.StringWriter())
        {
          var visitor = new SqlServerVisitor(writer, new NullAmlSqlWriterSettings());
          Expression.Visit(visitor);
          if (!Ascending)
            writer.Write(" desc");
          writer.Flush();
          return writer.ToString();
        }
      }
    }
  }
}
