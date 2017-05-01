using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aras.IOM;
using Innovator.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class IomTests
  {
    [TestMethod()]
    public void IomTest_Vault()
    {
      var creds = System.IO.File.ReadAllLines(@"C:\Users\eric.domke\Documents\Cred.txt");
      var connOld = IomFactory.CreateHttpServerConnection("http://zvm161-spdev:8080/Innovator11sp9", "InnTest11sp9", creds[0], creds[1]);
      connOld.Login();

      var inn = IomFactory.CreateInnovator(connOld);
      var item = inn.newItem("File", "add");
      item.setProperty("filename", "JavascriptProsCons.txt");
      item.attachPhysicalFile(@"C:\Users\eric.domke\Desktop\JavascriptProsCons.txt");
      item.apply();
    }

    [TestMethod()]
    public void IomTest_Main()
    {
      var creds = System.IO.File.ReadAllLines(@"C:\Users\eric.domke\Documents\Cred.txt");
      var connOld = IomFactory.CreateHttpServerConnection("http://zvm161-spdev:8080/Innovator11sp5", "InnVanilla11sp5", creds[0], creds[1]);
      connOld.Login();

      var conn = new IomConnection(connOld);
      Assert.AreEqual("InnVanilla11sp5", conn.Database);
      Assert.AreEqual("30B991F927274FA3829655F50C99472E", conn.UserId);

      var login = conn.Apply("<Item type='User' action='get' id='@0'></Item>", conn.UserId).AssertItem<User>().LoginName().Value;
      Assert.AreEqual("admin", login);
    }
  }
}
