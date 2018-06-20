#if NETFULL && !NET35
using Innovator.Client.Model;
using Innovator.Client.QueryModel;
using Innovator.Client.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ODataToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Innovator.Client.Queryable.Tests
{
  [TestClass()]
  public class QueryableTests
  {
    private string TestAml(Func<InnovatorQuery<IReadOnlyItem>, IQueryable> expression)
    {
      var conn = new TestConnection();
      var query = conn.Queryable("ItemType");
      var intermediate = expression(query);
      var rendered = intermediate.OfType<object>().ToArray();
      return intermediate.ToString();
    }

    private string TestQueryString(string queryString)
    {
      var conn = new TestConnection();
      var query = conn.Queryable<IReadOnlyItem>("ItemType");
      var intermediate = query.ExecuteOData(queryString, new ExecutionSettings().WithDynamicAccessor((obj, key) => ((IReadOnlyItem)obj).Property(key).Value));
      var result = ((IEnumerable)intermediate).OfType<object>().ToArray();
      //((IEnumerable)query.LinqToQuerystring(queryString, true)).OfType<object>().ToArray();
      return conn.LastRequest.ToNormalizedAml(conn.AmlContext.LocalizationContext);
    }

    [TestMethod()]
    public void Queryable_Attributes()
    {
      var conn = new TestConnection();
      var query = conn.Queryable<ItemType>()
        .Where(i => i.NameProp().Value == "Part")
        .Select(i => new { Name = i.NameProp().Value });
      var result = query.ToArray();
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" select=\"name\"><name>Part</name></Item>", query.ToString());
    }

    [TestMethod()]
    public void Queryable_Basic()
    {
      var aml = TestAml(q => q.Where(i => i.Property("name").Value == "Part"));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name>Part</name></Item>", aml);

      aml = TestAml(q => q.Where(i => i.Property("name").Value == "Part" && i.Generation().AsInt(0) > 1));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name>Part</name><generation condition=\"gt\">1</generation></Item>", aml);

      var name = "Part";
      aml = TestAml(q => q.Where(i => i.Property("name").Value == name));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name>Part</name></Item>", aml);

      aml = TestAml(q => q.Where(i => i.Property("name").Value == "Part" && i.CreatedOn().AsDateTime().Value > new DateTime(2000, 1, 1).AddDays(-30)));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name>Part</name><created_on condition=\"gt\">1999-12-02T00:00:00</created_on></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_WhereTrue()
    {
      var aml = TestAml(q => q.Where(i => true));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" />", aml);
    }

    [TestMethod()]
    public void Queryable_WhereFalse()
    {
      var aml = TestAml(q => q.Where(i => false));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" id=\"FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF\" />", aml);
    }

    [TestMethod()]
    public void Queryable_WhereEntityEqual()
    {
      var a = ElementFactory.Local;
      var compare = a.Item(a.Type("ItemType"), a.Id("4F1AC04A2B484F3ABA4E20DB63808A88"));

      var aml = TestAml(q => q.Where(i => i == compare));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>", aml);

      aml = TestAml(q => q.Where(i => i.Equals(compare)));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_StartsWith()
    {
      var aml = TestAml(q => q.Where(i => i.Property("name").Value.StartsWith("Part")));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name condition=\"like\">Part*</name></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_EndsWith()
    {
      var aml = TestAml(q => q.Where(i => i.Property("name").Value.EndsWith("Part")));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name condition=\"like\">*Part</name></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_Contains()
    {
      var aml = TestAml(q => q.Where(i => i.Property("name").Value.Contains("Part")));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name condition=\"like\">*Part*</name></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_StringNullOrEmpty()
    {
      var aml = TestAml(q => q.Where(i => !string.IsNullOrEmpty(i.Property("name").Value)));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><not><or><name condition=\"is null\" /><name></name></or></not></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_Id()
    {
      var aml = TestAml(q => q.Where(i => i.Id() == "4F1AC04A2B484F3ABA4E20DB63808A88"));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>", aml);

      aml = TestAml(q => q.Where(i => i.Attribute("id").Value == "4F1AC04A2B484F3ABA4E20DB63808A88"));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_BooleanSimple()
    {
      var aml = TestAml(q => q.Where(i => i.Property("is_versionable").AsBoolean(false)));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><is_versionable>1</is_versionable></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_IdsContains()
    {
      var ids = new string[] { "4F1AC04A2B484F3ABA4E20DB63808A88", "B88C14B99EF449828C5D926E39EE8B89" };
      var aml = TestAml(q => q.Where(i => ids.Contains(i.Id())));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><id condition=\"in\">'4F1AC04A2B484F3ABA4E20DB63808A88', 'B88C14B99EF449828C5D926E39EE8B89'</id></Item>", aml);

      var hash = new HashSet<string>(ids);
      aml = TestAml(q => q.Where(i => hash.Contains(i.Id())));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><id condition=\"in\">'4F1AC04A2B484F3ABA4E20DB63808A88', 'B88C14B99EF449828C5D926E39EE8B89'</id></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_ChildItem()
    {
      var aml = TestAml(q => q.Where(i => i.CreatedById().AsItem().KeyedName().Value.Contains("Domke")));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><created_by_id><Item action=\"get\"><keyed_name condition=\"like\">*Domke*</keyed_name></Item></created_by_id></Item>", aml);

      TestAml(q => q.Where(i => i.Property("created_by_id").AsItem().Property("keyed_name").AsString("").Contains("Domke")));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><created_by_id><Item action=\"get\"><keyed_name condition=\"like\">*Domke*</keyed_name></Item></created_by_id></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_SelectBasic()
    {
      var aml = TestAml(q => q.Select(i => new { CreatedOn = i.CreatedOn().Value }));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" select=\"created_on\" />", aml);

      aml = TestAml(q => q.Select(i => new { CreatedOn = i.CreatedOn().Value, CreatedBy = i.CreatedById().AsItem().KeyedName().Value }));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" select=\"created_on,created_by_id(keyed_name)\" />", aml);
    }

    [TestMethod()]
    public void Queryable_SelectBasicWithCondition()
    {
      var aml = TestAml(q => q.Where(i => i.Property("name").Value == "Part").Select(i => new { CreatedOn = i.CreatedOn().Value }));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" select=\"created_on\"><name>Part</name></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_OrderByBasic()
    {
      var aml = TestAml(q => q.OrderBy(i => i.Property("created_on").Value));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" orderBy=\"created_on\" />", aml);

      aml = TestAml(q => q.OrderByDescending(i => i.Property("created_on").Value));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" orderBy=\"created_on DESC\" />", aml);

      aml = TestAml(q => q.OrderBy(i => i.Property("created_on").Value).ThenBy(i => i.Generation().Value));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" orderBy=\"created_on,generation\" />", aml);

      aml = TestAml(q => q.OrderBy(i => i.Property("created_on").Value).ThenByDescending(i => i.Generation().Value));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" orderBy=\"created_on,generation DESC\" />", aml);
    }

    [TestMethod()]
    public void Queryable_ChildItemLogical()
    {
      var aml = TestAml(q => q.Where(i => i.CreatedById().AsItem().KeyedName().Value.Contains("Domke") && i.CreatedById().AsItem().IsReleased().AsBoolean(true) || i.CreatedById().AsItem().Generation().AsInt(0) > 0));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><created_by_id><Item action=\"get\"><or><and><keyed_name condition=\"like\">*Domke*</keyed_name><is_released>1</is_released></and><generation condition=\"gt\">0</generation></or></Item></created_by_id></Item>", aml);

      aml = TestAml(q => q.Where(i => i.CreatedById().AsItem().KeyedName().Value.Contains("Domke") && (i.CreatedById().AsItem().IsReleased().AsBoolean(true) || i.CreatedById().AsItem().Generation().AsInt(0) > 0)));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><created_by_id><Item action=\"get\"><keyed_name condition=\"like\">*Domke*</keyed_name><or><is_released>1</is_released><generation condition=\"gt\">0</generation></or></Item></created_by_id></Item>", aml);

      aml = TestAml(q => q.Where(i => i.CreatedById().AsItem().KeyedName().Value.Contains("Domke") && i.CreatedById().AsItem().Generation().AsInt(0) > 0));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><created_by_id><Item action=\"get\"><keyed_name condition=\"like\">*Domke*</keyed_name><generation condition=\"gt\">0</generation></Item></created_by_id></Item>", aml);

      aml = TestAml(q => q.Where(i => i.CreatedById().AsItem().KeyedName().Value.Contains("Domke") || i.Generation().AsInt(0) > 0));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><or><created_by_id><Item action=\"get\"><keyed_name condition=\"like\">*Domke*</keyed_name></Item></created_by_id><generation condition=\"gt\">0</generation></or></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_Take()
    {
      var aml = TestAml(q => q.Take(5));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" maxRecords=\"5\" />", aml);
    }

    [TestMethod()]
    public void Queryable_SkipTake()
    {
      var aml = TestAml(q => q.Skip(5).Take(5));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" page=\"2\" pagesize=\"5\" />", aml);
    }

    [TestMethod()]
    public void QueryString_Basic()
    {
      var aml = TestQueryString("?$filter=Name eq 'Part'");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name>Part</name></Item>", aml);

      aml = TestQueryString("?$filter='Part' eq Name");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name>Part</name></Item>", aml);

      aml = TestQueryString("?$filter=not (Name eq 'Apple')");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name condition=\"ne\">Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=Name ne 'Apple'");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name condition=\"ne\">Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=not (Name ne 'Apple')");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name>Apple</name></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_SingleBoolean()
    {
      var aml = TestQueryString("?$filter=is_versionable");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><is_versionable>1</is_versionable></Item>", aml);

      aml = TestQueryString("?$filter=not is_versionable");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><is_versionable>0</is_versionable></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Int()
    {
      var aml = TestQueryString("?$filter=generation ge 3");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><generation condition=\"ge\">3</generation></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_GuidId()
    {
      var aml = TestQueryString("?$filter=id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Date()
    {
      var aml = TestQueryString("?$filter=created_on eq datetime'2002-01-01T00:00'");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><created_on>2002-01-01T00:00:00</created_on></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Conditional()
    {
      var aml = TestQueryString("?$filter=created_on eq datetime'2002-01-01T00:00' and generation ge 3");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></Item>", aml);

      aml = TestQueryString("?$filter=created_on eq datetime'2002-01-01T00:00' and generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><or><and><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>", aml);

      aml = TestQueryString("?$filter=(created_on eq datetime'2002-01-01T00:00' and generation ge 3) or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><or><and><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>", aml);

      aml = TestQueryString("?$filter=created_on eq datetime'2002-01-01T00:00' and (generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88')");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><created_on>2002-01-01T00:00:00</created_on><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>", aml);

      aml = TestQueryString("?$filter=(generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88') and created_on eq datetime'2002-01-01T00:00'");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or><created_on>2002-01-01T00:00:00</created_on></Item>", aml);

      aml = TestQueryString("?$filter=generation ge 3 or (id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88' and created_on eq datetime'2002-01-01T00:00')");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><or><generation condition=\"ge\">3</generation><and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_on>2002-01-01T00:00:00</created_on></and></or></Item>", aml);

      aml = TestQueryString("?$filter=generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88' or created_on eq datetime'2002-01-01T00:00'");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_on>2002-01-01T00:00:00</created_on></or></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Functions()
    {
      var aml = TestQueryString("?$callback=jQuery112304312923812233427_1494592722830&%24inlinecount=allpages&%24format=json&%24filter=startswith(tolower(name)%2C%27c%27)");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><name condition=\"like\">c*</name></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_SkipTake()
    {
      var aml = TestQueryString("?$filter=Name eq 'Apple'&$top=1");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" maxRecords=\"1\"><name>Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=Name eq 'Apple'&$top=3");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" maxRecords=\"3\"><name>Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=Name eq 'Apple'&$skip=3");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" page=\"2\" pagesize=\"3\"><name>Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=Name eq 'Apple'&$skip=4&$top=2");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" page=\"3\" pagesize=\"2\"><name>Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=Name eq 'Apple'&$top=2&$skip=4");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" page=\"3\" pagesize=\"2\"><name>Apple</name></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Ordering()
    {
      var aml = TestQueryString("?$orderby=Name");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" orderBy=\"name\" />", aml);

      aml = TestQueryString("?$orderby=Name asc");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" orderBy=\"name\" />", aml);

      aml = TestQueryString("?$orderby=Name desc");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" orderBy=\"name DESC\" />", aml);
    }


    [TestMethod()]
    public void QueryString_Random()
    {
      var aml = TestQueryString("?$callback=jQuery1124032885557251554487_1494968811401&%24inlinecount=allpages&%24format=json&%24filter=startswith(tolower(keyed_name)%2C%27john+doe%27)");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><keyed_name condition=\"like\">john doe*</keyed_name></Item>", aml);
    }


    [TestMethod()]
    public void QueryString_ModifyQuery()
    {
      var conn = new TestConnection();
      var query = conn.Queryable<IReadOnlyItem>("ItemType", new QuerySettings()
      {
        ModifyQuery = i =>
{
  i.Action().Set("GetOData");
  i.MaxRecords().Set(10);
}
      });
      var intermediate = query.ExecuteOData("?$orderby=Name", new ExecutionSettings().WithDynamicAccessor((obj, key) => ((IReadOnlyItem)obj).Property(key).Value));
      var result = ((IEnumerable)intermediate).OfType<object>().ToArray();
      var aml = conn.LastRequest.ToNormalizedAml(conn.AmlContext.LocalizationContext);
      Assert.AreEqual("<Item type=\"ItemType\" action=\"GetOData\" orderBy=\"name\" maxRecords=\"10\" />", aml);
    }

    [TestMethod()]
    public void QueryString_Projection()
    {
      var aml = TestQueryString("?$select=Name");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" select=\"name\" />", aml);

      aml = TestQueryString("?$select=name,id");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" select=\"name,id\" />", aml);

      aml = TestQueryString("?$select=name,id&$orderby=name");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" select=\"name,id\" orderBy=\"name\" />", aml);

      aml = TestQueryString("?$select=name,id&$orderby=name,created_on desc");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\" select=\"name,id\" orderBy=\"name,created_on DESC\" />", aml);
    }

    [TestMethod()]
    public void Queryable_Include()
    {
      var aml = TestAml(q => q.Include("created_by_id").Where(i => i.Id() == "4F1AC04A2B484F3ABA4E20DB63808A88"));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_by_id><Item action=\"get\" /></created_by_id></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Include()
    {
      var aml = TestQueryString("?$filter=id eq '4F1AC04A2B484F3ABA4E20DB63808A88'&$expand=created_by_id");
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_by_id><Item action=\"get\" /></created_by_id></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Nested()
    {
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><created_by_id><Item action=\"get\"><keyed_name>Test</keyed_name></Item></created_by_id></Item>"
        , TestQueryString("?$filter=created_by_id/keyed_name eq 'Test'"));

      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><created_by_id><Item action=\"get\"><keyed_name>Test</keyed_name><state>Something</state></Item></created_by_id></Item>"
        , TestQueryString("?$filter=created_by_id/keyed_name eq 'Test' and created_by_id/state eq 'Something'"));
    }

    [TestMethod()]
    public void Queryable_Function_Now()
    {
      var model = QueryItem.FromLinq("Part", q => q.Where(i => i.ModifiedOn().AsDateTime() > DateTime.Now));
      Assert.IsTrue(model.Where is GreaterThanOperator op && op.Right is QueryModel.Functions.CurrentDateTime);
    }

    [TestMethod()]
    public void Queryable_Function_DayDiff()
    {
      var model = QueryItem.FromLinq("Part", q => q.Where(i => (i.ModifiedOn().AsDateTime(DateTime.MinValue) - i.CreatedOn().AsDateTime(DateTime.MinValue)).TotalDays > 4));
      Assert.IsTrue(model.Where is GreaterThanOperator op
        && op.Left is QueryModel.Functions.DiffDays diff
        && diff.StartExpression is PropertyReference prop
        && prop.Name == "created_on");
    }

    [TestMethod()]
    public void Queryable_Function_Regex()
    {
      var model = QueryItem.FromLinq("Part", q => q.Where(i => System.Text.RegularExpressions.Regex.IsMatch(i.Property("item_number").Value, @"\d{3}-\d{4}")));
      Assert.IsTrue(model.Where is LikeOperator op && op.Right is PatternList);
    }

    [TestMethod()]
    public void Queryable_Relationships()
    {
      var aml = TestAml(q => q.Where(i => i.Relationships("Property").Any(r => r.Property("name").Value == "ebs_orgs")));
      Assert.AreEqual("<Item type=\"ItemType\" action=\"get\"><Relationships><Item type=\"Property\" action=\"get\"><name>ebs_orgs</name></Item></Relationships></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_Count()
    {
      var aml = QueryItem.FromLinq("Part", q => q.Where(i => i.Property("item_number").Value.StartsWith("905-")).Count()).ToAml();
      Assert.AreEqual("<Item type=\"Part\" action=\"get\" returnMode=\"countOnly\" select=\"id\" page=\"1\" pagesize=\"1\"><item_number condition=\"like\">905-*</item_number></Item>", aml);

      aml = QueryItem.FromLinq("Part", q => q.Count(i => i.Property("item_number").Value.StartsWith("905-"))).ToAml();
      Assert.AreEqual("<Item type=\"Part\" action=\"get\" returnMode=\"countOnly\" select=\"id\" page=\"1\" pagesize=\"1\"><item_number condition=\"like\">905-*</item_number></Item>", aml);

      var conn = new TestConnection();
      var count = conn.Queryable("ItemType").Count(i => i.Property("name").Value == "Part");
      Assert.AreEqual(1, count);
    }
  }
}
#endif
