using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Innovator.Client.Tests.Aml
{
  [TestClass()]
  public class ODataTests
  {
    [TestMethod()]
    public void OData_MultipleSimpleItems()
    {
      var json = @"{""@odata.context"":""http://zvm-devplm/innovator11sp12/server/OData/$metadata#Part"",""value"":[{""classification"":""Assembly"",""created_on"":""2018-01-18T15:09:05"",""generation"":""1"",""has_change_pending"":""0"",""id"":""BA5F278BB651475EAE40C03498F2F20F"",
""is_current"":""1"",""is_released"":""0"",""keyed_name"":""ASSY-01"",""major_rev"":""A"",""make_buy"":""Make"",""modified_on"":""2018-01-18T20:44:15"",""name"":""Assembly of components #1"",""new_version"":""0"",""not_lockable"":""0"",""state"":""Preliminary"",""unit"":""EA"",
""item_number"":""ASSY-01""},{""classification"":""Component"",""created_on"":""2018-01-18T15:09:37"",""generation"":""3"",""has_change_pending"":""0"",""id"":""DA86EFE63F5F4D958D8A5446D8DC932F"",""is_current"":""1"",""is_released"":""0"",""keyed_name"":""COMP-01"",""major_rev"":""A"",
""make_buy"":""Make"",""modified_on"":""2018-01-19T15:31:15"",""name"":""Component part #1"",""new_version"":""0"",""not_lockable"":""0"",""state"":""Preliminary"",""unit"":""EA"",""item_number"":""COMP-01""},{""classification"":""Component"",""created_on"":""2018-01-18T15:10:02"",""generation"":""1"",
""has_change_pending"":""0"",""id"":""EA446AA4A8904CBF8E120843981526AB"",""is_current"":""1"",""is_released"":""0"",""keyed_name"":""COMP-02"",""major_rev"":""A"",""make_buy"":""Make"",""modified_on"":""2018-01-18T16:21:32"",""name"":""Component part #2"",""new_version"":""0"",""not_lockable"":""0"",
""state"":""Preliminary"",""unit"":""EA"",""item_number"":""COMP-02""},{""created_on"":""2018-02-03T21:22:03"",""generation"":""2"",""has_change_pending"":""0"",""id"":""ACF988E9074440D890014ED8D10DC1ED"",""is_current"":""1"",""is_released"":""0"",""keyed_name"":""FG-01"",""major_rev"":""A"",""make_buy"":""Make"",
""modified_on"":""2018-02-07T21:34:42"",""name"":""Finished Good"",""new_version"":""0"",""not_lockable"":""0"",""state"":""Preliminary"",""unit"":""EA"",""item_number"":""FG-01""}]}";
      var oData = new ODataAmlReader(new StringReader(json));
      var aml = XElement.Load(oData);
      var amlStr = aml.ToString();
      var expected = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <Result>
      <Item type=""Part"">
        <classification>Assembly</classification>
        <created_on>2018-01-18T15:09:05</created_on>
        <generation>1</generation>
        <has_change_pending>0</has_change_pending>
        <id>BA5F278BB651475EAE40C03498F2F20F</id>
        <is_current>1</is_current>
        <is_released>0</is_released>
        <keyed_name>ASSY-01</keyed_name>
        <major_rev>A</major_rev>
        <make_buy>Make</make_buy>
        <modified_on>2018-01-18T20:44:15</modified_on>
        <name>Assembly of components #1</name>
        <new_version>0</new_version>
        <not_lockable>0</not_lockable>
        <state>Preliminary</state>
        <unit>EA</unit>
        <item_number>ASSY-01</item_number>
      </Item>
      <Item type=""Part"">
        <classification>Component</classification>
        <created_on>2018-01-18T15:09:37</created_on>
        <generation>3</generation>
        <has_change_pending>0</has_change_pending>
        <id>DA86EFE63F5F4D958D8A5446D8DC932F</id>
        <is_current>1</is_current>
        <is_released>0</is_released>
        <keyed_name>COMP-01</keyed_name>
        <major_rev>A</major_rev>
        <make_buy>Make</make_buy>
        <modified_on>2018-01-19T15:31:15</modified_on>
        <name>Component part #1</name>
        <new_version>0</new_version>
        <not_lockable>0</not_lockable>
        <state>Preliminary</state>
        <unit>EA</unit>
        <item_number>COMP-01</item_number>
      </Item>
      <Item type=""Part"">
        <classification>Component</classification>
        <created_on>2018-01-18T15:10:02</created_on>
        <generation>1</generation>
        <has_change_pending>0</has_change_pending>
        <id>EA446AA4A8904CBF8E120843981526AB</id>
        <is_current>1</is_current>
        <is_released>0</is_released>
        <keyed_name>COMP-02</keyed_name>
        <major_rev>A</major_rev>
        <make_buy>Make</make_buy>
        <modified_on>2018-01-18T16:21:32</modified_on>
        <name>Component part #2</name>
        <new_version>0</new_version>
        <not_lockable>0</not_lockable>
        <state>Preliminary</state>
        <unit>EA</unit>
        <item_number>COMP-02</item_number>
      </Item>
      <Item type=""Part"">
        <created_on>2018-02-03T21:22:03</created_on>
        <generation>2</generation>
        <has_change_pending>0</has_change_pending>
        <id>ACF988E9074440D890014ED8D10DC1ED</id>
        <is_current>1</is_current>
        <is_released>0</is_released>
        <keyed_name>FG-01</keyed_name>
        <major_rev>A</major_rev>
        <make_buy>Make</make_buy>
        <modified_on>2018-02-07T21:34:42</modified_on>
        <name>Finished Good</name>
        <new_version>0</new_version>
        <not_lockable>0</not_lockable>
        <state>Preliminary</state>
        <unit>EA</unit>
        <item_number>FG-01</item_number>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      Assert.AreEqual(expected, amlStr);

      VerifyXmlDocument(new ODataAmlReader(new StringReader(json)), expected);

      var items = ElementFactory.Local.FromXml(new ODataAmlReader(new StringReader(json))).Items().ToArray();
      Assert.AreEqual(4, items.Length);
      Assert.AreEqual("Part", items[3].TypeName());
      Assert.AreEqual("EA446AA4A8904CBF8E120843981526AB", items[2].Id());
      Assert.AreEqual("ASSY-01", items[0].KeyedName().Value);
    }

    [TestMethod()]
    public void OData_ExpandAndSelect()
    {
      var json = @"{""@odata.context"":""http://zvm-devplm/innovator11sp12/server/OData/$metadata#Part(created_by_id,item_number,description)"",""value"":[{""@odata.id"":""Part('BA5F278BB651475EAE40C03498F2F20F')"",
""created_by_id"":{""created_on"":""2002-04-24T13:46:12"",""generation"":""1"",""id"":""30B991F927274FA3829655F50C99472E"",""is_current"":""1"",""is_released"":""0"",
""keyed_name"":""Innovator Admin"",""last_login_date"":""2018-02-15T21:06:13"",""login_name"":""admin"",""logon_enabled"":""1"",""major_rev"":""A"",""modified_on"":""2004-01-16T20:18:31"",
""new_version"":""0"",""not_lockable"":""0"",""state"":""Released"",""working_directory"":""C:"",""first_name"":""Innovator"",""last_name"":""Admin""},""item_number"":""ASSY-01"",""description"":null,
""created_by_id"":{""created_on"":""2002-04-24T13:46:12"",""generation"":""1"",""id"":""30B991F927274FA3829655F50C99472E"",""is_current"":""1"",""is_released"":""0"",
""keyed_name"":""Innovator Admin"",""last_login_date"":""2018-02-15T21:06:13"",""login_name"":""admin"",""logon_enabled"":""1"",""major_rev"":""A"",""modified_on"":""2004-01-16T20:18:31"",
""new_version"":""0"",""not_lockable"":""0"",""state"":""Released"",""working_directory"":""C:"",""first_name"":""Innovator"",""last_name"":""Admin""}},
{""@odata.id"":""Part('DA86EFE63F5F4D958D8A5446D8DC932F')"",""created_by_id"":{""created_on"":""2002-04-24T13:46:12"",""generation"":""1"",""id"":""30B991F927274FA3829655F50C99472E"",""is_current"":""1"",
""is_released"":""0"",""keyed_name"":""Innovator Admin"",""last_login_date"":""2018-02-15T21:06:13"",""login_name"":""admin"",""logon_enabled"":""1"",""major_rev"":""A"",""modified_on"":""2004-01-16T20:18:31"",
""new_version"":""0"",""not_lockable"":""0"",""state"":""Released"",""working_directory"":""C:"",""first_name"":""Innovator"",""last_name"":""Admin""},""item_number"":""COMP-01"",
""description"":null,""created_by_id"":{""created_on"":""2002-04-24T13:46:12"",""generation"":""1"",""id"":""30B991F927274FA3829655F50C99472E"",""is_current"":""1"",""is_released"":""0"",
""keyed_name"":""Innovator Admin"",""last_login_date"":""2018-02-15T21:06:13"",""login_name"":""admin"",""logon_enabled"":""1"",""major_rev"":""A"",""modified_on"":""2004-01-16T20:18:31"",
""new_version"":""0"",""not_lockable"":""0"",""state"":""Released"",""working_directory"":""C:"",""first_name"":""Innovator"",""last_name"":""Admin""}},
{""@odata.id"":""Part('EA446AA4A8904CBF8E120843981526AB')"",""created_by_id"":{""created_on"":""2002-04-24T13:46:12"",""generation"":""1"",""id"":""30B991F927274FA3829655F50C99472E"",
""is_current"":""1"",""is_released"":""0"",""keyed_name"":""Innovator Admin"",""last_login_date"":""2018-02-15T21:06:13"",""login_name"":""admin"",""logon_enabled"":""1"",""major_rev"":""A"",
""modified_on"":""2004-01-16T20:18:31"",""new_version"":""0"",""not_lockable"":""0"",""state"":""Released"",""working_directory"":""C:"",""first_name"":""Innovator"",
""last_name"":""Admin""},""item_number"":""COMP-02"",""description"":null,""created_by_id"":{""created_on"":""2002-04-24T13:46:12"",""generation"":""1"",
""id"":""30B991F927274FA3829655F50C99472E"",""is_current"":""1"",""is_released"":""0"",""keyed_name"":""Innovator Admin"",""last_login_date"":""2018-02-15T21:06:13"",""login_name"":""admin"",""logon_enabled"":""1"",""major_rev"":""A"",
""modified_on"":""2004-01-16T20:18:31"",""new_version"":""0"",""not_lockable"":""0"",""state"":""Released"",""working_directory"":""C:"",""first_name"":""Innovator"",""last_name"":""Admin""}},
{""@odata.id"":""Part('ACF988E9074440D890014ED8D10DC1ED')"",""created_by_id"":{""created_on"":""2002-04-24T13:46:12"",""generation"":""1"",""id"":""30B991F927274FA3829655F50C99472E"",
""is_current"":""1"",""is_released"":""0"",""keyed_name"":""Innovator Admin"",""last_login_date"":""2018-02-15T21:06:13"",""login_name"":""admin"",""logon_enabled"":""1"",""major_rev"":""A"",""modified_on"":""2004-01-16T20:18:31"",
""new_version"":""0"",""not_lockable"":""0"",""state"":""Released"",""working_directory"":""C:"",""first_name"":""Innovator"",""last_name"":""Admin""},
""item_number"":""FG-01"",""description"":null,""created_by_id"":{""created_on"":""2002-04-24T13:46:12"",""generation"":""1"",""id"":""30B991F927274FA3829655F50C99472E"",
""is_current"":""1"",""is_released"":""0"",""keyed_name"":""Innovator Admin"",""last_login_date"":""2018-02-15T21:06:13"",""login_name"":""admin"",""logon_enabled"":""1"",""major_rev"":""A"",""modified_on"":""2004-01-16T20:18:31"",
""new_version"":""0"",""not_lockable"":""0"",""state"":""Released"",""working_directory"":""C:"",""first_name"":""Innovator"",""last_name"":""Admin""}}]}";
      var oData = new ODataAmlReader(new StringReader(json));
      var aml = XElement.Load(oData);
      var amlStr = aml.ToString();

      var expected = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <Result>
      <Item type=""Part"">
        <id>BA5F278BB651475EAE40C03498F2F20F</id>
        <created_by_id>
          <Item>
            <created_on>2002-04-24T13:46:12</created_on>
            <generation>1</generation>
            <id>30B991F927274FA3829655F50C99472E</id>
            <is_current>1</is_current>
            <is_released>0</is_released>
            <keyed_name>Innovator Admin</keyed_name>
            <last_login_date>2018-02-15T21:06:13</last_login_date>
            <login_name>admin</login_name>
            <logon_enabled>1</logon_enabled>
            <major_rev>A</major_rev>
            <modified_on>2004-01-16T20:18:31</modified_on>
            <new_version>0</new_version>
            <not_lockable>0</not_lockable>
            <state>Released</state>
            <working_directory>C:</working_directory>
            <first_name>Innovator</first_name>
            <last_name>Admin</last_name>
          </Item>
        </created_by_id>
        <item_number>ASSY-01</item_number>
        <description is_null=""1""></description>
        <created_by_id>
          <Item>
            <created_on>2002-04-24T13:46:12</created_on>
            <generation>1</generation>
            <id>30B991F927274FA3829655F50C99472E</id>
            <is_current>1</is_current>
            <is_released>0</is_released>
            <keyed_name>Innovator Admin</keyed_name>
            <last_login_date>2018-02-15T21:06:13</last_login_date>
            <login_name>admin</login_name>
            <logon_enabled>1</logon_enabled>
            <major_rev>A</major_rev>
            <modified_on>2004-01-16T20:18:31</modified_on>
            <new_version>0</new_version>
            <not_lockable>0</not_lockable>
            <state>Released</state>
            <working_directory>C:</working_directory>
            <first_name>Innovator</first_name>
            <last_name>Admin</last_name>
          </Item>
        </created_by_id>
      </Item>
      <Item type=""Part"">
        <id>DA86EFE63F5F4D958D8A5446D8DC932F</id>
        <created_by_id>
          <Item>
            <created_on>2002-04-24T13:46:12</created_on>
            <generation>1</generation>
            <id>30B991F927274FA3829655F50C99472E</id>
            <is_current>1</is_current>
            <is_released>0</is_released>
            <keyed_name>Innovator Admin</keyed_name>
            <last_login_date>2018-02-15T21:06:13</last_login_date>
            <login_name>admin</login_name>
            <logon_enabled>1</logon_enabled>
            <major_rev>A</major_rev>
            <modified_on>2004-01-16T20:18:31</modified_on>
            <new_version>0</new_version>
            <not_lockable>0</not_lockable>
            <state>Released</state>
            <working_directory>C:</working_directory>
            <first_name>Innovator</first_name>
            <last_name>Admin</last_name>
          </Item>
        </created_by_id>
        <item_number>COMP-01</item_number>
        <description is_null=""1""></description>
        <created_by_id>
          <Item>
            <created_on>2002-04-24T13:46:12</created_on>
            <generation>1</generation>
            <id>30B991F927274FA3829655F50C99472E</id>
            <is_current>1</is_current>
            <is_released>0</is_released>
            <keyed_name>Innovator Admin</keyed_name>
            <last_login_date>2018-02-15T21:06:13</last_login_date>
            <login_name>admin</login_name>
            <logon_enabled>1</logon_enabled>
            <major_rev>A</major_rev>
            <modified_on>2004-01-16T20:18:31</modified_on>
            <new_version>0</new_version>
            <not_lockable>0</not_lockable>
            <state>Released</state>
            <working_directory>C:</working_directory>
            <first_name>Innovator</first_name>
            <last_name>Admin</last_name>
          </Item>
        </created_by_id>
      </Item>
      <Item type=""Part"">
        <id>EA446AA4A8904CBF8E120843981526AB</id>
        <created_by_id>
          <Item>
            <created_on>2002-04-24T13:46:12</created_on>
            <generation>1</generation>
            <id>30B991F927274FA3829655F50C99472E</id>
            <is_current>1</is_current>
            <is_released>0</is_released>
            <keyed_name>Innovator Admin</keyed_name>
            <last_login_date>2018-02-15T21:06:13</last_login_date>
            <login_name>admin</login_name>
            <logon_enabled>1</logon_enabled>
            <major_rev>A</major_rev>
            <modified_on>2004-01-16T20:18:31</modified_on>
            <new_version>0</new_version>
            <not_lockable>0</not_lockable>
            <state>Released</state>
            <working_directory>C:</working_directory>
            <first_name>Innovator</first_name>
            <last_name>Admin</last_name>
          </Item>
        </created_by_id>
        <item_number>COMP-02</item_number>
        <description is_null=""1""></description>
        <created_by_id>
          <Item>
            <created_on>2002-04-24T13:46:12</created_on>
            <generation>1</generation>
            <id>30B991F927274FA3829655F50C99472E</id>
            <is_current>1</is_current>
            <is_released>0</is_released>
            <keyed_name>Innovator Admin</keyed_name>
            <last_login_date>2018-02-15T21:06:13</last_login_date>
            <login_name>admin</login_name>
            <logon_enabled>1</logon_enabled>
            <major_rev>A</major_rev>
            <modified_on>2004-01-16T20:18:31</modified_on>
            <new_version>0</new_version>
            <not_lockable>0</not_lockable>
            <state>Released</state>
            <working_directory>C:</working_directory>
            <first_name>Innovator</first_name>
            <last_name>Admin</last_name>
          </Item>
        </created_by_id>
      </Item>
      <Item type=""Part"">
        <id>ACF988E9074440D890014ED8D10DC1ED</id>
        <created_by_id>
          <Item>
            <created_on>2002-04-24T13:46:12</created_on>
            <generation>1</generation>
            <id>30B991F927274FA3829655F50C99472E</id>
            <is_current>1</is_current>
            <is_released>0</is_released>
            <keyed_name>Innovator Admin</keyed_name>
            <last_login_date>2018-02-15T21:06:13</last_login_date>
            <login_name>admin</login_name>
            <logon_enabled>1</logon_enabled>
            <major_rev>A</major_rev>
            <modified_on>2004-01-16T20:18:31</modified_on>
            <new_version>0</new_version>
            <not_lockable>0</not_lockable>
            <state>Released</state>
            <working_directory>C:</working_directory>
            <first_name>Innovator</first_name>
            <last_name>Admin</last_name>
          </Item>
        </created_by_id>
        <item_number>FG-01</item_number>
        <description is_null=""1""></description>
        <created_by_id>
          <Item>
            <created_on>2002-04-24T13:46:12</created_on>
            <generation>1</generation>
            <id>30B991F927274FA3829655F50C99472E</id>
            <is_current>1</is_current>
            <is_released>0</is_released>
            <keyed_name>Innovator Admin</keyed_name>
            <last_login_date>2018-02-15T21:06:13</last_login_date>
            <login_name>admin</login_name>
            <logon_enabled>1</logon_enabled>
            <major_rev>A</major_rev>
            <modified_on>2004-01-16T20:18:31</modified_on>
            <new_version>0</new_version>
            <not_lockable>0</not_lockable>
            <state>Released</state>
            <working_directory>C:</working_directory>
            <first_name>Innovator</first_name>
            <last_name>Admin</last_name>
          </Item>
        </created_by_id>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      Assert.AreEqual(expected, amlStr);

      VerifyXmlDocument(new ODataAmlReader(new StringReader(json)), expected);
    }

    [TestMethod()]
    public void OData_NotFoundError()
    {
      var json = @"{""error"":{""code"":""NotFound"",""message"":""No items of type Part found.""}}";
      var oData = new ODataAmlReader(new StringReader(json));
      var aml = XElement.Load(oData);
      var amlStr = aml.ToString();
      var expected = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af=""http://www.aras.com/InnovatorFault"">
      <faultcode>0</faultcode>
      <faultstring>No items of type Part found.</faultstring>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      Assert.AreEqual(expected, amlStr);

      VerifyXmlDocument(new ODataAmlReader(new StringReader(json)), expected);

      var ex = ElementFactory.Local.FromXml(new ODataAmlReader(new StringReader(json))).Exception;
      Assert.IsInstanceOfType(ex, typeof(NoItemsFoundException));
      Assert.AreEqual("No items of type Part found.", ex.Message);
    }

    [TestMethod()]
    public void OData_GeneralError()
    {
      var json = @"{""error"":{""code"":""BadRequest"",""message"":""Could not find a property named \""generation*2\"" on type \""Part\""""}}";
      var oData = new ODataAmlReader(new StringReader(json));
      var aml = XElement.Load(oData);
      var amlStr = aml.ToString();
      var expected = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af=""http://www.aras.com/InnovatorFault"">
      <faultcode>BadRequest</faultcode>
      <faultstring>Could not find a property named ""generation*2"" on type ""Part""</faultstring>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      Assert.AreEqual(expected, amlStr);

      VerifyXmlDocument(new ODataAmlReader(new StringReader(json)), expected);

      var ex = ElementFactory.Local.FromXml(new ODataAmlReader(new StringReader(json))).Exception;
      Assert.IsInstanceOfType(ex, typeof(ServerException));
      Assert.IsNotInstanceOfType(ex, typeof(NoItemsFoundException));
      Assert.AreEqual("Could not find a property named \"generation*2\" on type \"Part\"", ex.Message);
    }

    private void VerifyXmlDocument(XmlReader reader, string expected)
    {
      var doc = new XmlDocument();
      doc.Load(reader);
      var str = new StringWriter();
      using (var writer = XmlWriter.Create(str, new XmlWriterSettings()
      {
        Indent = true,
        IndentChars = "  ",
        OmitXmlDeclaration = true
      }))
      {
        doc.WriteTo(writer);
      }
      Assert.AreEqual(expected, str.ToString());
    }
  }
}
