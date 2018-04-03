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
  public class AmlSqlWriter : QueryModel.AmlToModelWriter
  {
    private readonly IAmlSqlWriterSettings _settings;
    private readonly TextWriter _writer;

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

    /// <summary>Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.</summary>
    /// <exception cref="InvalidOperationException">An <see cref="XmlWriter" /> method was called before a previous asynchronous operation finished. In this case, <see cref="InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
    public override void Flush()
    {
      base.Flush();

      if (_writer != null)
        ToString(_writer, _settings.RenderOption);
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
      var visitor = new QueryModel.SqlServerVisitor(writer);
      switch (renderOption)
      {
        case AmlSqlRenderOption.CountQuery:
          //writer.Append("select isnull(sum(cnt), 0) count from (select ")
          //  .Append(_lastItem.Alias).Append(".permission_id, ")
          //  .Append(_lastItem.Alias).Append(".created_by_id, ")
          //  .Append(_lastItem.Alias).Append(".managed_by_id, ")
          //  .Append(_lastItem.Alias).Append(".owned_by_id, ")
          //  .Append(_lastItem.Alias).Append(".team_id, count(*) cnt");
          //AppendFromClause(writer, _lastItem.From, _settings.PermissionOption);
          //writer.Append(" where ");
          //AppendWhereClause(writer, _lastItem, false, false);
          //writer.Append(" group by ")
          //  .Append(_lastItem.Alias).Append(".permission_id, ")
          //  .Append(_lastItem.Alias).Append(".created_by_id, ")
          //  .Append(_lastItem.Alias).Append(".managed_by_id, ")
          //  .Append(_lastItem.Alias).Append(".owned_by_id, ")
          //  .Append(_lastItem.Alias).Append(".team_id) perm where ");
          //AppendPermissionCheck(writer, "perm");
          break;
        case AmlSqlRenderOption.OffsetQuery:
          //if (!_lastItem.Attributes.TryGetValue("offsetId", out buffer))
          //  throw new InvalidOperationException("No `offsetId` attribute was specified");

          //writer.Append("select isnull(sum(cnt), 0) offset from (select ")
          //  .Append(_lastItem.Alias).Append(".permission_id, ")
          //  .Append(_lastItem.Alias).Append(".created_by_id, ")
          //  .Append(_lastItem.Alias).Append(".managed_by_id, ")
          //  .Append(_lastItem.Alias).Append(".owned_by_id, ")
          //  .Append(_lastItem.Alias).Append(".team_id, count(*) cnt");
          //var tables = _lastItem.From.ToList();
          //tables.Insert(1, new Table() { Alias = "offset", Name = tables.First().Name });

          //var criteria = new StringBuilder()
          //  .Append("offset.id = '").Append(buffer.Replace("'", "''")).Append("'");
          //criteria.Append(" and (");

          //var cols = GetOrderBy(_lastItem).ToArray();
          //for (var i = 0; i < cols.Length; i++)
          //{
          //  if (i > 0)
          //    criteria.Append(" or ");
          //  criteria.Append('(');
          //  for (var j = 0; j < i; j++)
          //  {
          //    criteria.Append(tables[0].Alias).Append('.').Append(cols[j].Name).Append(" = ")
          //      .Append("offset.").Append(cols[j].Name).Append(" and ");
          //  }
          //  criteria.Append(tables[0].Alias).Append('.').Append(cols[i].Name)
          //    .Append(cols[i].Descending ? " > " : " < ")
          //    .Append("offset.").Append(cols[i].Name)
          //    .Append(')');
          //}
          //criteria.Append(')');

          //tables[1].OuterJoin = false;
          //tables[1].Criteria = criteria.ToString();
          //AppendFromClause(writer, tables, _settings.PermissionOption);

          //writer.Append(" where ");
          //AppendWhereClause(writer, _lastItem, false, false);
          //writer.Append(" group by ")
          //  .Append(_lastItem.Alias).Append(".permission_id, ")
          //  .Append(_lastItem.Alias).Append(".created_by_id, ")
          //  .Append(_lastItem.Alias).Append(".managed_by_id, ")
          //  .Append(_lastItem.Alias).Append(".owned_by_id, ")
          //  .Append(_lastItem.Alias).Append(".team_id) perm where ");
          //AppendPermissionCheck(writer, "perm");

          break;
        default:
          if ((renderOption & AmlSqlRenderOption.SelectClause) != 0)
            visitor.VisitSelect(Query.Select);
          if ((renderOption & AmlSqlRenderOption.FromClause) != 0)
            visitor.VisitFrom(Query.From);
          if ((renderOption & AmlSqlRenderOption.WhereClause) != 0)
            visitor.VisitWhere(Query.Where);
          if ((renderOption & AmlSqlRenderOption.OrderByClause) != 0)
            visitor.VisitOrderBy(Query.OrderBy);

          //if ((renderOption & AmlSqlRenderOption.OffsetClause) != 0
          //  && !_lastItem.Attributes.TryGetValue("maxRecords", out buffer)
          //  && _lastItem.Attributes.TryGetValue("pagesize", out buffer))
          //{
          //  var pageSize = int.Parse(buffer);
          //  var page = int.Parse(_lastItem.Attributes["page"]);
          //  writer.Append(" offset ")
          //    .Append(pageSize * (page - 1))
          //    .Append(" rows fetch next ")
          //    .Append(pageSize)
          //    .Append(" rows only");
          //}
          break;
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
  }
}
