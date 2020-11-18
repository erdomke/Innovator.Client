using Innovator.Client.QueryModel;
using Innovator.Client.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Innovator.Client.QueryModel.Tests
{
  [TestClass()]
  public class AmlSqlWriterTests
  {
    [TestMethod()]
    public void Aml2Sql_Basic()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' queryType='Latest' queryDate='2017-05-11T17:37:00'>
  <is_active_rev>1</is_active_rev>
  <keyed_name condition='like'>999-*</keyed_name>
  <owned_by_id><Item type='Identity' action='get'><keyed_name condition='like'>*super*</keyed_name></Item></owned_by_id>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction };
      var query = item.ToQueryItem();
      var sql = query.ToArasSql(settings);
      Assert.AreEqual("select Part.* from innovator.Part inner join innovator.[Identity] on Part.owned_by_id = [Identity].id where Part.is_active_rev = '1' and Part.keyed_name like N'999-%' and ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', Part.permission_id, Part.created_by_id, Part.managed_by_id, Part.owned_by_id, Part.team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0 and [Identity].keyed_name like N'%super%' and ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', [Identity].permission_id, [Identity].created_by_id, [Identity].managed_by_id, [Identity].owned_by_id, [Identity].team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0 order by Part.id"
        , sql);
    }

    [TestMethod()]
    public void Aml2Sql_NoPermissions()
    {
      var item = ElementFactory.Local.FromXml(@"<Item action='get' type='Part' select='config_id'><id condition='in'>'71B2D9633CA14B1486E1FE473C7CF950','C0A0F17A9E3346D380ED015B1FD1F2A7','C5F56BF14FB64AB3BD0AF6AEE67AF00A'</id></Item>").AssertItem();

      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection())
      {
        PermissionOption = AmlSqlPermissionOption.None,
        RenderOption = SqlRenderOption.SelectClause | SqlRenderOption.FromClause | SqlRenderOption.WhereClause
      };
      var sql = item.ToQueryItem().ToArasSql(settings);
      Assert.AreEqual("select Part.config_id from innovator.Part where Part.id in ('71B2D9633CA14B1486E1FE473C7CF950', 'C0A0F17A9E3346D380ED015B1FD1F2A7', 'C5F56BF14FB64AB3BD0AF6AEE67AF00A')"
        , sql);
    }

    [TestMethod()]
    public void Aml2Sql_MaxRecords()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' maxRecords='100' queryType='Latest' queryDate='2017-05-11T17:37:00' select='id'>
  <is_active_rev>1</is_active_rev>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.None };
      var sql = item.ToQueryItem().ToArasSql(settings);
      Assert.AreEqual("select top 100 Part.id from innovator.Part where Part.is_active_rev = '1' order by Part.id"
        , sql);
    }

    [TestMethod()]
    public void Aml2Sql_Paging()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' page='2' pagesize='100' select='id'>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.None };
      var sql = item.ToQueryItem().ToArasSql(settings);
      Assert.AreEqual("select Part.id from innovator.Part where Part.is_current = '1' order by Part.id offset 100 rows fetch next 100 rows only"
        , sql);
    }

    [TestMethod()]
    public void Aml2Sql_WhereClause()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' queryType='Latest' queryDate='2017-05-11T17:37:00'>
  <is_active_rev>1</is_active_rev>
  <keyed_name condition='like'>999-*</keyed_name>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction };
      settings.PermissionOption = AmlSqlPermissionOption.None;
      settings.RenderOption = SqlRenderOption.WhereClause;
      var query = item.ToQueryItem();

      var sql = query.ToArasSql(settings);
      Assert.AreEqual("Part.is_active_rev = '1' and Part.keyed_name like N'999-%'", sql);

      sql = ToBaseSql(query, settings);
      Assert.AreEqual("Part.is_active_rev = '1' and Part.keyed_name like '999-%'", sql);
    }

    [TestMethod()]
    public void Aml2Sql_WhereClause2()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Can Add' typeId='3A65F41FF1FC42518A702FDA164AF420' action='get'>
  <source_id keyed_name='Located' type='ItemType' name='Located'>5698BACD2A7A45D6AC3FA60EAB3E6566</source_id>
  <can_add>1</can_add>
  <related_id keyed_name='World' type='Identity'>A73B655731924CD0B027E4F4D5FCC0A9</related_id>
  <sort_order>128</sort_order>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction };
      settings.PermissionOption = AmlSqlPermissionOption.None;
      settings.RenderOption = SqlRenderOption.WhereClause;
      var query = item.ToQueryItem();

      var sql = query.ToArasSql(settings);
      Assert.AreEqual("Can_Add.source_id = '5698BACD2A7A45D6AC3FA60EAB3E6566' and Can_Add.can_add = 1 and Can_Add.related_id = 'A73B655731924CD0B027E4F4D5FCC0A9' and Can_Add.sort_order = 128 and Can_Add.is_current = '1'", sql);

      sql = ToBaseSql(query, settings);
      Assert.AreEqual("[Can Add].source_id = '5698BACD2A7A45D6AC3FA60EAB3E6566' and [Can Add].can_add = 1 and [Can Add].related_id = 'A73B655731924CD0B027E4F4D5FCC0A9' and [Can Add].sort_order = 128", sql);
    }

    [TestMethod()]
    public void Aml2Sql_WhereClause3()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Activity Assignment' action='get' queryType='ignore'>
  <source_id>E7D1C2C5431C4ECF9BA5ADAE0AC50377</source_id>
  <closed_on condition='is null'></closed_on>
  <is_disabled>0</is_disabled>
  <is_required>1</is_required>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction };
      settings.PermissionOption = AmlSqlPermissionOption.None;
      settings.RenderOption = SqlRenderOption.WhereClause;
      var query = item.ToQueryItem();

      var sql = query.ToArasSql(settings);
      Assert.AreEqual("Activity_Assignment.source_id = 'E7D1C2C5431C4ECF9BA5ADAE0AC50377' and Activity_Assignment.closed_on is null and Activity_Assignment.is_disabled = '0' and Activity_Assignment.is_required = '1'", sql);

      sql = ToBaseSql(query, settings);
      Assert.AreEqual("[Activity Assignment].source_id = 'E7D1C2C5431C4ECF9BA5ADAE0AC50377' and [Activity Assignment].closed_on is null and [Activity Assignment].is_disabled = '0' and [Activity Assignment].is_required = '1'", sql);
    }

    [TestMethod()]
    public void Aml2Sql_WhereClause4()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Activity Assignment' action='get' queryType='ignore' where=""closed_on is null and is_disabled = '0'"">
  <source_id>E7D1C2C5431C4ECF9BA5ADAE0AC50377</source_id>
  <is_required>1</is_required>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction };
      settings.PermissionOption = AmlSqlPermissionOption.None;
      settings.RenderOption = SqlRenderOption.WhereClause;
      var query = item.ToQueryItem();

      var sql = query.ToArasSql(settings);
      Assert.AreEqual("Activity_Assignment.source_id = 'E7D1C2C5431C4ECF9BA5ADAE0AC50377' and Activity_Assignment.is_required = '1' and Activity_Assignment.closed_on is null and Activity_Assignment.is_disabled = '0'", sql);

      sql = ToBaseSql(query, settings);
      Assert.AreEqual("[Activity Assignment].source_id = 'E7D1C2C5431C4ECF9BA5ADAE0AC50377' and [Activity Assignment].is_required = '1' and [Activity Assignment].closed_on is null and [Activity Assignment].is_disabled = '0'", sql);
    }


    [TestMethod()]
    public void Aml2Sql_WhereClause5()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Activity Assignment' action='get' queryType='ignore' where=""closed_on is null and is_disabled = '0'"">
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection())
      {
        PermissionOption = AmlSqlPermissionOption.None,
        RenderOption = SqlRenderOption.WhereClause
      };
      var query = item.ToQueryItem();

      var sql = query.ToArasSql(settings);
      Assert.AreEqual("Activity_Assignment.closed_on is null and Activity_Assignment.is_disabled = '0'", sql);

      sql = ToBaseSql(query, settings);
      Assert.AreEqual("[Activity Assignment].closed_on is null and [Activity Assignment].is_disabled = '0'", sql);
    }

    [TestMethod()]
    public void Aml2Sql_OrderBy()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' orderBy='item_number'>
  <keyed_name condition='like'>999-*</keyed_name>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.None };
      var query = item.ToQueryItem();

      var sql = query.ToArasSql(settings);
      Assert.AreEqual("select Part.* from innovator.Part where Part.keyed_name like N'999-%' and Part.is_current = '1' order by Part.item_number", sql);

      sql = ToBaseSql(query, settings);
      Assert.AreEqual("select Part.* from Part where Part.keyed_name like '999-%' order by Part.item_number", sql);
    }

    [TestMethod()]
    public void Aml2Sql_Count()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' queryType='Latest' queryDate='2017-05-11T17:37:00'>
  <is_active_rev>1</is_active_rev>
  <keyed_name condition='like'>999-*</keyed_name>
  <owned_by_id><Item type='Identity' action='get'><keyed_name condition='like'>*super*</keyed_name></Item></owned_by_id>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction };
      settings.RenderOption = SqlRenderOption.CountQuery;
      var sql = item.ToQueryItem().ToArasSql(settings);
      Assert.AreEqual("select isnull(sum(cnt), 0) count from (select Part.permission_id, Part.created_by_id, Part.managed_by_id, Part.owned_by_id, Part.team_id, count(*) cnt from innovator.Part inner join innovator.[Identity] on Part.owned_by_id = [Identity].id where Part.is_active_rev = '1' and Part.keyed_name like N'999-%' and [Identity].keyed_name like N'%super%' group by Part.permission_id, Part.created_by_id, Part.managed_by_id, Part.owned_by_id, Part.team_id) perm where ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', perm.permission_id, perm.created_by_id, perm.managed_by_id, perm.owned_by_id, perm.team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0"
        , sql);
    }


    [TestMethod()]
    public void Aml2Sql_Offset()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' offsetId='C0A0F17A9E3346D380ED015B1FD1F2A7' orderBy='item_number,generation,major_rev,id'>
  <state condition='ne'>Obsolete</state>
  <is_in_service condition='ne'>1</is_in_service>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction };
      settings.RenderOption = SqlRenderOption.OffsetQuery;
      var sql = item.ToQueryItem().ToArasSql(settings);
      Assert.AreEqual("select isnull(sum(cnt), 0) offset from ( select Part.permission_id, Part.created_by_id, Part.managed_by_id, Part.owned_by_id, Part.team_id, count(*) cnt from innovator.Part inner join ( select Part.item_number, Part.generation, Part.major_rev, Part.id from innovator.Part where Part.state <> N'Obsolete' and Part.is_in_service <> '1' and Part.is_current = '1' and Part.id = 'C0A0F17A9E3346D380ED015B1FD1F2A7') offset on (Part.[item_number] < offset.[item_number]) or (Part.[item_number] = offset.[item_number] and Part.[generation] < offset.[generation]) or (Part.[item_number] = offset.[item_number] and Part.[generation] = offset.[generation] and Part.[major_rev] < offset.[major_rev]) or (Part.[item_number] = offset.[item_number] and Part.[generation] = offset.[generation] and Part.[major_rev] = offset.[major_rev] and Part.[id] < offset.[id]) where Part.state <> N'Obsolete' and Part.is_in_service <> '1' and Part.is_current = '1' group by Part.permission_id, Part.created_by_id, Part.managed_by_id, Part.owned_by_id, Part.team_id ) perm where ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', perm.permission_id, perm.created_by_id, perm.managed_by_id, perm.owned_by_id, perm.team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0"
        , sql);
    }

    [TestMethod()]
    public void Aml2Sql_Offset_NoCriteria()
    {
      var item = ElementFactory.Local.FromXml("<Item type='Part' offsetId='C0A0F17A9E3346D380ED015B1FD1F2A7' />").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.SecuredFunctionEnviron };
      settings.RenderOption = SqlRenderOption.OffsetQuery;
      var sql = item.ToQueryItem().ToArasSql(settings);
      Assert.AreEqual("select isnull(sum(cnt), 0) offset from ( select Part.permission_id, Part.created_by_id, Part.managed_by_id, Part.owned_by_id, Part.team_id, count(*) cnt from innovator.Part inner join ( select Part.id from innovator.Part where Part.is_current = '1' and Part.id = 'C0A0F17A9E3346D380ED015B1FD1F2A7') offset on (Part.[id] < offset.[id]) where Part.is_current = '1' group by Part.permission_id, Part.created_by_id, Part.managed_by_id, Part.owned_by_id, Part.team_id ) perm where ( SELECT p FROM innovator.[EvaluatePermissions] ('can_get', perm.permission_id, perm.created_by_id, perm.managed_by_id, perm.owned_by_id, perm.team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0"
        , sql);
    }

    [TestMethod]
    public void Aml2Sql_Complex()
    {
      ServerContext._clock = () => DateTimeOffset.FromFileTime(131649408000000000);
      var item = ElementFactory.Local.FromXml(@"<Item action=""get"" type=""Thing"" select="""">
  <created_on condition=""between"" origDateRange=""Dynamic|Week|-1|Week|-1"">2018-02-25T00:00:00 and 2018-03-03T23:59:59</created_on>
  <or>
    <state condition=""like"">*Canceled*</state>
    <state condition=""like"">*Closed*</state>
    <state condition=""like"">*Closed : Conversion*</state>
    <state condition=""like"">*Review*</state>
    <state condition=""like"">*In Work*</state>
  </or>
  <or>
    <classification condition=""like"">Suspect Part</classification>
    <classification condition=""like"">Suspect Part/Customer</classification>
    <classification condition=""like"">Suspect Part/Incoming</classification>
    <classification condition=""like"">Suspect Part/Production</classification>
    <classification condition=""like"">Suspect Part/*</classification>
    <classification condition=""like"">Suspect Part/Customer/*</classification>
    <classification condition=""like"">Suspect Part/Incoming/*</classification>
    <classification condition=""like"">Suspect Part/Production/*</classification>
  </or>
  <owned_by_id>
    <Item action=""get"" type=""Identity"" doGetItem=""0"">
      <or>
        <keyed_name condition=""like"">*john smith*</keyed_name>
        <keyed_name condition=""like"">*jane doe*</keyed_name>
      </or>
    </Item>
  </owned_by_id>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction };
      settings.RenderOption = SqlRenderOption.WhereClause;
      settings.PermissionOption = AmlSqlPermissionOption.None;
      var sql = item.ToQueryItem().ToArasSql(settings);
      Assert.AreEqual("Thing.created_on between '2018-02-25T05:00:00' and '2018-03-04T04:59:59' and (Thing.state like N'%Canceled%' or Thing.state like N'%Closed%' or Thing.state like N'%Closed : Conversion%' or Thing.state like N'%Review%' or Thing.state like N'%In Work%') and (Thing.classification = N'Suspect Part' or Thing.classification = N'Suspect Part/Customer' or Thing.classification = N'Suspect Part/Incoming' or Thing.classification = N'Suspect Part/Production' or Thing.classification like N'Suspect Part/%' or Thing.classification like N'Suspect Part/Customer/%' or Thing.classification like N'Suspect Part/Incoming/%' or Thing.classification like N'Suspect Part/Production/%') and Thing.is_current = '1' and ([Identity].keyed_name like N'%john smith%' or [Identity].keyed_name like N'%jane doe%')"
        , sql);
    }

    [TestMethod]
    public void Aml2Sql_IdAttribute()
    {
      var item = ElementFactory.Local.FromXml("<Item type='DFMEA' action='get' id='18427D78485C4755BA8746CF1F839405' select='id'/>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection())
      {
        RenderOption = SqlRenderOption.SelectQuery,
        PermissionOption = AmlSqlPermissionOption.None
      };
      var sql = item.ToQueryItem().ToArasSql(settings);
      Assert.AreEqual("select DFMEA.id from innovator.DFMEA where DFMEA.id = '18427D78485C4755BA8746CF1F839405' order by DFMEA.id", sql);
    }

    [TestMethod]
    public void Aml2Sql_OrClause()
    {
      ServerContext._clock = () => DateTimeOffset.FromFileTime(131649408000000000);
      var item = ElementFactory.Local.FromXml(@"<Item action='get' type='Concern' select='name'>
  <or>
    <classification condition='like'>Suspect Part</classification>
    <classification condition='like'>Suspect Part/*</classification>
  </or>
  <created_on condition='between' origDateRange='Dynamic|Week|-1|Week|-1'>2014-09-28T00:00:00 and 2014-10-05T00:00:00</created_on>
</Item>").AssertItem();
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection())
      {
        RenderOption = SqlRenderOption.SelectQuery,
        PermissionOption = AmlSqlPermissionOption.None
      };
      var sql = item.ToQueryItem().ToArasSql(settings);
      Assert.AreEqual("select Concern.name from innovator.Concern where (Concern.classification = N'Suspect Part' or Concern.classification like N'Suspect Part/%') and Concern.created_on between '2018-02-25T05:00:00' and '2018-03-04T04:59:59' and Concern.is_current = '1' order by Concern.id", sql);
    }

    [TestMethod]
    public void Aml2Sql_Relationships()
    {
      var item = ElementFactory.Local.FromXml(@"<Item action='get' type='Part' select='id'>
  <Relationships>
    <Item action='get' type='Part CAD' select='id'>
      <related_id>
        <Item action='get' type='CAD' select='id'></Item>
      </related_id>
    </Item>
  </Relationships>
</Item>").AssertItem();
      var query = item.ToQueryItem();
      Assert.AreEqual(JoinType.LeftOuter, query.Joins.Single().Type);

      item = ElementFactory.Local.FromXml(@"<Item action='get' type='Part' select='id'>
        <Relationships>
          <Item action='get' type='Part CAD' select='id'>
            <related_id>
              <Item action='get' type='CAD' select='id'>
                <organization>158F22F4BAC8479E95D512ACEDB113C8</organization>
              </Item>
            </related_id>
          </Item>
        </Relationships>
      </Item>").AssertItem();
      query = item.ToQueryItem();
      Assert.AreEqual(JoinType.Inner, query.Joins.Single().Type);

      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection())
      {
        RenderOption = SqlRenderOption.SelectQuery,
        PermissionOption = AmlSqlPermissionOption.None
      };

      var sql = query.ToArasSql(settings);
      Assert.AreEqual("select Part.id from innovator.Part where Part.is_current = '1' and exists (select null from innovator.Part_CAD inner join innovator.CAD on Part_CAD.related_id = CAD.id where Part.id = Part_CAD.source_id and CAD.organization = '158F22F4BAC8479E95D512ACEDB113C8') order by Part.id", sql);

      sql = ToBaseSql(query, settings);
      Assert.AreEqual("select Part.id from Part where exists (select null from [Part CAD] inner join CAD on [Part CAD].related_id = CAD.id where Part.id = [Part CAD].source_id and CAD.organization = '158F22F4BAC8479E95D512ACEDB113C8')", sql);
    }

    [TestMethod]
    public void Aml2Sql_Relationships2()
    {
      var item = ElementFactory.Local.FromXml(@"<Item action='get' type='Alert'>
  <state>Open</state>
  <Relationships>
    <Item action='get' type='Alert Entity' related_expand='0'>
      <related_id>
        <Item action='get' type='Entity'>
          <criteria>thing</criteria>
        </Item>
      </related_id>
    </Item>
  </Relationships>
</Item>");

      var query = item.ToQueryItem();

      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection())
      {
        RenderOption = SqlRenderOption.WhereClause,
        PermissionOption = AmlSqlPermissionOption.None
      };

      var sql = query.ToArasSql(settings);
      Assert.AreEqual("Alert.state = N'Open' and Alert.is_current = '1' and exists (select null from innovator.Alert_Entity inner join innovator.Entity on Alert_Entity.related_id = Entity.id where Alert.id = Alert_Entity.source_id and Entity.criteria = N'thing')", sql);
    }

    private string ToBaseSql(QueryItem query, IAmlSqlWriterSettings settings)
    {
      using (var writer = new StringWriter())
      {
        var visitor = new SqlServerVisitor(writer, settings);
        var clone = new CloneVisitor().WithPropertyMapper(p =>
        {
          var table = p.Table;
          table.TryFillName(settings);
          if (string.IsNullOrEmpty(table.Type))
            return IgnoreNode.Instance;
          var props = settings.GetProperties(table.Type);
          if (props.Count < 1)
            return p;
          if (!props.TryGetValue(p.Name, out var propDefn))
            return IgnoreNode.Instance;
          if (propDefn.DataType().Value == "foreign")
            return table.GetProperty(propDefn);
          return p;
        }).Clone(query);
        visitor.Visit(clone);

        writer.Flush();
        return writer.ToString();
      }
    }
  }
}
