using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel.Tests
{
  [TestClass]
  public class ListExpressionTests
  {
    [TestMethod]
    public void TestInClauseParse()
    {
      var clause = "  ('string', -2.34e-3, +345, 'quote''d' )  ";
      var actual = ListExpression.FromSqlInClause(clause);
      var expected = new List<IOperand>()
      {
        new StringLiteral("string"), new FloatLiteral(-2.34e-3), new IntegerLiteral(345), new StringLiteral("quote'd")
      };
      CollectionAssert.AreEqual(expected, (List<IOperand>)actual.Values);


      clause = "'string',-2.34e-3,+345,'quote''d'";
      actual = ListExpression.FromSqlInClause(clause);
      CollectionAssert.AreEqual(expected, (List<IOperand>)actual.Values);

      clause = "-2.34e-3,'string','quote''d',+345";
      actual = ListExpression.FromSqlInClause(clause);
      expected = new List<IOperand>()
      {
        new FloatLiteral(-2.34e-3), new StringLiteral("string"), new StringLiteral("quote'd"), new IntegerLiteral(345)
      };
      CollectionAssert.AreEqual(expected, (List<IOperand>)actual.Values);
    }
  }
}
