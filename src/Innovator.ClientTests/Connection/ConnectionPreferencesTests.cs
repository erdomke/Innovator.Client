using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Innovator.Client.Tests
{
  [TestClass]
  public class ConnectionPreferencesTests
  {
    [TestMethod]
    public void ConnectionPreferencesRoundTrip()
    {
      var cred = new ExplicitCredentials("db", "user", "pass");
      var pref = new ConnectionPreferences()
      {
        Credentials = cred,
        DefaultTimeout = 1000,
        Name = "ConnName",
        Url = "http://example.com"
      };
      pref.Headers.Locale = "en-gb";

      var saved = new SavedConnections();
      saved.Add(pref);
      var newPref = saved.First();
      var newCred = newPref.Credentials as ExplicitCredentials;

      Assert.AreEqual(cred.Database, newCred.Database);
      Assert.AreEqual(cred.Username, newCred.Username);
      Assert.AreEqual(pref.DefaultTimeout, newPref.DefaultTimeout);
      Assert.AreEqual(pref.Name, newPref.Name);
      Assert.AreEqual(pref.Url, newPref.Url);
      Assert.AreEqual(pref.Headers.Locale, newPref.Headers.Locale);
    }

    //[TestMethod]
    //public async Task Login()
    //{
    //  var pref = new ConnectionPreferences();
    //  pref.Headers.UserAgent = "Innovator.Client Test";
    //  pref.DefaultTimeout = (int)TimeSpan.FromMinutes(3).TotalMilliseconds;
    //  var conn = Factory.GetConnection("http://localhost/Innovator/", pref, true).Continue(c =>
    //  {
    //    return c.Login(new ExplicitCredentials("DEV", "admin", "pass"), true)
    //      .Convert(u =>
    //      {
    //        return (IAsyncConnection)c;
    //      });
    //  }).Wait();
    //  conn.Apply("<Item action='get' type='User' maxRecords='1' />").AssertItem();
    //}
  }
}
