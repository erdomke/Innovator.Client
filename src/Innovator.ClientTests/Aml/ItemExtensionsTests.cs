using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class ItemExtensionsTests
  {
    [TestMethod()]
    public void ToXmlTest()
    {
      var aml = ElementFactory.Local;
      var res = aml.Result();
      Assert.AreEqual(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <Result />
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>", res.ToXml().ToString());

      res = aml.Result("Value");
      Assert.AreEqual(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <Result>Value</Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>", res.ToXml().ToString());

      res = aml.Result(aml.Item(aml.Type("Part"), aml.Id("1234")), aml.Item(aml.Type("Part"), aml.Id("4567")));
      Assert.AreEqual(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <Result>
      <Item type=""Part"" id=""1234"" />
      <Item type=""Part"" id=""4567"" />
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>", res.ToXml().ToString());
    }

    [TestMethod()]
    public void LazyMap_ItemDoesNotExist()
    {
      var conn = new TestConnection();
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Result><Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='BF3BF6C4795F431D880E7AF4D68D7A9C'>
  <created_by_id keyed_name='First Last' type='User'>8227040ABF0A46A8AF06C18ABD3967B3</created_by_id>
  <id keyed_name='Some Company' type='Company'>BF3BF6C4795F431D880E7AF4D68D7A9C</id>
  <permission_id keyed_name='Company' type='Permission'>A8FC3EC44ED0462B9A32D4564FAC0AD8</permission_id>
</Item></Result>").AssertItem();
      var result = item.LazyMap(conn, i => new
      {
        FirstName = i.CreatedById().AsItem().Property("first_name").Value,
        PermName = i.PermissionId().AsItem().Property("name").Value,
        KeyedName = i.Property("id").KeyedName().Value,
        Empty = i.OwnedById().Value
      });
      Assert.AreEqual("First", result.FirstName);
      Assert.AreEqual("Company", result.PermName);
      Assert.AreEqual("Some Company", result.KeyedName);
      Assert.AreEqual(null, result.Empty);
    }

    [TestMethod()]
    public void LazyMap_PropertyDoesNotExist()
    {
      var conn = new TestConnection();
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Result><Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='BF3BF6C4795F431D880E7AF4D68D7A9C'>
  <created_by_id keyed_name='First Last' type='User'>8227040ABF0A46A8AF06C18ABD3967B3</created_by_id>
  <id keyed_name='Some Company' type='Company'>BF3BF6C4795F431D880E7AF4D68D7A9C</id>
</Item></Result>").AssertItem();
      var result = item.LazyMap(conn, i => new
      {
        FirstName = i.CreatedById().AsItem().Property("first_name").Value,
        PermName = i.PermissionId().AsItem().Property("name").Value,
        KeyedName = i.Property("id").KeyedName().Value,
        Empty = i.OwnedById().Value
      });
      Assert.AreEqual("First", result.FirstName);
      Assert.AreEqual(null, result.PermName);
      Assert.AreEqual("Some Company", result.KeyedName);
      Assert.AreEqual(null, result.Empty);
    }

    [TestMethod()]
    public void LazyMap_PropertyDoesNotExistInDatabase()
    {
      var conn = new TestConnection();
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Result><Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='0E086FFA6C4646F6939B74C43D094182' fake_attr='This is a fake attribute for passing data'>
  <created_by_id keyed_name='First Last' type='User'>
    <Item type='User' typeId='45E899CD2859442982EB22BB2DF683E5' id='8227040ABF0A46A8AF06C18ABD3967B3'>
      <id keyed_name='First Last' type='User'>8227040ABF0A46A8AF06C18ABD3967B3</id>
      <first_name>First</first_name>
      <itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype>
    </Item>
  </created_by_id>
  <permission_id keyed_name='Company' type='Permission'>
    <Item type='Permission' typeId='C6A89FDE1294451497801DF78341B473' id='A8FC3EC44ED0462B9A32D4564FAC0AD8'>
      <id keyed_name='Company' type='Permission'>A8FC3EC44ED0462B9A32D4564FAC0AD8</id>
      <name>Company</name>
    </Item>
  </permission_id>
  <fake_prop>This is a fake prop for passing data</fake_prop>
</Item></Result>").AssertItem();
      var result = item.LazyMap(conn, i => new
      {
        FakeProp = i.Property("fake_prop").Value,
        FakeAttr = i.Attribute("fake_attr").Value,
        Empty = i.OwnedById().Value
      });
      Assert.AreEqual("This is a fake prop for passing data", result.FakeProp);
      Assert.AreEqual("This is a fake attribute for passing data", result.FakeAttr);
    }

    [TestMethod()]
    public void LazyMap_IncomingItemNotOverwrittenByDatabase()
    {
      var conn = new TestConnection();
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Result><Item action='update' type='Company' id='1470B001142748A5BB39CECB72CD83C8'>
  <permission_id>
    <Item type='Permission' typeId='C6A89FDE1294451497801DF78341B473' id='5B05BBB4945845248586C90BE83C7BDC'>
      <name>Restricted Company</name>
    </Item>
  </permission_id>
</Item></Result>").AssertItem();
      var result = item.LazyMap(conn, i => new
      {
        PermissionName = i.PermissionId().AsItem().Property("name").Value,
        TypeId = i.TypeId().Value,
        EmptyFirstName = i.CreatedById().AsItem().Property("first_name").Value
      });
      Assert.AreEqual("Restricted Company", result.PermissionName);
      Assert.AreEqual("3E71E373FC2940B288760C915120AABE", result.TypeId);
      Assert.AreEqual("First", result.EmptyFirstName);
    }

    [TestMethod()]
    public void LazyMap_NestedItem_RetrieveExtraData()
    {
      var conn = new TestConnection();
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Result><Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='0E086FFA6C4646F6939B74C43D094182'>
  <permission_id keyed_name='Company' type='Permission'>A8FC3EC44ED0462B9A32D4564FAC0AD8</permission_id>
</Item></Result>").AssertItem();
      var result = item.LazyMap(conn, i => new
      {
        PermissionName = i.Property("permission_id").AsItem().Property("name").Value
      });
      Assert.AreEqual("Company", result.PermissionName);
    }

    [TestMethod()]
    public void LazyMap_ChangingProperty_NestedItem()
    {
      var conn = new TestConnection();
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Result><Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='0E086FFA6C4646F6939B74C43D094182'>
  <permission_id>F8BAD68CCADB43DF901FDCA693A22705</permission_id>
  <owned_by_id>384C0326D719419F897C34163B8C5B2E</owned_by_id>
</Item></Result>").AssertItem();
      var result = item.LazyMap(conn, i => new
      {
        PermissionName = i.Property("permission_id").AsItem().Property("name").Value,
        OwnedById = i.OwnedById().Value
      });
      Assert.AreEqual("Vault", result.PermissionName);
      Assert.AreEqual("384C0326D719419F897C34163B8C5B2E", result.OwnedById);
    }

    [TestMethod()]
    public void LazyMap_AddItemViaProperty()
    {
      var conn = new TestConnection();
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Result><Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='0E086FFA6C4646F6939B74C43D094182'>
  <permission_id>
    <Item type='Permission' action='add'>
      <name>New Company</name>
    </Item>
  </permission_id>
</Item></Result>").AssertItem();
      var result = item.LazyMap(conn, i => new
      {
        PermissionItemTypeName = i.Property("permission_id").AsItem().TypeName(),
        PermissionName = i.Property("permission_id").AsItem().Property("name").Value
      });
      Assert.AreEqual("New Company", result.PermissionName);
      Assert.AreEqual("Permission", result.PermissionItemTypeName);
    }

    [TestMethod()]
    public void RenderingComposedItemsTest()
    {
      var conn = new TestConnection();
      var company = conn.ItemById("Company", "0E086FFA6C4646F6939B74C43D094182").Clone();
      var user = conn.ItemById("User", "8227040ABF0A46A8AF06C18ABD3967B3");
      company.ModifiedById().Set(user);
      var aml = company.ToAml();  // Attempt to trigger an exception
      Assert.AreEqual(company, company.ModifiedById().Parent);
    }

    [TestMethod]
    public void Descendants()
    {
      var aml = @"<Item type='FMEA Cause' typeId='19FB0D3D70404CCCB945562C10E7250F' id='B85977D20CD74A5A8EEE24647D6D4DD2'>
  <causes>Wrong cutting program</causes>
  <id keyed_name='B85977D20CD74A5A8EEE24647D6D4DD2' type='FMEA Cause'>B85977D20CD74A5A8EEE24647D6D4DD2</id>
  <occurrence>2</occurrence>
  <Relationships>
    <Item type='FMEA Cause FMEA Control' typeId='1BF911ED2E954E86B750301BE758E5B1' id='522D4660AA62450086DEAB19E96F540B'>
      <id keyed_name='522D4660AA62450086DEAB19E96F540B' type='FMEA Cause FMEA Control'>522D4660AA62450086DEAB19E96F540B</id>
      <related_id keyed_name='8599C8624DAE4FE2A40DC5DDA4335D8E' type='FMEA Control'>
        <Item type='FMEA Control' typeId='7F4031034940456A9C8FD0B920733B07' id='8599C8624DAE4FE2A40DC5DDA4335D8E'>
          <id keyed_name='8599C8624DAE4FE2A40DC5DDA4335D8E' type='FMEA Control'>8599C8624DAE4FE2A40DC5DDA4335D8E</id>
        </Item>
      </related_id>
      <sort_order>1073741823</sort_order>
      <source_id keyed_name='B85977D20CD74A5A8EEE24647D6D4DD2' type='FMEA Cause'>B85977D20CD74A5A8EEE24647D6D4DD2</source_id>
    </Item>
  </Relationships>
</Item>";
      var item = ElementFactory.Local.FromXml(aml).AssertItem();
      var all = ((IElement)item).Descendants().ToArray();
      Assert.AreEqual(11, all.Length);
      var childItems = ((IElement)item).Descendants().OfType<IItem>().ToArray();
      Assert.AreEqual(2, childItems.Length);
      Assert.AreEqual("FMEA Cause FMEA Control", childItems[0].TypeName());
      Assert.AreEqual("FMEA Control", childItems[1].TypeName());
    }

#if XMLLEGACY
    [TestMethod()]
    public void XPathSelectElements()
    {
      var aml = @"<Item type='FMEA Cause' typeId='19FB0D3D70404CCCB945562C10E7250F' id='B85977D20CD74A5A8EEE24647D6D4DD2'><causes>Wrong cutting program</causes><id keyed_name='B85977D20CD74A5A8EEE24647D6D4DD2' type='FMEA Cause'>B85977D20CD74A5A8EEE24647D6D4DD2</id><occurrence>2</occurrence><Relationships><Item type='FMEA Cause FMEA Control' typeId='1BF911ED2E954E86B750301BE758E5B1' id='522D4660AA62450086DEAB19E96F540B'><id keyed_name='522D4660AA62450086DEAB19E96F540B' type='FMEA Cause FMEA Control'>522D4660AA62450086DEAB19E96F540B</id><related_id keyed_name='8599C8624DAE4FE2A40DC5DDA4335D8E' type='FMEA Control'><Item type='FMEA Control' typeId='7F4031034940456A9C8FD0B920733B07' id='8599C8624DAE4FE2A40DC5DDA4335D8E'><id keyed_name='8599C8624DAE4FE2A40DC5DDA4335D8E' type='FMEA Control'>8599C8624DAE4FE2A40DC5DDA4335D8E</id></Item></related_id><sort_order>1073741823</sort_order><source_id keyed_name='B85977D20CD74A5A8EEE24647D6D4DD2' type='FMEA Cause'>B85977D20CD74A5A8EEE24647D6D4DD2</source_id></Item></Relationships></Item>";
      var item = ElementFactory.Local.FromXml(aml).AssertItem();
      var elems = item.XPath().SelectElements("//Item");
      var items = elems.OfType<IReadOnlyItem>().ToArray();
      Assert.AreEqual(2, items.Length);
      Assert.AreEqual(1, item.XPath().SelectElements("//Item[@type='FMEA Control']").Count());
      Assert.AreEqual(0, item.XPath().SelectElements("//Item[@type='FMEA Action']").Count());
    }
#endif
  }
}
