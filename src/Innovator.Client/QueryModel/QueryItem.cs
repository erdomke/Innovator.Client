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
    private readonly List<Join> _joins = new List<Join>();
    private readonly List<OrderByExpression> _orderBy = new List<OrderByExpression>();
    private readonly List<SelectExpression> _select = new List<SelectExpression>();
    private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();

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

    internal PropertyReference GetProperty(IEnumerable<string> path)
    {
      if (!path.Any())
        throw new ArgumentException();

      var table = this;
      var prop = default(PropertyReference);
      foreach (var segment in path)
      {
        if (prop != null)
          table = GetOrAddTable(prop);
        prop = new PropertyReference(segment, table);
      }

      return prop;
    }

    private QueryItem GetOrAddTable(PropertyReference prop)
    {
      var join = prop.Table.Joins.FirstOrDefault(j => j.Condition is EqualsOperator eq
        && new[] { eq.Left, eq.Right }.OfType<PropertyReference>()
          .Any(p => p.Table == prop.Table && p.Name == prop.Name));
      if (join != null)
        return join.Right;

      var newTable = new QueryItem(this.Context)
      {
        TypeProvider = prop
      };
      prop.Table.Joins.Add(new Join()
      {
        Left = prop.Table,
        Right = newTable,
        Condition = new EqualsOperator()
        {
          Left = prop,
          Right = new PropertyReference("id", newTable)
        },
        Type = JoinType.Inner
      });
      return newTable;
    }


    /// <summary>
    /// Converts an AML node into a query which can be converted to other forms (e.g. SQL, OData, ...)
    /// </summary>
    /// <param name="xml">XML data</param>
    /// <param name="context">Localization context for parsing and formating data</param>
    public static QueryItem FromXml(Stream xml, IServerContext context = null)
    {
      var xmlStream = xml as IXmlStream;
      using (var xmlReader = (xmlStream == null ? XmlReader.Create(xml) : xmlStream.CreateReader()))
      {
        return FromXml(xmlReader, context);
      }
    }

    /// <summary>
    /// Converts an AML node into a query which can be converted to other forms (e.g. SQL, OData, ...)
    /// </summary>
    /// <param name="xml">XML data</param>
    /// <param name="context">Localization context for parsing and formating data</param>
    public static QueryItem FromXml(TextReader xml, IServerContext context = null)
    {
      using (var xmlReader = XmlReader.Create(xml))
      {
        return FromXml(xmlReader, context);
      }
    }

    /// <summary>
    /// Converts an AML node into a query which can be converted to other forms (e.g. SQL, OData, ...)
    /// </summary>
    /// <param name="xml">XML data</param>
    /// <param name="context">Localization context for parsing and formating data</param>
    /// <param name="args">Arguments to substitute into the query</param>
    public static QueryItem FromXml(string xml, IServerContext context, params object[] args)
    {
      return FromXml(w =>
      {
        var sub = new ParameterSubstitution();
        sub.AddIndexedParameters(args);
        sub.Substitute(xml, context, w);
      }, context);
    }

    /// <summary>
    /// Converts an AML node into a query which can be converted to other forms (e.g. SQL, OData, ...)
    /// </summary>
    /// <param name="xml">XML data</param>
    /// <param name="context">Localization context for parsing and formating data</param>
    public static QueryItem FromXml(XmlReader xml, IServerContext context = null)
    {
      return FromXml(w => xml.CopyTo(w), context);
    }

    /// <summary>
    /// Converts an AML node into a query which can be converted to other forms (e.g. SQL, OData, ...)
    /// </summary>
    /// <param name="node">XML data</param>
    /// <param name="context">Localization context for parsing and formating data</param>
    public static QueryItem FromXml(IAmlNode node, IServerContext context = null)
    {
      context = context
        ?? (node as IReadOnlyElement)?.AmlContext.LocalizationContext
        ?? ElementFactory.Local.LocalizationContext;
      return FromXml(w => node.ToAml(w, new AmlWriterSettings()), context);
    }

    /// <summary>
    /// Converts an AML node into a query which can be converted to other forms (e.g. SQL, OData, ...)
    /// </summary>
    /// <param name="cmd">XML data</param>
    /// <param name="context">Localization context for parsing and formating data</param>
    public static QueryItem FromXml(Command cmd, IServerContext context = null)
    {
      context = context ?? ElementFactory.Local.LocalizationContext;
      return FromXml(w => cmd.ToNormalizedAml(context, w), context);
    }

    /// <summary>
    /// Converts an AML node into a query which can be converted to other forms (e.g. SQL, OData, ...)
    /// </summary>
    /// <param name="writer">XML data</param>
    /// <param name="context">Localization context for parsing and formating data</param>
    public static QueryItem FromXml(Action<XmlWriter> writer, IServerContext context = null)
    {
      context = context ?? ElementFactory.Local.LocalizationContext;
      using (var w = new AmlToModelWriter(context))
      {
        writer(w);
        return w.Query;
      }
    }
  }
}
