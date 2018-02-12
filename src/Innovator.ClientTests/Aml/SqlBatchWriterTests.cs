using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class SqlBatchWriterTests
  {
    [TestMethod()]
    public void SqlBatchWriter_ParamSubs()
    {
      var conn = new TestConnection();
      var sql = new SqlBatchWriter(conn);
      sql.Command("insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (@0, @1, 'Asset', @2);", "Zero", "One", "Two");
      sql.Command("insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (@0, @1, 'Asset', @2);", "2Zero", "2One", "2Two");
      sql.Command("insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (@0, @1, 'Asset', @2);", null, "3One", "3Two");
      Assert.AreEqual(@"<sql>insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (N&apos;Zero&apos;, N&apos;One&apos;, &apos;Asset&apos;, N&apos;Two&apos;);
insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (N&apos;2Zero&apos;, N&apos;2One&apos;, &apos;Asset&apos;, N&apos;2Two&apos;);
insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (null, N&apos;3One&apos;, &apos;Asset&apos;, N&apos;3Two&apos;);
</sql>", sql.ToString());

      sql = new SqlBatchWriter(conn);
      sql.Command("insert into @tableVar values (@0, @1, @2);", 1, null, true);
      Assert.AreEqual(@"<sql>insert into @tableVar values (1, null, &apos;1&apos;);
</sql>", sql.ToString());
    }

    [TestMethod()]
    public void SqlBatchWriter_NoConnection()
    {
      var sql = new SqlBatchWriter();
      sql.Command("insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (@0, @1, 'Asset', @2);", "Zero", "One", "Two");
      sql.Command("insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (@0, @1, 'Asset', @2);", "2Zero", "2One", "2Two");
      sql.Command("insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (@0, @1, 'Asset', @2);", null, "3One", "3Two");
      Assert.AreEqual(@"insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (N'Zero', N'One', 'Asset', N'Two');
insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (N'2Zero', N'2One', 'Asset', N'2Two');
insert into innovator._sync_mes_entity_tester (id, item_number, classification, line) values (null, N'3One', 'Asset', N'3Two');
", sql.ToString());

      sql = new SqlBatchWriter();
      sql.Command("insert into @tableVar values (@0, @1, @2);", 1, null, true);
      Assert.AreEqual(@"insert into @tableVar values (1, null, '1');
", sql.ToString());

      sql = new SqlBatchWriter();
      var date = new DateTime(2017, 2, 3, 4, 15, 20, DateTimeKind.Utc).ToLocalTime();
      sql.Command("insert into @tableVar values (@0, @1, @2);", 1, null, date);
      Assert.AreEqual(@"insert into @tableVar values (1, null, '2017-02-03T04:15:20');
", sql.ToString());
    }
  }
}
