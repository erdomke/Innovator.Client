#if NETFULL && !NET35
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Innovator.Client.Queryable;
using System.Collections;
using Innovator.Client.Model;
using ODataToolkit;

namespace Innovator.Client.Tests
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
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" select=\"name\"><name>Part</name></Item>", query.ToString());
    }

    [TestMethod()]
    public void Queryable_Basic()
    {
      var aml = TestAml(q => q.Where(i => i.Property("name").Value == "Part"));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name>Part</name></Item>", aml);

      aml = TestAml(q => q.Where(i => i.Property("name").Value == "Part" && i.Generation().AsInt(0) > 1));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name>Part</name><generation condition=\"gt\">1</generation></Item>", aml);

      var name = "Part";
      aml = TestAml(q => q.Where(i => i.Property("name").Value == name));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name>Part</name></Item>", aml);

      aml = TestAml(q => q.Where(i => i.Property("name").Value == "Part" && i.CreatedOn().AsDateTime().Value > new DateTime(2000, 1, 1).AddDays(-30)));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name>Part</name><created_on condition=\"gt\">1999-12-02T00:00:00</created_on></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_WhereTrue()
    {
      var aml = TestAml(q => q.Where(i => true));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" />", aml);
    }

    [TestMethod()]
    public void Queryable_WhereFalse()
    {
      var aml = TestAml(q => q.Where(i => false));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" id=\"FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF\" />", aml);
    }

    [TestMethod()]
    public void Queryable_WhereEntityEqual()
    {
      var a = ElementFactory.Local;
      var compare = a.Item(a.Type("ItemType"), a.Id("4F1AC04A2B484F3ABA4E20DB63808A88"));

      var aml = TestAml(q => q.Where(i => i == compare));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>", aml);

      aml = TestAml(q => q.Where(i => i.Equals(compare)));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_StartsWith()
    {
      var aml = TestAml(q => q.Where(i => i.Property("name").Value.StartsWith("Part")));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name condition=\"like\">Part*</name></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_EndsWith()
    {
      var aml = TestAml(q => q.Where(i => i.Property("name").Value.EndsWith("Part")));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name condition=\"like\">*Part</name></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_Contains()
    {
      var aml = TestAml(q => q.Where(i => i.Property("name").Value.Contains("Part")));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name condition=\"like\">*Part*</name></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_StringNullOrEmpty()
    {
      var aml = TestAml(q => q.Where(i => !string.IsNullOrEmpty(i.Property("name").Value)));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><not><name condition=\"is null\"></name></not></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_Id()
    {
      var aml = TestAml(q => q.Where(i => i.Id() == "4F1AC04A2B484F3ABA4E20DB63808A88"));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>", aml);

      aml = TestAml(q => q.Where(i => i.Attribute("id").Value == "4F1AC04A2B484F3ABA4E20DB63808A88"));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_BooleanSimple()
    {
      var aml = TestAml(q => q.Where(i => i.Property("is_versionable").AsBoolean(false)));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><is_versionable>1</is_versionable></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_IdsContains()
    {
      var ids = new string[] { "4F1AC04A2B484F3ABA4E20DB63808A88", "B88C14B99EF449828C5D926E39EE8B89" };
      var aml = TestAml(q => q.Where(i => ids.Contains(i.Id())));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><id condition=\"in\">'4F1AC04A2B484F3ABA4E20DB63808A88','B88C14B99EF449828C5D926E39EE8B89'</id></Item>", aml);

      var hash = new HashSet<string>(ids);
      aml = TestAml(q => q.Where(i => hash.Contains(i.Id())));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><id condition=\"in\">'4F1AC04A2B484F3ABA4E20DB63808A88','B88C14B99EF449828C5D926E39EE8B89'</id></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_ChildItem()
    {
      var aml = TestAml(q => q.Where(i => i.CreatedById().AsItem().KeyedName().Value.Contains("Domke")));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><created_by_id><Item action=\"get\"><keyed_name condition=\"like\">*Domke*</keyed_name></Item></created_by_id></Item>", aml);

      TestAml(q => q.Where(i => i.Property("created_by_id").AsItem().Property("keyed_name").AsString("").Contains("Domke")));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><created_by_id><Item action=\"get\"><keyed_name condition=\"like\">*Domke*</keyed_name></Item></created_by_id></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_SelectBasic()
    {
      var aml = TestAml(q => q.Select(i => new { CreatedOn = i.CreatedOn().Value }));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" select=\"created_on\" />", aml);

      aml = TestAml(q => q.Select(i => new { CreatedOn = i.CreatedOn().Value, CreatedBy = i.CreatedById().AsItem().KeyedName().Value }));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" select=\"created_on,created_by_id(keyed_name)\" />", aml);
    }

    [TestMethod()]
    public void Queryable_SelectBasicWithCondition()
    {
      var aml = TestAml(q => q.Where(i => i.Property("name").Value == "Part").Select(i => new { CreatedOn = i.CreatedOn().Value }));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" select=\"created_on\"><name>Part</name></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_OrderByBasic()
    {
      var aml = TestAml(q => q.OrderBy(i => i.Property("created_on").Value));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" orderBy=\"created_on\" />", aml);

      aml = TestAml(q => q.OrderByDescending(i => i.Property("created_on").Value));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" orderBy=\"created_on DESC\" />", aml);

      aml = TestAml(q => q.OrderBy(i => i.Property("created_on").Value).ThenBy(i => i.Generation().Value));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" orderBy=\"created_on,generation\" />", aml);

      aml = TestAml(q => q.OrderBy(i => i.Property("created_on").Value).ThenByDescending(i => i.Generation().Value));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" orderBy=\"created_on,generation DESC\" />", aml);
    }

    [TestMethod()]
    public void Queryable_ChildItemLogical()
    {
      var aml = TestAml(q => q.Where(i => i.CreatedById().AsItem().KeyedName().Value.Contains("Domke") && i.CreatedById().AsItem().IsCurrent().AsBoolean(true) || i.CreatedById().AsItem().Generation().AsInt(0) > 0));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><created_by_id><Item action=\"get\"><or><generation condition=\"gt\">0</generation><and><keyed_name condition=\"like\">*Domke*</keyed_name><is_current>1</is_current></and></or></Item></created_by_id></Item>", aml);

      aml = TestAml(q => q.Where(i => i.CreatedById().AsItem().KeyedName().Value.Contains("Domke") && (i.CreatedById().AsItem().IsCurrent().AsBoolean(true) || i.CreatedById().AsItem().Generation().AsInt(0) > 0)));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><created_by_id><Item action=\"get\"><keyed_name condition=\"like\">*Domke*</keyed_name><or><is_current>1</is_current><generation condition=\"gt\">0</generation></or></Item></created_by_id></Item>", aml);

      aml = TestAml(q => q.Where(i => i.CreatedById().AsItem().KeyedName().Value.Contains("Domke") && i.CreatedById().AsItem().Generation().AsInt(0) > 0));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><created_by_id><Item action=\"get\"><keyed_name condition=\"like\">*Domke*</keyed_name><generation condition=\"gt\">0</generation></Item></created_by_id></Item>", aml);

      aml = TestAml(q => q.Where(i => i.CreatedById().AsItem().KeyedName().Value.Contains("Domke") || i.Generation().AsInt(0) > 0));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><or><created_by_id><Item action=\"get\"><keyed_name condition=\"like\">*Domke*</keyed_name></Item></created_by_id><generation condition=\"gt\">0</generation></or></Item>", aml);
    }

    [TestMethod()]
    public void Queryable_Take()
    {
      var aml = TestAml(q => q.Take(5));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" maxRecords=\"5\" />", aml);
    }

    [TestMethod()]
    public void QueryString_Basic()
    {
      var aml = TestQueryString("?$filter=Name eq 'Part'");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name>Part</name></Item>", aml);

      aml = TestQueryString("?$filter='Part' eq Name");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name>Part</name></Item>", aml);

      aml = TestQueryString("?$filter=not (Name eq 'Apple')");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><not><name>Apple</name></not></Item>", aml);

      aml = TestQueryString("?$filter=Name ne 'Apple'");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name condition=\"ne\">Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=not (Name ne 'Apple')");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><not><name condition=\"ne\">Apple</name></not></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_SingleBoolean()
    {
      var aml = TestQueryString("?$filter=is_versionable");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><is_versionable>1</is_versionable></Item>", aml);

      aml = TestQueryString("?$filter=not is_versionable");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><not><is_versionable>1</is_versionable></not></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Int()
    {
      var aml = TestQueryString("?$filter=generation ge 3");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><generation condition=\"ge\">3</generation></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_GuidId()
    {
      var aml = TestQueryString("?$filter=id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Date()
    {
      var aml = TestQueryString("?$filter=created_on eq datetime'2002-01-01T00:00'");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><created_on>2002-01-01T00:00:00</created_on></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Conditional()
    {
      var aml = TestQueryString("?$filter=created_on eq datetime'2002-01-01T00:00' and generation ge 3");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></Item>", aml);

      aml = TestQueryString("?$filter=created_on eq datetime'2002-01-01T00:00' and generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><or><and><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>", aml);

      aml = TestQueryString("?$filter=(created_on eq datetime'2002-01-01T00:00' and generation ge 3) or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><or><and><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>", aml);

      aml = TestQueryString("?$filter=created_on eq datetime'2002-01-01T00:00' and (generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88')");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><created_on>2002-01-01T00:00:00</created_on><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>", aml);

      aml = TestQueryString("?$filter=(generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88') and created_on eq datetime'2002-01-01T00:00'");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or><created_on>2002-01-01T00:00:00</created_on></Item>", aml);

      aml = TestQueryString("?$filter=generation ge 3 or (id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88' and created_on eq datetime'2002-01-01T00:00')");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><or><generation condition=\"ge\">3</generation><and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_on>2002-01-01T00:00:00</created_on></and></or></Item>", aml);

      aml = TestQueryString("?$filter=generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88' or created_on eq datetime'2002-01-01T00:00'");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_on>2002-01-01T00:00:00</created_on></or></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Functions()
    {
      var aml = TestQueryString("?$callback=jQuery112304312923812233427_1494592722830&%24inlinecount=allpages&%24format=json&%24filter=startswith(tolower(name)%2C%27c%27)");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name condition=\"like\">c*</name></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_SkipTake()
    {
      var aml = TestQueryString("?$filter=Name eq 'Apple'&$top=1");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" maxRecords=\"1\"><name>Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=Name eq 'Apple'&$top=3");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" maxRecords=\"3\"><name>Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=Name eq 'Apple'&$skip=3");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" page=\"2\" pagesize=\"3\"><name>Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=Name eq 'Apple'&$skip=4&$top=2");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" page=\"3\" pagesize=\"2\"><name>Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=Name eq 'Apple'&$top=2&$skip=4");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" page=\"3\" pagesize=\"2\"><name>Apple</name></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Ordering()
    {
      var aml = TestQueryString("?$orderby=Name");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" orderBy=\"name\" />", aml);

      aml = TestQueryString("?$orderby=Name asc");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" orderBy=\"name\" />", aml);

      aml = TestQueryString("?$orderby=Name desc");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" orderBy=\"name DESC\" />", aml);
    }


    [TestMethod()]
    public void QueryString_Random()
    {
      var aml = TestQueryString("?$callback=jQuery1124032885557251554487_1494968811401&%24inlinecount=allpages&%24format=json&%24filter=startswith(tolower(keyed_name)%2C%27kent+ypma%27)");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><keyed_name condition=\"like\">kent ypma*</keyed_name></Item>", aml);
    }


    [TestMethod()]
    public void QueryString_ModifyQuery()
    {
      var conn = new TestConnection();
      var query = conn.Queryable<IReadOnlyItem>("ItemType", new QuerySettings() { ModifyQuery = i =>
      {
        i.Action().Set("GetOData");
        i.MaxRecords().Set(10);
      }});
      var intermediate = query.ExecuteOData("?$orderby=Name", new ExecutionSettings().WithDynamicAccessor((obj, key) => ((IReadOnlyItem)obj).Property(key).Value));
      var result = ((IEnumerable)intermediate).OfType<object>().ToArray();
      var aml = conn.LastRequest.ToNormalizedAml(conn.AmlContext.LocalizationContext);
      Assert.AreEqual("<Item action=\"GetOData\" type=\"ItemType\" orderBy=\"name\" maxRecords=\"10\" />", aml);
    }

    [TestMethod()]
    public void QueryString_Projection()
    {
      var aml = TestQueryString("?$select=Name");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" select=\"name\" />", aml);

      aml = TestQueryString("?$select=name,id");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" select=\"name,id\" />", aml);

      aml = TestQueryString("?$select=name,id&$orderby=name");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" orderBy=\"name\" select=\"name,id\" />", aml);

      aml = TestQueryString("?$select=name,id&$orderby=name,created_on desc");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\" orderBy=\"name,created_on DESC\" select=\"name,id\" />", aml);
    }

    [TestMethod()]
    public void Queryable_Include()
    {
      var aml = TestAml(q => q.Include("created_by_id").Where(i => i.Id() == "4F1AC04A2B484F3ABA4E20DB63808A88"));
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_by_id><Item action=\"get\" /></created_by_id></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Include()
    {
      var aml = TestQueryString("?$filter=id eq '4F1AC04A2B484F3ABA4E20DB63808A88'&$expand=created_by_id");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_by_id><Item action=\"get\" /></created_by_id></Item>", aml);
    }

    [TestMethod()]
    public void QueryString_Nested()
    {
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><created_by_id><Item action=\"get\"><keyed_name>Test</keyed_name></Item></created_by_id></Item>"
        , TestQueryString("?$filter=created_by_id/keyed_name eq 'Test'"));

      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><created_by_id><Item action=\"get\"><keyed_name>Test</keyed_name><state>Something</state></Item></created_by_id></Item>"
        , TestQueryString("?$filter=created_by_id/keyed_name eq 'Test' and created_by_id/state eq 'Something'"));
    }
  }
}
#endif
