using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Tests
{
  [TestClass]
  public class LanguageTests
  {
    [TestMethod]
    public void LanguageHandling()
    {
      var aml = new ElementFactory(new ServerContext(false) { LanguageCode = "fr" });
      var item = aml.FromXml(@"<Item type='Supplier' action='get' select='name' language='en,fr' xmlns:i18n='http://www.aras.com/I18N'>
  <thing>All</thing>
  <name xml:lang='fr'>Dell France</name>
  <i18n:name xml:lang='en'>Dell US</i18n:name>
  <i18n:name xml:lang='fr'>Dell France</i18n:name>
  <description xml:lang='en'>Computers</description>
  <i18n:description xml:lang='en'>Computers</i18n:description>
  <i18n:description xml:lang='fr' is_null='1' />
</Item>").AssertItem();
      Assert.AreEqual("All", item.Property("thing").Value);
      Assert.AreEqual(null, item.Property("thing", "en").Value);
      Assert.AreEqual(false, item.Property("thing", "en").Exists);
      Assert.AreEqual(null, item.Property("thing", "fr").Value);
      Assert.AreEqual(false, item.Property("thing", "fr").Exists);
      Assert.AreEqual("Dell France", item.Property("name").Value);
      Assert.AreEqual("Dell US", item.Property("name", "en").Value);
      Assert.AreEqual("Dell France", item.Property("name", "fr").Value);
      Assert.AreEqual("Computers", item.Property("description").Value);
      Assert.AreEqual("Computers", item.Property("description", "en").Value);
      Assert.AreEqual(null, item.Property("description", "fr").Value);

      item.Property("test1").Set("1");
      item.Property("test2", "fr").Set("2");
      item.Property("test3", "fr").Set("3");
      item.Property("test3", "en").Set("3.en");
      AssertExtensions.AreEqual(@"<Item type='Supplier' action='get' select='name' language='en,fr' xmlns:i18n='http://www.aras.com/I18N'>
  <thing>All</thing>
  <name xml:lang='fr'>Dell France</name>
  <i18n:name xml:lang='en'>Dell US</i18n:name>
  <i18n:name xml:lang='fr'>Dell France</i18n:name>
  <description xml:lang='en'>Computers</description>
  <i18n:description xml:lang='en'>Computers</i18n:description>
  <i18n:description xml:lang='fr' is_null='1' />
  <test1>1</test1>
  <i18n:test2 xml:lang='fr'>2</i18n:test2>
  <i18n:test3 xml:lang='fr'>3</i18n:test3>
  <i18n:test3 xml:lang='en'>3.en</i18n:test3>
</Item>", item);
    }

    [TestMethod]
    public void LanguageHandling_v2()
    {
      const string itemAml = @"<Item type='Property' id='1234' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='en'>Finished Part Diameter</label>
  <i18n:label xml:lang='de'>Fertigteildurchmesser</i18n:label>
  <i18n:label xml:lang='en'>Finished Part Diameter</i18n:label>
</Item>";
      var item = ElementFactory.Local.FromXml(itemAml).AssertItem();
      Assert.AreEqual("Finished Part Diameter", item.Property("label").Value);
      Assert.AreEqual("Finished Part Diameter", item.Property("label", "en").Value);
      Assert.AreEqual("Fertigteildurchmesser", item.Property("label", "de").Value);
    }

    [TestMethod]
    public void StandardGet()
    {
      var amlEnglish = new ElementFactory(new ServerContext(false) { LanguageCode = "en" });
      var amlGerman = new ElementFactory(new ServerContext(false) { LanguageCode = "de" });

      var getEnglish = amlEnglish.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='en'>Description</label>
</Item>").AssertItem();
      Assert.AreEqual("Description", getEnglish.Property("label").Value);
      Assert.AreEqual(null, getEnglish.Property("label", "de").Value);
      //Assert.AreEqual("Description", getEnglish.Property("label", "en").Value);
      Assert.AreEqual(null, getEnglish.Property("label", "en").Value);

      var getGerman = amlGerman.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='de'>Beschreibung</label>
</Item>").AssertItem();

      Assert.AreEqual("Beschreibung", getGerman.Property("label").Value);
      //Assert.AreEqual("Beschreibung", getGerman.Property("label", "de").Value);
      Assert.AreEqual(null, getGerman.Property("label", "de").Value);
      Assert.AreEqual(null, getGerman.Property("label", "en").Value);

      getGerman = amlGerman.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='de'>Beschreibung2</label>
</Item>").AssertItem();

      Assert.AreEqual("Beschreibung2", getGerman.Property("label").Value);
      Assert.AreEqual(null, getGerman.Property("label", "de").Value);
      Assert.AreEqual(null, getGerman.Property("label", "en").Value);

      getGerman.Property("label", "en").Set("Description2");
      getGerman.Property("label", "de").Set("Beschreibung2");
      Assert.AreEqual("Beschreibung2", getGerman.Property("label").Value);
      Assert.AreEqual("Beschreibung2", getGerman.Property("label", "de").Value);
      Assert.AreEqual("Description2", getGerman.Property("label", "en").Value);
      AssertExtensions.AreEqual(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='de'>Beschreibung2</label>
  <i18n:label xml:lang='en'>Description2</i18n:label>
  <i18n:label xml:lang='de'>Beschreibung2</i18n:label>
</Item>", getGerman);
    }

    [TestMethod]
    public void AllLanguages_NoSelect()
    {
      var amlEnglish = new ElementFactory(new ServerContext(false) { LanguageCode = "en" });
      var amlGerman = new ElementFactory(new ServerContext(false) { LanguageCode = "de" });

      var getEnglish = amlEnglish.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <i18n:label xml:lang='de'>Beschreibung</i18n:label>
  <i18n:label xml:lang='en'>Description</i18n:label>
</Item>").AssertItem();
      Assert.AreEqual("Description", getEnglish.Property("label").Value);
      Assert.AreEqual("Beschreibung", getEnglish.Property("label", "de").Value);
      Assert.AreEqual("Description", getEnglish.Property("label", "en").Value);

      var getGerman = amlGerman.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <i18n:label xml:lang='de'>Beschreibung</i18n:label>
  <i18n:label xml:lang='en'>Description</i18n:label>
</Item>").AssertItem();

      Assert.AreEqual("Beschreibung", getGerman.Property("label").Value);
      Assert.AreEqual("Beschreibung", getGerman.Property("label", "de").Value);
      Assert.AreEqual("Description", getGerman.Property("label", "en").Value);
    }

    [TestMethod]
    public void AllLanguages_Select()
    {
      var amlEnglish = new ElementFactory(new ServerContext(false) { LanguageCode = "en" });
      var amlGerman = new ElementFactory(new ServerContext(false) { LanguageCode = "de" });

      var getEnglish = amlEnglish.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='en'>Description</label>
  <i18n:label xml:lang='de'>Beschreibung</i18n:label>
  <i18n:label xml:lang='en'>Description</i18n:label>
</Item>").AssertItem();
      Assert.AreEqual("Description", getEnglish.Property("label").Value);
      Assert.AreEqual("Beschreibung", getEnglish.Property("label", "de").Value);
      Assert.AreEqual("Description", getEnglish.Property("label", "en").Value);

      var getGerman = amlGerman.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='de'>Beschreibung</label>
  <i18n:label xml:lang='de'>Beschreibung</i18n:label>
  <i18n:label xml:lang='en'>Description</i18n:label>
</Item>").AssertItem();

      Assert.AreEqual("Beschreibung", getGerman.Property("label").Value);
      Assert.AreEqual("Beschreibung", getGerman.Property("label", "de").Value);
      Assert.AreEqual("Description", getGerman.Property("label", "en").Value);
    }

    [TestMethod]
    public void MissingTranslation_StandardGet()
    {
      var amlEnglish = new ElementFactory(new ServerContext(false) { LanguageCode = "en" });
      var amlGerman = new ElementFactory(new ServerContext(false) { LanguageCode = "de" });
      var amlSpanish = new ElementFactory(new ServerContext(false) { LanguageCode = "es" });

      var getEnglish = amlEnglish.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='en'>Description</label>
</Item>").AssertItem();
      Assert.AreEqual("Description", getEnglish.Property("label").Value);
      Assert.AreEqual(null, getEnglish.Property("label", "de").Value);
      //Assert.AreEqual("Description", getEnglish.Property("label", "en").Value);
      Assert.AreEqual(null, getEnglish.Property("label", "en").Value);

      getEnglish.Property("label", "en").Set("Description2");
      getEnglish.Property("label", "de").Set("Beschreibung2");
      //Assert.AreEqual("Description2", getEnglish.Property("label").Value);
      Assert.AreEqual("Description", getEnglish.Property("label").Value);
      Assert.AreEqual("Beschreibung2", getEnglish.Property("label", "de").Value);
      Assert.AreEqual("Description2", getEnglish.Property("label", "en").Value);
      AssertExtensions.AreEqual(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='en'>Description</label>
  <i18n:label xml:lang='en'>Description2</i18n:label>
  <i18n:label xml:lang='de'>Beschreibung2</i18n:label>
</Item>", getEnglish);

      var getGerman = amlGerman.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='en'>Description</label>
</Item>").AssertItem();

      Assert.AreEqual("Description", getGerman.Property("label").Value);
      Assert.AreEqual(null, getGerman.Property("label", "de").Value);
      Assert.AreEqual(null, getGerman.Property("label", "en").Value);

      getGerman.Property("label", "en").Set("Description2");
      getGerman.Property("label", "de").Set("Beschreibung2");
      Assert.AreEqual("Description", getGerman.Property("label").Value);
      Assert.AreEqual("Beschreibung2", getGerman.Property("label", "de").Value);
      Assert.AreEqual("Description2", getGerman.Property("label", "en").Value);
      AssertExtensions.AreEqual(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='en'>Description</label>
  <i18n:label xml:lang='en'>Description2</i18n:label>
  <i18n:label xml:lang='de'>Beschreibung2</i18n:label>
</Item>", getGerman);

      var getSpanish = amlSpanish.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='en'>Description</label>
</Item>").AssertItem();

      Assert.AreEqual("Description", getSpanish.Property("label").Value);
      Assert.AreEqual(null, getSpanish.Property("label", "de").Value);
      Assert.AreEqual(null, getSpanish.Property("label", "es").Value);
      Assert.AreEqual(null, getSpanish.Property("label", "en").Value);
    }

    [TestMethod]
    public void MissingTranslation_AllLanguages_NoSelect()
    {
      var amlEnglish = new ElementFactory(new ServerContext(false) { LanguageCode = "en" });
      var amlGerman = new ElementFactory(new ServerContext(false) { LanguageCode = "de" });

      var getEnglish = amlEnglish.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <i18n:label xml:lang='en'>Description</i18n:label>
</Item>").AssertItem();
      Assert.AreEqual("Description", getEnglish.Property("label").Value);
      Assert.AreEqual(null, getEnglish.Property("label", "de").Value);
      Assert.AreEqual("Description", getEnglish.Property("label", "en").Value);

      var getGerman = amlGerman.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <i18n:label xml:lang='en'>Description</i18n:label>
</Item>").AssertItem();

      Assert.AreEqual(null, getGerman.Property("label").Value);
      Assert.AreEqual(null, getGerman.Property("label", "de").Value);
      Assert.AreEqual("Description", getGerman.Property("label", "en").Value);
    }

    [TestMethod]
    public void MissingTranslation_AllLanguages_Select()
    {
      var amlEnglish = new ElementFactory(new ServerContext(false) { LanguageCode = "en" });
      var amlGerman = new ElementFactory(new ServerContext(false) { LanguageCode = "de" });

      var getEnglish = amlEnglish.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='en'>Description</label>
  <i18n:label is_null='1' xml:lang='de' />
  <i18n:label xml:lang='en'>Description</i18n:label>
</Item>").AssertItem();
      Assert.AreEqual("Description", getEnglish.Property("label").Value);
      Assert.AreEqual(null, getEnglish.Property("label", "de").Value);
      Assert.AreEqual("Description", getEnglish.Property("label", "en").Value);

      var getGerman = amlGerman.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label xml:lang='en'>Description</label>
  <i18n:label is_null='1' xml:lang='de' />
  <i18n:label xml:lang='en'>Description</i18n:label>
</Item>").AssertItem();

      Assert.AreEqual("Description", getGerman.Property("label").Value);
      Assert.AreEqual(null, getGerman.Property("label", "de").Value);
      Assert.AreEqual("Description", getGerman.Property("label", "en").Value);
    }

    [TestMethod]
    public void MissingLanguageProperties()
    {
      var amlEnglish = new ElementFactory(new ServerContext(false) { LanguageCode = "en" });
      var amlGerman = new ElementFactory(new ServerContext(false) { LanguageCode = "de" });

      var getEnglish = amlEnglish.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label>Description</label>
</Item>").AssertItem();
      Assert.AreEqual("Description", getEnglish.Property("label").Value);
      Assert.AreEqual(null, getEnglish.Property("label", "de").Value);
      Assert.AreEqual(null, getEnglish.Property("label", "en").Value);

      getEnglish.Property("label").Set("Description2");
      getEnglish.Property("label", "en").Set("Description2");
      getEnglish.Property("label", "de").Set("Beschreibung2");
      Assert.AreEqual("Description2", getEnglish.Property("label").Value);
      Assert.AreEqual("Description2", getEnglish.Property("label", "en").Value);
      Assert.AreEqual("Beschreibung2", getEnglish.Property("label", "de").Value);
      AssertExtensions.AreEqual(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label>Description2</label>
  <i18n:label xml:lang='en'>Description2</i18n:label>
  <i18n:label xml:lang='de'>Beschreibung2</i18n:label>
</Item>", getEnglish);

      var getGerman = amlGerman.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label>Beschreibung</label>
</Item>").AssertItem();

      Assert.AreEqual("Beschreibung", getGerman.Property("label").Value);
      Assert.AreEqual(null, getGerman.Property("label", "de").Value);
      Assert.AreEqual(null, getGerman.Property("label", "en").Value);

      getGerman.Property("label").Set("Beschreibung2");
      getGerman.Property("label", "en").Set("Description2");
      getGerman.Property("label", "de").Set("Beschreibung2");
      Assert.AreEqual("Beschreibung2", getGerman.Property("label").Value);
      Assert.AreEqual("Description2", getGerman.Property("label", "en").Value);
      Assert.AreEqual("Beschreibung2", getGerman.Property("label", "de").Value);
      AssertExtensions.AreEqual(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label>Beschreibung2</label>
  <i18n:label xml:lang='en'>Description2</i18n:label>
  <i18n:label xml:lang='de'>Beschreibung2</i18n:label>
</Item>", getGerman);
    }

    [TestMethod]
    public void NewItem_Set()
    {
      var amlEnglish = new ElementFactory(new ServerContext(false) { LanguageCode = "en" });
      var amlGerman = new ElementFactory(new ServerContext(false) { LanguageCode = "de" });

      var getEnglish = amlEnglish.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
</Item>").AssertItem();

      getEnglish.Property("label").Set("Description2");
      getEnglish.Property("label", "en").Set("Description2");
      getEnglish.Property("label", "de").Set("Beschreibung2");
      Assert.AreEqual("Description2", getEnglish.Property("label").Value);
      Assert.AreEqual("Description2", getEnglish.Property("label", "en").Value);
      Assert.AreEqual("Beschreibung2", getEnglish.Property("label", "de").Value);
      AssertExtensions.AreEqual(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label>Description2</label>
  <i18n:label xml:lang='en'>Description2</i18n:label>
  <i18n:label xml:lang='de'>Beschreibung2</i18n:label>
</Item>", getEnglish);

      var getGerman = amlGerman.FromXml(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
</Item>").AssertItem();

      getGerman.Property("label").Set("Beschreibung2");
      getGerman.Property("label", "en").Set("Description2");
      getGerman.Property("label", "de").Set("Beschreibung2");
      Assert.AreEqual("Beschreibung2", getGerman.Property("label").Value);
      Assert.AreEqual("Description2", getGerman.Property("label", "en").Value);
      Assert.AreEqual("Beschreibung2", getGerman.Property("label", "de").Value);
      AssertExtensions.AreEqual(@"<Item type='Property' xmlns:i18n='http://www.aras.com/I18N'>
  <label>Beschreibung2</label>
  <i18n:label xml:lang='en'>Description2</i18n:label>
  <i18n:label xml:lang='de'>Beschreibung2</i18n:label>
</Item>", getGerman);
    }
  }
}
