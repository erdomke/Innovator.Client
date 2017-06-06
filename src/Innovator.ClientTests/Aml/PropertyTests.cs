using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class PropertyTests
  {
    [TestMethod()]
    public void IsCurrentPropertyAccess()
    {
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Item type='Part' typeId='4F1AC04A2B484F3ABA4E20DB63808A88' id='A8F945913D58433A98D4E8DE57D4B008'>
  <classification>Electronic/Transistor/Other</classification>
  <current_state name='Implemented' keyed_name='Implemented' type='Life Cycle State'>1FA687F2F75F4BCCAF978684B3E6B482</current_state>
  <effective_date>2010-08-16T15:41:31</effective_date>
  <generation>2</generation>
  <id keyed_name='asdf' type='Part'>A8F945913D58433A98D4E8DE57D4B008</id>
  <is_active_rev>1</is_active_rev>
  <is_current>1</is_current>
  <is_released>1</is_released>
  <keyed_name>asdf</keyed_name>
  <major_rev>DAB</major_rev>
  <permission_id keyed_name='Released Part' type='Permission'>95475AE006E7415794BDC93808DC04D2</permission_id>
  <plm_only>0</plm_only>
  <state>Implemented</state>
  <state_image>../images/customer/images/Part_Implemented16.png</state_image>
  <team_id keyed_name='Part: General' type='Team'>EC30D51403344F689242E451481286D8</team_id>
  <unit>EA</unit>
  <itemtype>4F1AC04A2B484F3ABA4E20DB63808A88</itemtype>
</Item>", "query", null).AssertItem();

      Assert.AreEqual(true, item.IsCurrent().AsBoolean(false));
    }

    [TestMethod()]
    public void IsNullPropertyAsString()
    {
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Item type='Life Cycle State' typeId='5EFB53D35BAE468B851CD388BEA46B30' id='E7A383494C724B338518A4DD1EB867FA'>
  <id keyed_name='Superseded' type='Life Cycle State'>E7A383494C724B338518A4DD1EB867FA</id>
  <keyed_name>Superseded</keyed_name>
  <label is_null='1' />
  <name>Superseded</name>
</Item>").AssertItem();
      Assert.AreEqual("Superseded", item.Property("name").AsString("stuff"));
      Assert.AreEqual("Superseded", item.Property("label").AsString(item.Property("name").Value));
    }

    [TestMethod()]
    public void InvalidDateTime()
    {
      var context = new ServerContext("Eastern Standard Time");
      var aml = new ElementFactory(context);

      var date = DateTime.SpecifyKind(DateTime.Parse("3/12/2017 2:38:04 AM"), DateTimeKind.Unspecified);
      var item = aml.Item(aml.Property("invalid_date", date));
      var str = item.ToAml();
      Assert.AreEqual("<Item><invalid_date>2017-03-12T02:38:04</invalid_date></Item>", str);

      var result = aml.FromXml(str);
      var newDate = result.AssertItem().Property("invalid_date").AsDateTime();
      Assert.AreEqual(date, newDate);
    }

    [TestMethod()]
    public void TimeZoneTests()
    {
      var zonesToIgnore = new HashSet<string>(new[] { "UTC", "Mid-Atlantic Standard Time", "Morocco Standard Time"
        , "Turkey Standard Time", "Omsk Standard Time", "Magallanes Standard Time"
        , "Saratov Standard Time", "W. Mongolia Standard Time", "Ulaanbaatar Standard Time", "UTC+13"
        , "Tonga Standard Time"});

      foreach (var zone in TimeZoneInfo.GetSystemTimeZones().Where(t => !zonesToIgnore.Contains(t.Id)))
      {
        var summer = new DateTime(2017, 7, 1, 12, 0, 0, DateTimeKind.Unspecified);
        var winter = new DateTime(2017, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);
        var custZone = TimeZoneData.ById(zone.Id);

        Assert.AreEqual(TimeZoneInfo.ConvertTime(summer, TimeZoneInfo.Utc, zone)
          , TimeZoneData.ConvertTime(summer, TimeZoneData.Utc, custZone));
        Assert.AreEqual(TimeZoneInfo.ConvertTime(summer, zone, TimeZoneInfo.Utc)
          , TimeZoneData.ConvertTime(summer, custZone, TimeZoneData.Utc));
        Assert.AreEqual(TimeZoneInfo.ConvertTime(winter, TimeZoneInfo.Utc, zone)
          , TimeZoneData.ConvertTime(winter, TimeZoneData.Utc, custZone));
        Assert.AreEqual(TimeZoneInfo.ConvertTime(winter, zone, TimeZoneInfo.Utc)
          , TimeZoneData.ConvertTime(winter, custZone, TimeZoneData.Utc));

        Assert.AreEqual(zone.GetUtcOffset(summer), custZone.GetUtcOffset(summer));
        Assert.AreEqual(zone.GetUtcOffset(winter), custZone.GetUtcOffset(winter));

        Assert.AreEqual(TimeZoneInfo.ConvertTime(new DateTimeOffset(summer, TimeSpan.FromHours(3)), zone)
          , TimeZoneData.ConvertTime(new DateTimeOffset(summer, TimeSpan.FromHours(3)), custZone));
        Assert.AreEqual(TimeZoneInfo.ConvertTime(new DateTimeOffset(winter, TimeSpan.FromHours(3)), zone)
          , TimeZoneData.ConvertTime(new DateTimeOffset(winter, TimeSpan.FromHours(3)), custZone));
        Assert.AreEqual(TimeZoneInfo.ConvertTime(new DateTimeOffset(summer, TimeSpan.FromHours(-7.5)), zone)
          , TimeZoneData.ConvertTime(new DateTimeOffset(summer, TimeSpan.FromHours(-7.5)), custZone));
        Assert.AreEqual(TimeZoneInfo.ConvertTime(new DateTimeOffset(winter, TimeSpan.FromHours(-7.5)), zone)
          , TimeZoneData.ConvertTime(new DateTimeOffset(winter, TimeSpan.FromHours(-7.5)), custZone));
      }
    }

    [TestMethod]
    public void DateTimeOffsetTest()
    {
      var aml = ElementFactory.Utc;
      var item = aml.FromXml(@"<Item><end_date>1/12/2016 8:13:54 PM</end_date></Item>").AssertItem();
      var offset = item.Property("end_date").AsDateTimeOffset();
      Assert.AreEqual(DateTimeOffset.Parse("2016-01-12T20:13:54Z"), offset.Value);

      item = aml.FromXml(@"<Item><end_date>2016-01-12T20:13:54</end_date></Item>").AssertItem();
      offset = item.Property("end_date").AsDateTimeOffset();
      Assert.AreEqual(DateTimeOffset.Parse("2016-01-12T20:13:54Z"), offset.Value);

      aml = ElementFactory.Local;
      item = aml.FromXml(@"<Item><end_date>2016-01-12T20:13:54</end_date></Item>").AssertItem();
      offset = item.Property("end_date").AsDateTimeOffset();
      Assert.AreEqual(DateTimeOffset.Parse("2016-01-12T20:13:54"), offset.Value);

      aml = new ElementFactory(new ServerContext("Central Standard Time"));
      item = aml.FromXml(@"<Item><end_date>2016-01-12T20:13:54</end_date></Item>").AssertItem();
      offset = item.Property("end_date").AsDateTimeOffset();
      Assert.AreEqual(new DateTimeOffset(DateTime.Parse("2016-01-12T20:13:54"), TimeSpan.FromHours(-6)), offset.Value);
    }

    [TestMethod]
    public void PropertyClone()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Property("prop1", aml.Attribute("keyed_name", "some name"), "value1"), aml.Property("prop2", aml.Attribute("keyed_name", "another name"), "value2"));
      var item2 = aml.Item(item.Property("prop1"));
      Assert.AreEqual("some name", item2.Property("prop1").KeyedName().Value);
    }
  }
}
