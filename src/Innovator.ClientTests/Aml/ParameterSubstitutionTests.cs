using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class ParameterSubstitutionTests
  {
    [TestMethod()]
    public void CSharpSubstitute_SimpleTest()
    {
      var sub = new ParameterSubstitution()
      {
        Style = ParameterStyle.CSharp
      };
      sub.AddIndexedParameters(new object[] { "first & second > third", true, new DateTime(2015, 1, 1) });
      var newQuery = sub.Substitute("<Item><name>{0}</name><is_current>{1}</is_current><date>{2:yyyy-MM-dd}</date></Item>", ElementFactory.Local.LocalizationContext);

      Assert.AreEqual("<Item><name>first &amp; second &gt; third</name><is_current>1</is_current><date>2015-01-01T00:00:00</date></Item>", newQuery);
    }

    [TestMethod()]
    public void SubstituteSqlString()
    {
      Assert.AreEqual(@"<sql>SELECT 5.46</sql>"
        , new Command(@"<sql>SELECT @0</sql>", 5.46).ToNormalizedAml(ElementFactory.Local.LocalizationContext));
      Assert.AreEqual(@"<sql>SELECT '1'</sql>"
        , new Command(@"<sql>SELECT @0</sql>", true).ToNormalizedAml(ElementFactory.Local.LocalizationContext));
      Assert.AreEqual(@"<sql>SELECT '0'</sql>"
        , new Command(@"<sql>SELECT @0</sql>", false).ToNormalizedAml(ElementFactory.Local.LocalizationContext));
      Assert.AreEqual(@"<sql>SELECT '2017-01-01T17:00:00'</sql>"
        , new Command(@"<sql>SELECT @0</sql>", DateTime.Parse("2017-01-01T12:00:00")).ToNormalizedAml(ElementFactory.Local.LocalizationContext));
      Assert.AreEqual(@"<sql>SELECT '3373C6B037F14CFABF9968BF9FEA5056'</sql>"
        , new Command(@"<sql>SELECT @0</sql>", new Guid("3373C6B037F14CFABF9968BF9FEA5056")).ToNormalizedAml(ElementFactory.Local.LocalizationContext));
      Assert.AreEqual(@"<sql>SELECT '3373C6B037F14CFABF9968BF9FEA5056'</sql>"
        , new Command(@"<sql>SELECT @0</sql>", "3373C6B037F14CFABF9968BF9FEA5056").ToNormalizedAml(ElementFactory.Local.LocalizationContext));
      Assert.AreEqual(@"<sql>SELECT N'Some string'</sql>"
        , new Command(@"<sql>SELECT @0</sql>", "Some string").ToNormalizedAml(ElementFactory.Local.LocalizationContext));
    }

    [TestMethod()]
    public void SubstituteSqlInClause()
    {
      var ids = new string[] { "3373C6B037F14CFABF9968BF9FEA5056", "C23BE7B25F8C4E8AB9A8293F6D4D2CC7" };
      var cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in (@0)</sql>", ids);
      var aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in ('3373C6B037F14CFABF9968BF9FEA5056','C23BE7B25F8C4E8AB9A8293F6D4D2CC7')</sql>", aml);

      cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in @0</sql>", ids);
      aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in ('3373C6B037F14CFABF9968BF9FEA5056','C23BE7B25F8C4E8AB9A8293F6D4D2CC7')</sql>", aml);

      cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in (@0)</sql>", "3373C6B037F14CFABF9968BF9FEA5056", "C23BE7B25F8C4E8AB9A8293F6D4D2CC7");
      aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in ('3373C6B037F14CFABF9968BF9FEA5056')</sql>", aml);

      cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in @0</sql>", "3373C6B037F14CFABF9968BF9FEA5056", "C23BE7B25F8C4E8AB9A8293F6D4D2CC7");
      aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in ('3373C6B037F14CFABF9968BF9FEA5056')</sql>", aml);

      cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in (@0)</sql>", new int[] { 2, 3 });
      aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in (2,3)</sql>", aml);

      cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in @0</sql>", new int[] { 2, 3 });
      aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in (2,3)</sql>", aml);

      cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in (@0)</sql>", 2, 3);
      aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in (2)</sql>", aml);

      cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in @0</sql>", 2, 3);
      aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in (2)</sql>", aml);

      cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in (@0)</sql>", (object[])null);
      aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in (null)</sql>", aml);

      cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in @0</sql>", (object[])null);
      aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in (null)</sql>", aml);

      cmd = new Command(@"<sql>SELECT * from innovator.[User] where id in @0</sql>", new string[] { });
      aml = cmd.ToNormalizedAml(ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(@"<sql>SELECT * from innovator.[User] where id in (N'`EMTPY_LIST_MUST_MATCH_0_ITEMS!`')</sql>", aml);
    }

    [TestMethod()]
    public void SubstituteStringToItem()
    {
      var factory = ElementFactory.Local;

      var result = factory.FromXml(@"<Item type='Measurements' action='add'>
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
          , true
          , new Guid("6DBCE2E15B6D405793B322355A9E1D9C")
          , null
          , "7DFEF415AB7444BFB9D529665C44D444"
          , "Point 1");
      Assert.AreEqual(new System.DateTime(635802584310000000), result.AssertItem().Property("extradate").AsDateTime());
      Assert.AreEqual(27.878, result.AssertItem().Property("measurement").AsDouble());
      Assert.AreEqual(true, result.AssertItem().Property("notes").AsBoolean());
      Assert.AreEqual("Point 1", result.AssertItem().Property("tag_name").Value);
      var query = result.AssertItem().ToAml();
      Assert.AreEqual("<Item type=\"Measurements\" action=\"add\"><cavity>1</cavity><extradate>2015-10-12T14:53:51</extradate><measurement>27.878</measurement><notes>1</notes><opperator>6DBCE2E15B6D405793B322355A9E1D9C</opperator><session_id /><source>7DFEF415AB7444BFB9D529665C44D444</source><tag_name>Point 1</tag_name></Item>", query);
    }

    [TestMethod()]
    public void InterpolatedString_InterfaceTest()
    {
      var name = "first & second > third";
      var isCurrent = true;
      var date = new DateTime(2015, 1, 1);

      Assert.AreEqual("<Item><name>first &amp; second &gt; third</name><is_current>1</is_current><date>2015-01-01T00:00:00</date></Item>",
        new Command((IFormattable)$"<Item><name>{name}</name><is_current>{isCurrent}</is_current><date>{date:yyyy-MM-dd}</date></Item>").ToNormalizedAml(ElementFactory.Local.LocalizationContext));
      Assert.AreEqual("<Item><name>first &amp; second &gt; third</name><is_current>1</is_current><date>2015-01-01T00:00:00</date></Item>",
        new Command((IFormattable)$@"<Item>
  <name>{name}</name>
  <is_current>{isCurrent}</is_current>
  <date>{date:yyyy-MM-dd}</date>
</Item>").ToNormalizedAml(ElementFactory.Local.LocalizationContext));
    }

#if NET46
    [TestMethod()]
    public void InterpolatedString_SimpleTest()
    {
      var name = "first & second > third";
      var isCurrent = true;
      var date = new DateTime(2015, 1, 1);

      Assert.AreEqual("<Item><name>first &amp; second &gt; third</name><is_current>1</is_current><date>2015-01-01T00:00:00</date></Item>",
        new Command((FormattableString)$"<Item><name>{name}</name><is_current>{isCurrent}</is_current><date>{date:yyyy-MM-dd}</date></Item>").ToNormalizedAml(ElementFactory.Local.LocalizationContext));
      Assert.AreEqual("<Item><name>first &amp; second &gt; third</name><is_current>1</is_current><date>2015-01-01T00:00:00</date></Item>",
        new Command((FormattableString)$@"<Item>
  <name>{name}</name>
  <is_current>{isCurrent}</is_current>
  <date>{date:yyyy-MM-dd}</date>
</Item>").ToNormalizedAml(ElementFactory.Local.LocalizationContext));
    }
#endif

    [TestMethod()]
    public void SubstituteSqlProperty()
    {
      var aml = @"<Item type='Method' action='ApplySelect'>
  <sql><![CDATA[select @0, @1]]></sql>
</Item>";
      Assert.AreEqual(@"<Item type=""Method"" action=""ApplySelect""><sql><![CDATA[select N'test', 2]]></sql></Item>", new Command(aml, "test", 2).ToNormalizedAml(ElementFactory.Local.LocalizationContext));
    }
  }
}
