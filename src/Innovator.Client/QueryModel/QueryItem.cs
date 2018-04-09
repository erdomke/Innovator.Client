using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client.QueryModel
{
  public class QueryItem : IAmlNode
  {
    private List<Join> _joins = new List<Join>();
    private List<OrderByExpression> _orderBy = new List<OrderByExpression>();
    private List<SelectExpression> _select = new List<SelectExpression>();
    private Dictionary<string, string> _attributes = new Dictionary<string, string>();

    public QueryItem(IServerContext context)
    {
      Context = context;
    }

    /// <summary>
    /// Gets or sets the alias to use
    /// </summary>
    public string Alias { get; set; }
    /// <summary>
    /// Gets additional AML attributes.
    /// </summary>
    public IDictionary<string, string> Attributes { get { return _attributes; } }
    /// <summary>
    /// Gets the context for rendering native .Net types
    /// </summary>
    public IServerContext Context { get; }
    /// <summary>
    /// Gets or sets the name of the item type.
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// Gets or sets the item property from which this <see cref="QueryItem"/> was generated
    /// </summary>
    public PropertyReference TypeProvider { get; set; }
    /// <summary>
    /// Gets or sets how many records to fetch.
    /// </summary>
    public int? Fetch { get; set; }
    /// <summary>
    /// Gets or sets how many records to skip.
    /// </summary>
    public int? Offset { get; set; }

    /// <summary>
    /// Gets the joined tables.
    /// </summary>
    public IList<Join> Joins { get { return _joins; } }
    /// <summary>
    /// Gets the expressions to order the results by.
    /// </summary>
    public IList<OrderByExpression> OrderBy { get { return _orderBy; } }
    /// <summary>
    /// Gets the select columns to return.
    /// </summary>
    public IList<SelectExpression> Select { get { return _select; } }
    /// <summary>
    /// Gets or sets the criteria used to filter the items to return.
    /// </summary>
    public IExpression Where { get; set; }

    /// <summary>
    /// Write the query to the specified <see cref="XmlWriter" /> as AML
    /// </summary>
    /// <param name="writer"><see cref="XmlWriter" /> to write the node to</param>
    /// <param name="settings">Settings controlling how the node is written</param>
    public void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      var visitor = new AmlVisitor(Context, writer);
      visitor.Visit(this);
    }

    /// <summary>
    /// Write the query as SQL
    /// </summary>
    /// <param name="settings">The settings.</param>
    public string ToSql(IAmlSqlWriterSettings settings)
    {
      using (var writer = new StringWriter())
      {
        ToSql(writer, settings);
        writer.Flush();
        return writer.ToString();
      }
    }

    /// <summary>
    /// Write the query to the <paramref name="writer"/> as SQL
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="settings">The settings.</param>
    public void ToSql(TextWriter writer, IAmlSqlWriterSettings settings)
    {
      var visitor = new SqlServerVisitor(writer, settings);
      visitor.Visit(this);
    }
  }
}
