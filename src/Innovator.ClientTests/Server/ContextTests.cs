using System;
using Innovator.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Innovator.Client.Tests
{
  [TestClass]
  public class ContextTests
  {
    [TestMethod]
    public void ValidationContext_IsChanging_WorksCorrectly()
    {
      var conn = new TestConnection();
      var aml = conn.AmlContext;

      var changes = aml.FromXml(@"<Item type='User' action='update' id='49403709D9F847ECA1A2DE9ADE68660F'>
  <first_name>John</first_name>
</Item>").AssertItem();
      var validationContext = new ValidationContext(conn, changes);

      Assert.IsFalse(validationContext.IsChanging("first_name"));
      Assert.IsFalse(validationContext.IsChanging("first_name", "last_name"));
      Assert.IsFalse(validationContext.IsChanging("last_name"));

      changes = aml.FromXml(@"<Item type='User' action='update' id='49403709D9F847ECA1A2DE9ADE68660F'>
  <first_name>Jim</first_name>
</Item>").AssertItem();
      validationContext = new ValidationContext(conn, changes);

      Assert.IsTrue(validationContext.IsChanging("first_name"));
      Assert.IsTrue(validationContext.IsChanging("first_name", "last_name"));
      Assert.IsFalse(validationContext.IsChanging("last_name"));
    }
  }
}
