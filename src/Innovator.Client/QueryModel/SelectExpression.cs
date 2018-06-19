using System.Diagnostics;

namespace Innovator.Client.QueryModel
{
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public class SelectExpression
  {
    public IExpression Expression { get; set; }
    public string Alias { get; set; }
    public bool OnlyReturnNonNull { get; set; }

    private string DebuggerDisplay
    {
      get
      {
        using (var writer = new System.IO.StringWriter())
        {
          var visitor = new SqlServerVisitor(writer, new NullAmlSqlWriterSettings());
          Expression.Visit(visitor);
          if (!string.IsNullOrEmpty(Alias))
          {
            writer.Write(" as ");
            writer.Write(Alias);
          }
          writer.Flush();
          return writer.ToString();
        }
      }
    }
  }
}
