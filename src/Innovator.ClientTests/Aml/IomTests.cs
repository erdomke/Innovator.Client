using Innovator.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"start\" /><Item type=\"append\" /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", start.ToAml());
    }

    [TestMethod]
    public void AppendItem_SingleItemWithParent()
    {
      var start = CreateItem("<AML><Item type='start'/></AML>");
      var append = CreateItem("<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><ApplyItemResponse><Item type='append'/></ApplyItemResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
      start.appendItem(append);
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"start\" /><Item type=\"append\" /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", start.ToAml());
    }

    [TestMethod]
    public void AppendItem_MultipleParents()
    {
      var start = CreateItem("<AML><Item type='start'/><Item type='second'/></AML>");
      var append = CreateItem("<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><ApplyItemResponse><Item type='append'/></ApplyItemResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>");
      start.appendItem(append);
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"start\" /><Item type=\"second\" /><Item type=\"append\" /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", start.ToAml());
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

      Assert.IsTrue(XNode.DeepEquals(target, XElement.Parse(first.ToAmlRoot())));

      var second = inn.newItemFromAml(target.ToString());
      Assert.IsTrue(XNode.DeepEquals(target, XElement.Parse(second.ToAmlRoot())));

      var third = inn.newItem();
      third.loadAML(target.ToString());
      Assert.IsTrue(XNode.DeepEquals(target, XElement.Parse(third.ToAmlRoot())));
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
#endif
  }
}
