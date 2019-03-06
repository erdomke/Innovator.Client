using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class JsonTests
  {
    [TestMethod()]
    public void FlattenTests()
    {
      var flatList = Flatten("{\"bool\": true, \"num\": 3.45, \"string\": \"value string\", \"array\": [2, \"second\"] }");
      CollectionAssert.AreEqual(new[]
      {
        new KeyValuePair<string, object>("$.bool", true),
        new KeyValuePair<string, object>("$.num", 3.45),
        new KeyValuePair<string, object>("$.string", "value string"),
        new KeyValuePair<string, object>("$.array[0]", 2),
        new KeyValuePair<string, object>("$.array[1]", "second"),
      }, flatList);

      flatList = Flatten("[4, 8, 9]");
      CollectionAssert.AreEqual(new[]
      {
        new KeyValuePair<string, object>("$[0]", 4),
        new KeyValuePair<string, object>("$[1]", 8),
        new KeyValuePair<string, object>("$[2]", 9),
      }, flatList);

      flatList = Flatten("{\"locations\":[{\"uri\":\"http://localhost.fiddler/Innovator12Beta/oauthserver/\"}]}");
      CollectionAssert.AreEqual(new[]
      {
        new KeyValuePair<string, object>("$.locations[0].uri", "http://localhost.fiddler/Innovator12Beta/oauthserver/"),
      }, flatList);
    }

    private List<KeyValuePair<string, object>> Flatten(string json)
    {
      using (var reader = new StringReader(json))
      using (var jReader = new Json.Embed.JsonTextReader(reader))
      {
        return jReader.Flatten().ToList();
      }
    }
  }
}
