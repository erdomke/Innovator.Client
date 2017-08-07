using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class CommandTests
  {
    [TestMethod()]
    public void ToNormalizedAmlTest()
    {
      var cmd = new Command(@"<Item type='Measurements' action='add'>
                                    <cavity>@0</cavity>
                                    <extradate>@1</extradate>
                                    <measurement>@2</measurement>
                                    <notes>@3</notes>
                                    <opperator>@4</opperator>
                                    <session_id>@5</session_id>
                                    <source>@6</source>
                                    <tag_name>@7</tag_name>
                                  </Item>"
          , "1"
          , new System.DateTime(635802584310000000)
          , 27.878
          , "Nominal = 27.90; USL = 28.00; LSL = 27.80; Average = 27.88133; StdDev = 0.00684"
          , "Matt"
          , "SEC-0000051120"
          , "7DFEF415AB7444BFB9D529665C44D444"
          , "Point 1");
      var factory = ElementFactory.Local;
      var query = cmd.ToNormalizedAml(factory.LocalizationContext);
      Assert.AreEqual("<Item type=\"Measurements\" action=\"add\"><cavity>1</cavity><extradate>2015-10-12T14:53:51</extradate><measurement>27.878</measurement><notes>Nominal = 27.90; USL = 28.00; LSL = 27.80; Average = 27.88133; StdDev = 0.00684</notes><opperator>Matt</opperator><session_id>SEC-0000051120</session_id><source>7DFEF415AB7444BFB9D529665C44D444</source><tag_name>Point 1</tag_name></Item>", query);
    }

    [TestMethod()]
    public void ToNormalizedAmlTest_Enumerable()
    {
      var values = new[] { "Matt", "Eric" };
      var cmd = new Command(@"<Item type='Measurements' action='get'>
                                <simple>@0</simple>
                                <in_clause condition='in'>@1</in_clause>
                                <no_values condition='in'>@2</no_values>
                              </Item>"
          , values
          , values
          , new string[] { });
      var factory = ElementFactory.Local;
      var query = cmd.ToNormalizedAml(factory.LocalizationContext);
      Assert.AreEqual("<Item type=\"Measurements\" action=\"get\"><simple>Matt,Eric</simple><in_clause condition=\"in\">N'Matt',N'Eric'</in_clause><no_values condition=\"in\">N'`EMTPY_VALUE_LIST`'</no_values></Item>", query);
    }

    [TestMethod]
    public void ToNormalizedAmlTest_IProperty()
    {
      var conn = new TestConnection();
      var query = "<Item type='Company' action='get' id='0E086FFA6C4646F6939B74C43D094182'></Item>";
      var result = conn.Apply(query);

      var cmd = new Command(@"<Item type='Permission' action='get' id='@0'></Item>"
          , result.AssertItem().PermissionId());
      var factory = ElementFactory.Local;
      var queryString = cmd.ToNormalizedAml(factory.LocalizationContext);
      Assert.AreEqual("<Item type=\"Permission\" action=\"get\" id=\"A8FC3EC44ED0462B9A32D4564FAC0AD8\" />", queryString);

      cmd = new Command(@"<Item type='Permission' action='get'>@0!</Item>"
          , result.AssertItem().PermissionId());
      queryString = cmd.ToNormalizedAml(factory.LocalizationContext);
      Assert.AreEqual("<Item type=\"Permission\" action=\"get\"><permission_id keyed_name=\"Company\" type=\"Permission\"><Item type=\"Permission\" typeId=\"C6A89FDE1294451497801DF78341B473\" id=\"A8FC3EC44ED0462B9A32D4564FAC0AD8\"><id keyed_name=\"Company\" type=\"Permission\">A8FC3EC44ED0462B9A32D4564FAC0AD8</id><name>Company</name></Item></permission_id></Item>", queryString);
    }
  }
}
