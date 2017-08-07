using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class AmlReaderTests
  {
    [TestMethod()]
    public void XmlReader_Result()
    {
      const string input = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='File' typeId='8052A558B9084D41B9F11805E464F443' id='1CD793698353444CA6DF901A732A523B'>
        <classification>/*</classification>
      </Item>
    </Result>
    <Message>
      <event name='items_with_no_access_count' value='83' />
    </Message>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      const string expected = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"File\" typeId=\"8052A558B9084D41B9F11805E464F443\" id=\"1CD793698353444CA6DF901A732A523B\"><classification>/*</classification></Item></Result><Message><event name=\"items_with_no_access_count\" value=\"83\" /></Message></SOAP-ENV:Body></SOAP-ENV:Envelope>";

      var result = ElementFactory.Local.FromXml(input);
      VerifyXml(() => result.CreateReader(), expected);
    }

    [TestMethod()]
    public void XmlReader_Exception()
    {
      const string input = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af='http://www.aras.com/InnovatorFault'>
      <faultcode>0</faultcode>
      <faultstring>No items of type File found.</faultstring>
      <detail>
        <af:legacy_detail>No items of type File found.</af:legacy_detail>
        <af:legacy_faultstring>No items of type 'File' found</af:legacy_faultstring>
        <af:legacy_faultactor>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</af:legacy_faultactor>
        <message key='items_with_no_access_count' value='83' />
      </detail>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      const string expected = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><SOAP-ENV:Fault xmlns:af=\"http://www.aras.com/InnovatorFault\"><faultcode>0</faultcode><faultstring>No items of type File found.</faultstring><detail><af:legacy_detail>No items of type File found.</af:legacy_detail><af:legacy_faultstring>No items of type 'File' found</af:legacy_faultstring><af:legacy_faultactor>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</af:legacy_faultactor><message key=\"items_with_no_access_count\" value=\"83\" /></detail></SOAP-ENV:Fault></SOAP-ENV:Body></SOAP-ENV:Envelope>";

      var result = ElementFactory.Local.FromXml(input);
      VerifyXml(() => new AmlReader(result), expected);
    }

    private void VerifyXml(Func<XmlReader> factory, string expected)
    {
      var doc = new XmlDocument();
      doc.Load(factory());
      Assert.AreEqual(expected, doc.OuterXml);

      var xDoc = XElement.Load(factory());
      Assert.AreEqual(expected, xDoc.ToString(SaveOptions.DisableFormatting));
    }

#if XMLLEGACY
    [TestMethod()]
    public void XPath_WhereUsed()
    {
      var aml = ElementFactory.Local;
      var result = aml.FromXml(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
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

      var strs = (string[])result.AssertItem().Element("relatedItems").XPath().Evaluate("./Item[@type='Part']/@id");
      CollectionAssert.AreEqual(new[] { "F7C39CE1AB4245D4A1695075BC7F9B49", "F921E5F7576342698771C2539AFC23BD" }, strs);

      var elem = result.XPath().SelectElement("//Item[@type='Affected Item']");
      Assert.AreEqual("ACD4739883864B548FE0671634CB7670", elem.Attribute("id").Value);

      // Make sure that the structure didn't mutate and that it still works a second time.
      elem = result.XPath().SelectElement("//Item[@type='Affected Item']");
      Assert.AreEqual("ACD4739883864B548FE0671634CB7670", elem.Attribute("id").Value);

      var str = result.XPath().Evaluate("//Item[@type='Affected Item']/@id");
      Assert.AreEqual("ACD4739883864B548FE0671634CB7670", str);

      strs = (string[])result.XPath().Evaluate("//Item[@type='Part']/@id");
      CollectionAssert.AreEqual(new[] { "60664D33AEC245BBBEAEFC46612B87F5", "F7C39CE1AB4245D4A1695075BC7F9B49", "F921E5F7576342698771C2539AFC23BD" }, strs);

      // Check this again after the result changed
      strs = (string[])result.AssertItem().Element("relatedItems").XPath().Evaluate("./Item[@type='Part']/@id");
      CollectionAssert.AreEqual(new[] { "F7C39CE1AB4245D4A1695075BC7F9B49", "F921E5F7576342698771C2539AFC23BD" }, strs);

      Assert.AreEqual(0, result.XPath().SelectElements("*[@id]").ToArray().Length);
      Assert.AreEqual(4, result.XPath().SelectElements("//*[@id]").ToArray().Length);

      Assert.AreEqual("F7C39CE1AB4245D4A1695075BC7F9B49", result.XPath().SelectElement("//relatedItems/Item[2]").Attribute("id").Value);
      Assert.AreEqual("F921E5F7576342698771C2539AFC23BD", result.XPath().SelectElement("//relatedItems/Item[last()]").Attribute("id").Value);

      Assert.AreEqual(3.0, result.XPath().Evaluate("count(//Item[@type='Part'])"));
    }
#endif
  }
}
