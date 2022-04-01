using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class ConnectionExtensionsTests
  {
    [TestMethod()]
    public void ItemByIdTest()
    {
      var conn = new TestConnection();
      var result = conn.ItemById("Company", "0E086FFA6C4646F6939B74C43D094182", i => new
      {
        FirstName = i.CreatedById().AsItem().Property("first_name").Value,
        PermName = i.PermissionId().AsItem().Property("name").Value,
        KeyedName = i.Property("id").KeyedName().Value,
        Empty = i.ManagedById().Value
      });
      Assert.AreEqual("First", result.FirstName);
      Assert.AreEqual("Company", result.PermName);
      Assert.AreEqual("Another Company", result.KeyedName);
      Assert.AreEqual(null, result.Empty);
    }

    [TestMethod()]
    public void ItemByKeyedNameTest()
    {
      var conn = new TestConnection();
      var result = conn.ItemByKeyedName("Company", "Another Company");
      Assert.AreEqual("Another Company", result.KeyedName().Value);
    }

    [TestMethod()]
    public void ApplySqlTest()
    {
      var req = new Command(@"select c.id cad_id, c.VIEWABLE_FILE id, f.FILENAME
from innovator.CAD c
inner join innovator.[FILE] f
on f.ID = c.VIEWABLE_FILE
where (
  isnull(c.VIEWABLE_FILE, '') <> isnull(c.VIEWABLE_Watermark, '')
)
and c.state = @0", "Preliminary");
      var conn = new TestConnection();
      var result = conn.ApplySql(req);
      var aml = req.ToNormalizedAml(conn.AmlContext.LocalizationContext);
      Assert.AreEqual(@"<sql>select c.id cad_id, c.VIEWABLE_FILE id, f.FILENAME
from innovator.CAD c
inner join innovator.[FILE] f
on f.ID = c.VIEWABLE_FILE
where (
  isnull(c.VIEWABLE_FILE, '') &lt;&gt; isnull(c.VIEWABLE_Watermark, '')
)
and c.state = N'Preliminary'</sql>", aml);
    }

    [TestMethod()]
    public void TokenParsing()
    {
      var tokenStr = "thing.eyJuYmYiOjE1NjM5MDUzNzIsImV4cCI6MTU2MzkwODk3MiwiaXNzIjoiT0F1dGhTZXJ2ZXIiLCJhdWQiOlsiT0F1dGhTZXJ2ZXIvcmVzb3VyY2VzIiwiSW5ub3ZhdG9yIl0sImNsaWVudF9pZCI6Iklubm92YXRvckNsaWVudCIsInN1YiI6ImFkbWluIiwiYXV0aF90aW1lIjoxNTYzOTA1MzcyLCJpZHAiOiJsb2NhbCIsInVzZXJuYW1lIjoiYWRtaW4iLCJkYXRhYmFzZSI6Iklubm92YXRvcjEyIiwic2NvcGUiOlsib3BlbmlkIiwiSW5ub3ZhdG9yIl0sImFtciI6WyJwd2QiXX0.other";
      var cred = new TokenCredentials(tokenStr);
      Assert.AreEqual("Innovator12", cred.Database);
      Assert.AreEqual("admin", cred.Username);
      Assert.AreEqual(DateTime.Parse("Tuesday, July 23, 2019 7:09:32 PM", CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AssumeUniversal).ToUniversalTime()
        , cred.Expires);
    }
  }
}
