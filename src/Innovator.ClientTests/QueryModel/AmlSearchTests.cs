using Innovator.Client.Model;
using Innovator.Client.QueryModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel.Tests
{
  [TestClass]
  public class AmlSearchTests
  {
    [TestMethod]
    public void SimpleSearchMode()
    {
      var parser = new SimpleSearchParser();
      var prop = (IPropertyDefinition)ElementFactory.Local.FromXml("<Item type='Property'><name>name</name></Item>").AssertItem();
      var query = new QueryItem(ElementFactory.Local.LocalizationContext);
      query.AddCondition(prop, "*|short*", parser);
      if (query.Where is OrOperator orOp)
      {
        Assert.IsTrue(orOp.Left is LikeOperator);
        Assert.IsTrue(orOp.Right is LikeOperator);
      }
      else
      {
        Assert.Fail();
      }

      query = new QueryItem(ElementFactory.Local.LocalizationContext);
      query.AddCondition(prop, @"\*\|sh\ort*", parser);
      if (query.Where is LikeOperator likeOp
        && likeOp.Right is PatternList pat)
      {
        Assert.AreEqual(1, pat.Patterns.Count);
        Assert.AreEqual(1, pat.Patterns[0].Matches.Count);

        var strMatch = pat.Patterns[0].Matches[0] as StringMatch;
        Assert.IsNotNull(strMatch);
        Assert.AreEqual("|short", strMatch.Match.ToString());
      }
      else
      {
        Assert.Fail();
      }
    }
  }
}
