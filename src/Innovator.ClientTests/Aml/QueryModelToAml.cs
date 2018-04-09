using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Tests
{
  [TestClass]
  public class QueryModelToAml
  {
    [TestMethod()]
    public void AmlRoundTrip_Basic()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' queryType='Latest' queryDate='2017-05-11T17:37:00'>
  <is_active_rev>1</is_active_rev>
  <keyed_name condition='like'>999-*</keyed_name>
  <owned_by_id><Item type='Identity' action='get'><keyed_name condition='like'>*super*</keyed_name></Item></owned_by_id>
</Item>").AssertItem();
      var aml = item.ToQueryItem().ToAml();
      Assert.AreEqual(@"<Item type=""Part"" action=""get"" queryDate=""2017-05-11T17:37:00"" queryType=""Latest""><is_active_rev>1</is_active_rev><keyed_name condition=""like"">999-%</keyed_name><owned_by_id><Item type=""Identity"" action=""get""><keyed_name condition=""like"">%super%</keyed_name></Item></owned_by_id></Item>"
        , aml);
    }

    [TestMethod()]
    public void AmlRoundTrip_NoPermissions()
    {
      var item = ElementFactory.Local.FromXml(@"<Item action='get' type='Part' select='config_id'><id condition='in'>'71B2D9633CA14B1486E1FE473C7CF950','C0A0F17A9E3346D380ED015B1FD1F2A7','C5F56BF14FB64AB3BD0AF6AEE67AF00A'</id></Item>").AssertItem();

      var aml = item.ToQueryItem().ToAml();
      Assert.AreEqual(@"<Item type=""Part"" action=""get"" select=""config_id"" idlist=""71B2D9633CA14B1486E1FE473C7CF950,C0A0F17A9E3346D380ED015B1FD1F2A7,C5F56BF14FB64AB3BD0AF6AEE67AF00A"" />"
        , aml);
    }

    [TestMethod()]
    public void AmlRoundTrip_MaxRecords()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' maxRecords='100' queryType='Latest' queryDate='2017-05-11T17:37:00' select='id'>
  <is_active_rev>1</is_active_rev>
</Item>").AssertItem();

      var aml = item.ToQueryItem().ToAml();
      Assert.AreEqual(@"<Item type=""Part"" maxRecords=""100"" action=""get"" queryDate=""2017-05-11T17:37:00"" queryType=""Latest"" select=""id""><is_active_rev>1</is_active_rev></Item>"
        , aml);
    }

    [TestMethod()]
    public void AmlRoundTrip_Paging()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' page='2' pagesize='100' select='id'>
</Item>").AssertItem();
      var aml = item.ToQueryItem().ToAml();
      Assert.AreEqual(@"<Item type=""Part"" page=""2"" pagesize=""100"" action=""get"" select=""id"" />"
        , aml);
    }

    [TestMethod()]
    public void AmlRoundTrip_WhereClause2()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Can Add' typeId='3A65F41FF1FC42518A702FDA164AF420' action='get'>
  <source_id keyed_name='Located' type='ItemType' name='Located'>5698BACD2A7A45D6AC3FA60EAB3E6566</source_id>
  <can_add>1</can_add>
  <related_id keyed_name='World' type='Identity'>A73B655731924CD0B027E4F4D5FCC0A9</related_id>
  <sort_order>128</sort_order>
</Item>").AssertItem();
      var aml = item.ToQueryItem().ToAml();
      Assert.AreEqual(@"<Item type=""Can Add"" action=""get""><source_id>5698BACD2A7A45D6AC3FA60EAB3E6566</source_id><can_add>1</can_add><related_id>A73B655731924CD0B027E4F4D5FCC0A9</related_id><sort_order>128</sort_order></Item>"
        , aml);
    }

    [TestMethod()]
    public void AmlRoundTrip_OrderBy()
    {
      var item = ElementFactory.Local.FromXml(@"<Item type='Part' action='get' orderBy='item_number'>
  <keyed_name condition='like'>999-*</keyed_name>
</Item>").AssertItem();
      var aml = item.ToQueryItem().ToAml();
      Assert.AreEqual(@"<Item type=""Part"" action=""get"" orderBy=""item_number""><keyed_name condition=""like"">999-%</keyed_name></Item>"
        , aml);
    }

    [TestMethod]
    public void AmlRoundTrip_Complex()
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
      var aml = item.ToQueryItem().ToAml();
      Assert.AreEqual(@"<Item type=""Thing"" action=""get""><created_on condition=""between"">'2018-02-25T00:00:00' and '2018-03-03T23:59:59'</created_on><or><state condition=""like"">%Canceled%</state><state condition=""like"">%Closed%</state><state condition=""like"">%Closed : Conversion%</state><state condition=""like"">%Review%</state><state condition=""like"">%In Work%</state></or><or><classification condition=""like"">Suspect Part</classification><classification condition=""like"">Suspect Part/Customer</classification><classification condition=""like"">Suspect Part/Incoming</classification><classification condition=""like"">Suspect Part/Production</classification><classification condition=""like"">Suspect Part/%</classification><classification condition=""like"">Suspect Part/Customer/%</classification><classification condition=""like"">Suspect Part/Incoming/%</classification><classification condition=""like"">Suspect Part/Production/%</classification></or><owned_by_id><Item type=""Identity"" action=""get""><or><keyed_name condition=""like"">%john smith%</keyed_name><keyed_name condition=""like"">%jane doe%</keyed_name></or></Item></owned_by_id></Item>"
        , aml);
    }
  }
}
