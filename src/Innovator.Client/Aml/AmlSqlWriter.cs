using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client
{
  public class AmlSqlWriter : XmlWriter
  {
    private List<Tag> _tags = new List<Tag>();
    //private TextWriter _writer = new StringWriter();
    private string _name;
    private StringBuilder _buffer = new StringBuilder();
    private ItemTag _lastItem;
    private HashSet<string> _aliases = new HashSet<string>();
    private ItemTag _criteriaItem;
    private IAmlSqlWriterSettings _settings;

    public override WriteState WriteState { get { return WriteState.Start; } }

    public AmlSqlWriter(IAmlSqlWriterSettings settings)
    {
      _settings = settings;
    }

#if XMLLEGACY
    public override void Close()
    {
      Flush();
    }
#endif

    protected override void Dispose(bool disposing)
    {
      if (disposing)
        Flush();
    }

    public override void Flush()
    {
      // Reclaim memory
      _buffer.Length = 0;
      _aliases.Clear();
      _criteriaItem = null;
      _tags.Clear();
    }

    public override string LookupPrefix(string ns)
    {
      return PrefixFromNamespace(ns);
    }

    internal static string PrefixFromNamespace(string ns)
    {
      switch ((ns ?? "").TrimEnd('/'))
      {
        case "http://schemas.xmlsoap.org/soap/envelope":
          return "SOAP-ENV";
        case "http://www.w3.org/XML/1998/namespace":
          return "xml";
        case "http://www.aras.com/InnovatorFault":
          return "af";
        case "http://www.aras.com/I18N":  //return "i18n";
        case "":
          return "";
      }
      throw new ArgumentException();
    }

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      throw new NotSupportedException();
    }

    public override void WriteCData(string text)
    {
      WriteString(text);
    }

    public override void WriteCharEntity(char ch)
    {
      WriteString(new string(ch, 1));
    }

    public override void WriteChars(char[] buffer, int index, int count)
    {
      WriteString(new string(buffer, index, count));
    }

    public override void WriteComment(string text)
    {
      // Do nothing
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
      // Do nothing
    }

    public override void WriteEndAttribute()
    {
      _tags.Last().Attributes[_name] = _buffer.ToString();
      _buffer.Length = 0;
    }

    public override void WriteEndDocument()
    {
      // Do nothing
    }

    public override void WriteEndElement()
    {
      FlushAttributes();

      var curr = _tags.Last();
      var clause = _lastItem.Where;
      string buffer;

      switch (curr.Name)
      {
        case "and":
        case "or":
        case "not":
          clause.Append(")");
          break;
        case "Item":
          // Id criteria supercede all other criteria
          if (curr.Attributes.TryGetValue("id", out buffer))
          {
            clause.Length = 0;
            clause.Append(_lastItem.Alias)
              .Append(".id = '")
              .Append(buffer.Replace("'", "''"))
              .Append("'");
          }
          else if (curr.Attributes.TryGetValue("idlist", out buffer))
          {
            clause.Length = 0;
            clause.Append(_lastItem.Alias)
              .Append(".id in (")
              .Append(buffer.Split(',')
                .Select(i => "'" + i.Trim().Trim('\'').Replace("'", "''") + "'")
                .GroupConcat(", ")
              ).Append(")");
          }

          if (_settings.PermissionOption == AmlSqlPermissionOption.LegacyFunction)
          {
            if (clause.Length > 0)
              clause.Append(" and ");
            clause.Append("( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', ")
              .Append(_lastItem.Alias).Append(".permission_id, ")
              .Append(_lastItem.Alias).Append(".created_by_id, ")
              .Append(_lastItem.Alias).Append(".managed_by_id, ")
              .Append(_lastItem.Alias).Append(".owned_by_id, ")
              .Append(_lastItem.Alias).Append(".team_id, '")
              .Append(_settings.IdentityList)
              .Append("', null, '")
              .Append(_settings.UserId)
              .Append("', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0");
          }
          break;
        default:
          string condition;
          if (!curr.Attributes.TryGetValue("condition", out condition))
            condition = "eq";
          if (!curr.Attributes.TryGetValue("is_null", out buffer) && buffer == "1")
            condition = "is null";

          if (clause.Length > 0)
          {
            if (_tags[_tags.Count - 2].Name == "or")
              clause.Append(" or ");
            else
              clause.Append(" and ");
          }

          if (_criteriaItem != null)
          {
            _lastItem.From
                .Append(" left join innovator.")
                .Append(_criteriaItem.TableName);
            if (_criteriaItem.Alias != _criteriaItem.TableName)
              _lastItem.From
                .Append(" as ")
                .Append(_criteriaItem.Alias);
            _lastItem.From
              .Append(" on ")
              .Append(_criteriaItem.Alias)
              .Append(".id = ")
              .Append(_lastItem.Alias)
              .Append(".")
              .Append(AssertPropertyName(curr.Name));
            _lastItem.Where
              .Append('(')
              .Append(_criteriaItem.Where.ToString())
              .Append(')');
          }
          else
          {
            clause.Append(_lastItem.Alias);
            clause.Append('.');
            clause.Append(curr.Name);

            var type = _lastItem.Attributes["type"];

            var str = _buffer.ToString();
            switch (condition)
            {
              case "between":
              case "not between":
                var i = str.IndexOf(" and ", StringComparison.OrdinalIgnoreCase);
                if (i <= 0)
                  throw new InvalidOperationException("A `between` condition requires an `and` between the parameters.  None was found with `" + str + "`.");
                clause.Append(' ').Append(condition).Append(' ');
                RenderCriteria(type, curr.Name, str.Substring(0, i));
                clause.Append(" and ");
                RenderCriteria(type, curr.Name, str.Substring(i + 5));
                break;
              case "ge":
                clause.Append(" >= ");
                RenderCriteria(type, curr.Name, str);
                break;
              case "gt":
                clause.Append(" > ");
                RenderCriteria(type, curr.Name, str);
                break;
              case "in":
              case "not in":
                str = str.Trim();
                if (string.IsNullOrEmpty(str))
                  throw new InvalidOperationException("An `in` condition requires a non-empty criteria");

                if ((str[0] >= '0' && str[0] <= '0') || str[0] == '\'')
                {
                  clause.Append(' ').Append(condition).Append(" (");
                  var parts = str.Split(',')
                    .Select(s => s.Trim().TrimStart('N').Replace("''", "'").Trim('\''));
                  var first = true;
                  foreach (var part in parts)
                  {
                    if (!first)
                      clause.Append(", ");
                    RenderCriteria(type, curr.Name, part);
                  }
                  clause.Append(")");
                }
                else
                {
                  throw new InvalidOperationException("An invalid `in` condition was found. `" + str + "`");
                }

                break;
              case "is not null":
              case "is null":
                clause.Append(condition);
                break;
              case "is":
                str = str.Trim();
                if (string.Equals(str, "null", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(str, "not null", StringComparison.OrdinalIgnoreCase))
                  clause.Append(" is ").Append(str);
                else
                  throw new InvalidOperationException("An `is` condition requires the value `null` or `not null`. `" + str + "` is invalid.");
                break;
              case "le":
                clause.Append(" <= ");
                RenderCriteria(type, curr.Name, str);
                break;
              case "like":
              case "not like":
                clause.Append(' ').Append(condition).Append(" N'").Append(str.Replace("'", "''").Replace('*', '%')).Append('\'');
                break;
              case "lt":
                clause.Append(" < ");
                RenderCriteria(type, curr.Name, str);
                break;
              case "ne":
                clause.Append(" <> ");
                RenderCriteria(type, curr.Name, str);
                break;
              default: // eq
                clause.Append(" = ");
                RenderCriteria(type, curr.Name, str);
                break;
            }
          }
          break;
      }

      _buffer.Length = 0;
      _tags.RemoveAt(_tags.Count - 1);
      _lastItem = _tags.OfType<ItemTag>().LastOrDefault() ?? _lastItem;
      _criteriaItem = curr as ItemTag;
    }

    private void RenderCriteria(string itemType, string name, string str)
    {
      var props = _settings.GetProperties(itemType) ?? new Dictionary<string, Model.Property>();
      Model.Property prop;
      if (!props.TryGetValue(name, out prop))
        prop = Item.GetNullItem<Model.Property>();

      switch (prop.DataType().AsString("unknown"))
      {
        case "date":
          RenderDate(str);
          break;
        case "float":
        case "decimal":
        case "integer":
          RenderNumeric(str);
          break;
        case "unknown":
          double val;
          DateTime dateVal;
          if (double.TryParse(str, out val))
            RenderNumeric(str);
          else if (DateTime.TryParse(str, out dateVal))
            RenderDate(str);
          else
            RenderText(str);
          break;
        default:
          RenderText(str);
          break;
      }
    }

    private void RenderDate(string str)
    {
      var dateVal = _settings.AmlContext.LocalizationContext.AsDateTimeUtc(str).Value;
      _lastItem.Where.Append('\'').Append(dateVal.ToString("s")).Append('\'');
    }

    private void RenderNumeric(string str)
    {
      _lastItem.Where.Append(str);
    }

    private void RenderText(string str)
    {
      _lastItem.Where.Append('\'').Append(str.Replace("'", "''")).Append('\'');
    }

    public override void WriteEntityRef(string name)
    {
      if (name == "amp")
      {
        WriteString("&");
      }
      else if (name == "apos")
      {
        WriteString("'");
      }
      else if (name == "gt")
      {
        WriteString(">");
      }
      else if (name == "lt")
      {
        WriteString("<");
      }
      else if (name == "quot")
      {
        WriteString("\"");
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    public override void WriteFullEndElement()
    {
      WriteEndElement();
    }

    public override void WriteProcessingInstruction(string name, string text)
    {
      throw new NotSupportedException();
    }

    public override void WriteRaw(string data)
    {
      throw new NotSupportedException();
    }

    public override void WriteRaw(char[] buffer, int index, int count)
    {
      throw new NotSupportedException();
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      _buffer.Length = 0;
      _name = localName;
    }

    public override void WriteStartDocument()
    {
      // Do nothing
    }

    public override void WriteStartDocument(bool standalone)
    {
      // Do nothing
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      if (localName == "Relationships")
        throw new NotSupportedException("Relationships are not supported at this time");

      FlushAttributes();
      if (localName == "Item")
      {
        if (_tags.OfType<ItemTag>().Count() >= 2)
          throw new NotSupportedException("Doubly-nested `Item` tags are not supported at this time");

        _lastItem = new ItemTag() { Name = localName };
        _tags.Add(_lastItem);
      }
      else
      {
        _tags.Add(new Tag() { Name = localName });
      }
      _buffer.Length = 0;
    }

    public override void WriteString(string text)
    {
      _buffer.Append(text);
    }

    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
      WriteString(new string(new char[] { highChar, lowChar }));
    }

    public override void WriteWhitespace(string ws)
    {
      WriteString(ws);
    }

    private void FlushAttributes()
    {
      if (_tags.Count < 1)
        return;

      var curr = _tags.Last();
      if (curr.AttributesProcessed)
        return;
      switch (curr.Name)
      {
        case "Item":
          string type;
          if (!curr.Attributes.TryGetValue("type", out type))
            throw new NotSupportedException("Items must have a `type` attribute specified");
          type = "[" + AssertPropertyName(type.Replace(' ', '_')) + "]";
          var alias = type;
          var i = 2;
          while (!_aliases.Add(alias))
          {
            alias = "[" + type.Replace(' ', '_') + (i++) + "]";
          }
          _lastItem.Alias = alias;

          var select = _lastItem.Select;
          select.Append("select ");
          string buffer;
          if (curr.Attributes.TryGetValue("maxRecords", out buffer))
          {
            select.Append("top ");
            select.Append(AssertInt(buffer));
            select.Append(" ");
          }

          if (curr.Attributes.TryGetValue("select", out buffer))
          {
            var cols = SubSelect.FromString(buffer);
            var first = true;
            foreach (var col in cols)
            {
              if (!first)
                select.Append(", ");
              first = false;
              select.Append(type);
              select.Append(".");
              select.Append(AssertPropertyName(col.Name));
            }
          }
          else
          {
            select.Append(type);
            select.Append(".");
            select.Append("*");
          }
          _lastItem.TableName = type;
          _lastItem.From.Append("from ")
            .Append(_settings.PermissionOption == AmlSqlPermissionOption.SecuredFunction ? "secured" : "innovator")
            .Append(".").Append(type);

          if (_settings.PermissionOption == AmlSqlPermissionOption.SecuredFunction)
          {
            _lastItem.From.Append("('can_get','")
              .Append(_settings.IdentityList)
              .Append("',null,'")
              .Append(_settings.UserId)
              .Append("',null)");
          }

          if (alias != type)
            _lastItem.From.Append(" as ").Append(alias);

          if (curr.Attributes.TryGetValue("where", out buffer))
            throw new NotSupportedException("`where` attributes are currently not supported");

          break;
        case "and":
        case "or":
          _lastItem.Where.Append("(");
          break;
        case "not":
          _lastItem.Where.Append(" not (");
          break;
      }
      curr.AttributesProcessed = true;
    }

    public override string ToString()
    {
      return ToString(AmlSqlRenderOption.SelectQuery);
    }
    public string ToString(AmlSqlRenderOption renderOption)
    {
      var builder = new StringBuilder();
      switch (renderOption)
      {
        case AmlSqlRenderOption.SelectQuery:
          builder.Append(_lastItem.Select.ToString())
            .Append(' ')
            .Append(_lastItem.From.ToString())
            .Append(" where ");
          break;
        case AmlSqlRenderOption.CountQuery:
          builder.Append("select count(")
            .Append(_lastItem.Alias)
            .Append(".id) ")
            .Append(_lastItem.From.ToString())
            .Append(" where ");
          break;
      }
      builder.Append(_lastItem.Where.ToString());

      string queryType;
      if (!_lastItem.Attributes.TryGetValue("queryType", out queryType))
        queryType = "current";

      if (string.Equals(queryType, "current", StringComparison.OrdinalIgnoreCase))
      {
        if (_lastItem.Where.Length > 0)
          builder.Append(" and");
        builder.Append(' ')
          .Append(_lastItem.TableName)
          .Append(".is_current = '1'");
      }
      else if (string.Equals(queryType, "latest", StringComparison.OrdinalIgnoreCase)
        && builder.ToString().IndexOf(_lastItem.Alias + ".is_active_rev", StringComparison.OrdinalIgnoreCase) >= 0)
      {
        // all good here
      }
      else
      {
        throw new NotSupportedException("The `queryType` of `" + queryType + "` is not supported");
      }

      if (renderOption == AmlSqlRenderOption.SelectQuery)
      {
        builder.Append(" order by ");

        string buffer;
        if (_lastItem.Attributes.TryGetValue("orderBy", out buffer) && !string.IsNullOrEmpty(buffer))
        {

          var columns = buffer.Split(',')
            .Select(c => c.Trim());
          var first = true;
          foreach (var col in columns)
          {
            if (!first)
              builder.Append(", ");
            first = false;

            var parts = col.Split(' ');
            if (parts.Length >= 2)
              throw new InvalidOperationException("Invalid `orderBy` column: `" + col + "`");
            builder.Append(_lastItem.Alias).Append('.');
            builder.Append(AssertPropertyName(parts[0]));

            if (parts.Length == 2)
            {
              switch (parts[1].ToLowerInvariant())
              {
                case "asc":
                case "desc":
                  builder.Append(parts[1]);
                  break;
                default:
                  throw new InvalidOperationException("Invalid `orderBy` column: `" + col + "`");
              }
            }
          }
        }
        else
        {
          var props = _settings.GetProperties(_lastItem.Attributes["type"]).Values;
          var orderProps = props
            .Where(p => p.OrderBy().HasValue())
            .OrderBy(p => p.OrderBy().AsInt(int.MaxValue));
          var first = true;
          foreach (var prop in orderProps)
          {
            if (!first)
              builder.Append(", ");
            first = false;
            builder.Append(_lastItem.Alias).Append('.');
            builder.Append(prop.NameProp().Value);
          }

          if (!first)
            builder.Append(", ");
          builder.Append(_lastItem.Alias).Append('.');
          builder.Append("id");
        }

        if (!_lastItem.Attributes.TryGetValue("maxRecords", out buffer)
          && _lastItem.Attributes.TryGetValue("pagesize", out buffer))
        {
          var pageSize = int.Parse(buffer);
          var page = int.Parse(_lastItem.Attributes["page"]);
          builder.Append(" offset ")
            .Append(pageSize * (page - 1))
            .Append(" rows fetch next ")
            .Append(pageSize)
            .Append(" rows only");
        }
      }

      return builder.ToString();
    }

    private class Tag
    {
      private Dictionary<string, string> _attrs = new Dictionary<string, string>();

      public Dictionary<string, string> Attributes { get { return _attrs; } }
      public string Name { get; set; }
      public bool AttributesProcessed { get; set; }
    }

    private class ItemTag : Tag
    {
      private StringBuilder _select = new StringBuilder();
      private StringBuilder _from = new StringBuilder();
      private StringBuilder _where = new StringBuilder();

      public string Alias { get; set; }
      public string TableName { get; set; }
      public StringBuilder Select { get { return _select; } }
      public StringBuilder From { get { return _from; } }
      public StringBuilder Where { get { return _where; } }
    }

    private string AssertInt(string value)
    {
      var x = int.Parse(value); // This will throw an exception if it is not an int
      return value;
    }

    private string AssertPropertyName(string name)
    {
      for (var i = 0; i < name.Length; i++)
      {
        if (!((name[i] >= 'a' && name[i] <= 'z')
          || (name[i] >= 'A' && name[i] <= 'Z')
          || (name[i] >= '0' && name[i] <= '0')
          || name[i] == '_'))
          throw new InvalidOperationException("`" + name + "` is not a valid property name");
      }
      return name;
    }
  }
}
