#if NETFULL
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Innovator.Client.Queryable;
using LinqToQuerystring;
using System.Collections;
using Linq2Rest;
using System.Collections.Specialized;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class QueryableTests
  {
    private string TestAml(Func<IQueryable<IReadOnlyItem>, IQueryable> expression)
    {
      var conn = new TestConnection();
      var query = conn.Queryable("ItemType");
      var intermediate = expression(query);
      var rendered = intermediate.OfType<object>().Apply();
      return intermediate.ToString();
    }

    private string TestQueryString(string queryString)
    {
      var conn = new TestConnection();
      var query = conn.Queryable<IIndexedItem>("ItemType");
      var intermediate = query.LinqToQuerystring(queryString, true);
      var result = intermediate.Apply();
      //((IEnumerable)query.LinqToQuerystring(queryString, true)).OfType<object>().ToArray();
      return intermediate.ToString();
    }

    //private string TestQuery2Rest(string queryString)
    //{
    //  var conn = new TestConnection();
    //  var query = conn.Queryable("ItemType");

    //  var queryCol = new NameValueCollection();
    //  foreach (var pair in queryString.TrimStart('?')
    //    .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
    //    .Select(k => k.Split('='))
    //    .Where(k => k.Length == 2))
    //  {
    //    queryCol.Add(pair[0], pair[1]);
    //  }
    //  var result = query.Filter(queryCol).ToArray();
    //  //((IEnumerable)query.LinqToQuerystring(queryString, true)).OfType<object>().ToArray();
    //  return conn.LastRequest.ToNormalizedAml(conn.AmlContext.LocalizationContext);
    //}

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

      aml = TestQueryString("?$filter=not Name eq 'Apple'");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><not><name>Apple</name></not></Item>", aml);

      aml = TestQueryString("?$filter=Name ne 'Apple'");
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><name condition=\"ne\">Apple</name></Item>", aml);

      aml = TestQueryString("?$filter=not Name ne 'Apple'");
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
      Assert.AreEqual("<Item action=\"get\" type=\"ItemType\"><or><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>", aml);
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
  }
}
#endif
