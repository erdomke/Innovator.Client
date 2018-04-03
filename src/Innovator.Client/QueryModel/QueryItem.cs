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

    public string Alias { get; set; }
    public IDictionary<string, string> Attributes { get { return _attributes; } }
    public string Name { get; set; }
    public PropertyReference TypeProvider { get; set; }
    public int? Fetch { get; set; }
    public int? Offset { get; set; }

    public IList<Join> Joins { get { return _joins; } }
    public IList<OrderByExpression> OrderBy { get { return _orderBy; } }
    public IList<SelectExpression> Select { get { return _select; } }
    public IExpression Where { get; set; }

    public void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      throw new NotImplementedException();
    }

    public string ToSql(IAmlSqlWriterSettings settings)
    {
      using (var writer = new StringWriter())
      {
        ToSql(writer, settings);
        writer.Flush();
        return writer.ToString();
      }
    }

    public void ToSql(TextWriter writer, IAmlSqlWriterSettings settings)
    {
      var visitor = new SqlServerVisitor(writer, settings);
      visitor.Visit(this);
    }
  }
}
