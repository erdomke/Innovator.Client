/*
 * https://docs.microsoft.com/en-us/vsts/collaborate/wiql-syntax?view=vsts
 * https://msdn.microsoft.com/en-us/library/aa394054(v=vs.85).aspx
 * http://docs.oasis-open.org/odata/odata/v4.0/errata03/os/complete/part2-url-conventions/odata-v4.0-errata03-os-part2-url-conventions-complete.html
 * https://developer.atlassian.com/server/confluence/advanced-searching-using-cql/
 * https://confluence.atlassian.com/jirasoftwarecloud/advanced-searching-764478330.html
 * https://docs.newrelic.com/docs/insights/nrql-new-relic-query-language/nrql-resources/nrql-syntax-components-functions
 * https://www.ibm.com/developerworks/community/wikis/form/anonymous/api/wiki/02db2a84-fc66-4667-b760-54e495526ec1/page/87348f89-b8b4-4c4a-94bd-ecbe1e4e8857/attachment/2f27f2b3-3583-4b3c-8ad1-ed35bb4e4279/media/MaximoNextGenRESTAPI%20%285%29.pdf
 * https://developers.google.com/chart/interactive/docs/querylanguage
 * 
 * https://restdb.io/docs/querying-with-the-api
 * https://tools.ietf.org/html/draft-nottingham-atompub-fiql-00
 * http://htsql.org/doc/overview.html
 *
 * https://lucene.apache.org/core/6_0_0/queryparser/org/apache/lucene/queryparser/classic/package-summary.html
 * https://docs.microsoft.com/en-us/rest/api/searchservice/lucene-query-syntax-in-azure-search
 * https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-query-string-query.html
 *
 * IDEAS:
 *    Add support for mapping the values in a query to another data model
 *    Create an "under" or "isa" operator for heirarchical fields
 *    
 *    IQueryable with Entity Framework: https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/ef/language-reference/clr-method-to-canonical-function-mapping
 */

#if REFLECTION
using Innovator.Client.Queryable;
using System.Linq.Expressions;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Innovator.Client.Model;

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
    /// Gets or sets the version.
    /// </summary>
    public IVersionCriteria Version { get; set; }
    /// <summary>
    /// Gets or sets the criteria used to filter the items to return.
    /// </summary>
    public IExpression Where { get; set; }

    /// <summary>
    /// Add a condition to the <see cref="Where"/> clause using an <see cref="AndOperator"/>.
    /// </summary>
    /// <param name="expr">Expression to add</param>
    public void AddCondition(IExpression expr)
    {
      if (Where == null)
      {
        Where = expr;
      }
      else
      {
        Where = new AndOperator()
        {
          Left = Where,
          Right = expr
        }.Normalize();
      }
    }

    /// <summary>
    /// Add a condition to the <see cref="Where"/> clause using an <see cref="AndOperator"/>.
    /// </summary>
    /// <param name="prop">Definition of the property</param>
    /// <param name="value">Serialized query to parse</param>
    /// <param name="parser">Parser settings to use</param>
    /// <param name="condition">The condition operate to use (if specified)</param>
    public void AddCondition(IPropertyDefinition prop, string value, SimpleSearchParser parser, Condition condition = Condition.Undefined)
    {
      AddCondition(parser.Parse(this, prop, value, condition));
    }

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
    public string ToArasSql(IAmlSqlWriterSettings settings)
    {
      using (var writer = new StringWriter())
      {
        ToArasSql(writer, settings);
        writer.Flush();
        return writer.ToString();
      }
    }

    /// <summary>
    /// Write the query to the <paramref name="writer"/> as SQL
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="settings">The settings.</param>
    public void ToArasSql(TextWriter writer, IAmlSqlWriterSettings settings)
    {
      var visitor = new ArasSqlServerVisitor(writer, settings);
      visitor.Visit(this);
    }

    public string ToOData(IQueryWriterSettings settings, IServerContext context, ODataVersion version = ODataVersion.All)
    {
      using (var writer = new StringWriter())
      {
        var visitor = new ODataVisitor(writer, settings, context, version);
        visitor.Visit(this);
        writer.Flush();
        return writer.ToString();
      }
    }

    internal PropertyReference GetProperty(IPropertyDefinition propDefn)
    {
      var defn = propDefn;
      var prop = new PropertyReference(defn.NameProp().Value
        ?? defn.KeyedName().Value
        ?? defn.IdProp().KeyedName().Value, this);

      while (defn.DataType().Value == "foreign"
        && defn.Property("foreign_property").HasValue()
        && defn.DataSource().KeyedName().HasValue())
      {
        var linkProp = new PropertyReference(defn.DataSource().KeyedName().Value, prop.Table);
        var table = linkProp.GetOrAddTable(Context);

        defn = (IPropertyDefinition)defn.Property("foreign_property").AsItem();
        if (string.IsNullOrEmpty(table.Type))
          table.Type = defn.SourceId().Attribute("name").Value ?? defn.SourceId().KeyedName().Value;
        prop = new PropertyReference(defn.NameProp().Value
          ?? defn.KeyedName().Value
          ?? defn.IdProp().KeyedName().Value, table);
      }

      return prop;
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
          table = prop.GetOrAddTable(Context);
        prop = new PropertyReference(segment, table);
      }

      return prop;
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
      using (var w = new AnyAmlWriter(context))
      {
        writer(w);
        return w.Query;
      }
    }

    public static QueryItem FromOData(string url, IServerContext context = null, ODataVersion version = ODataVersion.All)
    {
      context = context ?? ElementFactory.Local.LocalizationContext;
      return ODataParser.Parse(url, context, version);
    }

#if REFLECTION
    public static QueryItem FromLinq(string itemType, Func<IOrderedQueryable<IReadOnlyItem>, IQueryable> writer, IServerContext context = null)
    {
      return FromLinq<IReadOnlyItem>(itemType, writer, context);
    }

    public static QueryItem FromLinq<T>(string itemType, Func<IOrderedQueryable<T>, IQueryable> writer, IServerContext context = null) where T : IReadOnlyItem
    {
      context = context ?? ElementFactory.Local.LocalizationContext;

      var queryable = writer(new InnovatorQuery<T>(new Provider(context), itemType));
      var provider = (Provider)queryable.Provider;
      return provider.Translate(queryable.Expression);
    }

    private class Provider : QueryProvider
    {
      private readonly IServerContext _context;

      public Provider(IServerContext context)
      {
        _context = context;
      }

      public override object Execute(Expression expression) => throw new NotSupportedException();

      internal QueryItem Translate(Expression expression)
      {
        expression = Evaluator.PartialEval(expression);
        return new QueryTranslator(new ElementFactory(_context))
          .Translate(expression)
          .QueryItem;
      }
    }
#endif
  }
}
