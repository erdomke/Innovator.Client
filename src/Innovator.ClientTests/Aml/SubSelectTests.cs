using Innovator.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class SubSelectTests
  {
    [TestMethod()]
    public void ParseItemSelect_Complex()
    {
      var cols = SelectNode.FromString("first, second (thing, another2(id, config_id)), no_paren, third (stuff), another (id)");
      var expected = new string[] { "first", "second", "no_paren", "third", "another" };
      CollectionAssert.AreEqual(expected, cols.Select(c => c.Name).ToArray());
    }

    [TestMethod()]
    public void ParseItemSelect_Simple()
    {
      var cols = SelectNode.FromString("config_id,name,is_relationship");
      var expected = new string[] { "config_id", "name", "is_relationship" };
      CollectionAssert.AreEqual(expected, cols.Select(c => c.Name).ToArray());
    }

    [TestMethod()]
    public void ParseItemSelect_ConfigPath()
    {
      const string str = "id|related_id(id)|created_by_id,config_id";
      var cols = SelectNode.FromString("id|related_id(id)|created_by_id,config_id");
      Assert.AreEqual(str, cols.ToString());
      var expected = new string[] { "id", "related_id", "created_by_id", "config_id" };
      var names = cols.SelectMany(c => c).Select(c => c.Name).ToArray();
      CollectionAssert.AreEqual(expected, names);
    }

    [TestMethod()]
    public void ParseItemSelect_ExtendedClassification()
    {
      const string str = "xp-*[is_not_null()],id";
      var cols = SelectNode.FromString(str);
      Assert.AreEqual(str, cols.ToString());
      var expected = new string[] { "xp-*", "id" };
      CollectionAssert.AreEqual(expected, cols.Select(c => c.Name).ToArray());

      cols = SelectNode.FromString("xp-*(@explicit,@defined_as,@permission_id,$value)");
      expected = new string[] { "@explicit", "@defined_as", "@permission_id", "$value" };
      var subCols = cols.First().Select(c => c.Name).ToArray();
      CollectionAssert.AreEqual(expected, subCols);
    }

    [TestMethod()]
    public void ParseItemSelect_ComplexFunction()
    {
      const string str = "major_rev,owned_by_id[is_not_null()],managed_by_id[is_not_null()],team_id,id";
      var cols = SelectNode.FromString(str);
      Assert.AreEqual(str, cols.ToString());
      var expected = new string[] { "major_rev", "owned_by_id", "managed_by_id", "team_id", "id" };
      CollectionAssert.AreEqual(expected, cols.Select(c => c.Name).ToArray());
    }

    [TestMethod()]
    public void PropertiesToString()
    {
      var actual = ElementFactory.Local.Select("source_id", "source_type").Value;
      Assert.AreEqual("source_id,source_type", actual);
    }
  }
}
