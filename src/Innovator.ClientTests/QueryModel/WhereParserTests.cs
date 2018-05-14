using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel.Tests
{
  [TestClass]
  public class WhereParserTests
  {
    private static readonly QueryItem _table = new QueryItem(ElementFactory.Local.LocalizationContext)
    {
      Type = "user"
    };

    [TestMethod]
    public void WhereClauseParse01()
    {
      var expr = SqlWhereParser.Parse("prop = 'value' OR [User].owned_by_id = 'first' and [User].managed_by_id = 'another'", _table);
      var orOp = expr as OrOperator;
      Assert.IsNotNull(orOp);
      Assert.IsInstanceOfType(orOp.Left, typeof(EqualsOperator));
      Assert.IsInstanceOfType(orOp.Right, typeof(AndOperator));
    }

    [TestMethod]
    public void WhereClauseParse02()
    {
      var expr = SqlWhereParser.Parse("not [User].owned_by_id >= 'first' and [User].managed_by_id <> 'another'", _table);
      var andOp = expr as AndOperator;
      Assert.IsNotNull(andOp);
      var notOp = andOp.Left as NotOperator;
      Assert.IsNotNull(notOp);
      Assert.IsInstanceOfType(notOp.Arg, typeof(GreaterThanOrEqualsOperator));
      Assert.IsInstanceOfType(andOp.Right, typeof(NotEqualsOperator));
    }

    [TestMethod]
    public void WhereClauseParse03()
    {
      var expr = SqlWhereParser.Parse("created_on > getdate() and [User].owned_by_id in ('first', 'second', 'third', 'fourth')", _table);

      var andOp = expr as AndOperator;
      Assert.IsNotNull(andOp);
      var gtOp = andOp.Left as GreaterThanOperator;
      Assert.IsNotNull(gtOp);
      var func = gtOp.Right as FunctionExpression;
      Assert.IsNotNull(func);
      Assert.AreEqual("getdate", func.Name);

      var inOp = andOp.Right as InOperator;
      Assert.IsNotNull(inOp);

      CollectionAssert.AreEqual(new[] { new StringLiteral("first"), new StringLiteral("second"), new StringLiteral("third"), new StringLiteral("fourth") }, inOp.Right.Values.ToArray());
    }

    [TestMethod]
    public void WhereClauseParse04()
    {
      var expr = SqlWhereParser.Parse("(prop = 'value' OR [User].owned_by_id = 'first') and [User].managed_by_id = 'another'", _table);
      var andOp = expr as AndOperator;
      Assert.IsNotNull(andOp);
      Assert.IsInstanceOfType(andOp.Left, typeof(OrOperator));
      Assert.IsInstanceOfType(andOp.Right, typeof(EqualsOperator));
    }

    [TestMethod]
    public void WhereClauseParse05()
    {
      var expr = SqlWhereParser.Parse("prop not like 'val%' OR [User].owned_by_id is not null", _table);
      var orOp = expr as OrOperator;
      Assert.IsNotNull(orOp);
      Assert.IsInstanceOfType(orOp.Left, typeof(NotLikeOperator));

      var isOp = orOp.Right as IsOperator;
      Assert.IsNotNull(isOp);
      Assert.AreEqual(IsOperand.NotNull, isOp.Right);
    }

    [TestMethod]
    public void WhereClauseParse06()
    {
      var expr = SqlWhereParser.Parse("prop like 'val%' OR [User].owned_by_id is null", _table);
      var orOp = expr as OrOperator;
      Assert.IsNotNull(orOp);
      Assert.IsInstanceOfType(orOp.Left, typeof(LikeOperator));

      var isOp = orOp.Right as IsOperator;
      Assert.IsNotNull(isOp);
      Assert.AreEqual(IsOperand.Null, isOp.Right);
    }

    [TestMethod]
    public void WhereClauseParse07()
    {
      var expr = SqlWhereParser.Parse("O_ORDERDATE >= '1993-07-01' AND O_ORDERDATE < dateadd(mm,3, '1993-07-01')", _table);
      var andOp = expr as AndOperator;
      Assert.IsNotNull(andOp);
      Assert.IsInstanceOfType(andOp.Left, typeof(GreaterThanOrEqualsOperator));

      var ltOp = andOp.Right as LessThanOperator;
      Assert.IsNotNull(ltOp);

      var func = ltOp.Right as FunctionExpression;
      Assert.IsNotNull(func);
      Assert.AreEqual(3, func.Args.Count);
    }

    [TestMethod]
    public void WhereClauseParse08()
    {
      var expr = SqlWhereParser.Parse("L_SHIPDATE >= '1994-01-01' AND L_DISCOUNT BETWEEN .06 - 0.01 AND .06 + 0.01 AND L_QUANTITY < 24", _table);
      var andOp = expr as AndOperator;
      Assert.IsNotNull(andOp);
      Assert.IsInstanceOfType(andOp.Right, typeof(LessThanOperator));

      andOp = andOp.Left as AndOperator;
      Assert.IsNotNull(andOp);
      Assert.IsInstanceOfType(andOp.Left, typeof(GreaterThanOrEqualsOperator));

      var between = andOp.Right as BetweenOperator;
      Assert.IsNotNull(between);
      Assert.IsInstanceOfType(between.Min, typeof(SubtractionOperator));
      Assert.IsInstanceOfType(between.Max, typeof(AdditionOperator));
    }

    [TestMethod]
    public void WhereClauseParse09()
    {
      var expr = SqlWhereParser.Parse("L_SHIPDATE >= '1994-01-01' AND L_DISCOUNT NOT BETWEEN .06 - 0.01 AND .06 + 0.01 AND L_QUANTITY < 24", _table);
      var andOp = expr as AndOperator;
      Assert.IsNotNull(andOp);
      Assert.IsInstanceOfType(andOp.Right, typeof(LessThanOperator));

      andOp = andOp.Left as AndOperator;
      Assert.IsNotNull(andOp);
      Assert.IsInstanceOfType(andOp.Left, typeof(GreaterThanOrEqualsOperator));

      var between = andOp.Right as NotBetweenOperator;
      Assert.IsNotNull(between);
      Assert.IsInstanceOfType(between.Min, typeof(SubtractionOperator));
      Assert.IsInstanceOfType(between.Max, typeof(AdditionOperator));
    }

    [TestMethod]
    public void WhereClauseParse10()
    {
      var expr = SqlWhereParser.Parse("SUBSTRING(C_PHONE,1,2) IN ('13', '31', '23', '29', '30', '18', '17')", _table);
      var inOp = expr as InOperator;
      Assert.IsNotNull(inOp);
      Assert.AreEqual(7, inOp.Right.Values.Count);

      var func = inOp.Left as FunctionExpression;
      Assert.IsNotNull(func);
      Assert.AreEqual(3, func.Args.Count);
    }
  }
}
