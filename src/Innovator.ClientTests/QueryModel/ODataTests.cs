using Innovator.Client.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel.Tests
{
  [TestClass]
  public class ODataTests
  {
    [DataTestMethod]
    [DataRow("Part?$filter=name in ('Redmond', 'London')&$format=json", "<Item type=\"Part\" action=\"get\"><name condition=\"in\">'Redmond', 'London'</name></Item>")]
    [DataRow("Part?$format=json&$filter=name eq 'Apple'", "<Item type=\"Part\" action=\"get\"><name>Apple</name></Item>")]
    [DataRow("Part?$filter=name eq 'Apple'&$format=json", "<Item type=\"Part\" action=\"get\"><name>Apple</name></Item>")]
    [DataRow("Part?$filter=complete", "<Item type=\"Part\" action=\"get\"><complete>1</complete></Item>")]
    [DataRow("Part?$filter=not complete", "<Item type=\"Part\" action=\"get\"><complete>0</complete></Item>")]
    [DataRow("Part?$filter=name eq 'Apple'&$top=1", "<Item type=\"Part\" action=\"get\" maxRecords=\"1\"><name>Apple</name></Item>")]
    [DataRow("Part?$filter=not (name eq 'Apple')", "<Item type=\"Part\" action=\"get\"><name condition=\"ne\">Apple</name></Item>")]
    [DataRow("Part?$filter=name ne 'Apple'", "<Item type=\"Part\" action=\"get\"><name condition=\"ne\">Apple</name></Item>")]
    [DataRow("Part?$filter=not (name ne 'Apple')", "<Item type=\"Part\" action=\"get\"><name>Apple</name></Item>")]
    [DataRow(@"Part?$filter=name eq 'Apple""Bob'", "<Item type=\"Part\" action=\"get\"><name>Apple\"Bob</name></Item>")]
    [DataRow("Part?$filter=name eq 'Apple''Bob'", "<Item type=\"Part\" action=\"get\"><name>Apple'Bob</name></Item>")]
    [DataRow("Part?$filter=name eq 'x%20y%20%26%20z'", "<Item type=\"Part\" action=\"get\"><name>x y &amp; z</name></Item>")]
    [DataRow("Part?$filter=age eq 4", "<Item type=\"Part\" action=\"get\"><age>4</age></Item>")]
    [DataRow("Part?$filter=age gt -4", "<Item type=\"Part\" action=\"get\"><age condition=\"gt\">-4</age></Item>")]
    [DataRow("Part?$filter=not (age eq 4)", "<Item type=\"Part\" action=\"get\"><age condition=\"ne\">4</age></Item>")]
    [DataRow("Part?$filter=age ne 4", "<Item type=\"Part\" action=\"get\"><age condition=\"ne\">4</age></Item>")]
    [DataRow("Part?$filter=not (age ne 4)", "<Item type=\"Part\" action=\"get\"><age>4</age></Item>")]
    [DataRow("Part?$filter=age gt 3", "<Item type=\"Part\" action=\"get\"><age condition=\"gt\">3</age></Item>")]
    [DataRow("Part?$filter=not (age gt 3)", "<Item type=\"Part\" action=\"get\"><age condition=\"le\">3</age></Item>")]
    [DataRow("Part?$filter=age ge 3", "<Item type=\"Part\" action=\"get\"><age condition=\"ge\">3</age></Item>")]
    [DataRow("Part?$filter=not (age ge 3)", "<Item type=\"Part\" action=\"get\"><age condition=\"lt\">3</age></Item>")]
    [DataRow("Part?$filter=age lt 3", "<Item type=\"Part\" action=\"get\"><age condition=\"lt\">3</age></Item>")]
    [DataRow("Part?$filter=not (age lt 3)", "<Item type=\"Part\" action=\"get\"><age condition=\"ge\">3</age></Item>")]
    [DataRow("Part?$filter=age le 3", "<Item type=\"Part\" action=\"get\"><age condition=\"le\">3</age></Item>")]
    [DataRow("Part?$filter=not (age le 3)", "<Item type=\"Part\" action=\"get\"><age condition=\"gt\">3</age></Item>")]
    [DataRow("Part?$filter=population eq 40000000000L", "<Item type=\"Part\" action=\"get\"><population>40000000000</population></Item>")]
    [DataRow("Part?$filter=name eq 'Custard' and age ge 2", "<Item type=\"Part\" action=\"get\"><name>Custard</name><age condition=\"ge\">2</age></Item>")]
    [DataRow("Part?$filter=name eq 'Custard' and not (age lt 2)", "<Item type=\"Part\" action=\"get\"><name>Custard</name><age condition=\"ge\">2</age></Item>")]
    [DataRow("Part?$filter=name eq 'Banana' or date gt datetime'2003-01-01T00:00'", "<Item type=\"Part\" action=\"get\"><or><name>Banana</name><date condition=\"gt\">2003-01-01T00:00:00</date></or></Item>")]
    [DataRow("Part?$filter=name eq 'Banana' or not (date le datetime'2003-01-01T00:00')", "<Item type=\"Part\" action=\"get\"><or><name>Banana</name><date condition=\"gt\">2003-01-01T00:00:00</date></or></Item>")]
    [DataRow("Part?$filter=name eq 'Apple' and complete eq true or date gt datetime'2003-01-01T00:00'"
      , "<Item type=\"Part\" action=\"get\"><or><and><name>Apple</name><complete>1</complete></and><date condition=\"gt\">2003-01-01T00:00:00</date></or></Item>")]
    [DataRow("Part?$filter=name eq 'Apple' and (complete eq true or date gt datetime'2003-01-01T00:00')"
      , "<Item type=\"Part\" action=\"get\"><name>Apple</name><or><complete>1</complete><date condition=\"gt\">2003-01-01T00:00:00</date></or></Item>")]
    [DataRow("Part?$filter=not (name eq 'Apple' and (complete eq true or date gt datetime'2003-01-01T00:00'))"
      , "<Item type=\"Part\" action=\"get\"><not><name>Apple</name><or><complete>1</complete><date condition=\"gt\">2003-01-01T00:00:00</date></or></not></Item>")]
    [DataRow("ItemType?$filter=id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'", "<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>")]
    [DataRow("ItemType?$filter=created_on eq datetime'2002-01-01T00:00' and generation ge 3", "<Item type=\"ItemType\" action=\"get\"><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></Item>")]
    [DataRow("ItemType?$filter=created_on eq datetime'2002-01-01T00:00' and generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'", "<Item type=\"ItemType\" action=\"get\"><or><and><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>")]
    [DataRow("ItemType?$filter=(created_on eq datetime'2002-01-01T00:00' and generation ge 3) or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'", "<Item type=\"ItemType\" action=\"get\"><or><and><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>")]
    [DataRow("ItemType?$filter=created_on eq datetime'2002-01-01T00:00' and (generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88')", "<Item type=\"ItemType\" action=\"get\"><created_on>2002-01-01T00:00:00</created_on><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>")]
    [DataRow("ItemType?$filter=(generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88') and created_on eq datetime'2002-01-01T00:00'", "<Item type=\"ItemType\" action=\"get\"><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or><created_on>2002-01-01T00:00:00</created_on></Item>")]
    [DataRow("ItemType?$filter=generation ge 3 or (id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88' and created_on eq datetime'2002-01-01T00:00')", "<Item type=\"ItemType\" action=\"get\"><or><generation condition=\"ge\">3</generation><and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_on>2002-01-01T00:00:00</created_on></and></or></Item>")]
    [DataRow("ItemType?$filter=generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88' or created_on eq datetime'2002-01-01T00:00'", "<Item type=\"ItemType\" action=\"get\"><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_on>2002-01-01T00:00:00</created_on></or></Item>")]
    [DataRow("ItemType?$callback=jQuery112304312923812233427_1494592722830&%24inlinecount=allpages&%24format=json&%24filter=startswith(tolower(name)%2C%27c%27)", "<Item type=\"ItemType\" action=\"get\"><name condition=\"like\">c*</name></Item>")]
    [DataRow("ItemType?$callback=jQuery1124032885557251554487_1494968811401&%24inlinecount=allpages&%24format=json&%24filter=startswith(tolower(keyed_name)%2C%27john+doe%27)", "<Item type=\"ItemType\" action=\"get\"><keyed_name condition=\"like\">john doe*</keyed_name></Item>")]
    public void FilterToAml(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("Part?$select=name", "<Item type=\"Part\" action=\"get\" select=\"name\" />")]
    [DataRow("Part?$select=name,age", "<Item type=\"Part\" action=\"get\" select=\"name,age\" />")]
    [DataRow("Part?$select=name,age&$orderby=name", "<Item type=\"Part\" action=\"get\" select=\"name,age\" orderBy=\"name\" />")]
    public void SelectToAml(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("Part?$top=1", "<Item type=\"Part\" action=\"get\" maxRecords=\"1\" />")]
    [DataRow("Part?$top=3", "<Item type=\"Part\" action=\"get\" maxRecords=\"3\" />")]
    [DataRow("Part?$skip=1", "<Item type=\"Part\" action=\"get\" page=\"2\" pagesize=\"1\" />")]
    [DataRow("Part?$skip=3", "<Item type=\"Part\" action=\"get\" page=\"2\" pagesize=\"3\" />")]
    [DataRow("Part?$skip=2&$top=2", "<Item type=\"Part\" action=\"get\" page=\"2\" pagesize=\"2\" />")]
    [DataRow("Part?$top=2&$skip=2", "<Item type=\"Part\" action=\"get\" page=\"2\" pagesize=\"2\" />")]
    [DataRow("Part?$orderby=age", "<Item type=\"Part\" action=\"get\" orderBy=\"age\" />")]
    [DataRow("Part?$orderby=age asc", "<Item type=\"Part\" action=\"get\" orderBy=\"age\" />")]
    [DataRow("Part?$orderby=age desc", "<Item type=\"Part\" action=\"get\" orderBy=\"age DESC\" />")]
    [DataRow("Part?$orderby=complete,age", "<Item type=\"Part\" action=\"get\" orderBy=\"complete,age\" />")]
    [DataRow("Part?$orderby=complete desc,age", "<Item type=\"Part\" action=\"get\" orderBy=\"complete DESC,age\" />")]
    [DataRow("Part?$orderby=complete,age desc", "<Item type=\"Part\" action=\"get\" orderBy=\"complete,age DESC\" />")]
    [DataRow("Part?$orderby=complete desc,age desc", "<Item type=\"Part\" action=\"get\" orderBy=\"complete DESC,age DESC\" />")]
    public void PagingAndOrdering(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("Part?$orderby=Concrete/Complete")]
    public void PagingAndOrdering_NotSuported(string odata)
    {
      var item = QueryItem.FromOData(odata);
      Assert.ThrowsException<NotSupportedException>(() => item.ToAml());
    }

    [DataTestMethod]
    [DataRow("Part?$filter=startswith(name,'Sat')", "<Item type=\"Part\" action=\"get\"><name condition=\"like\">Sat*</name></Item>")]
    [DataRow("Part?$filter=endswith(name,'day')", "<Item type=\"Part\" action=\"get\"><name condition=\"like\">*day</name></Item>")]
    [DataRow("Part?$filter=substringof('urn',name)", "<Item type=\"Part\" action=\"get\"><name condition=\"like\">*urn*</name></Item>")]
    [DataRow("Part?$filter=contains(name,'urn')", "<Item type=\"Part\" action=\"get\"><name condition=\"like\">*urn*</name></Item>")]
    public void FunctionsToAml(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("Part?$format=json&$filter=concrete/name eq 'Apple'", "<Item type=\"Part\" action=\"get\"><concrete><Item action=\"get\"><name>Apple</name></Item></concrete></Item>")]
    [DataRow("Part?$format=json&$filter=concrete/name eq 'Apple' and concrete/age eq 5", "<Item type=\"Part\" action=\"get\"><concrete><Item action=\"get\"><name>Apple</name><age>5</age></Item></concrete></Item>")]
    [DataRow("ItemType?$filter=created_by_id/keyed_name eq 'Test'", "<Item type=\"ItemType\" action=\"get\"><created_by_id><Item action=\"get\"><keyed_name>Test</keyed_name></Item></created_by_id></Item>")]
    [DataRow("ItemType?$filter=created_by_id/keyed_name eq 'Test' and created_by_id/state eq 'Something'", "<Item type=\"ItemType\" action=\"get\"><created_by_id><Item action=\"get\"><keyed_name>Test</keyed_name><state>Something</state></Item></created_by_id></Item>")]
    [DataRow("Program%20Part?$filter=false+and+startswith(related_id/keyed_name,%27903%27)", "<Item type=\"Program Part\" action=\"get\" id=\"FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF\" />")]
    public void NestedToAml(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("ItemType?$filter=id eq '4F1AC04A2B484F3ABA4E20DB63808A88'&$expand=created_by_id", "<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_by_id><Item action=\"get\" /></created_by_id></Item>")]
    [DataRow("ItemType?$filter=id eq '4F1AC04A2B484F3ABA4E20DB63808A88'&$expand=created_by_id($select=first_name,last_name)", "<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_by_id><Item action=\"get\" select=\"first_name,last_name\" /></created_by_id></Item>")]
    [DataRow("ItemType?$filter=id eq '4F1AC04A2B484F3ABA4E20DB63808A88'&$select=created_by_id/first_name,created_by_id/last_name", "<Item type=\"ItemType\" action=\"get\" select=\"created_by_id(first_name,last_name)\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>")]
    [DataRow("Part?$expand=Part%20BOM&$filter=item_number%20eq%20'ASSY-01'", "<Item type=\"Part\" action=\"get\"><item_number>ASSY-01</item_number><Relationships><Item type=\"Part BOM\" action=\"get\" /></Relationships></Item>")]
    [DataRow("Part?$expand=Part%20BOM($expand=owned_by_id)&$filter=item_number%20eq%20'ASSY-01'", "<Item type=\"Part\" action=\"get\"><item_number>ASSY-01</item_number><Relationships><Item type=\"Part BOM\" action=\"get\"><owned_by_id><Item action=\"get\" /></owned_by_id></Item></Relationships></Item>")]
    [DataRow("Part_bom?$filter=source_id/keyed_name%20eq%20%27Part1%27%20and%20(startswith(related_id/classification,%27PCB%20(Raw)%27)%20or%20startswith(related_id/classification,%27PCB%20Sub-assembly%27))&$expand=related_id($select=owned_by_id)", "<Item type=\"Part_bom\" action=\"get\"><source_id><Item action=\"get\"><keyed_name>Part1</keyed_name></Item></source_id><related_id><Item action=\"get\" select=\"owned_by_id\"><or><classification condition=\"like\">PCB (Raw)*</classification><classification condition=\"like\">PCB Sub-assembly*</classification></or></Item></related_id></Item>")]
    public void ExpandToAml(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("Part?$filter=pn eq 'Apple'&$select=keyed_name,pn&$orderBy=pn&$compute=item_number as pn", "<Item type=\"Part\" action=\"get\" select=\"keyed_name,item_number\" orderBy=\"item_number\"><item_number>Apple</item_number></Item>")]
    public void ComputeToAml(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("Part/$count?$filter=name eq 'Apple'&$format=json", "<Item type=\"Part\" action=\"get\" returnMode=\"countOnly\" select=\"id\" page=\"1\" pagesize=\"1\"><name>Apple</name></Item>")]
    [DataRow("Part('C3F13FE4A2674B9691A6B311B5CCBCDB')/Part CAD/$count", "<Item type=\"Part CAD\" action=\"get\" returnMode=\"countOnly\" select=\"id\" page=\"1\" pagesize=\"1\"><source_id>C3F13FE4A2674B9691A6B311B5CCBCDB</source_id></Item>")]
    public void CountToAml(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("http://example.com/api/Part('F45F259F527942EB8A6C4011BC784EF0')", "<Item type=\"Part\" action=\"get\"><id>F45F259F527942EB8A6C4011BC784EF0</id></Item>")]
    [DataRow("Part('C3F13FE4A2674B9691A6B311B5CCBCDB')/Part CAD?$expand=related_id", "<Item type=\"Part CAD\" action=\"get\"><source_id>C3F13FE4A2674B9691A6B311B5CCBCDB</source_id><related_id><Item action=\"get\" /></related_id></Item>")]
    [DataRow("Part('F45F259F527942EB8A6C4011BC784EF0')/item_number/$value", "<Item type=\"Part\" action=\"get\" select=\"item_number\"><id>F45F259F527942EB8A6C4011BC784EF0</id></Item>")]
    public void ItemById(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("http://host/service/$entity?$id=http://host/service/Part('F45F259F527942EB8A6C4011BC784EF0')", "<Item type=\"Part\" action=\"get\"><id>F45F259F527942EB8A6C4011BC784EF0</id></Item>")]
    [DataRow("http://host/service/$entity?$id=Part('F45F259F527942EB8A6C4011BC784EF0')", "<Item type=\"Part\" action=\"get\"><id>F45F259F527942EB8A6C4011BC784EF0</id></Item>")]
    public void EntityToAml(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("Part%20BOM?$filter=source_id%20eq%20%27F45F259F527942EB8A6C4011BC784EF0%27&$format=json&$select=quantity,reference_designator,related_id,source_id&$expand=related_id&$callback=jQuery1124016690295736214167_1532034753866&%24inlinecount=allpages&%24format=json", "<Item type=\"Part BOM\" action=\"get\" select=\"quantity,reference_designator,source_id,related_id\"><source_id>F45F259F527942EB8A6C4011BC784EF0</source_id><related_id><Item action=\"get\" /></related_id></Item>")]
    public void RelatedIdSelectToAml(string odata, string expected)
    {
      var item = QueryItem.FromOData(odata);
      var aml = item.ToAml();
      Assert.AreEqual(expected, aml);
    }

    [DataTestMethod]
    [DataRow("Part?$format=json&$filter=name eq 'Apple'", "Part?$filter=name%20eq%20'Apple'")]
    [DataRow("Part?$filter=name eq 'Apple'&$format=json", "Part?$filter=name%20eq%20'Apple'")]
    [DataRow("Part?$filter=complete", "Part?$filter=complete%20eq%20true")]
    [DataRow("Part?$filter=not complete", "Part?$filter=complete%20eq%20false")]
    [DataRow("Part?$filter=name eq 'Apple'&$top=1", "Part?$filter=name%20eq%20'Apple'&$top=1")]
    [DataRow("Part?$filter=not (name eq 'Apple')", "Part?$filter=name%20ne%20'Apple'")]
    [DataRow("Part?$filter=name ne 'Apple'", "Part?$filter=name%20ne%20'Apple'")]
    [DataRow("Part?$filter=not (name ne 'Apple')", "Part?$filter=name%20eq%20'Apple'")]
    [DataRow(@"Part?$filter=name eq 'Apple""Bob'", @"Part?$filter=name%20eq%20'Apple%22Bob'")]
    [DataRow("Part?$filter=name eq 'Apple''Bob'", "Part?$filter=name%20eq%20'Apple''Bob'")]
    [DataRow("Part?$filter=name eq 'x%20y%20%26%20z'", "Part?$filter=name%20eq%20'x%20y%20%26%20z'")]
    [DataRow("Part?$filter=age eq 4", "Part?$filter=age%20eq%204")]
    [DataRow("Part?$filter=age gt -4", "Part?$filter=age%20gt%20-4")]
    [DataRow("Part?$filter=not (age eq 4)", "Part?$filter=age%20ne%204")]
    [DataRow("Part?$filter=age ne 4", "Part?$filter=age%20ne%204")]
    [DataRow("Part?$filter=not (age ne 4)", "Part?$filter=age%20eq%204")]
    [DataRow("Part?$filter=age gt 3", "Part?$filter=age%20gt%203")]
    [DataRow("Part?$filter=not (age gt 3)", "Part?$filter=age%20le%203")]
    [DataRow("Part?$filter=age ge 3", "Part?$filter=age%20ge%203")]
    [DataRow("Part?$filter=not (age ge 3)", "Part?$filter=age%20lt%203")]
    [DataRow("Part?$filter=age lt 3", "Part?$filter=age%20lt%203")]
    [DataRow("Part?$filter=not (age lt 3)", "Part?$filter=age%20ge%203")]
    [DataRow("Part?$filter=age le 3", "Part?$filter=age%20le%203")]
    [DataRow("Part?$filter=not (age le 3)", "Part?$filter=age%20gt%203")]
    [DataRow("Part?$filter=population eq 40000000000L", "Part?$filter=population%20eq%2040000000000")]
    [DataRow("Part?$filter=name eq 'Custard' and age ge 2", "Part?$filter=name%20eq%20'Custard'%20and%20age%20ge%202")]
    [DataRow("Part?$filter=name eq 'Custard' and not (age lt 2)", "Part?$filter=name%20eq%20'Custard'%20and%20age%20ge%202")]
    [DataRow("Part?$filter=name eq 'Banana' or date gt datetime'2003-01-01T00:00'", "Part?$filter=name%20eq%20'Banana'%20or%20date%20gt%202003-01-01")]
    [DataRow("Part?$filter=name eq 'Banana' or not (date le datetime'2003-01-01T00:00')", "Part?$filter=name%20eq%20'Banana'%20or%20date%20gt%202003-01-01")]
    [DataRow("Part?$filter=name eq 'Apple' and complete eq true or date gt datetime'2003-01-01T00:00'"
      , "Part?$filter=name%20eq%20'Apple'%20and%20complete%20eq%20true%20or%20date%20gt%202003-01-01")]
    [DataRow("Part?$filter=name eq 'Apple' and (complete eq true or date gt datetime'2003-01-01T00:00')"
      , "Part?$filter=name%20eq%20'Apple'%20and%20(complete%20eq%20true%20or%20date%20gt%202003-01-01)")]
    [DataRow("Part?$filter=not (name eq 'Apple' and (complete eq true or date gt datetime'2003-01-01T00:00'))"
      , "Part?$filter=%20not%20(name%20eq%20'Apple'%20and%20(complete%20eq%20true%20or%20date%20gt%202003-01-01))")]
    //[DataRow("ItemType?$filter=id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'", "<Item type=\"ItemType\" action=\"get\"><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></Item>")]
    //[DataRow("ItemType?$filter=created_on eq datetime'2002-01-01T00:00' and generation ge 3", "<Item type=\"ItemType\" action=\"get\"><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></Item>")]
    //[DataRow("ItemType?$filter=created_on eq datetime'2002-01-01T00:00' and generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'", "<Item type=\"ItemType\" action=\"get\"><or><and><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>")]
    //[DataRow("ItemType?$filter=(created_on eq datetime'2002-01-01T00:00' and generation ge 3) or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88'", "<Item type=\"ItemType\" action=\"get\"><or><and><created_on>2002-01-01T00:00:00</created_on><generation condition=\"ge\">3</generation></and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>")]
    //[DataRow("ItemType?$filter=created_on eq datetime'2002-01-01T00:00' and (generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88')", "<Item type=\"ItemType\" action=\"get\"><created_on>2002-01-01T00:00:00</created_on><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or></Item>")]
    //[DataRow("ItemType?$filter=(generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88') and created_on eq datetime'2002-01-01T00:00'", "<Item type=\"ItemType\" action=\"get\"><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id></or><created_on>2002-01-01T00:00:00</created_on></Item>")]
    //[DataRow("ItemType?$filter=generation ge 3 or (id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88' and created_on eq datetime'2002-01-01T00:00')", "<Item type=\"ItemType\" action=\"get\"><or><generation condition=\"ge\">3</generation><and><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_on>2002-01-01T00:00:00</created_on></and></or></Item>")]
    //[DataRow("ItemType?$filter=generation ge 3 or id eq guid'4F1AC04A-2B48-4F3A-BA4E-20DB63808A88' or created_on eq datetime'2002-01-01T00:00'", "<Item type=\"ItemType\" action=\"get\"><or><generation condition=\"ge\">3</generation><id>4F1AC04A2B484F3ABA4E20DB63808A88</id><created_on>2002-01-01T00:00:00</created_on></or></Item>")]
    //[DataRow("ItemType?$callback=jQuery112304312923812233427_1494592722830&%24inlinecount=allpages&%24format=json&%24filter=startswith(tolower(name)%2C%27c%27)", "<Item type=\"ItemType\" action=\"get\"><name condition=\"like\">c*</name></Item>")]
    //[DataRow("ItemType?$callback=jQuery1124032885557251554487_1494968811401&%24inlinecount=allpages&%24format=json&%24filter=startswith(tolower(keyed_name)%2C%27john+doe%27)", "<Item type=\"ItemType\" action=\"get\"><keyed_name condition=\"like\">john doe*</keyed_name></Item>")]
    [DataRow("Part?$filter=startswith(name,'Sat')", "Part?$filter=startswith(name,'Sat')")]
    [DataRow("Part?$filter=endswith(name,'day')", "Part?$filter=endswith(name,'day')")]
    [DataRow("Part?$filter=substringof('urn',name)", "Part?$filter=contains(name,'urn')")]
    [DataRow("Part?$filter=contains(name,'urn')", "Part?$filter=contains(name,'urn')")]
    [DataRow("Part?$format=json&$filter=concrete/name eq 'Apple'", "Part?$filter=concrete/name%20eq%20'Apple'")]
    [DataRow("Part?$format=json&$filter=concrete/name eq 'Apple' and concrete/age eq 5", "Part?$filter=concrete/name%20eq%20'Apple'%20and%20concrete/age%20eq%205")]
    public void RoundTrip(string original, string expected)
    {
      var settings = new ConnectedAmlSqlWriterSettings(new TestConnection());
      var item = QueryItem.FromOData(original);
      var odata = item.ToOData(settings, ElementFactory.Local.LocalizationContext);
      Assert.AreEqual(expected, odata);
    }


  }
}
