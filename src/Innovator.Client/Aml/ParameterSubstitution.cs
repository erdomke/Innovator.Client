using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// Whether the query is AML or SQL
  /// </summary>
  public enum ParameterSubstitutionMode
  {
    /// <summary>
    /// The query is AML
    /// </summary>
    Aml,
    /// <summary>
    /// The query is SQL
    /// </summary>
    Sql
  }

  /// <summary>
  /// Class for substituting @-prefixed parameters with their values
  /// </summary>
  /// <remarks>
  /// <para>This class will substitute values into an AML template.  Using this class to perform
  /// the substitution (via creating a <see cref="Command"/>) provides several benefits over other
  /// techniques (e.g. <see cref="String.Format(string, object[])"/>)</para>
  /// <list type="table">
  ///   <item>
  ///     <term>String values will be properly escaped for AML.</term>
  ///     <description>For example, <c>Items 1 &amp; 2 are &gt; Item 3</c> will be encoded as
  ///     <c>Items 1 &amp;amp; 2 are &amp;gt; Item 3</c>.  If you don't want this to happen, add an
  ///     exclamation mark after your parameter name (e.g. using <c>"&lt;prop&gt;@0&lt;/prop&gt;"</c>
  ///     will encode the string while using <c>"&lt;prop&gt;@0!&lt;/prop&gt;"</c> will not perform
  ///     any encoding</description>
  ///   </item>
  ///   <item>
  ///     <term>Basic .Net types will be handled properly</term>
  ///     <description>For example, <c>true</c> will be encoded as <c>1</c> and 
  ///     <c>new DateTime(2000, 1, 1)</c> will be encoded as <c>2000-01-01T00:00:00</c></description>
  ///   </item>
  /// </list>
  /// <para>Parameters can appear in AML attributes, elements, or in SQL strings.  Since AML 
  /// attributes always are surrounded by quotes, be sure to put your parameter name in quotes as
  /// well (whether they are single quotes or double quotes).  In SQL, parameters are not quoted
  /// as not all values need to be quoted.  Rather, the replacement process will decide whether or
  /// not quotes are needed.  For example,</para>
  /// <code lang="XML">
  /// &lt;Item type='Part' action='@0'&gt;
  ///   &lt;state condition='in'&gt;(select name from innovator.table where id = @1)&lt;/state&gt;
  /// &lt;/Item&gt;
  /// </code>
  /// </remarks>
  public class ParameterSubstitution : IEnumerable<KeyValuePair<string, object>>, IFormatProvider, ICustomFormatter
  {
    private const string EmptyListMatch = "`EMTPY_LIST_MUST_MATCH_0_ITEMS!`";

    private IServerContext _context;
    private SqlFormatter _sqlFormatter;
    private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
    private int _itemCount = 0;

    /// <summary>
    /// Gets the number of <c>Item</c> tags found in the query
    /// </summary>
    public int ItemCount { get { return _itemCount; } }

    /// <summary>
    /// Whether the query is AML or SQL
    /// </summary>
    public ParameterSubstitutionMode Mode { get; set; }

    /// <summary>
    /// Gets the number of parameters for which values were specified
    /// </summary>
    public int ParamCount { get { return _parameters.Count; } }

    /// <summary>
    /// Gets or sets a callback function which will be called when
    /// a parameter is accessed
    /// </summary>
    public Action<string> ParameterAccessListener { get; set; }

    /// <summary>
    /// Initializes a new <see cref="ParameterSubstitution"/> instance for 
    /// substituting @-prefixed parameters with their values.
    /// </summary>
    /// <remarks>A new instance should be created for each substitution
    /// operation</remarks>
    public ParameterSubstitution()
    {
      this.Mode = ParameterSubstitutionMode.Aml;
    }

    /// <summary>
    /// Adds the array of values as parameters where each value is named
    /// according to its index in the arra
    /// </summary>
    /// <param name="values">The array of values.</param>
    public void AddIndexedParameters(object[] values)
    {
      if (values == null)
      {
        AddParameter("0", null);
        return;
      }

      if (values.GetType().GetElementType().Name != "Object")
        values = new object[] { values };

      for (var i = 0; i < values.Length; i++)
      {
        AddParameter(i.ToString(), values[i]);
      }
    }

    /// <summary>
    /// Adds the parameter.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    public void AddParameter(string name, object value)
    {
      _parameters[name] = value;
    }

    /// <summary>
    /// Clears the parameters.
    /// </summary>
    public void ClearParameters()
    {
      _parameters.Clear();
    }

    /// <summary>
    /// Substitutes the stored parameters into the specified query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="context">The context.</param>
    /// <returns>A string of the new query</returns>
    public string Substitute(string query, IServerContext context)
    {
      if (string.IsNullOrEmpty(query))
        return query;
      var builder = new StringBuilder(query.Length);
      using (var writer = new StringWriter(builder))
      {
        Substitute(query, context, writer);
        writer.Flush();
        return writer.ToString();
      }
    }
    /// <summary>
    /// Substitutes the stored parameters into the specified query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="context">The context.</param>
    /// <param name="writer"><see cref="XmlWriter"/> into which the new query is written</param>
    public void Substitute(string query, IServerContext context, XmlWriter writer)
    {
      switch (InitializeSubstitute(query, context))
      {
        case QueryType.Aml:
          SubstituteAml(query, context, writer);
          break;
        case QueryType.Sql:
          throw new NotSupportedException("Cannot write a SQL command to an XmlWriter");
      }
    }
    /// <summary>
    /// Substitutes the stored parameters into the specified query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="context">The context.</param>
    /// <param name="builder"><see cref="StringBuilder"/> into which the new query is written</param>
    public void Substitute(string query, IServerContext context, StringBuilder builder)
    {
      using (var writer = new StringWriter(builder))
      {
        Substitute(query, context, writer);
      }
    }
    /// <summary>
    /// Substitutes the stored parameters into the specified query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="context">The context.</param>
    /// <param name="writer"><see cref="TextWriter"/> into which the new query is written</param>
    public void Substitute(string query, IServerContext context, TextWriter writer)
    {
      switch (InitializeSubstitute(query, context))
      {
        case QueryType.Aml:
          using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings() { OmitXmlDeclaration = true }))
          {
            SubstituteAml(query, context, xmlWriter);
          }
          break;
        case QueryType.Sql:
          SqlReplace(query, writer);
          break;
      }
    }

    private enum QueryType
    {
      Empty,
      Aml,
      Sql
    }

    private QueryType InitializeSubstitute(string query, IServerContext context)
    {
      if (string.IsNullOrEmpty(query)) return QueryType.Empty;
      var i = 0;
      while (i < query.Length && char.IsWhiteSpace(query[i])) i++;
      if (i >= query.Length) return QueryType.Empty;


      if (_context != context)
      {
        _context = context;
        _sqlFormatter = new SqlFormatter(context);
      }

      return query[i] == '<' && this.Mode == ParameterSubstitutionMode.Aml
        ? QueryType.Aml
        : QueryType.Sql;
    }

    private void SubstituteAml(string query, IServerContext context, XmlWriter xmlWriter)
    {
      using (var reader = new StringReader(query))
      using (var xmlReader = XmlReader.Create(reader))
      {
        Parameter condition = null;
        DateOffset? offsetStart = null;
        DateOffset? offsetEnd = null;
        var tagNames = new List<string>();
        string tagName;
        Parameter param;
        var attrs = new List<Parameter>();

        while (xmlReader.Read())
        {
          switch (xmlReader.NodeType)
          {
            case XmlNodeType.CDATA:
              param = RenderValue((string)condition, xmlReader.Value);
              if (param.IsRaw)
                throw new InvalidOperationException("Can't have a raw parameter in a CDATA section");
              xmlWriter.WriteCData(param.Value);
              break;
            case XmlNodeType.Comment:
              xmlWriter.WriteComment(xmlReader.Value);
              break;
            case XmlNodeType.Element:
              offsetStart = null;
              offsetEnd = null;

              xmlWriter.WriteStartElement(xmlReader.Prefix, xmlReader.LocalName
                , xmlReader.NamespaceURI);
              tagName = xmlReader.LocalName;

              var isEmpty = xmlReader.IsEmptyElement;

              attrs.Clear();
              for (var i = 0; i < xmlReader.AttributeCount; i++)
              {
                xmlReader.MoveToAttribute(i);
                param = RenderValue(xmlReader.LocalName, xmlReader.Value);
                param.ContextName = xmlReader.LocalName;
                param.Prefix = xmlReader.Prefix;
                param.NsUri = xmlReader.NamespaceURI;
                if (param.IsRaw)
                  throw new InvalidOperationException("Can't have a raw parameter in an attribute");
                attrs.Add(param);
                if (xmlReader.LocalName == "condition")
                {
                  condition = param;
                }
                else if (xmlReader.LocalName == "origDateRange" && !TryDeserializeDateRange(xmlReader.Value, out offsetStart, out offsetEnd))
                {
                  offsetStart = null;
                  offsetEnd = null;
                }
              }

              // Deal with date ranges
              if (offsetStart != null || offsetEnd != null)
              {
                if (condition == null)
                {
                  condition = new Parameter() { ContextName = "condition" };
                  attrs.Add(condition);
                }

                if (offsetStart != null && offsetEnd != null)
                  condition.Value = "between";
                else if (offsetStart != null)
                  condition.Value = "ge";
                else
                  condition.Value = "le";
              }

              foreach (var attr in attrs)
              {
                xmlWriter.WriteAttributeString(attr.Prefix, attr.ContextName, attr.NsUri, attr.Value);
              }

              switch (tagName)
              {
                case "Item":
                  if (!tagNames.Any(n => n == "Item")) _itemCount++;
                  break;
                case "sql":
                case "SQL":
                  condition = "sql";
                  break;
              }

              if (isEmpty)
              {
                xmlWriter.WriteEndElement();
              }
              else
              {
                tagNames.Add(tagName);
              }
              break;
            case XmlNodeType.EndElement:
              xmlWriter.WriteEndElement();
              tagNames.RemoveAt(tagNames.Count - 1);
              condition = null;
              break;
            case XmlNodeType.SignificantWhitespace:
              xmlWriter.WriteWhitespace(xmlReader.Value);
              break;
            case XmlNodeType.Text:
              var value = xmlReader.Value;
              if (offsetStart != null && offsetEnd != null)
                value = _context.Format(new Range<DateOffset>(offsetStart.Value, offsetEnd.Value));
              else if (offsetStart != null)
                value = _context.Format(offsetStart.Value.AsDate(_context.Now()));
              else if (offsetEnd != null)
                value = _context.Format(offsetEnd.Value.AsDate(_context.Now(), true));
              param = RenderValue((string)condition, value);

              var range = param.Original as IRange;
              if (range != null)
              {
                if (param.Original is Range<DateOffset>)
                {
                  if (condition == null)
                    xmlWriter.WriteAttributeString("condition", "between");
                  if (!attrs.Any(p => p.Name == "origDateRange"))
                    xmlWriter.WriteAttributeString("origDateRange", SerializeDateRange((Range<DateOffset>)param.Original));
                }
                else
                {
                  if (condition == null)
                    xmlWriter.WriteAttributeString("condition", "between");
                }
              }
              else if (param.Original is DateOffset && condition != null)
              {
                if (condition.Value == "le" || condition.Value == "lt")
                  xmlWriter.WriteAttributeString("origDateRange", SerializeDateRange(null, (DateOffset)param.Original));
                else
                  xmlWriter.WriteAttributeString("origDateRange", SerializeDateRange((DateOffset)param.Original, null));
              }

              if (param.Value != null)
              {
                if (param.IsRaw)
                {
                  xmlWriter.WriteRaw(param.Value);
                }
                else
                {
                  xmlWriter.WriteValue(param.Value);
                }
              }
              break;
          }

        }
      }
    }

    internal static string SerializeDateRange(DateOffset? start, DateOffset? end)
    {
      var parts = new string[5];
      if (start.HasValue)
      {
        parts[0] = start.Value.Magnitude.ToString();
        parts[1] = start.Value.Offset.ToString();
      }
      else
      {
        parts[0] = DateMagnitude.Year.ToString();
      }

      if (end.HasValue)
      {
        parts[2] = end.Value.Magnitude.ToString();
        parts[3] = end.Value.Offset.ToString();
      }
      else
      {
        parts[2] = DateMagnitude.Year.ToString();
      }

      parts[4] = (start ?? end).Value.FirstDayOfWeek.ToString();

      return string.Join("|", parts);
    }
    internal static string SerializeDateRange(Range<DateOffset> range)
    {
      return SerializeDateRange(range.Minimum, range.Maximum);
    }

    internal static bool TryDeserializeDateRange(string value, out DateOffset? start, out DateOffset? end)
    {
      start = null;
      end = null;

      var parts = value.Split('|');
      if (parts[0] == "Static")
        return false;
      if (parts[0] == "Dynamic")
        parts = parts.Skip(1).Concat(new string[] { DayOfWeek.Sunday.ToString() }).ToArray();
      if (parts.Length != 5) return false;


      try
      {
        var firstDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), parts[4]);
        if (!string.IsNullOrEmpty(parts[1]))
        {
          start = new DateOffset(short.Parse(parts[1]), (DateMagnitude)Enum.Parse(typeof(DateMagnitude), parts[0]))
          {
            FirstDayOfWeek = firstDay
          };
          if (start.Value.Magnitude == DateMagnitude.Year && Math.Abs(start.Value.Offset) > 900)
            start = null;
        }

        if (!string.IsNullOrEmpty(parts[3]))
        {
          end = new DateOffset(short.Parse(parts[3]), (DateMagnitude)Enum.Parse(typeof(DateMagnitude), parts[2]))
          {
            FirstDayOfWeek = firstDay
          };
          if (end.Value.Magnitude == DateMagnitude.Year && Math.Abs(end.Value.Offset) > 900)
            end = null;
        }
        return true;
      }
      catch (ArgumentException) { return false; }
      catch (OverflowException) { return false; }
    }

    internal string RenderParameter(string name, IServerContext context)
    {
      _context = context;
      return RenderValue("", name).Value;
    }
    private Parameter RenderValue(string context, string content)
    {
      object value;
      var param = new Parameter();
      if (content.IsNullOrWhiteSpace())
      {
        return param.WithValue(content);
      }
      else if (TryFillParameter(content, param) && TryGetParamValue(param.Name, out value))
      {
        param.Original = value;
        if (param.IsRaw) return param.WithValue((param.Original ?? "").ToString());
        if (context == "le" && value is DateOffset)
          value = ((DateOffset)value).AsDate(_context.Now(), true);
        else if (context == "lt" && value is DateOffset)
          value = ((DateOffset)value).AsDate(_context.Now(), true).AddMilliseconds(1);

        switch (context)
        {
          case "idlist":
            return param.WithValue(RenderSqlEnum(value, false, o => _context.Format(o)));
          case "in":
          case "not in":
            return param.WithValue(RenderSqlEnum(value, true, o => _context.Format(o)));
          case "like":
          case "not like":
            // Do something useful with context
            return param.WithValue(RenderSqlEnum(value, false, o => _context.Format(o)));
          default:
            return param.WithValue(RenderSqlEnum(value, false, o => _context.Format(o)));
        }
      }
      else if (context == "between" || context == "not between")
      {
        // Do something useful here
        return param.WithValue(content);
      }
      else if (context == "sql"
        || context == "where")
      {
        return param.WithValue(SqlReplace(content));
      }
      else if ((context == "in" || context == "not in")
            && content.TrimStart()[0] == '(')
      {
        content = content.Trim();
        if (content.Length > 2 && content[1] == '@')
        {
          // Dapper is trying to be too helpful with parameter expansion
          return param.WithValue(SqlReplace(content.TrimStart('(').TrimEnd(')')));
        }
        else
        {
          return param.WithValue(SqlReplace(content));
        }
      }
      else
      {
        return param.WithValue(content);
      }
    }

    private bool TryGetNumericEnumerable(object value, out IEnumerable enumerable)
    {
      if (value is IEnumerable<short>
        || value is IEnumerable<int>
        || value is IEnumerable<long>
        || value is IEnumerable<ushort>
        || value is IEnumerable<uint>
        || value is IEnumerable<ulong>
        || value is IEnumerable<byte>
        || value is IEnumerable<decimal>)
      {
        enumerable = (IEnumerable)value;
        return true;
      }
      else if (value is IEnumerable<float>)
      {
        enumerable = ((IEnumerable<float>)value).Cast<decimal>();
        return true;
      }
      else if (value is IEnumerable<double>)
      {
        enumerable = ((IEnumerable<double>)value).Cast<decimal>();
        return true;
      }
      enumerable = null;
      return false;
    }

    private bool TryGetParamValue(string name, out object value)
    {
      if (ParameterAccessListener != null)
        ParameterAccessListener.Invoke(name);

      return _parameters.TryGetValue(name, out value);
    }

    private bool TryFillParameter(string value, Parameter param)
    {
      if (value == null || value.Length < 2) return false;

      var start = 0;
      var closingBracket = false;
      switch (value[0])
      {
        case '@':
          start = 1;
          if (value[start] == '{')
          {
            start++;
            closingBracket = true;
          }
          break;
        case '$':
          if (value[1] != '{')
            return false;
          start = 2;
          closingBracket = true;
          break;
        case '{':
          start = 1;
          if (value[start] == '@')
            start++;
          closingBracket = true;
          break;
        default:
          return false;
      }

      var end = value.Length;
      if (value[value.Length - 1] == '!')
      {
        param.IsRaw = true;
        end--;
      }

      if (closingBracket)
      {
        if (value[end - 1] != '}')
          return false;
        end--;
      }

      for (var i = start; i < end; i++)
      {
        if (!char.IsLetterOrDigit(value[i]) && value[i] != '_') return false;
      }
      param.Name = value.Substring(start, end - start);
      return true;
    }

    private string RenderSqlEnum(object value, bool quoteStrings, Func<object, string> format)
    {
      if (value is string)
        return format.Invoke(value);

      IEnumerable enumerable = value as IEnumerable;
      bool first = true;
      var builder = new StringBuilder();
      if ((!quoteStrings && enumerable != null) || TryGetNumericEnumerable(value, out enumerable))
      {
        foreach (var item in enumerable)
        {
          if (!first) builder.Append(",");
          builder.Append(format.Invoke(item));
          first = false;
        }

        if (first)
          return format.Invoke(EmptyListMatch);
      }
      else
      {
        enumerable = value as IEnumerable;
        if (enumerable != null)
        {
          foreach (var item in enumerable)
          {
            if (!first) builder.Append(",");
            builder.Append(SqlFormatter.Quote(format.Invoke(item)));
            first = false;
          }

          // Nothing was written as there were not values in the IEnumerable
          // Therefore, write a bogus value to match zero results
          if (quoteStrings && first)
          {
            return "N'" + format.Invoke(EmptyListMatch) + "'";
          }
        }
        else
        {
          return format.Invoke(value);
        }
      }
      return builder.ToString();
    }

    private string SqlReplace(string query)
    {
      using (var writer = new StringWriter())
      {
        SqlReplace(query, writer);
        writer.Flush();
        return writer.ToString();
      }
    }

    private void SqlReplace(string query, TextWriter builder)
    {
      SqlReplace(query, '@', builder, p =>
      {
        object value;

        if (TryGetParamValue(p, out value))
        {
          Func<string, string> finalAction = s => s;
          var inClause = false;

          if (builder.ToString().EndsWith(" in "))
          {
            finalAction = s => "(" + s + ")";
            inClause = true;
          }
          else if (builder.ToString().EndsWith(" in ("))
          {
            inClause = true;
          }


          IFormattable num;
          if (value == null
#if DBDATA
            || value == DBNull.Value
#endif
          )
          {
            return finalAction("null");
          }
          else if (ServerContext.TryCastNumber(value, out num))
          {
            return finalAction(_sqlFormatter.Format(num));
          }
          else if (value is string)
          {
            return finalAction(SqlFormatter.Quote(RenderSqlEnum(value, false, o => _sqlFormatter.Format(o))));
          }
          else if (inClause && value is IEnumerable)
          {
            return finalAction(RenderSqlEnum(value, true, o => _sqlFormatter.Format(o)));
          }
          else if (value is DateTime || value is bool || value is Guid)
          {
            return finalAction("'" + RenderSqlEnum(value, false, o => _sqlFormatter.Format(o)) + "'");
          }
          else
          {
            return finalAction(SqlFormatter.Quote(RenderSqlEnum(value, false, o => _sqlFormatter.Format(o))));
          }
        }
        else
        {
          return "@" + p;
        }
      });
    }

    private void SqlReplace(string sql, char paramPrefix, TextWriter builder, Func<string, string> replace)
    {
      char endChar = '\0';
      int i = 0;
      var paramName = new StringBuilder(32);
      int lastWrite = 0;

      while (i < sql.Length)
      {
        if (endChar == '\0')
        {
          switch (sql[i])
          {
            case '\'':
              endChar = '\'';
              break;
            case '"':
              endChar = '"';
              break;
            case '[':
              endChar = ']';
              break;
            case '-':
              if (i + 1 < sql.Length && sql[i + 1] == '-')
              {
                endChar = '\n';
              }
              break;
            case '/':
              if (i + 1 < sql.Length && sql[i + 1] == '*')
              {
                endChar = '/';
              }
              break;
          }

          if (sql[i] == paramPrefix)
          {
            builder.Append(sql.Substring(lastWrite, i - lastWrite));
            i++;
            paramName.Length = 0;
            while (i < sql.Length && (Char.IsLetterOrDigit(sql[i]) || sql[i] == '_'))
            {
              paramName.Append(sql[i]);
              i++;
            }
            builder.Append(replace.Invoke(paramName.ToString()));
            lastWrite = i;
            i--;
          }
        }
        else if ((endChar == '\n' && sql[i] == '\r')
              || (endChar == '/' && sql[i] == '*' && i + 1 < sql.Length && sql[i + 1] == '/')
              || (sql[i] == endChar))
        {
          endChar = '\0';
        }
        i++;
      }

      if ((i - lastWrite) > 0) builder.Append(sql.Substring(lastWrite, i - lastWrite));
    }

    /// <summary>
    /// Returns an enumerator that iterates through the list of parameters
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
      return _parameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    object IFormatProvider.GetFormat(Type formatType)
    {
      if (formatType == typeof(ICustomFormatter))
        return this;
      else
        return null;
    }

    string ICustomFormatter.Format(string format, object arg, IFormatProvider formatProvider)
    {
      var paramName = _parameters.Count.ToString();
      _parameters[paramName] = arg;
      return "@" + paramName;
    }

    private class Parameter
    {
      public string ContextName { get; set; }
      public bool IsRaw { get; set; }
      public string Name { get; set; }
      public string NsUri { get; set; }
      public object Original { get; set; }
      public string Prefix { get; set; }
      public string Value { get; set; }

      public Parameter WithValue(string value)
      {
        this.Value = value;
        return this;
      }

      public static implicit operator Parameter(string value)
      {
        return new Parameter() { Value = value };
      }
      public static explicit operator string(Parameter value)
      {
        if (value == null)
          return null;
        return value.Value;
      }
    }
  }
}
