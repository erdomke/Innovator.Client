using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel.Tests
{
  [TestClass]
  public class PatternTests
  {
    private PatternParser Default = new PatternParser('%', '_', '\0', '\0');
    private PatternParser Access = new PatternParser('*', '?', '#', '\0', '!', '-');
    private PatternParser Wql = new PatternParser('%', '_', '\0', '\0', '^', '=');
    private PatternParser SqlServer = new PatternParser('%', '_', '\0', '\0', '^', '-');
    private PatternParser MySql = new PatternParser('%', '_', '\0', '\\');

    private void TestSqlPattern(PatternParser inputDefn, PatternParser outputDefn, string input, string expected)
    {
      var patternOpts = inputDefn.Parse(input);
      var writer = new SqlPatternWriter(outputDefn);
      patternOpts.Visit(writer);
      Assert.AreEqual(expected, writer.ToString());
    }

    [TestMethod]
    public void PatternMatch_ParseRender01() { TestSqlPattern(Access, Access, "a*a", "a*a"); }
    [TestMethod]
    public void PatternMatch_ParseRender02() { TestSqlPattern(Access, SqlServer, "a*a", "a%a"); }
    [TestMethod]
    public void PatternMatch_ParseRender03() { TestSqlPattern(Access, Access, "a[*]a", "a[*]a"); }
    [TestMethod]
    public void PatternMatch_ParseRender04() { TestSqlPattern(Access, SqlServer, "a[*]a", "a*a"); }
    [TestMethod]
    public void PatternMatch_ParseRender05() { TestSqlPattern(Access, Access, "a?a", "a?a"); }
    [TestMethod]
    public void PatternMatch_ParseRender06() { TestSqlPattern(Access, SqlServer, "a?a", "a_a"); }
    [TestMethod]
    public void PatternMatch_ParseRender07() { TestSqlPattern(Access, Access, "a#a", "a#a"); }
    [TestMethod]
    public void PatternMatch_ParseRender08() { TestSqlPattern(Access, SqlServer, "a#a", "a[0-9]a"); }
    [TestMethod]
    public void PatternMatch_ParseRender09() { TestSqlPattern(Access, Wql, "a#a", "a[0=9]a"); }
    [TestMethod]
    public void PatternMatch_ParseRender10() { TestSqlPattern(Access, Access, "[a-z]", "[a-z]"); }
    [TestMethod]
    public void PatternMatch_ParseRender11() { TestSqlPattern(Access, SqlServer, "[a-z]", "[a-z]"); }
    [TestMethod]
    public void PatternMatch_ParseRender12() { TestSqlPattern(Access, Wql, "[a-z]", "[a=z]"); }
    [TestMethod]
    public void PatternMatch_ParseRender13() { TestSqlPattern(Access, Access, "[!a-z]", "[!a-z]"); }
    [TestMethod]
    public void PatternMatch_ParseRender14() { TestSqlPattern(Access, SqlServer, "[!a-z]", "[^a-z]"); }
    [TestMethod]
    public void PatternMatch_ParseRender15() { TestSqlPattern(Access, Wql, "[!a-z]", "[^a=z]"); }
    [TestMethod]
    public void PatternMatch_ParseRender16() { TestSqlPattern(Access, Access, "a[!b-m]#", "a[!b-m]#"); }
    [TestMethod]
    public void PatternMatch_ParseRender17() { TestSqlPattern(Access, SqlServer, "a[!b-m]#", "a[^b-m][0-9]"); }
    [TestMethod]
    public void PatternMatch_ParseRender18() { TestSqlPattern(Access, Wql, "a[!b-m]#", "a[^b=m][0=9]"); }
    [TestMethod]
    public void PatternMatch_ParseRender19() { TestSqlPattern(SqlServer, Wql, "%computer%", "%computer%"); }
    [TestMethod]
    public void PatternMatch_ParseRender20() { TestSqlPattern(SqlServer, Access, "%computer%", "*computer*"); }
    [TestMethod]
    public void PatternMatch_ParseRender21() { TestSqlPattern(SqlServer, Wql, "_ean", "_ean"); }
    [TestMethod]
    public void PatternMatch_ParseRender22() { TestSqlPattern(SqlServer, Access, "_ean", "?ean"); }
    [TestMethod]
    public void PatternMatch_ParseRender23() { TestSqlPattern(SqlServer, Wql, "[C-P]arsen", "[C=P]arsen"); }
    [TestMethod]
    public void PatternMatch_ParseRender24() { TestSqlPattern(SqlServer, Access, "[C-P]arsen", "[C-P]arsen"); }
    [TestMethod]
    public void PatternMatch_ParseRender25() { TestSqlPattern(SqlServer, Wql, "de[^l]%", "de[^l]%"); }
    [TestMethod]
    public void PatternMatch_ParseRender26() { TestSqlPattern(SqlServer, Access, "de[^l]%", "de[!l]*"); }

  }
}
