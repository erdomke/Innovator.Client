using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Innovator.Client.Tests
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
      var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction });
      item.ToAml(writer);
      writer.Flush();
      Assert.AreEqual("select [Part].* from innovator.[Part] left join innovator.[Identity] on [Identity].id = [Part].owned_by_id where [Part].is_active_rev = 1 and [Part].keyed_name like N'999-%' and ([Identity].keyed_name like N'%super%' and ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', [Identity].permission_id, [Identity].created_by_id, [Identity].managed_by_id, [Identity].owned_by_id, [Identity].team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9,1942894C56164DF4AB400434FF5EFE3B,56A96DA9E981481688563E2D14D5D878,F18B4A8C523A4B3F88CFFEE3BC68F5D4,2618D6F5A90949BAA7E920D1B04C7EE1,43CC1A53BECA4364A22C136E98E185C7,5A8E8A27AA5A47238BD96AC60A6562F3,5E47C25D3FE6464688442ABE64478537,08C52237B1124FC4BB5D4E48F660C8E2,22DF5704DCA14D86B79BFDFF3CA030B9,2C5CF38CB45C4E1F848EA6DF969364EF,31387021B15E42548FC818E893A19D08,344632A16E8949CCA5CAD0A013D50655,3A0D61E9629141C8934B948C596B2E42,3A976285BB9A4D05AE1878004A33D440,484451EB7B524ECD92FFA372A4675E38,4CAAF215654F43098B415EC0EAB1829A,4CD258EF03834E7FAC9CD76876777EBC,5E12CD824411477AA56D9F539FA295EB,600B965EA3DB41D0B7F9D9D7BDD923EC,62E1A4258C764C5E814DDA3B4209A34F,66022731E81D4040A155F13CD083826A,66F68F2DC7D3410699EA302411608618,685069C4626B406BA5EEFBD87CCABBAC,7C63ED1CFD0A4E0DAAD27AAB0C90A58D,7F50E4A530A84C22935ED7E2231198E1,82F4AA3DD22648288D63938D648CD814,84D2CEC2D1E247DC9D2D28B78D1DF591,A11E92826CA44BEEA4A84E494D13F0B8,A4AFDB4EBD2A4130A4D0616E2E63F552,AD55DC9C04274E67A2C4C45AADC9D059,B3DC39013C13458F9D3A479046B25266,BFE82C27CAAB4667B51EEC64D818BEC8,C02E1CEE4C2B492496EAEC1C19F814B0,C3DB96EFB5EC47AE816CE16AE3E79959,C5C35BB6D73145DB8C88084BEA96BB1D,C6D220539A8D4458B27CA579B637967F,C8FE09099D8D4C25BED39BBFE3BF0476,D78B07BAF283495E9540115C00AD9CED,DE6E87399974415F948D4468C07C5134,E42675D6FC354C86880230D5DA79E4CD,FFB8C4C174054BD3AA17A00023AD49BC,25EAA8CA05254E1DB191F984D994751D,5D58B17DB5094950B93AE13202E46C5E,6A2F0C9740B74625B744E5FCE43879F6,7FA8E5852B7F4FE1817C24A11798A784,B32BD81D1AD04207BF1E61E39A4E0E13,FA8573065D534FBB8DA1E40C14FA54EF,0CDBE4C0F8A045159339D1C8BE8665FA,31E5C20B933D433CA8F509F7AED9E68A,5E7DC598214E4BBAA09CDB91D644FABD,78A5819F71E2414EB48C5984AB005ABB,988240C797D548A79A78B1AD4CDED7B3,3FCE6B2163FC4F5983BF00A13E6B047E', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0) and ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', [Part].permission_id, [Part].created_by_id, [Part].managed_by_id, [Part].owned_by_id, [Part].team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9,1942894C56164DF4AB400434FF5EFE3B,56A96DA9E981481688563E2D14D5D878,F18B4A8C523A4B3F88CFFEE3BC68F5D4,2618D6F5A90949BAA7E920D1B04C7EE1,43CC1A53BECA4364A22C136E98E185C7,5A8E8A27AA5A47238BD96AC60A6562F3,5E47C25D3FE6464688442ABE64478537,08C52237B1124FC4BB5D4E48F660C8E2,22DF5704DCA14D86B79BFDFF3CA030B9,2C5CF38CB45C4E1F848EA6DF969364EF,31387021B15E42548FC818E893A19D08,344632A16E8949CCA5CAD0A013D50655,3A0D61E9629141C8934B948C596B2E42,3A976285BB9A4D05AE1878004A33D440,484451EB7B524ECD92FFA372A4675E38,4CAAF215654F43098B415EC0EAB1829A,4CD258EF03834E7FAC9CD76876777EBC,5E12CD824411477AA56D9F539FA295EB,600B965EA3DB41D0B7F9D9D7BDD923EC,62E1A4258C764C5E814DDA3B4209A34F,66022731E81D4040A155F13CD083826A,66F68F2DC7D3410699EA302411608618,685069C4626B406BA5EEFBD87CCABBAC,7C63ED1CFD0A4E0DAAD27AAB0C90A58D,7F50E4A530A84C22935ED7E2231198E1,82F4AA3DD22648288D63938D648CD814,84D2CEC2D1E247DC9D2D28B78D1DF591,A11E92826CA44BEEA4A84E494D13F0B8,A4AFDB4EBD2A4130A4D0616E2E63F552,AD55DC9C04274E67A2C4C45AADC9D059,B3DC39013C13458F9D3A479046B25266,BFE82C27CAAB4667B51EEC64D818BEC8,C02E1CEE4C2B492496EAEC1C19F814B0,C3DB96EFB5EC47AE816CE16AE3E79959,C5C35BB6D73145DB8C88084BEA96BB1D,C6D220539A8D4458B27CA579B637967F,C8FE09099D8D4C25BED39BBFE3BF0476,D78B07BAF283495E9540115C00AD9CED,DE6E87399974415F948D4468C07C5134,E42675D6FC354C86880230D5DA79E4CD,FFB8C4C174054BD3AA17A00023AD49BC,25EAA8CA05254E1DB191F984D994751D,5D58B17DB5094950B93AE13202E46C5E,6A2F0C9740B74625B744E5FCE43879F6,7FA8E5852B7F4FE1817C24A11798A784,B32BD81D1AD04207BF1E61E39A4E0E13,FA8573065D534FBB8DA1E40C14FA54EF,0CDBE4C0F8A045159339D1C8BE8665FA,31E5C20B933D433CA8F509F7AED9E68A,5E7DC598214E4BBAA09CDB91D644FABD,78A5819F71E2414EB48C5984AB005ABB,988240C797D548A79A78B1AD4CDED7B3,3FCE6B2163FC4F5983BF00A13E6B047E', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0 order by [Part].id"
        , writer.ToString());
    }

    [TestMethod()]
    public void Aml2Sql_NoPermissions()
    {
      var item = ElementFactory.Local.FromXml(@"<Item action='get' type='Part' select='config_id'><id condition='in'>'71B2D9633CA14B1486E1FE473C7CF950','C0A0F17A9E3346D380ED015B1FD1F2A7','C5F56BF14FB64AB3BD0AF6AEE67AF00A'</id></Item>").AssertItem();
      var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.None });
      item.ToAml(writer);
      writer.Flush();
      var render = AmlSqlRenderOption.SelectClause | AmlSqlRenderOption.FromClause | AmlSqlRenderOption.WhereClause;
      Assert.AreEqual("select [Part].config_id from innovator.[Part] where [Part].id in ('71B2D9633CA14B1486E1FE473C7CF950', 'C0A0F17A9E3346D380ED015B1FD1F2A7', 'C5F56BF14FB64AB3BD0AF6AEE67AF00A')"
        , writer.ToString(render));
    }

    [TestMethod()]
    public void Aml2Sql_MaxRecords()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' maxRecords='100' queryType='Latest' queryDate='2017-05-11T17:37:00' select='id'>
  <is_active_rev>1</is_active_rev>
</Item>").AssertItem();
      var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction });
      item.ToAml(writer);
      writer.Flush();
      Assert.AreEqual("select top 100 [Part].id from innovator.[Part] where [Part].is_active_rev = 1 and ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', [Part].permission_id, [Part].created_by_id, [Part].managed_by_id, [Part].owned_by_id, [Part].team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9,1942894C56164DF4AB400434FF5EFE3B,56A96DA9E981481688563E2D14D5D878,F18B4A8C523A4B3F88CFFEE3BC68F5D4,2618D6F5A90949BAA7E920D1B04C7EE1,43CC1A53BECA4364A22C136E98E185C7,5A8E8A27AA5A47238BD96AC60A6562F3,5E47C25D3FE6464688442ABE64478537,08C52237B1124FC4BB5D4E48F660C8E2,22DF5704DCA14D86B79BFDFF3CA030B9,2C5CF38CB45C4E1F848EA6DF969364EF,31387021B15E42548FC818E893A19D08,344632A16E8949CCA5CAD0A013D50655,3A0D61E9629141C8934B948C596B2E42,3A976285BB9A4D05AE1878004A33D440,484451EB7B524ECD92FFA372A4675E38,4CAAF215654F43098B415EC0EAB1829A,4CD258EF03834E7FAC9CD76876777EBC,5E12CD824411477AA56D9F539FA295EB,600B965EA3DB41D0B7F9D9D7BDD923EC,62E1A4258C764C5E814DDA3B4209A34F,66022731E81D4040A155F13CD083826A,66F68F2DC7D3410699EA302411608618,685069C4626B406BA5EEFBD87CCABBAC,7C63ED1CFD0A4E0DAAD27AAB0C90A58D,7F50E4A530A84C22935ED7E2231198E1,82F4AA3DD22648288D63938D648CD814,84D2CEC2D1E247DC9D2D28B78D1DF591,A11E92826CA44BEEA4A84E494D13F0B8,A4AFDB4EBD2A4130A4D0616E2E63F552,AD55DC9C04274E67A2C4C45AADC9D059,B3DC39013C13458F9D3A479046B25266,BFE82C27CAAB4667B51EEC64D818BEC8,C02E1CEE4C2B492496EAEC1C19F814B0,C3DB96EFB5EC47AE816CE16AE3E79959,C5C35BB6D73145DB8C88084BEA96BB1D,C6D220539A8D4458B27CA579B637967F,C8FE09099D8D4C25BED39BBFE3BF0476,D78B07BAF283495E9540115C00AD9CED,DE6E87399974415F948D4468C07C5134,E42675D6FC354C86880230D5DA79E4CD,FFB8C4C174054BD3AA17A00023AD49BC,25EAA8CA05254E1DB191F984D994751D,5D58B17DB5094950B93AE13202E46C5E,6A2F0C9740B74625B744E5FCE43879F6,7FA8E5852B7F4FE1817C24A11798A784,B32BD81D1AD04207BF1E61E39A4E0E13,FA8573065D534FBB8DA1E40C14FA54EF,0CDBE4C0F8A045159339D1C8BE8665FA,31E5C20B933D433CA8F509F7AED9E68A,5E7DC598214E4BBAA09CDB91D644FABD,78A5819F71E2414EB48C5984AB005ABB,988240C797D548A79A78B1AD4CDED7B3,3FCE6B2163FC4F5983BF00A13E6B047E', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0 order by [Part].id"
        , writer.ToString());
    }

    [TestMethod()]
    public void Aml2Sql_Paging()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' page='2' pagesize='100' select='id'>
</Item>").AssertItem();
      var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction });
      item.ToAml(writer);
      writer.Flush();
      Assert.AreEqual("select [Part].id from innovator.[Part] where ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', [Part].permission_id, [Part].created_by_id, [Part].managed_by_id, [Part].owned_by_id, [Part].team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9,1942894C56164DF4AB400434FF5EFE3B,56A96DA9E981481688563E2D14D5D878,F18B4A8C523A4B3F88CFFEE3BC68F5D4,2618D6F5A90949BAA7E920D1B04C7EE1,43CC1A53BECA4364A22C136E98E185C7,5A8E8A27AA5A47238BD96AC60A6562F3,5E47C25D3FE6464688442ABE64478537,08C52237B1124FC4BB5D4E48F660C8E2,22DF5704DCA14D86B79BFDFF3CA030B9,2C5CF38CB45C4E1F848EA6DF969364EF,31387021B15E42548FC818E893A19D08,344632A16E8949CCA5CAD0A013D50655,3A0D61E9629141C8934B948C596B2E42,3A976285BB9A4D05AE1878004A33D440,484451EB7B524ECD92FFA372A4675E38,4CAAF215654F43098B415EC0EAB1829A,4CD258EF03834E7FAC9CD76876777EBC,5E12CD824411477AA56D9F539FA295EB,600B965EA3DB41D0B7F9D9D7BDD923EC,62E1A4258C764C5E814DDA3B4209A34F,66022731E81D4040A155F13CD083826A,66F68F2DC7D3410699EA302411608618,685069C4626B406BA5EEFBD87CCABBAC,7C63ED1CFD0A4E0DAAD27AAB0C90A58D,7F50E4A530A84C22935ED7E2231198E1,82F4AA3DD22648288D63938D648CD814,84D2CEC2D1E247DC9D2D28B78D1DF591,A11E92826CA44BEEA4A84E494D13F0B8,A4AFDB4EBD2A4130A4D0616E2E63F552,AD55DC9C04274E67A2C4C45AADC9D059,B3DC39013C13458F9D3A479046B25266,BFE82C27CAAB4667B51EEC64D818BEC8,C02E1CEE4C2B492496EAEC1C19F814B0,C3DB96EFB5EC47AE816CE16AE3E79959,C5C35BB6D73145DB8C88084BEA96BB1D,C6D220539A8D4458B27CA579B637967F,C8FE09099D8D4C25BED39BBFE3BF0476,D78B07BAF283495E9540115C00AD9CED,DE6E87399974415F948D4468C07C5134,E42675D6FC354C86880230D5DA79E4CD,FFB8C4C174054BD3AA17A00023AD49BC,25EAA8CA05254E1DB191F984D994751D,5D58B17DB5094950B93AE13202E46C5E,6A2F0C9740B74625B744E5FCE43879F6,7FA8E5852B7F4FE1817C24A11798A784,B32BD81D1AD04207BF1E61E39A4E0E13,FA8573065D534FBB8DA1E40C14FA54EF,0CDBE4C0F8A045159339D1C8BE8665FA,31E5C20B933D433CA8F509F7AED9E68A,5E7DC598214E4BBAA09CDB91D644FABD,78A5819F71E2414EB48C5984AB005ABB,988240C797D548A79A78B1AD4CDED7B3,3FCE6B2163FC4F5983BF00A13E6B047E', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0 and [Part].is_current = '1' order by [Part].id offset 100 rows fetch next 100 rows only"
        , writer.ToString());
    }

    [TestMethod()]
    public void Aml2Sql_WhereClause()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' queryType='Latest' queryDate='2017-05-11T17:37:00'>
  <is_active_rev>1</is_active_rev>
  <keyed_name condition='like'>999-*</keyed_name>
</Item>").AssertItem();
      var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.None });
      item.ToAml(writer);
      writer.Flush();
      Assert.AreEqual("[Part].is_active_rev = 1 and [Part].keyed_name like N'999-%'"
        , writer.ToString(AmlSqlRenderOption.WhereClause));
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
      var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.None });
      item.ToAml(writer);
      writer.Flush();
      Assert.AreEqual("[Can_Add].source_id = '5698BACD2A7A45D6AC3FA60EAB3E6566' and [Can_Add].can_add = 1 and [Can_Add].related_id = 'A73B655731924CD0B027E4F4D5FCC0A9' and [Can_Add].sort_order = 128"
        , writer.ToString(AmlSqlRenderOption.WhereClause | AmlSqlRenderOption.IgnoreQueryType));
    }

    [TestMethod()]
    public void Aml2Sql_OrderBy()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' orderBy='item_number'>
  <keyed_name condition='like'>999-*</keyed_name>
</Item>").AssertItem();
      var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction });
      item.ToAml(writer);
      writer.Flush();
      Assert.AreEqual("select [Part].* from innovator.[Part] where [Part].keyed_name like N'999-%' and ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', [Part].permission_id, [Part].created_by_id, [Part].managed_by_id, [Part].owned_by_id, [Part].team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9,1942894C56164DF4AB400434FF5EFE3B,56A96DA9E981481688563E2D14D5D878,F18B4A8C523A4B3F88CFFEE3BC68F5D4,2618D6F5A90949BAA7E920D1B04C7EE1,43CC1A53BECA4364A22C136E98E185C7,5A8E8A27AA5A47238BD96AC60A6562F3,5E47C25D3FE6464688442ABE64478537,08C52237B1124FC4BB5D4E48F660C8E2,22DF5704DCA14D86B79BFDFF3CA030B9,2C5CF38CB45C4E1F848EA6DF969364EF,31387021B15E42548FC818E893A19D08,344632A16E8949CCA5CAD0A013D50655,3A0D61E9629141C8934B948C596B2E42,3A976285BB9A4D05AE1878004A33D440,484451EB7B524ECD92FFA372A4675E38,4CAAF215654F43098B415EC0EAB1829A,4CD258EF03834E7FAC9CD76876777EBC,5E12CD824411477AA56D9F539FA295EB,600B965EA3DB41D0B7F9D9D7BDD923EC,62E1A4258C764C5E814DDA3B4209A34F,66022731E81D4040A155F13CD083826A,66F68F2DC7D3410699EA302411608618,685069C4626B406BA5EEFBD87CCABBAC,7C63ED1CFD0A4E0DAAD27AAB0C90A58D,7F50E4A530A84C22935ED7E2231198E1,82F4AA3DD22648288D63938D648CD814,84D2CEC2D1E247DC9D2D28B78D1DF591,A11E92826CA44BEEA4A84E494D13F0B8,A4AFDB4EBD2A4130A4D0616E2E63F552,AD55DC9C04274E67A2C4C45AADC9D059,B3DC39013C13458F9D3A479046B25266,BFE82C27CAAB4667B51EEC64D818BEC8,C02E1CEE4C2B492496EAEC1C19F814B0,C3DB96EFB5EC47AE816CE16AE3E79959,C5C35BB6D73145DB8C88084BEA96BB1D,C6D220539A8D4458B27CA579B637967F,C8FE09099D8D4C25BED39BBFE3BF0476,D78B07BAF283495E9540115C00AD9CED,DE6E87399974415F948D4468C07C5134,E42675D6FC354C86880230D5DA79E4CD,FFB8C4C174054BD3AA17A00023AD49BC,25EAA8CA05254E1DB191F984D994751D,5D58B17DB5094950B93AE13202E46C5E,6A2F0C9740B74625B744E5FCE43879F6,7FA8E5852B7F4FE1817C24A11798A784,B32BD81D1AD04207BF1E61E39A4E0E13,FA8573065D534FBB8DA1E40C14FA54EF,0CDBE4C0F8A045159339D1C8BE8665FA,31E5C20B933D433CA8F509F7AED9E68A,5E7DC598214E4BBAA09CDB91D644FABD,78A5819F71E2414EB48C5984AB005ABB,988240C797D548A79A78B1AD4CDED7B3,3FCE6B2163FC4F5983BF00A13E6B047E', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0 and [Part].is_current = '1' order by [Part].item_number"
        , writer.ToString());
    }

    [TestMethod()]
    public void Aml2Sql_Count()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' queryType='Latest' queryDate='2017-05-11T17:37:00'>
  <is_active_rev>1</is_active_rev>
  <keyed_name condition='like'>999-*</keyed_name>
  <owned_by_id><Item type='Identity' action='get'><keyed_name condition='like'>*super*</keyed_name></Item></owned_by_id>
</Item>").AssertItem();
      var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction });
      item.ToAml(writer);
      writer.Flush();
      Assert.AreEqual("select isnull(sum(cnt), 0) count from (select [Part].permission_id, [Part].created_by_id, [Part].managed_by_id, [Part].owned_by_id, [Part].team_id, count(*) cnt from innovator.[Part] left join innovator.[Identity] on [Identity].id = [Part].owned_by_id where [Part].is_active_rev = 1 and [Part].keyed_name like N'999-%' and ([Identity].keyed_name like N'%super%') group by [Part].permission_id, [Part].created_by_id, [Part].managed_by_id, [Part].owned_by_id, [Part].team_id) perm where ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', perm.permission_id, perm.created_by_id, perm.managed_by_id, perm.owned_by_id, perm.team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9,1942894C56164DF4AB400434FF5EFE3B,56A96DA9E981481688563E2D14D5D878,F18B4A8C523A4B3F88CFFEE3BC68F5D4,2618D6F5A90949BAA7E920D1B04C7EE1,43CC1A53BECA4364A22C136E98E185C7,5A8E8A27AA5A47238BD96AC60A6562F3,5E47C25D3FE6464688442ABE64478537,08C52237B1124FC4BB5D4E48F660C8E2,22DF5704DCA14D86B79BFDFF3CA030B9,2C5CF38CB45C4E1F848EA6DF969364EF,31387021B15E42548FC818E893A19D08,344632A16E8949CCA5CAD0A013D50655,3A0D61E9629141C8934B948C596B2E42,3A976285BB9A4D05AE1878004A33D440,484451EB7B524ECD92FFA372A4675E38,4CAAF215654F43098B415EC0EAB1829A,4CD258EF03834E7FAC9CD76876777EBC,5E12CD824411477AA56D9F539FA295EB,600B965EA3DB41D0B7F9D9D7BDD923EC,62E1A4258C764C5E814DDA3B4209A34F,66022731E81D4040A155F13CD083826A,66F68F2DC7D3410699EA302411608618,685069C4626B406BA5EEFBD87CCABBAC,7C63ED1CFD0A4E0DAAD27AAB0C90A58D,7F50E4A530A84C22935ED7E2231198E1,82F4AA3DD22648288D63938D648CD814,84D2CEC2D1E247DC9D2D28B78D1DF591,A11E92826CA44BEEA4A84E494D13F0B8,A4AFDB4EBD2A4130A4D0616E2E63F552,AD55DC9C04274E67A2C4C45AADC9D059,B3DC39013C13458F9D3A479046B25266,BFE82C27CAAB4667B51EEC64D818BEC8,C02E1CEE4C2B492496EAEC1C19F814B0,C3DB96EFB5EC47AE816CE16AE3E79959,C5C35BB6D73145DB8C88084BEA96BB1D,C6D220539A8D4458B27CA579B637967F,C8FE09099D8D4C25BED39BBFE3BF0476,D78B07BAF283495E9540115C00AD9CED,DE6E87399974415F948D4468C07C5134,E42675D6FC354C86880230D5DA79E4CD,FFB8C4C174054BD3AA17A00023AD49BC,25EAA8CA05254E1DB191F984D994751D,5D58B17DB5094950B93AE13202E46C5E,6A2F0C9740B74625B744E5FCE43879F6,7FA8E5852B7F4FE1817C24A11798A784,B32BD81D1AD04207BF1E61E39A4E0E13,FA8573065D534FBB8DA1E40C14FA54EF,0CDBE4C0F8A045159339D1C8BE8665FA,31E5C20B933D433CA8F509F7AED9E68A,5E7DC598214E4BBAA09CDB91D644FABD,78A5819F71E2414EB48C5984AB005ABB,988240C797D548A79A78B1AD4CDED7B3,3FCE6B2163FC4F5983BF00A13E6B047E', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0"
        , writer.ToString(AmlSqlRenderOption.CountQuery));
    }


    [TestMethod()]
    public void Aml2Sql_Offset()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' offsetId='C0A0F17A9E3346D380ED015B1FD1F2A7' orderBy='item_number,generation,major_rev,id'>
  <state condition='ne'>Obsolete</state>
  <is_in_service condition='ne'>1</is_in_service>
</Item>").AssertItem();
      var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.LegacyFunction });
      item.ToAml(writer);
      writer.Flush();
      Assert.AreEqual("select isnull(sum(cnt), 0) offset from (select [Part].permission_id, [Part].created_by_id, [Part].managed_by_id, [Part].owned_by_id, [Part].team_id, count(*) cnt from innovator.[Part] inner join innovator.[Part] as offset on offset.id = 'C0A0F17A9E3346D380ED015B1FD1F2A7' and (([Part].item_number < offset.item_number) or ([Part].item_number = offset.item_number and [Part].generation < offset.generation) or ([Part].item_number = offset.item_number and [Part].generation = offset.generation and [Part].major_rev < offset.major_rev) or ([Part].item_number = offset.item_number and [Part].generation = offset.generation and [Part].major_rev = offset.major_rev and [Part].id < offset.id)) where [Part].state <> 'Obsolete' and [Part].is_in_service <> 1 and [Part].is_current = '1' group by [Part].permission_id, [Part].created_by_id, [Part].managed_by_id, [Part].owned_by_id, [Part].team_id) perm where ( SELECT p FROM innovator.[GetDiscoverPermissions] ('can_get', perm.permission_id, perm.created_by_id, perm.managed_by_id, perm.owned_by_id, perm.team_id, 'F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9,1942894C56164DF4AB400434FF5EFE3B,56A96DA9E981481688563E2D14D5D878,F18B4A8C523A4B3F88CFFEE3BC68F5D4,2618D6F5A90949BAA7E920D1B04C7EE1,43CC1A53BECA4364A22C136E98E185C7,5A8E8A27AA5A47238BD96AC60A6562F3,5E47C25D3FE6464688442ABE64478537,08C52237B1124FC4BB5D4E48F660C8E2,22DF5704DCA14D86B79BFDFF3CA030B9,2C5CF38CB45C4E1F848EA6DF969364EF,31387021B15E42548FC818E893A19D08,344632A16E8949CCA5CAD0A013D50655,3A0D61E9629141C8934B948C596B2E42,3A976285BB9A4D05AE1878004A33D440,484451EB7B524ECD92FFA372A4675E38,4CAAF215654F43098B415EC0EAB1829A,4CD258EF03834E7FAC9CD76876777EBC,5E12CD824411477AA56D9F539FA295EB,600B965EA3DB41D0B7F9D9D7BDD923EC,62E1A4258C764C5E814DDA3B4209A34F,66022731E81D4040A155F13CD083826A,66F68F2DC7D3410699EA302411608618,685069C4626B406BA5EEFBD87CCABBAC,7C63ED1CFD0A4E0DAAD27AAB0C90A58D,7F50E4A530A84C22935ED7E2231198E1,82F4AA3DD22648288D63938D648CD814,84D2CEC2D1E247DC9D2D28B78D1DF591,A11E92826CA44BEEA4A84E494D13F0B8,A4AFDB4EBD2A4130A4D0616E2E63F552,AD55DC9C04274E67A2C4C45AADC9D059,B3DC39013C13458F9D3A479046B25266,BFE82C27CAAB4667B51EEC64D818BEC8,C02E1CEE4C2B492496EAEC1C19F814B0,C3DB96EFB5EC47AE816CE16AE3E79959,C5C35BB6D73145DB8C88084BEA96BB1D,C6D220539A8D4458B27CA579B637967F,C8FE09099D8D4C25BED39BBFE3BF0476,D78B07BAF283495E9540115C00AD9CED,DE6E87399974415F948D4468C07C5134,E42675D6FC354C86880230D5DA79E4CD,FFB8C4C174054BD3AA17A00023AD49BC,25EAA8CA05254E1DB191F984D994751D,5D58B17DB5094950B93AE13202E46C5E,6A2F0C9740B74625B744E5FCE43879F6,7FA8E5852B7F4FE1817C24A11798A784,B32BD81D1AD04207BF1E61E39A4E0E13,FA8573065D534FBB8DA1E40C14FA54EF,0CDBE4C0F8A045159339D1C8BE8665FA,31E5C20B933D433CA8F509F7AED9E68A,5E7DC598214E4BBAA09CDB91D644FABD,78A5819F71E2414EB48C5984AB005ABB,988240C797D548A79A78B1AD4CDED7B3,3FCE6B2163FC4F5983BF00A13E6B047E', null, '2D246C5838644C1C8FD34F8D2796E327', '8FE5430B42014D94AE83246F299D9CC4', '9200A800443E4A5AAA80D0BCE5760307', '538B300BB2A347F396C436E9EEE1976C' ) ) > 0"
        , writer.ToString(AmlSqlRenderOption.OffsetQuery));
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
      var writer = new AmlSqlWriter(new ConnectedAmlSqlWriterSettings(new TestConnection()) { PermissionOption = AmlSqlPermissionOption.None });
      item.ToAml(writer);
      writer.Flush();
      Assert.AreEqual("[Thing].created_on between '2018-02-25T05:00:00' and '2018-03-04T04:59:59' and ([Thing].state like N'%Canceled%' or [Thing].state like N'%Closed%' or [Thing].state like N'%Closed : Conversion%' or [Thing].state like N'%Review%' or [Thing].state like N'%In Work%') and ([Thing].classification like N'Suspect Part' or [Thing].classification like N'Suspect Part/Customer' or [Thing].classification like N'Suspect Part/Incoming' or [Thing].classification like N'Suspect Part/Production' or [Thing].classification like N'Suspect Part/%' or [Thing].classification like N'Suspect Part/Customer/%' or [Thing].classification like N'Suspect Part/Incoming/%' or [Thing].classification like N'Suspect Part/Production/%') and (([Identity].keyed_name like N'%john smith%' or [Identity].keyed_name like N'%jane doe%')) and [Thing].is_current = '1'"
        , writer.ToString(AmlSqlRenderOption.WhereClause));
    }
  }
}
