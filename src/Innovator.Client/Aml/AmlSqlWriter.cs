using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// Generates a SQL statement for querying the Aras database directly from an AML query
  /// </summary>
  /// <example>
  /// <code lang="C#">
  /// var item = ElementFactory.Local.FromXml(@"&lt;Item type='Part' action='get' queryType='Latest' queryDate='2017-05-11T17:37:00'&gt;
  ///   &lt;is_active_rev&gt;1&lt;/is_active_rev&gt;
  ///   &lt;keyed_name condition='like'&gt;999-*&lt;/keyed_name&gt;
  ///   &lt;owned_by_id&gt;&lt;Item type='Identity' action='get'&gt;&lt;keyed_name condition='like'&gt;*super*&lt;/keyed_name&gt;&lt;/Item&gt;&lt;/owned_by_id&gt;
  /// &lt;/Item&gt;").AssertItem();
  ///   var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction });
  ///   item.ToAml(writer);
  /// writer.Flush();
  /// </code>
  /// <para>Afterwards, <see cref="AmlSqlWriter.ToString()"/> should return something like</para>
  /// <code lang="SQL">
  /// select [Part].* 
  /// from innovator.[Part] 
  /// left join innovator.[Identity] 
  /// on [Identity].id = [Part].owned_by_id 
  /// where [Part].is_active_rev = 1 
  /// and [Part].keyed_name like N'999-%' 
  /// and ([Identity].keyed_name like N'%super%' and ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', [Identity].permission_id, [Identity].created_by_id, [Identity].managed_by_id, [Identity].owned_by_id, [Identity].team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9,1942894C56164DF4AB400434FF5EFE3B,56A96DA9E981481688563E2D14D5D878,F18B4A8C523A4B3F88CFFEE3BC68F5D4,2618D6F5A90949BAA7E920D1B04C7EE1,43CC1A53BECA4364A22C136E98E185C7,5A8E8A27AA5A47238BD96AC60A6562F3,5E47C25D3FE6464688442ABE64478537,08C52237B1124FC4BB5D4E48F660C8E2,22DF5704DCA14D86B79BFDFF3CA030B9,2C5CF38CB45C4E1F848EA6DF969364EF,31387021B15E42548FC818E893A19D08,344632A16E8949CCA5CAD0A013D50655,3A0D61E9629141C8934B948C596B2E42,3A976285BB9A4D05AE1878004A33D440,484451EB7B524ECD92FFA372A4675E38,4CAAF215654F43098B415EC0EAB1829A,4CD258EF03834E7FAC9CD76876777EBC,5E12CD824411477AA56D9F539FA295EB,600B965EA3DB41D0B7F9D9D7BDD923EC,62E1A4258C764C5E814DDA3B4209A34F,66022731E81D4040A155F13CD083826A,66F68F2DC7D3410699EA302411608618,685069C4626B406BA5EEFBD87CCABBAC,7C63ED1CFD0A4E0DAAD27AAB0C90A58D,7F50E4A530A84C22935ED7E2231198E1,82F4AA3DD22648288D63938D648CD814,84D2CEC2D1E247DC9D2D28B78D1DF591,A11E92826CA44BEEA4A84E494D13F0B8,A4AFDB4EBD2A4130A4D0616E2E63F552,AD55DC9C04274E67A2C4C45AADC9D059,B3DC39013C13458F9D3A479046B25266,BFE82C27CAAB4667B51EEC64D818BEC8,C02E1CEE4C2B492496EAEC1C19F814B0,C3DB96EFB5EC47AE816CE16AE3E79959,C5C35BB6D73145DB8C88084BEA96BB1D,C6D220539A8D4458B27CA579B637967F,C8FE09099D8D4C25BED39BBFE3BF0476,D78B07BAF283495E9540115C00AD9CED,DE6E87399974415F948D4468C07C5134,E42675D6FC354C86880230D5DA79E4CD,FFB8C4C174054BD3AA17A00023AD49BC,25EAA8CA05254E1DB191F984D994751D,5D58B17DB5094950B93AE13202E46C5E,6A2F0C9740B74625B744E5FCE43879F6,7FA8E5852B7F4FE1817C24A11798A784,B32BD81D1AD04207BF1E61E39A4E0E13,FA8573065D534FBB8DA1E40C14FA54EF,0CDBE4C0F8A045159339D1C8BE8665FA,31E5C20B933D433CA8F509F7AED9E68A,5E7DC598214E4BBAA09CDB91D644FABD,78A5819F71E2414EB48C5984AB005ABB,988240C797D548A79A78B1AD4CDED7B3,3FCE6B2163FC4F5983BF00A13E6B047E', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) &gt; 0) 
  /// and ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', [Part].permission_id, [Part].created_by_id, [Part].managed_by_id, [Part].owned_by_id, [Part].team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9,1942894C56164DF4AB400434FF5EFE3B,56A96DA9E981481688563E2D14D5D878,F18B4A8C523A4B3F88CFFEE3BC68F5D4,2618D6F5A90949BAA7E920D1B04C7EE1,43CC1A53BECA4364A22C136E98E185C7,5A8E8A27AA5A47238BD96AC60A6562F3,5E47C25D3FE6464688442ABE64478537,08C52237B1124FC4BB5D4E48F660C8E2,22DF5704DCA14D86B79BFDFF3CA030B9,2C5CF38CB45C4E1F848EA6DF969364EF,31387021B15E42548FC818E893A19D08,344632A16E8949CCA5CAD0A013D50655,3A0D61E9629141C8934B948C596B2E42,3A976285BB9A4D05AE1878004A33D440,484451EB7B524ECD92FFA372A4675E38,4CAAF215654F43098B415EC0EAB1829A,4CD258EF03834E7FAC9CD76876777EBC,5E12CD824411477AA56D9F539FA295EB,600B965EA3DB41D0B7F9D9D7BDD923EC,62E1A4258C764C5E814DDA3B4209A34F,66022731E81D4040A155F13CD083826A,66F68F2DC7D3410699EA302411608618,685069C4626B406BA5EEFBD87CCABBAC,7C63ED1CFD0A4E0DAAD27AAB0C90A58D,7F50E4A530A84C22935ED7E2231198E1,82F4AA3DD22648288D63938D648CD814,84D2CEC2D1E247DC9D2D28B78D1DF591,A11E92826CA44BEEA4A84E494D13F0B8,A4AFDB4EBD2A4130A4D0616E2E63F552,AD55DC9C04274E67A2C4C45AADC9D059,B3DC39013C13458F9D3A479046B25266,BFE82C27CAAB4667B51EEC64D818BEC8,C02E1CEE4C2B492496EAEC1C19F814B0,C3DB96EFB5EC47AE816CE16AE3E79959,C5C35BB6D73145DB8C88084BEA96BB1D,C6D220539A8D4458B27CA579B637967F,C8FE09099D8D4C25BED39BBFE3BF0476,D78B07BAF283495E9540115C00AD9CED,DE6E87399974415F948D4468C07C5134,E42675D6FC354C86880230D5DA79E4CD,FFB8C4C174054BD3AA17A00023AD49BC,25EAA8CA05254E1DB191F984D994751D,5D58B17DB5094950B93AE13202E46C5E,6A2F0C9740B74625B744E5FCE43879F6,7FA8E5852B7F4FE1817C24A11798A784,B32BD81D1AD04207BF1E61E39A4E0E13,FA8573065D534FBB8DA1E40C14FA54EF,0CDBE4C0F8A045159339D1C8BE8665FA,31E5C20B933D433CA8F509F7AED9E68A,5E7DC598214E4BBAA09CDB91D644FABD,78A5819F71E2414EB48C5984AB005ABB,988240C797D548A79A78B1AD4CDED7B3,3FCE6B2163FC4F5983BF00A13E6B047E', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) &gt; 0 
  /// order by [Part].id
  /// </code>
  /// </example>
  public class AmlSqlWriter : XmlWriter
  {
    private readonly List<Tag> _tags = new List<Tag>();
    private string _name;
    private readonly StringBuilder _buffer = new StringBuilder();
    private ItemTag _lastItem;
    private readonly HashSet<string> _aliases = new HashSet<string>();
    private ItemTag _criteriaItem;
    private readonly IAmlSqlWriterSettings _settings;
    private readonly TextWriter _writer;

    /// <summary>Gets the state of the writer.</summary>
    /// <returns>One of the <see cref="WriteState" /> values.</returns>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override WriteState WriteState { get { return WriteState.Start; } }

    /// <summary>
    /// Creates an <see cref="XmlWriter"/> that will output a SQL statement from an AML query
    /// </summary>
    public AmlSqlWriter(IAmlSqlWriterSettings settings)
    {
      _settings = settings;
    }

    /// <summary>
    /// Creates an <see cref="XmlWriter"/> that will output a SQL statement from an AML query
    /// </summary>
    public AmlSqlWriter(TextWriter writer, IAmlSqlWriterSettings settings)
    {
      _writer = writer;
      _settings = settings;
    }

#if XMLLEGACY
    /// <summary>Closes this stream and the underlying stream.</summary>
    /// <exception cref="InvalidOperationException">A call is made to write more output after Close has been called or the result of this call is an invalid XML document.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void Close()
    {
      Flush();
    }
#endif

    /// <summary>Releases the unmanaged resources used by the <see cref="XmlWriter" /> and optionally releases the managed resources.</summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        Flush();
    }

    /// <summary>Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.</summary>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void Flush()
    {
      // Reclaim memory
      _buffer.Length = 0;
      _aliases.Clear();
      _criteriaItem = null;
      _tags.Clear();

      if (_writer != null)
        ToString(_writer, _settings.RenderOption);
    }

    /// <summary>Returns the closest prefix defined in the current namespace scope for the namespace URI.</summary>
    /// <returns>The matching prefix or null if no matching namespace URI is found in the current scope.</returns>
    /// <param name="ns">The namespace URI whose prefix you want to find.</param>
    /// <exception cref="ArgumentException">
    ///   <paramref name="ns" /> is either null or String.Empty.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
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

    /// <summary>
    /// Not supported
    /// </summary>
    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Writes out a &lt;![CDATA[...]]&gt; block containing the specified text.
    /// </summary>
    /// <param name="text">The text to place inside the CDATA block.</param>
    public override void WriteCData(string text)
    {
      WriteString(text);
    }

    /// <summary>Forces the generation of a character entity for the specified Unicode character value.</summary>
    /// <param name="ch">The Unicode character for which to generate a character entity.</param>
    /// <exception cref="ArgumentException">The character is in the surrogate pair character range, 0xd800 - 0xdfff.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteCharEntity(char ch)
    {
      WriteString(new string(ch, 1));
    }

    /// <summary>Writes text one buffer at a time.</summary>
    /// <param name="buffer">Character array containing the text to write.</param>
    /// <param name="index">The position in the buffer indicating the start of the text to write.</param>
    /// <param name="count">The number of characters to write.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="buffer" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or-The buffer length minus <paramref name="index" /> is less than <paramref name="count" />; the call results in surrogate pair characters being split or an invalid surrogate pair being written.</exception>
    /// <exception cref="ArgumentException">The <paramref name="buffer" /> parameter value is not valid.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteChars(char[] buffer, int index, int count)
    {
      WriteString(new string(buffer, index, count));
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void WriteComment(string text)
    {
      // Do nothing
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
      // Do nothing
    }

    /// <summary>Closes the previous <see cref="M:System.Xml.XmlWriter.WriteStartAttribute(System.String,System.String)" /> call.</summary>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteEndAttribute()
    {
      _tags.Last().Attributes[_name] = _buffer.ToString();
      _buffer.Length = 0;
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void WriteEndDocument()
    {
      // Do nothing
    }

    /// <summary>Closes one element and pops the corresponding namespace scope.</summary>
    /// <exception cref="InvalidOperationException">This results in an invalid XML document.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
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
            _lastItem.IgnoreVersions = true;
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
            _lastItem.IgnoreVersions = true;
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
            var table = _criteriaItem.From.First();
            table.OuterJoin = true;
            table.Criteria = table.Alias + ".id = " + _lastItem.Alias + "." + AssertPropertyName(curr.Name);
            _lastItem.From.Add(table);

            _lastItem.RelatedWhere.Add(new RelatedWhere()
            {
              Index = clause.Length,
              Alias = _criteriaItem.Alias,
              Criteria = _criteriaItem.Where.ToString()
            });
          }
          else
          {
            clause.Append(_lastItem.Alias);
            clause.Append('.');
            clause.Append(curr.Name);

            _lastItem.IgnoreVersions = _lastItem.IgnoreVersions || curr.Name == "id";

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

                if ((str[0] >= '0' && str[0] <= '9') || str[0] == '\'')
                {
                  clause.Append(' ').Append(condition).Append(" (");
                  var parts = str.Split(',')
                    .Select(s => s.Trim().TrimStart('N').Replace("''", "'").Trim('\''));
                  var first = true;
                  foreach (var part in parts)
                  {
                    if (!first)
                      clause.Append(", ");
                    first = false;
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

    /// <summary>Writes out an entity reference as &amp;name;.</summary>
    /// <param name="name">The name of the entity reference.</param>
    /// <exception cref="ArgumentException">
    ///   <paramref name="name" /> is either null or String.Empty.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
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

    /// <summary>Closes one element and pops the corresponding namespace scope.</summary>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteFullEndElement()
    {
      WriteEndElement();
    }

    /// <summary>
    /// Not supported
    /// </summary>
    public override void WriteProcessingInstruction(string name, string text)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Not supported
    /// </summary>
    public override void WriteRaw(string data)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Not supported
    /// </summary>
    public override void WriteRaw(char[] buffer, int index, int count)
    {
      throw new NotSupportedException();
    }

    /// <summary>Writes the start of an attribute with the specified prefix, local name, and namespace URI.</summary>
    /// <param name="prefix">The namespace prefix of the attribute.</param>
    /// <param name="localName">The local name of the attribute.</param>
    /// <param name="ns">The namespace URI for the attribute.</param>
    /// <exception cref="EncoderFallbackException">There is a character in the buffer that is a valid XML character but is not valid for the output encoding. For example, if the output encoding is ASCII, you should only use characters from the range of 0 to 127 for element and attribute names. The invalid character might be in the argument of this method or in an argument of previous methods that were writing to the buffer. Such characters are escaped by character entity references when possible (for example, in text nodes or attribute values). However, the character entity reference is not allowed in element and attribute names, comments, processing instructions, or CDATA sections. </exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      _buffer.Length = 0;
      _name = localName;
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void WriteStartDocument()
    {
      // Do nothing
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void WriteStartDocument(bool standalone)
    {
      // Do nothing
    }

    /// <summary>Writes the specified start tag and associates it with the given namespace and prefix.</summary>
    /// <param name="prefix">The namespace prefix of the element.</param>
    /// <param name="localName">The local name of the element.</param>
    /// <param name="ns">The namespace URI to associate with the element.</param>
    /// <exception cref="InvalidOperationException">The writer is closed.</exception>
    /// <exception cref="EncoderFallbackException">There is a character in the buffer that is a valid XML character but is not valid for the output encoding. For example, if the output encoding is ASCII, you should only use characters from the range of 0 to 127 for element and attribute names. The invalid character might be in the argument of this method or in an argument of previous methods that were writing to the buffer. Such characters are escaped by character entity references when possible (for example, in text nodes or attribute values). However, the character entity reference is not allowed in element and attribute names, comments, processing instructions, or CDATA sections.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      if (localName == "Relationships")
        throw new NotSupportedException("Relationships are not supported at this time");

      FlushAttributes();
      if (localName == "Item")
      {
        if (_tags.OfType<ItemTag>().Skip(1).Any())
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

    /// <summary>Writes the given text content.</summary>
    /// <param name="text">The text to write.</param>
    /// <exception cref="ArgumentException">The text string contains an invalid surrogate pair.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteString(string text)
    {
      _buffer.Append(text);
    }

    /// <summary>Generates and writes the surrogate character entity for the surrogate character pair.</summary>
    /// <param name="lowChar">The low surrogate. This must be a value between 0xDC00 and 0xDFFF.</param>
    /// <param name="highChar">The high surrogate. This must be a value between 0xD800 and 0xDBFF.</param>
    /// <exception cref="ArgumentException">An invalid surrogate character pair was passed.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
      WriteString(new string(new char[] { highChar, lowChar }));
    }

    /// <summary>Writes out the given white space.</summary>
    /// <param name="ws">The string of white space characters.</param>
    /// <exception cref="ArgumentException">The string contains non-white space characters.</exception>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
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
            var cols = SelectNode.FromString(buffer);
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

    /// <summary>
    /// Returns the generated SQL using the <see cref="IAmlSqlWriterSettings.RenderOption"/> 
    /// specified in the <see cref="IAmlSqlWriterSettings"/>
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return ToString(_settings.RenderOption);
    }

    /// <summary>
    /// Returns the generated SQL
    /// </summary>
    /// <param name="renderOption">Option specifying what SQL to generate</param>
    public string ToString(AmlSqlRenderOption renderOption)
    {
      using (var writer = new StringWriter())
      {
        ToString(writer, renderOption);
        writer.Flush();
        return writer.ToString();
      }
    }

    /// <summary>
    /// Returns the generated SQL
    /// </summary>
    /// <param name="writer"><see cref="TextWriter"/> to write the SQL to</param>
    /// <param name="renderOption">Option specifying what SQL to generate</param>
    public void ToString(TextWriter writer, AmlSqlRenderOption renderOption)
    {
      renderOption = renderOption == AmlSqlRenderOption.Default ? AmlSqlRenderOption.SelectQuery : renderOption;

      string buffer;
      switch (renderOption)
      {
        case AmlSqlRenderOption.CountQuery:
          writer.Append("select isnull(sum(cnt), 0) count from (select ")
            .Append(_lastItem.Alias).Append(".permission_id, ")
            .Append(_lastItem.Alias).Append(".created_by_id, ")
            .Append(_lastItem.Alias).Append(".managed_by_id, ")
            .Append(_lastItem.Alias).Append(".owned_by_id, ")
            .Append(_lastItem.Alias).Append(".team_id, count(*) cnt");
          AppendFromClause(writer, _lastItem.From, _settings.PermissionOption);
          writer.Append(" where ");
          AppendWhereClause(writer, _lastItem, false);
          writer.Append(" group by ")
            .Append(_lastItem.Alias).Append(".permission_id, ")
            .Append(_lastItem.Alias).Append(".created_by_id, ")
            .Append(_lastItem.Alias).Append(".managed_by_id, ")
            .Append(_lastItem.Alias).Append(".owned_by_id, ")
            .Append(_lastItem.Alias).Append(".team_id) perm where ");
          AppendPermissionCheck(writer, "perm");
          break;
        case AmlSqlRenderOption.OffsetQuery:
          if (!_lastItem.Attributes.TryGetValue("offsetId", out buffer))
            throw new InvalidOperationException("No `offsetId` attribute was specified");

          writer.Append("select isnull(sum(cnt), 0) offset from (select ")
            .Append(_lastItem.Alias).Append(".permission_id, ")
            .Append(_lastItem.Alias).Append(".created_by_id, ")
            .Append(_lastItem.Alias).Append(".managed_by_id, ")
            .Append(_lastItem.Alias).Append(".owned_by_id, ")
            .Append(_lastItem.Alias).Append(".team_id, count(*) cnt");
          var tables = _lastItem.From.ToList();
          tables.Insert(1, new Table() { Alias = "offset", Name = tables.First().Name });

          var criteria = new StringBuilder()
            .Append("offset.id = '").Append(buffer.Replace("'", "''")).Append("'");
          criteria.Append(" and (");

          var cols = GetOrderBy(_lastItem).ToArray();
          for (var i = 0; i < cols.Length; i++)
          {
            if (i > 0)
              criteria.Append(" or ");
            criteria.Append('(');
            for (var j = 0; j < i; j++)
            {
              criteria.Append(tables[0].Alias).Append('.').Append(cols[j].Name).Append(" = ")
                .Append("offset.").Append(cols[j].Name).Append(" and ");
            }
            criteria.Append(tables[0].Alias).Append('.').Append(cols[i].Name)
              .Append(cols[i].Descending ? " > " : " < ")
              .Append("offset.").Append(cols[i].Name)
              .Append(')');
          }
          criteria.Append(')');

          tables[1].OuterJoin = false;
          tables[1].Criteria = criteria.ToString();
          AppendFromClause(writer, tables, _settings.PermissionOption);

          writer.Append(" where ");
          AppendWhereClause(writer, _lastItem, false);
          writer.Append(" group by ")
            .Append(_lastItem.Alias).Append(".permission_id, ")
            .Append(_lastItem.Alias).Append(".created_by_id, ")
            .Append(_lastItem.Alias).Append(".managed_by_id, ")
            .Append(_lastItem.Alias).Append(".owned_by_id, ")
            .Append(_lastItem.Alias).Append(".team_id) perm where ");
          AppendPermissionCheck(writer, "perm");

          break;
        default:
          if ((renderOption & AmlSqlRenderOption.SelectClause) != 0)
            writer.Append(_lastItem.Select.ToString());
          if ((renderOption & AmlSqlRenderOption.FromClause) != 0)
            AppendFromClause(writer, _lastItem.From, _settings.PermissionOption);
          if (renderOption != AmlSqlRenderOption.WhereClause)
            writer.Append(" where ");
          if ((renderOption & AmlSqlRenderOption.WhereClause) != 0)
            AppendWhereClause(writer, _lastItem, true);

          if ((renderOption & AmlSqlRenderOption.OrderByClause) != 0)
          {
            var first = true;
            foreach (var col in GetOrderBy(_lastItem))
            {
              if (first)
                writer.Append(" order by ");
              else
                writer.Append(", ");
              first = false;

              writer.Append(_lastItem.Alias).Append('.');
              writer.Append(col.Name);

              if (col.Descending)
                writer.Append(" DESC");
            }
          }

          if ((renderOption & AmlSqlRenderOption.OffsetClause) != 0
            && !_lastItem.Attributes.TryGetValue("maxRecords", out buffer)
            && _lastItem.Attributes.TryGetValue("pagesize", out buffer))
          {
            var pageSize = int.Parse(buffer);
            var page = int.Parse(_lastItem.Attributes["page"]);
            writer.Append(" offset ")
              .Append(pageSize * (page - 1))
              .Append(" rows fetch next ")
              .Append(pageSize)
              .Append(" rows only");
          }
          break;
      }
    }

    private void AppendFromClause(TextWriter builder, IEnumerable<Table> tables, AmlSqlPermissionOption permission)
    {
      var first = true;
      foreach (var table in tables)
      {
        if (first)
          builder.Append(" from ");
        else
          builder.Append(table.OuterJoin ? " left" : " inner").Append(" join ");

        builder.Append(permission == AmlSqlPermissionOption.SecuredFunction ? "secured" : "innovator")
          .Append('.').Append(table.Name);

        if (permission == AmlSqlPermissionOption.SecuredFunction)
        {
          builder.Append("('can_get','")
            .Append(_settings.IdentityList)
            .Append("',null,'")
            .Append(_settings.UserId)
            .Append("',null)");
        }

        if (table.Alias != table.Name)
          builder.Append(" as ").Append(table.Alias);

        if (!string.IsNullOrEmpty(table.Criteria))
          builder.Append(" on ").Append(table.Criteria);

        first = false;
      }
    }

    private void AppendWhereClause(TextWriter builder, ItemTag tag, bool addPermissionChecks)
    {
      var start = 0;
      var hasWhere = tag.RelatedWhere.Any() || tag.Where.Length > 0;
      foreach (var related in tag.RelatedWhere)
      {
        builder.Append(tag.Where.ToString(start, related.Index - start))
          .Append('(')
          .Append(related.Criteria);
        if (addPermissionChecks && _settings.PermissionOption == AmlSqlPermissionOption.LegacyFunction)
        {
          if (!string.IsNullOrEmpty(related.Criteria))
            builder.Append(" and ");
          AppendPermissionCheck(builder, related.Alias);
        }
        builder.Append(')');
        start = related.Index;
      }
      builder.Append(tag.Where.ToString(start, tag.Where.Length - start));

      if (addPermissionChecks && _settings.PermissionOption == AmlSqlPermissionOption.LegacyFunction)
      {
        if (hasWhere)
          builder.Append(" and ");
        AppendPermissionCheck(builder, tag.Alias);
        hasWhere = true;
      }

      string queryType;
      if (!tag.Attributes.TryGetValue("queryType", out queryType))
        queryType = "current";

      if (!tag.IgnoreVersions)
      {
        if (string.Equals(queryType, "current", StringComparison.OrdinalIgnoreCase))
        {
          if (hasWhere)
            builder.Append(" and ");
          builder.Append(tag.TableName)
            .Append(".is_current = '1'");
        }
        else if (string.Equals(queryType, "latest", StringComparison.OrdinalIgnoreCase)
          && tag.Where.ToString().IndexOf(tag.Alias + ".is_active_rev", StringComparison.OrdinalIgnoreCase) >= 0)
        {
          // all good here
        }
        else
        {
          throw new NotSupportedException("The `queryType` of `" + queryType + "` is not supported");
        }
      }
    }

    private void AppendPermissionCheck(TextWriter builder, string alias)
    {
      builder.Append("( SELECT p FROM innovator.[")
        .Append(_settings.PermissionOption == AmlSqlPermissionOption.LegacyFunction ? "GetDiscoverPermissions" : "EvaluatePermissions")
        .Append("] ('can_get', ")
        .Append(alias).Append(".permission_id, ")
        .Append(alias).Append(".created_by_id, ")
        .Append(alias).Append(".managed_by_id, ")
        .Append(alias).Append(".owned_by_id, ")
        .Append(alias).Append(".team_id, '")
        .Append(_settings.IdentityList)
        .Append("', null, '")
        .Append(_settings.UserId)
        .Append("', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0");
    }

    private IEnumerable<OrderByColumn> GetOrderBy(ItemTag item)
    {
      string buffer;
      if (item.Attributes.TryGetValue("orderBy", out buffer) && !string.IsNullOrEmpty(buffer))
      {

        var columns = buffer.Split(',')
          .Select(c => c.Trim());
        foreach (var col in columns)
        {
          var parts = col.Split(' ');
          if (parts.Length >= 2)
            throw new InvalidOperationException("Invalid `orderBy` column: `" + col + "`");

          if (parts.Length == 1)
          {
            yield return new OrderByColumn()
            {
              Name = AssertPropertyName(parts[0])
            };
          }
          else if (parts.Length == 2)
          {
            switch (parts[1].ToLowerInvariant())
            {
              case "asc":
              case "desc":
                yield return new OrderByColumn()
                {
                  Name = AssertPropertyName(parts[0]),
                  Descending = string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase)
                };
                break;
              default:
                throw new InvalidOperationException("Invalid `orderBy` column: `" + col + "`");
            }
          }
        }
      }
      else
      {
        var props = _settings.GetProperties(item.Attributes["type"]).Values;
        var orderProps = props
          .Where(p => p.OrderBy().HasValue())
          .OrderBy(p => p.OrderBy().AsInt(int.MaxValue));
        foreach (var prop in orderProps)
        {
          yield return new OrderByColumn() { Name = prop.NameProp().Value };
        }

        yield return new OrderByColumn() { Name = "id" };
      }
    }

    private class Tag
    {
      private readonly Dictionary<string, string> _attrs = new Dictionary<string, string>();

      public Dictionary<string, string> Attributes { get { return _attrs; } }
      public string Name { get; set; }
      public bool AttributesProcessed { get; set; }
    }

    private class ItemTag : Tag
    {
      private readonly List<Table> _tables = new List<Table>() { new Table() };
      private readonly List<RelatedWhere> _relatedWhere = new List<RelatedWhere>();

      public string Alias
      {
        get { return _tables[0].Alias; }
        set { _tables[0].Alias = value; }
      }
      public string TableName
      {
        get { return _tables[0].Name; }
        set { _tables[0].Name = value; }
      }
      public StringBuilder Select { get; } = new StringBuilder();
      public IList<Table> From { get { return _tables; } }
      public IList<RelatedWhere> RelatedWhere { get { return _relatedWhere; } }
      public StringBuilder Where { get; } = new StringBuilder();
      public bool IgnoreVersions { get; set; } = false;
    }

    private class Table
    {
      public string Name { get; set; }
      public string Alias { get; set; }
      public bool OuterJoin { get; set; }
      public string Criteria { get; set; }
    }

    private class RelatedWhere
    {
      public int Index { get; set; }
      public string Alias { get; set; }
      public string Criteria { get; set; }
    }

    private class OrderByColumn
    {
      public string Name { get; set; }
      public bool Descending { get; set; }
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
