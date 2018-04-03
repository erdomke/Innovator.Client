using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Innovator.Client;

namespace Innovator.ClientTests
{
  [TestClass]
  public class CommandFileTests
  {
    static string testFilePath = "testFile.txt";
    static long testFileSize = 0;
    [ClassInitialize]
    public static void Init(TestContext context)
    {
      using (var file = new StreamWriter(testFilePath))
      {
        for (int i = 0; i < 500; i++)
        {
          file.WriteLine(Guid.NewGuid());
        }
      }
      testFileSize = new FileInfo(testFilePath).Length;
      Console.WriteLine(testFileSize);
    }

#if NETFULL
    [TestMethod]
    public void CommandFile_ByPath_TestLength()
    {
      var file = new CommandFile("ABC", testFilePath, null, "DEF", true);
      Console.WriteLine(file.Length);
      Assert.AreEqual(testFileSize, file.Length);
      Assert.IsTrue(file.Aml.Contains(testFileSize.ToString()));
      Console.WriteLine(file.Aml);
    }
#endif

    [TestMethod]
    public void CommandFile_ByPathAndStream_TestLength()
    {
      using (var stream = File.OpenRead(testFilePath))
      {
        var file = new CommandFile("ABC", testFilePath, stream, "DEF", true);
        Console.WriteLine(file.Length);
        Assert.AreEqual(testFileSize, file.Length);
        Assert.IsTrue(file.Aml.Contains(testFileSize.ToString()));
        Console.WriteLine(file.Aml);
      }
    }

    [ClassCleanup]
    public static void Cleanup()
    {
      if (File.Exists(testFilePath))
      {
        File.Delete(testFilePath);
      }
    }
  }
}
