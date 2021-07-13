using Innovator.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class ServerExceptionTests
  {
#if NETFULL
    [TestMethod()]
    public void VerifySerialization()
    {
      var factory = ElementFactory.Local;

      var ex = new ServerException("A bad exception");
      var expected = ex.ToString();
      ex = SerializeException<ServerException>(ex);
      Assert.AreEqual(expected, ex.ToString());

      ex = factory.NoItemsFoundException("Part", "<query />");
      expected = ex.ToString();
      ex = SerializeException<ServerException>(ex);
      Assert.AreEqual(expected, ex.ToString());

      ex = factory.ValidationException("Missing Properties", factory.Item(factory.Type("Method")), "owned_by_id", "managed_by_id");
      expected = ex.ToString();
      ex = SerializeException<ServerException>(ex);
      Assert.AreEqual(expected, ex.ToString());
    }
#endif

    [TestMethod()]
    public void ExceptionToAml()
    {
      var aml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af=""http://www.aras.com/InnovatorFault"">
      <faultcode>0</faultcode>
      <faultstring>No items of type SavedSearch found.</faultstring>
      <detail>
        <af:legacy_detail>No items of type SavedSearch found.</af:legacy_detail>
        <af:legacy_faultstring>No items of type 'SavedSearch' found using the criteria:
&lt;Item type=""SavedSearch"" action=""get""&gt;
  &lt;is_email_subscription&gt;1&lt;/is_email_subscription&gt;
  &lt;itname&gt;asdfasdfasdf&lt;/itname&gt;
&lt;/Item&gt;
</af:legacy_faultstring>
        <af:legacy_faultactor>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</af:legacy_faultactor>
      </detail>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      var exAml = ElementFactory.Local.FromXml(aml).Exception.ToAml();
      var expected = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/""><SOAP-ENV:Body><SOAP-ENV:Fault xmlns:af=""http://www.aras.com/InnovatorFault""><faultcode>0</faultcode><faultstring>No items of type SavedSearch found.</faultstring><detail><af:legacy_detail>No items of type SavedSearch found.</af:legacy_detail><af:legacy_faultstring>No items of type 'SavedSearch' found using the criteria:
&lt;Item type=""SavedSearch"" action=""get""&gt;
  &lt;is_email_subscription&gt;1&lt;/is_email_subscription&gt;
  &lt;itname&gt;asdfasdfasdf&lt;/itname&gt;
&lt;/Item&gt;
</af:legacy_faultstring><af:legacy_faultactor>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</af:legacy_faultactor></detail></SOAP-ENV:Fault></SOAP-ENV:Body></SOAP-ENV:Envelope>";
      Assert.AreEqual(expected, exAml);
    }

    [TestMethod()]
    public void ThrownExceptionStackTrace()
    {
      var aml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af=""http://www.aras.com/InnovatorFault"">
      <faultcode>0</faultcode>
      <faultstring>No items of type SavedSearch found.</faultstring>
      <detail>
        <af:legacy_detail>No items of type SavedSearch found.</af:legacy_detail>
        <af:legacy_faultstring>No items of type 'SavedSearch' found using the criteria:
&lt;Item type=""SavedSearch"" action=""get""&gt;
  &lt;is_email_subscription&gt;1&lt;/is_email_subscription&gt;
  &lt;itname&gt;asdfasdfasdf&lt;/itname&gt;
&lt;/Item&gt;
</af:legacy_faultstring>
        <af:legacy_faultactor>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</af:legacy_faultactor>
      </detail>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";

      var serverEx = Assert.ThrowsException<NoItemsFoundException>(() => ElementFactory.Local.FromXml(aml).AssertNoError());

      var str = serverEx.ToString();
      Assert.IsTrue(str.IndexOf("[Server") > 0);

      str = ElementFactory.Local.FromXml(aml).Exception.ToString();
      Assert.IsTrue(str.IndexOf("[Server") > 0);
    }

    [TestMethod()]
    public void AssertNoErrorNoItemsFound()
    {
      var aml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af=""http://www.aras.com/InnovatorFault"">
      <faultcode>0</faultcode>
      <faultstring>No items of type SavedSearch found.</faultstring>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";

      var result = ElementFactory.Local.FromXml(aml);
      try
      {
        result.AssertNoError(true);
      }
      catch (NoItemsFoundException)
      {
        Assert.Fail("AssertNoError(true) should not throw NoItemsFoundExceptions");
      }
      Assert.ThrowsException<NoItemsFoundException>(() => result.AssertNoError());
      Assert.ThrowsException<NoItemsFoundException>(() => result.AssertNoError(false));
    }

    [TestMethod()]
    public void ValidationReportException()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Type("Part"), aml.Id("46258F3D37F243E7B160837790817C80"));
      var report = "Report content";
      var ex = aml.ValidationException("Validation report", item, report);
      Assert.AreEqual(report, ex.Report);
      Assert.AreEqual(item, ex.Item);

      var result = aml.FromXml(ex.ToAml());
      ex = (ValidationReportException)result.Exception;
      Assert.AreEqual(report, ex.Report);
      Assert.AreEqual(item, ex.Item);
    }

    [TestMethod()]
    public void ValidationException()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Type("Part"), aml.Id("46258F3D37F243E7B160837790817C80"));
      var props = new string[] { "owned_by_id", "managed_by_id" };
      var ex = aml.ValidationException("Validation report", item, props);
      CollectionAssert.AreEqual(props, ex.Properties.ToArray());
      Assert.AreEqual(item, ex.Item);

      var result = aml.FromXml(ex.ToAml());
      ex = (ValidationException)result.Exception;
      CollectionAssert.AreEqual(props, ex.Properties.ToArray());
      Assert.AreEqual(item, ex.Item);
    }


    [TestMethod()]
    public void ExceptionMetadata()
    {
      var conn = new TestConnection();
      var query = "<Item type='thing' action='get'></Item>";
      var result = conn.Apply(query);
      Assert.AreEqual(query, result.Exception.Query);
      Assert.AreEqual("Test", result.Exception.Database);

      var query2 = "<Item type='@0' action='get'></Item>";
      result = conn.Apply(query2, "another");
      Assert.AreEqual("<Item type=\"another\" action=\"get\" />", result.Exception.Query);
      Assert.AreEqual("Test", result.Exception.Database);
    }

    [TestMethod]
    public void HttpExceptionMetadata()
    {
      var ex = Assert.ThrowsException<AggregateException>(() =>
      {
        var conn = Factory.GetConnection("http://invalid.example.com", "test agent");
        conn.Login(new ExplicitCredentials("db", "user", "pass"));
      }).InnerException;
      Assert.AreEqual("ValidateUser", ex.Data["soap_action"]);
      Assert.AreEqual("http://invalid.example.com/Server/InnovatorServer.aspx", ex.Data["url"].ToString());
    }

    [TestMethod]
    public void LogMetadata()
    {
      var _data = default(Dictionary<string, object>);

      Factory.LogListener = (level, msg, p) =>
      {
        _data = p
          .ToLookup(k => k.Key, k => k.Value)
          .ToDictionary(k => k.Key, k => k.First());
      };

      try
      {
        var conn = Factory.GetConnection("http://invalid.example.com", "test agent");
        conn.Login(new ExplicitCredentials("db", "user", "pass"));
      }
      catch (Exception) { }

      Assert.IsNotNull(_data);
      Assert.AreEqual("ValidateUser", _data["soap_action"]);
      Assert.AreEqual("http://invalid.example.com/Server/InnovatorServer.aspx", _data["url"].ToString());
    }

    private T SerializeException<T>(Exception ex) where T : Exception
    {
      var bf = new BinaryFormatter();
      using (var ms = new MemoryStream())
      {
        // "Save" object state
        bf.Serialize(ms, ex);

        // Re-use the same stream for de-serialization
        ms.Seek(0, 0);

        // Replace the original exception with de-serialized one
        return (T)bf.Deserialize(ms);
      }
    }
  }
}
