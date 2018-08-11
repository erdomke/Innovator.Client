#if XMLLEGACY
using Aras.IOM;
#endif
using Innovator.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Innovator.Client.Tests
{
  [TestClass]
  public class IomTests
  {
    private IOM.Item CreateItem(string aml)
    {
      var conn = new TestConnection();
      var inn = new IOM.Innovator(conn);
      return inn.newItemFromAml(aml);
    }

    private void AssertRaisesException(Action action)
    {
      try
      {
        action.Invoke();
        Assert.Fail("Exception expected");
      }
      catch (Exception)
      {
        // Do nothing
      }
    }

    [TestMethod]
    public void ErrorItemTesting()
    {
      var target = CreateItem(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af='http://www.aras.com/InnovatorFault'>
      <faultcode>0</faultcode>
      <faultstring>No items of type Inbox found.</faultstring>
      <detail>Other stuff</detail>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>");

      Assert.AreEqual("0", target.getErrorCode());
      Assert.AreEqual("No items of type Inbox found.", target.getErrorString());
      Assert.AreEqual("Other stuff", target.getErrorDetail());

      Assert.AreEqual(true, target.isEmpty());
      Assert.AreEqual(true, target.isError());
      Assert.AreEqual(false, target.isCollection());
      Assert.AreEqual(false, target.isLogical());
      Assert.AreEqual(0, target.getItemCount());

      AssertRaisesException(() => target.getProperty("something"));
      AssertRaisesException(() => target.getPropertyItem("something"));
      AssertRaisesException(() => target.getRelatedItem());
      AssertRaisesException(() => target.getPropertyAttribute("first", "something"));
      AssertRaisesException(() => target.getAttribute("first", "something"));

      var newItem = target.getInnovator().newError("Some new error");
      Assert.AreEqual("1", newItem.getErrorCode());
      Assert.AreEqual("Some new error", newItem.getErrorString());

      Assert.AreEqual(false, newItem.isEmpty());
      Assert.AreEqual(true, newItem.isError());
      Assert.AreEqual(-1, newItem.getItemCount());
    }

    [TestMethod]
    public void AppendItem_EmptyStart()
    {
      var start = CreateItem("<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><ApplyItemResponse></ApplyItemResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
      var append = CreateItem("<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><ApplyItemResponse><Item type='append'/></ApplyItemResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
      start.appendItem(append);
      Assert.AreEqual("<Item type=\"append\" />", start.ToAml());
    }

    [TestMethod]
    public void AppendItem_BlankResult()
    {
      var start = CreateItem("<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><Result></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>");
      var append = CreateItem("<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><ApplyItemResponse><Item type='append'/></ApplyItemResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
      start.appendItem(append);
      Assert.AreEqual("<Item type=\"append\" />", start.ToAml());
    }

    [TestMethod]
    public void AppendItem_SingleItemNoParent()
    {
      var start = CreateItem("<Item type='start'/>");
      var append = CreateItem("<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><ApplyItemResponse><Item type='append'/></ApplyItemResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
      start.appendItem(append);
#if XMLLEGACY
      Assert.AreEqual("<AML><Item type=\"start\" /><Item type=\"append\" /></AML>", start.ToAml());
#else
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"start\" /><Item type=\"append\" /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", start.ToAml());
#endif
    }

    [TestMethod]
    public void AppendItem_SingleItemWithParent()
    {
      var start = CreateItem("<AML><Item type='start'/></AML>");
      var append = CreateItem("<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><ApplyItemResponse><Item type='append'/></ApplyItemResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
      start.appendItem(append);
#if XMLLEGACY
      Assert.AreEqual("<AML><Item type=\"start\" /><Item type=\"append\" /></AML>", start.ToAml());
#else
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"start\" /><Item type=\"append\" /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", start.ToAml());
#endif
    }

    [TestMethod]
    public void AppendItem_MultipleParents()
    {
      var start = CreateItem("<AML><Item type='start'/><Item type='second'/></AML>");
      var append = CreateItem("<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><ApplyItemResponse><Item type='append'/></ApplyItemResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
      start.appendItem(append);
#if XMLLEGACY
      Assert.AreEqual("<AML><Item type=\"start\" /><Item type=\"second\" /><Item type=\"append\" /></AML>", start.ToAml());
#else
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"start\" /><Item type=\"second\" /><Item type=\"append\" /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", start.ToAml());
#endif
    }

    [TestMethod]
    public void ResultItemTesting()
    {
      var target = CreateItem("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><ApplyItemResponse><Result>1</Result></ApplyItemResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
      Assert.AreEqual("1", target.getResult());
      var newItem = target.getInnovator().newResult("OK");
      Assert.AreEqual("OK", newItem.getResult());

      AssertRaisesException(() => newItem.getProperty("something"));
      AssertRaisesException(() => target.getPropertyItem("something"));
      AssertRaisesException(() => target.getRelatedItem());
      AssertRaisesException(() => target.getPropertyAttribute("first", "something"));
      AssertRaisesException(() => target.getAttribute("first", "something"));

      Assert.AreEqual(false, target.isEmpty());
      Assert.AreEqual(false, target.isError());
      Assert.AreEqual(false, target.isCollection());
      Assert.AreEqual(false, target.isLogical());
      Assert.AreEqual(-1, target.getItemCount());
    }

    [TestMethod]
    public void SimpleItemPropertyAccess()
    {
      var target = CreateItem("<?xml version='1.0' encoding='utf-8'?><Item access_type='can_delete' action='getPermissions' id='5B6025EB25FE4BCEB90111C16C2A26A4' type='Workflow Process' />");
      Assert.AreEqual("can_delete", target.getAttribute("access_type"));
      Assert.AreEqual("getPermissions", target.getAction());
      Assert.AreEqual("getPermissions", target.getAttribute("action"));
      Assert.AreEqual("5B6025EB25FE4BCEB90111C16C2A26A4", target.getID());
      Assert.AreEqual("Workflow Process", target.getType());
      Assert.AreEqual(null, target.getAttribute("another"));
      Assert.AreEqual("DEFAULT", target.getAttribute("another", "DEFAULT"));
      Assert.AreEqual(null, target.getProperty("another"));
      Assert.AreEqual("DEFAULT", target.getProperty("another", "DEFAULT"));

      Assert.AreEqual(false, target.isEmpty());
      Assert.AreEqual(false, target.isError());
      Assert.AreEqual(false, target.isCollection());
      Assert.AreEqual(false, target.isLogical());
      Assert.AreEqual(1, target.getItemCount());
    }

    [TestMethod]
    public void LargerItemPropertyAccess()
    {
      var target = CreateItem(@"<?xml version='1.0' encoding='utf-8'?>
<AML>
  <Item type='Inbox' action='get'>
    <item_id>3962701DDA4F44068F8CE106DCBCF055</item_id>
    <created_by_id>75ABC2E1E41C4AE6B978B86A657CFA07</created_by_id>
    <itemtype>
      <Item type='ItemType' action='get'>
        <name>BOM Review</name>
      </Item>
    </itemtype>
  </Item>
</AML>");
      Assert.AreEqual("Inbox", target.getType());
      Assert.AreEqual("get", target.getAction());
      Assert.AreEqual("3962701DDA4F44068F8CE106DCBCF055", target.getProperty("item_id"));
      Assert.AreEqual("75ABC2E1E41C4AE6B978B86A657CFA07", target.getProperty("created_by_id"));

      var newItem = target.getPropertyItem("itemtype");
      Assert.AreEqual("ItemType", newItem.getType());
      Assert.AreEqual("get", newItem.getAction());
      Assert.AreEqual("BOM Review", newItem.getProperty("name"));

      target.setProperty("new_prop", "stuff");
      target.setPropertyAttribute("item_id", "a", "b");
      newItem.setPropertyCondition("name", "like");
      newItem.setProperty("description", "stuff");
      newItem.setProperty("is_bool", true);
      newItem.setProperty("a_date", new DateTime(2000, 1, 1));

      Assert.IsTrue(XNode.DeepEquals(XElement.Parse(@"<Item type='Inbox' action='get'>
  <item_id a='b'>3962701DDA4F44068F8CE106DCBCF055</item_id>
  <created_by_id>75ABC2E1E41C4AE6B978B86A657CFA07</created_by_id>
  <itemtype>
    <Item type='ItemType' action='get'>
      <name condition='like'>BOM Review</name>
      <description>stuff</description>
      <is_bool>1</is_bool>
      <a_date>2000-01-01T00:00:00</a_date>
    </Item>
  </itemtype>
  <new_prop>stuff</new_prop>
</Item>"), XElement.Parse(target.ToString())));
    }

    [TestMethod]
    public void CreatingItems()
    {
      var target = XElement.Parse(@"<Item type='Method' action='add' id='46D9FDF379AD4B4DA0143C92BBA16720'>
  <name>A new method</name>
  <owned_by_id>
    <Item type='Identity' action='get'>
      <name condition='like'>Admin*</name>
      <test is_null='1' />
    </Item>
  </owned_by_id>
</Item>");
      var inn = new IOM.Innovator(new TestConnection());
      var first = inn.newItem("Method", "add");
      first.removeAttribute("isNew");
      first.removeAttribute("isTemp");
      first.setID("46D9FDF379AD4B4DA0143C92BBA16720");
      first.setProperty("name", "A new method");
      var inner = inn.newItem("Identity", "get");
      inner.removeAttribute("isNew");
      inner.removeAttribute("isTemp");
      inner.setProperty("name", "Admin*");
      inner.setPropertyCondition("name", "like");
      inner.setProperty("test", null);
      first.setPropertyItem("owned_by_id", inner);

      Assert.IsTrue(XNode.DeepEquals(target, XElement.Parse(first.ToAml())));

      var second = inn.newItemFromAml(target.ToString());
      Assert.IsTrue(XNode.DeepEquals(target, XElement.Parse(second.ToAml())));

      var third = inn.newItem();
      third.loadAML(target.ToString());
      Assert.IsTrue(XNode.DeepEquals(target, XElement.Parse(third.ToAml())));
    }

    [TestMethod]
    public void TestCollection()
    {
      var target = CreateItem(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='Property' typeId='26D7CD4E033242148E2724D3D054B4D3' id='875AA7D5801B4FA38DB16AE818D9DB26'>
        <behavior>float</behavior>
        <can_update_released>0</can_update_released>
        <column_alignment>left</column_alignment>
        <created_on>2004-08-04T17:26:04</created_on>
        <data_type>string</data_type>
        <generation>1</generation>
        <id keyed_name='classification' type='Property'>875AA7D5801B4FA38DB16AE818D9DB26</id>
        <is_class_required>0</is_class_required>
        <is_current>1</is_current>
        <is_hidden>0</is_hidden>
        <is_hidden2>1</is_hidden2>
        <is_indexed>0</is_indexed>
        <is_keyed>0</is_keyed>
        <is_multi_valued>0</is_multi_valued>
        <is_released>0</is_released>
        <is_required>0</is_required>
        <keyed_name>classification</keyed_name>
        <label xml:lang='en'>Classification</label>
        <major_rev>A</major_rev>
        <modified_on>2011-08-03T16:48:11</modified_on>
        <new_version>0</new_version>
        <not_lockable>0</not_lockable>
        <range_inclusive>0</range_inclusive>
        <readonly>0</readonly>
        <stored_length>512</stored_length>
        <track_history>0</track_history>
        <name>classification</name>
      </Item>
      <Item type='Property' typeId='26D7CD4E033242148E2724D3D054B4D3' id='DEA9466482CB4198AED0D859668D331B'>
        <can_update_released>0</can_update_released>
        <column_alignment>left</column_alignment>
        <created_on>2004-08-04T17:26:04</created_on>
        <data_type>item</data_type>
        <generation>1</generation>
        <id keyed_name='config_id' type='Property'>DEA9466482CB4198AED0D859668D331B</id>
        <is_class_required>0</is_class_required>
        <is_current>1</is_current>
        <is_hidden>1</is_hidden>
        <is_hidden2>1</is_hidden2>
        <is_indexed>0</is_indexed>
        <is_keyed>0</is_keyed>
        <is_multi_valued>0</is_multi_valued>
        <is_released>0</is_released>
        <is_required>1</is_required>
        <item_behavior>float</item_behavior>
        <keyed_name>config_id</keyed_name>
        <label xml:lang='en'>Config_id</label>
        <modified_on>2011-08-03T16:48:11</modified_on>
        <new_version>0</new_version>
        <not_lockable>0</not_lockable>
        <range_inclusive>0</range_inclusive>
        <readonly>0</readonly>
        <sort_order>2688</sort_order>
        <track_history>0</track_history>
        <name>config_id</name>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>");
      Assert.AreEqual(false, target.isEmpty());
      Assert.AreEqual(false, target.isError());
      Assert.AreEqual(true, target.isCollection());
      Assert.AreEqual(false, target.isLogical());
      Assert.AreEqual(2, target.getItemCount());

      AssertRaisesException(() => target.getProperty("name"));

      var second = target.getItemByIndex(1);
      Assert.IsTrue(XNode.DeepEquals(XElement.Parse(second.ToString()), XElement.Parse(@"<Item type='Property' typeId='26D7CD4E033242148E2724D3D054B4D3' id='DEA9466482CB4198AED0D859668D331B'>
  <can_update_released>0</can_update_released>
  <column_alignment>left</column_alignment>
  <created_on>2004-08-04T17:26:04</created_on>
  <data_type>item</data_type>
  <generation>1</generation>
  <id keyed_name='config_id' type='Property'>DEA9466482CB4198AED0D859668D331B</id>
  <is_class_required>0</is_class_required>
  <is_current>1</is_current>
  <is_hidden>1</is_hidden>
  <is_hidden2>1</is_hidden2>
  <is_indexed>0</is_indexed>
  <is_keyed>0</is_keyed>
  <is_multi_valued>0</is_multi_valued>
  <is_released>0</is_released>
  <is_required>1</is_required>
  <item_behavior>float</item_behavior>
  <keyed_name>config_id</keyed_name>
  <label xml:lang='en'>Config_id</label>
  <modified_on>2011-08-03T16:48:11</modified_on>
  <new_version>0</new_version>
  <not_lockable>0</not_lockable>
  <range_inclusive>0</range_inclusive>
  <readonly>0</readonly>
  <sort_order>2688</sort_order>
  <track_history>0</track_history>
  <name>config_id</name>
</Item>")));

      Assert.AreEqual("0", second.getProperty("range_inclusive"));
    }

    [TestMethod()]
    public void RelationshipSerialization()
    {
      var aml = new IOM.Innovator(new TestConnection());
      var input = @"<Item type=""List"" typeId=""5736C479A8CB49BCA20138514C637266"" id=""D7D72BF68937462B947DAC6BE7E28322""><Relationships><Item type=""Value"" /></Relationships></Item>";
      var item = aml.newItemFromAml(input);
      Assert.AreEqual(input, item.ToAml());
    }

    [TestMethod()]
    public void PropertySetWithNullableData()
    {
      var aml = new IOM.Innovator(new TestConnection());
      var item = aml.newItemFromAml("<Item type=\"Stuff\" action=\"edit\" />");
      DateTime? someDate = null;
      DateTime? someDate2 = new DateTime(2016, 01, 01);
      item.Property("some_date").Set(someDate);
      item.Property("some_date_2").Set(someDate2);
      Assert.AreEqual("<Item type=\"Stuff\" action=\"edit\"><some_date is_null=\"1\" /><some_date_2>2016-01-01T00:00:00</some_date_2></Item>", item.ToAml());
    }

    [TestMethod()]
    public void PropertySetWithNumber()
    {
      var aml = new IOM.Innovator(new TestConnection());
      var item = aml.newItemFromAml("<Item type=\"Stuff\" action=\"edit\" />");
      item.Property("some_val").Set(1000);
      item.Property("some_val_2").Set(0.00000000000000000000000000000000000000000000064879);
      Assert.AreEqual("<Item type=\"Stuff\" action=\"edit\"><some_val>1000</some_val><some_val_2>0.00000000000000000000000000000000000000000000064879</some_val_2></Item>", item.ToAml());
    }

    [TestMethod()]
    public void UtcDateConversion()
    {
      var aml = new IOM.Innovator(new TestConnection());
      var item = aml.newItemFromAml("<Item type=\"Stuff\" action=\"edit\"><created_on>2016-05-24T13:22:42</created_on></Item>");
      var localDate = item.CreatedOn().AsDateTime().Value;
      var utcDate = item.CreatedOn().AsDateTimeUtc().Value;
      Assert.AreEqual(DateTime.Parse("2016-05-24T13:22:42"), localDate);
      Assert.AreEqual(DateTime.Parse("2016-05-24T17:22:42"), utcDate);
    }

    [TestMethod()]
    public void PropertyItemExtraction()
    {
      var aml = new IOM.Innovator(new TestConnection());
      var result = aml.newItemFromAml("<Item type='thing' id='1234'><item_prop type='another' keyed_name='stuff'>12345ABCDE12345612345ABCDE123456</item_prop></Item>");
      var propItem = result.AssertItem().Property("item_prop").AsItem().ToAml();
#if XMLLEGACY
      Assert.AreEqual("<Item type=\"another\" id=\"12345ABCDE12345612345ABCDE123456\"><id type=\"another\" keyed_name=\"stuff\">12345ABCDE12345612345ABCDE123456</id><keyed_name>stuff</keyed_name></Item>", propItem);
#else
      Assert.AreEqual("<Item type=\"another\" id=\"12345ABCDE12345612345ABCDE123456\"><keyed_name>stuff</keyed_name><id keyed_name=\"stuff\" type=\"another\">12345ABCDE12345612345ABCDE123456</id></Item>", propItem);
#endif
    }

    [TestMethod()]
    public void VaultPictureUrlToItem()
    {
      var aml = new IOM.Innovator(new TestConnection());
      var result = aml.newItemFromAml(@"<Item type='CAD' typeId='CCF205347C814DD1AF056875E0A880AC' id='2B2444304435441AA1137972D2B8B534'>
  <thumbnail>vault:///?fileId=1E49D4C8BE6545F9882A28C0763F473A</thumbnail>
</Item>");
      var propItem = result.AssertItem().Property("thumbnail").AsItem().ToAml();
      Assert.AreEqual("<Item type=\"File\" id=\"1E49D4C8BE6545F9882A28C0763F473A\"><id type=\"File\">1E49D4C8BE6545F9882A28C0763F473A</id></Item>", propItem);
      Assert.AreEqual("1E49D4C8BE6545F9882A28C0763F473A", result.AssertItem().Property("thumbnail").AsGuid().ToArasId());
      Assert.AreEqual("vault:///?fileId=1E49D4C8BE6545F9882A28C0763F473A", result.AssertItem().Property("thumbnail").Value);
      Assert.AreEqual("vault:///?fileId=1E49D4C8BE6545F9882A28C0763F473A", result.AssertItem().Property("thumbnail").AsString(""));
    }

    [TestMethod()]
    public void WhereUsedTest()
    {
      var aml = new IOM.Innovator(new TestConnection());
      var result = aml.newItemFromAml(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='Part' id='60664D33AEC245BBBEAEFC46612B87F5' icon='../images/customer/images/Part16.png' keyed_name='Part - 1 - D.2' loaded='1'>
        <relatedItems>
          <Item type='Affected Item' id='ACD4739883864B548FE0671634CB7670' keyed_name='Affected Item - 7 - A.1'></Item>
          <Item type='Part' id='F7C39CE1AB4245D4A1695075BC7F9B49' icon='../images/customer/images/Part16.png' keyed_name='Part - 7 - 2.2'></Item>
          <Item type='Part' id='F921E5F7576342698771C2539AFC23BD' icon='../images/customer/images/Part16.png' keyed_name='Part - 7 - 3.3'></Item>
        </relatedItems>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>");
      var items = result.AssertItem().Element("relatedItems").Elements().OfType<IReadOnlyItem>().ToArray();
      Assert.AreEqual(3, items.Length);
      Assert.AreEqual("ACD4739883864B548FE0671634CB7670", items[0].Id());
      Assert.AreEqual("Part", items[1].Type().Value);
    }

    [TestMethod]
    public void LanguageHandling()
    {
      var aml = new IOM.Innovator(new TestConnection());
      var item = aml.newItemFromAml("<Item type='Supplier' action='get' select='name' language='en,fr'><thing>All</thing><name xml:lang='en'>Dell US</name><i18n:name xml:lang='fr' xmlns:i18n='http://www.aras.com/I18N'>Dell France</i18n:name></Item>");
      Assert.AreEqual("All", item.Property("thing").Value);
      Assert.AreEqual(null, item.Property("thing", "en").Value);
      Assert.AreEqual(null, item.Property("thing", "fr").Value);
      Assert.AreEqual("Dell US", item.Property("name").Value);
      Assert.AreEqual("Dell US", item.Property("name", "en").Value);
      Assert.AreEqual("Dell France", item.Property("name", "fr").Value);
    }

#if XMLLEGACY
    //[TestMethod]
    //public void TestIomConnection()
    //{
    //  var prefs = SavedConnections.Load().Default;
    //  var exp = (ExplicitCredentials)prefs.Credentials;
    //  var password = exp.Password.UseString<string>((ref string p) => new string(p.ToCharArray()));
    //  var iConn = Aras.IOM.IomFactory.CreateHttpServerConnection(prefs.Url, exp.Database, exp.Username, password);
    //  //iConn.Login();
    //  var conn = new IOM.IomConnection(iConn);
    //  var item = conn.Apply("<Item type='ItemType' action='get' id='450906E86E304F55A34B3C0D65C097EA' select='name'></Item>").AssertItem();
    //  Assert.AreEqual("ItemType", item.Property("name").Value);
    //}


    [TestMethod]
    public void AddRelationship()
    {
      CompareIoms(@"<Item type='Method' action='add' id='46D9FDF379AD4B4DA0143C92BBA16720'>
  <name>A new method</name>
  <owned_by_id>
    <Item type='Identity' action='get'>
      <name condition='like'>Admin*</name>
      <test is_null='1' />
    </Item>
  </owned_by_id>
</Item>", (inn, item, compares) =>
      {
        var newRel = inn.newItem();
        newRel.loadAML("<Item type='relation' />");
        item.getPropertyItem("owned_by_id").addRelationship(newRel);
      });
    }

    [TestMethod]
    public void Clone()
    {
      CompareIoms(@"<Item type='Method' action='add' id='46D9FDF379AD4B4DA0143C92BBA16720'>
  <name>A new method</name>
  <owned_by_id>
    <Item type='Identity' action='get'>
      <name condition='like'>Admin*</name>
      <test is_null='1' />
    </Item>
  </owned_by_id>
</Item>", (inn, item, compares) =>
      {
        var clone = item.clone(true);
        clone.setID("FFEEA3C8A2AB4B6AB961CF03A4FFF519");
        compares.Add(clone.dom.OuterXml);
      });
    }

    [TestMethod]
    public void CreatePropertyItem()
    {
      CompareIoms(@"<Item type='Method' action='add' id='46D9FDF379AD4B4DA0143C92BBA16720'>
  <name>A new method</name>
  <owned_by_id>
    <Item type='Identity' action='get'>
      <name condition='like'>Admin*</name>
      <test is_null='1' />
    </Item>
  </owned_by_id>
</Item>", (inn, item, compares) =>
      {
        var propItem = item.createPropertyItem("managed_by_id", "Identity", "get");
        propItem.setProperty("name", "Wor*");
        propItem.setPropertyCondition("name", "like");
      });
    }

    [TestMethod]
    public void CreateRelationship()
    {
      CompareIoms(@"<Item type='Method' action='add' id='46D9FDF379AD4B4DA0143C92BBA16720'>
  <name>A new method</name>
  <owned_by_id>
    <Item type='Identity' action='get'>
      <name condition='like'>Admin*</name>
      <test is_null='1' />
    </Item>
  </owned_by_id>
</Item>", (inn, item, compares) =>
      {
        var relItemItem = item.createRelationship("Identity", "get");
        relItemItem.setProperty("name", "Wor*");
        relItemItem.setPropertyCondition("name", "like");
      });
    }

    [TestMethod]
    public void GetAction()
    {
      CompareIoms(@"<Item type='Method' action='add' id='46D9FDF379AD4B4DA0143C92BBA16720'>
  <name>A new method</name>
  <owned_by_id>
    <Item type='Identity' action='get' id='DEA9466482CB4198AED0D859668D331B'>
      <name condition='like'>Admin*</name>
      <test is_null='1' />
    </Item>
  </owned_by_id>
</Item>", (inn, item, compares) =>
      {
        compares.Add(item.getAction());
        compares.Add(item.getPropertyItem("owned_by_id").getAction());
      });
    }

    [TestMethod]
    public void GetAttribute()
    {
      CompareIoms(@"<Item type='Method' action='add' id='46D9FDF379AD4B4DA0143C92BBA16720'>
  <name>A new method</name>
  <owned_by_id>
    <Item type='Identity' action='get' id='DEA9466482CB4198AED0D859668D331B'>
      <name condition='like'>Admin*</name>
      <test is_null='1' />
    </Item>
  </owned_by_id>
</Item>", (inn, item, compares) =>
      {
        compares.Add(item.getAttribute("not_there"));
        compares.Add(item.getAttribute("not_there", "default"));
        compares.Add(item.getAttribute("id"));
        compares.Add(item.getAttribute("id", "default"));
        compares.Add(item.getID());
        compares.Add(item.getItemCount());
        compares.Add(item.getItemByIndex(0).dom.OuterXml);
        compares.Add(item.getLockStatus());
        compares.Add(item.getProperty("name"));
        compares.Add(item.getProperty("name", "default"));
        compares.Add(item.getProperty("not_there"));
        compares.Add(item.getProperty("not_there", "default"));
        compares.Add(item.getType());

        var propItem = item.getPropertyItem("owned_by_id");
        compares.Add(propItem.getProperty("name"));
        compares.Add(propItem.getPropertyAttribute("test", "not_there"));
        compares.Add(propItem.getPropertyAttribute("test", "not_there", "default"));
        compares.Add(propItem.getPropertyAttribute("test", "is_null"));
        compares.Add(propItem.getPropertyAttribute("test", "is_null", "default"));
        compares.Add(propItem.getPropertyCondition("test"));
        compares.Add(propItem.getPropertyCondition("test", "default"));
        compares.Add(propItem.getPropertyCondition("name"));
        compares.Add(propItem.getPropertyCondition("name", "default"));

        compares.Add(item.getItemCount());
        compares.Add(item.isError());
        compares.Add(item.isEmpty());
        compares.Add(item.isCollection());
        compares.Add(item.isLogical());
        compares.Add(item.isNew());
        compares.Add(item.isRoot());
      });
    }

    [TestMethod]
    public void GetZeroItemsErrorInfo()
    {
      CompareIoms(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af='http://www.aras.com/InnovatorFault'>
      <faultcode>0</faultcode>
      <faultstring>No items of type Inbox found.</faultstring>
      <detail>Other stuff</detail>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>", (inn, item, compares) =>
      {
        compares.Add(item.getErrorCode());
        compares.Add(item.getErrorDetail());
        compares.Add(item.getErrorSource());
        compares.Add(item.getErrorString());
        compares.Add(item.getItemCount());
        compares.Add(item.isError());
        compares.Add(item.isEmpty());
        compares.Add(item.isCollection());
        compares.Add(item.isLogical());
        compares.Add(item.isNew());
        compares.Add(item.isRoot());
      });
    }


    [TestMethod]
    public void GetGeneralErrorInfo()
    {
      CompareIoms(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af='http://www.aras.com/InnovatorFault'>
      <faultcode>Exception</faultcode>
      <faultstring>Everything went wront.</faultstring>
      <detail>Other stuff</detail>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>", (inn, item, compares) =>
      {
        compares.Add(item.getErrorCode());
        compares.Add(item.getErrorDetail());
        compares.Add(item.getErrorSource());
        compares.Add(item.getErrorString());
        compares.Add(item.getItemCount());
        compares.Add(item.isError());
        compares.Add(item.isEmpty());
        compares.Add(item.isCollection());
        compares.Add(item.isLogical());
        compares.Add(item.isNew());
        compares.Add(item.isRoot());
      });
    }

    [TestMethod]
    public void GetCollection()
    {
      CompareIoms(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='Property' typeId='26D7CD4E033242148E2724D3D054B4D3' id='875AA7D5801B4FA38DB16AE818D9DB26'>
        <behavior>float</behavior>
        <can_update_released>0</can_update_released>
        <column_alignment>left</column_alignment>
        <created_on>2004-08-04T17:26:04</created_on>
        <data_type>string</data_type>
        <generation>1</generation>
        <id keyed_name='classification' type='Property'>875AA7D5801B4FA38DB16AE818D9DB26</id>
        <is_class_required>0</is_class_required>
        <is_current>1</is_current>
        <is_hidden>0</is_hidden>
        <is_hidden2>1</is_hidden2>
        <is_indexed>0</is_indexed>
        <is_keyed>0</is_keyed>
        <is_multi_valued>0</is_multi_valued>
        <is_released>0</is_released>
        <is_required>0</is_required>
        <keyed_name>classification</keyed_name>
        <label xml:lang='en'>Classification</label>
        <major_rev>A</major_rev>
        <modified_on>2011-08-03T16:48:11</modified_on>
        <new_version>0</new_version>
        <not_lockable>0</not_lockable>
        <range_inclusive>0</range_inclusive>
        <readonly>0</readonly>
        <stored_length>512</stored_length>
        <track_history>0</track_history>
        <name>classification</name>
      </Item>
      <Item type='Property' typeId='26D7CD4E033242148E2724D3D054B4D3' id='DEA9466482CB4198AED0D859668D331B'>
        <can_update_released>0</can_update_released>
        <column_alignment>left</column_alignment>
        <created_on>2004-08-04T17:26:04</created_on>
        <data_type>item</data_type>
        <generation>1</generation>
        <id keyed_name='config_id' type='Property'>DEA9466482CB4198AED0D859668D331B</id>
        <is_class_required>0</is_class_required>
        <is_current>1</is_current>
        <is_hidden>1</is_hidden>
        <is_hidden2>1</is_hidden2>
        <is_indexed>0</is_indexed>
        <is_keyed>0</is_keyed>
        <is_multi_valued>0</is_multi_valued>
        <is_released>0</is_released>
        <is_required>1</is_required>
        <item_behavior>float</item_behavior>
        <keyed_name>config_id</keyed_name>
        <label xml:lang='en'>Config_id</label>
        <modified_on>2011-08-03T16:48:11</modified_on>
        <new_version>0</new_version>
        <not_lockable>0</not_lockable>
        <range_inclusive>0</range_inclusive>
        <readonly>0</readonly>
        <sort_order>2688</sort_order>
        <track_history>0</track_history>
        <name>config_id</name>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>", (inn, item, compares) =>
      {
        compares.Add(item.getErrorCode());
        compares.Add(item.getErrorDetail());
        compares.Add(item.getItemCount());
        compares.Add(item.isError());
        compares.Add(item.isEmpty());
        compares.Add(item.isCollection());
        compares.Add(item.isLogical());
        compares.Add(item.isNew());
        compares.Add(item.isRoot());
        compares.Add(item.getItemByIndex(0).isLogical());
        compares.Add(item.getItemByIndex(0).isNew());
        compares.Add(item.getItemByIndex(0).isRoot());
        item.removeItem(item.getItemByIndex(0));
      });
    }

    [TestMethod]
    public void GetParentItem()
    {
      CompareIoms(@"<Item type='Method' action='add' id='46D9FDF379AD4B4DA0143C92BBA16720'>
  <name>A new method</name>
  <owned_by_id>
    <Item type='Identity' action='get' id='DEA9466482CB4198AED0D859668D331B'>
      <name condition='like'>Admin*</name>
      <test is_null='1' />
    </Item>
  </owned_by_id>
</Item>", (inn, item, compares) =>
      {
        compares.Add(item.getPropertyItem("owned_by_id").getParentItem().node.OuterXml);
        compares.Add(item.getPropertyItem("owned_by_id").getParentItem().getID());
      });
    }

    [TestMethod]
    public void LogicalItems()
    {
      CompareIoms(@"<?xml version='1.0' encoding='utf-8'?>
<AML>
  <Item type='Inbox' action='get'>
    <or>
      <item_id>3962701DDA4F44068F8CE106DCBCF055</item_id>
      <created_by_id>75ABC2E1E41C4AE6B978B86A657CFA07</created_by_id>
    </or>
    <not><keyed_name>abc</keyed_name></not>
    <itemtype>
      <Item type='ItemType' action='get'>
        <name>BOM Review</name>
      </Item>
    </itemtype>
  </Item>
</AML>", (inn, item, compares) =>
      {
        var logical = item.getLogicalChildren();
        compares.Add(logical.getItemCount());
        compares.Add(logical.isError());
        compares.Add(logical.isEmpty());
        compares.Add(logical.isCollection());
        compares.Add(logical.isLogical());
        compares.Add(logical.isNew());
        compares.Add(logical.isRoot());
        compares.Add(logical.nodeList.Item(0).OuterXml);
        compares.Add(logical.nodeList.Item(1).OuterXml);
        item.removeLogical(item.getLogicalChildren().getItemByIndex(1));
        var andElem = item.newAND();
        andElem.setProperty("keyed_name", "thing");
        andElem.setProperty("major_rev", "C");
      });
    }

    [TestMethod]
    public void SetAttributes()
    {
      CompareIoms(@"<?xml version='1.0' encoding='utf-8'?>
<AML>
  <Item type='Inbox' action='get' random='2'>
    <item_id>3962701DDA4F44068F8CE106DCBCF055</item_id>
    <created_by_id>75ABC2E1E41C4AE6B978B86A657CFA07</created_by_id>
    <keyed_name>abc</keyed_name>
    <itemtype>
      <Item type='ItemType' action='get'>
        <name>BOM Review</name>
      </Item>
    </itemtype>
  </Item>
</AML>", (inn, item, compares) =>
      {
        item.removeAttribute("random");
        item.removeAttribute("not_there");
        item.setAttribute("new", "1");
        item.setType("Thingy");
        item.setAction("add");
      });
    }

    [TestMethod]
    public void LanguageHandlingCompare()
    {
      CompareIoms(@"<Item type='Supplier' action='get' select='name' language='en,fr'><thing>All</thing><name xml:lang='en'>Dell US</name><i18n:name xml:lang='fr' xmlns:i18n='http://www.aras.com/I18N'>Dell France</i18n:name></Item>",
        (inn, item, compares) =>
        {
          compares.Add(item.getProperty("thing"));
          compares.Add(item.getProperty("thing", "", "en"));
          compares.Add(item.getProperty("thing", "", "fr"));
          compares.Add(item.getProperty("name"));
          compares.Add(item.getProperty("name", "", "en"));
          compares.Add(item.getProperty("name", "", "fr"));

          item.setProperty("test1", "1");
        });
    }

    private void CompareIoms(string aml, Action<dynamic, dynamic, List<object>> action)
    {
      var comparesAras = new List<object>();
      var comparesClient = new List<object>();
      Assert.AreEqual(PerformArasIomTest(aml, comparesAras, action), PerformClientIomTest(aml, comparesClient, action));
      CollectionAssert.AreEqual(comparesAras, comparesClient);
    }

    private string PerformClientIomTest(string aml, List<object> compares, Action<dynamic, dynamic, List<object>> action)
    {
      var conn = new TestConnection();
      var inn = new IOM.Innovator(conn);
      var item = inn.newItemFromAml(aml);
      action.Invoke(inn, item, compares);
      return item.dom.OuterXml;
    }

    private string PerformArasIomTest(string aml, List<object> compares, Action<dynamic, dynamic, List<object>> action)
    {
      var conn = new TestArasConnection();
      var inn = Aras.IOM.IomFactory.CreateInnovator(conn);
      var item = inn.newItem();
      item.loadAML(aml);
      action.Invoke(inn, item, compares);
      return item.dom.OuterXml;
    }

    private class TestArasConnection : Aras.IOM.IServerConnection
    {
      public void CallAction(string actionName, XmlDocument inDom, XmlDocument outDom)
      {
        throw new NotImplementedException();
      }

      public void DebugLog(string reason, object msg)
      {
        throw new NotImplementedException();
      }

      public bool DebugLogP()
      {
        throw new NotImplementedException();
      }

      public void DownloadFile(Aras.IOM.Item fileItem, string directoryPath, bool overwriteFile)
      {
        throw new NotImplementedException();
      }

      public string GetDatabaseName()
      {
        return "Test";
      }

      public string[] GetDatabases()
      {
        return new string[] { "Test" };
      }

      public string getFileUrl(string fileId, UrlType type)
      {
        throw new NotImplementedException();
      }

      public ArrayList getFileUrls(ArrayList fileIds, UrlType type)
      {
        throw new NotImplementedException();
      }

      public object GetFromCache(string key)
      {
        throw new NotImplementedException();
      }

      public string GetLicenseInfo()
      {
        throw new NotImplementedException();
      }

      public string GetLicenseInfo(string issuer, string addonName)
      {
        throw new NotImplementedException();
      }

      public object GetOperatingParameter(string name, object defaultvalue)
      {
        throw new NotImplementedException();
      }

      public object GetSrvContext()
      {
        throw new NotImplementedException();
      }

      public string getUserID()
      {
        return "2D246C5838644C1C8FD34F8D2796E327";
      }

      public string GetValidateUserXmlResult()
      {
        var local = ElementFactory.Local.LocalizationContext;
        return $@"<a><b><c><i18nsessioncontext>
  <locale>{local.Locale}</locale>
  <language_code>{local.LanguageCode}</language_code>
  <language_suffix>{local.LanguageSuffix}</language_suffix>
  <default_language_code>{local.DefaultLanguageCode}</default_language_code>
  <default_language_suffix>{local.DefaultLanguageSuffix}</default_language_suffix>
  <time_zone>{local.TimeZone}</time_zone>
  <corporate_to_local_offset>0</corporate_to_local_offset>
</i18nsessioncontext></c></b></a>";
      }

      public void InsertIntoCache(string key, object value, string path)
      {
        throw new NotImplementedException();
      }
    }
#endif
  }
}
