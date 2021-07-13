using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class ItemTests
  {
    [TestMethod()]
    public void NullItemForInterface()
    {
      var nullItem = Item.GetNullItem<IReadOnlyItem>();
      Assert.AreNotEqual(null, nullItem);
      nullItem = Item.GetNullItem<IItem>();
      Assert.AreNotEqual(null, nullItem);
      nullItem = new IReadOnlyItem[] { }.FirstOrNullItem();
      Assert.AreNotEqual(null, nullItem);
      nullItem = new IItem[] { }.FirstOrNullItem();
      Assert.AreNotEqual(null, nullItem);
    }

    [TestMethod()]
    public void RelationshipSerialization()
    {
      var aml = ElementFactory.Local;
      var input = @"<Item type=""List"" typeId=""5736C479A8CB49BCA20138514C637266"" id=""D7D72BF68937462B947DAC6BE7E28322""><Relationships><Item type=""Value"" /></Relationships></Item>";
      var item = aml.FromXml(input).AssertItem();
      Assert.AreEqual(input, item.ToAml());
    }

#if NETFULL
    [TestMethod()]
    public void NullPropertySetIsNullAttribute()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Type("Stuff"), aml.Action("edit"), aml.Property("first", null), aml.Property("second", DBNull.Value), aml.Property("third", "stuff"), aml.Property("fourth"));
      Assert.AreEqual("<Item type=\"Stuff\" action=\"edit\"><first is_null=\"1\" /><second is_null=\"1\" /><third>stuff</third><fourth /></Item>", item.ToAml());
    }
#else
    [TestMethod()]
    public void NullPropertySetIsNullAttribute()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Type("Stuff"), aml.Action("edit"), aml.Property("first", null), aml.Property("second", null), aml.Property("third", "stuff"), aml.Property("fourth"));
      Assert.AreEqual("<Item type=\"Stuff\" action=\"edit\"><first is_null=\"1\" /><second is_null=\"1\" /><third>stuff</third><fourth /></Item>", item.ToAml());
    }
#endif

    [TestMethod()]
    public void PropertySetWithNullableData()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Type("Stuff"), aml.Action("edit"));
      DateTime? someDate = null;
      DateTime? someDate2 = new DateTime(2016, 01, 01);
      item.Property("some_date").Set(someDate);
      item.Property("some_date_2").Set(someDate2);
      Assert.AreEqual("<Item type=\"Stuff\" action=\"edit\"><some_date is_null=\"1\" /><some_date_2>2016-01-01T00:00:00</some_date_2></Item>", item.ToAml());
    }

    [TestMethod()]
    public void PropertySetWithNumber()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Type("Stuff"), aml.Action("edit"));
      item.Property("some_val").Set(1000);
      item.Property("some_val_2").Set(0.00000000000000000000000000000000000000000000064879);
      Assert.AreEqual("<Item type=\"Stuff\" action=\"edit\"><some_val>1000</some_val><some_val_2>0.00000000000000000000000000000000000000000000064879</some_val_2></Item>", item.ToAml());
    }

    [TestMethod()]
    public void UtcDateConversion()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Type("stuff"), aml.Property("created_on", "2016-05-24T13:22:42"));
      var localDate = item.CreatedOn().AsDateTime().Value;
      var utcDate = item.CreatedOn().AsDateTimeUtc().Value;
      Assert.AreEqual(DateTime.Parse("2016-05-24T13:22:42"), localDate);
      Assert.AreEqual(DateTime.Parse("2016-05-24T17:22:42"), utcDate);
    }

    [TestMethod()]
    public void PropertyItemExtraction()
    {
      var aml = ElementFactory.Local;
      var result = aml.FromXml("<Item type='thing' id='1234'><item_prop type='another' keyed_name='stuff'>12345ABCDE12345612345ABCDE123456</item_prop></Item>");
      var propItem = result.AssertItem().Property("item_prop").AsItem().ToAml();
      Assert.AreEqual("<Item type=\"another\" id=\"12345ABCDE12345612345ABCDE123456\"><keyed_name>stuff</keyed_name><id keyed_name=\"stuff\" type=\"another\">12345ABCDE12345612345ABCDE123456</id></Item>", propItem);
    }

    [TestMethod()]
    public void VaultPictureUrlToItem()
    {
      var aml = ElementFactory.Local;
      var result = aml.FromXml(@"<Item type='CAD' typeId='CCF205347C814DD1AF056875E0A880AC' id='2B2444304435441AA1137972D2B8B534'>
  <thumbnail>vault:///?fileId=1E49D4C8BE6545F9882A28C0763F473A</thumbnail>
</Item>");
      var propItem = result.AssertItem().Property("thumbnail").AsItem().ToAml();
      Assert.AreEqual("<Item type=\"File\" id=\"1E49D4C8BE6545F9882A28C0763F473A\"><id type=\"File\">1E49D4C8BE6545F9882A28C0763F473A</id></Item>", propItem);
      Assert.AreEqual("1E49D4C8BE6545F9882A28C0763F473A", result.AssertItem().Property("thumbnail").AsGuid().ToArasId());
      Assert.AreEqual("vault:///?fileId=1E49D4C8BE6545F9882A28C0763F473A", result.AssertItem().Property("thumbnail").Value);
      Assert.AreEqual("vault:///?fileId=1E49D4C8BE6545F9882A28C0763F473A", result.AssertItem().Property("thumbnail").AsString(""));
    }


    [TestMethod()]
    public void AmlFromArray()
    {
      var aml = ElementFactory.Local;
      IEnumerable<object> parts = new object[] { aml.Type("stuff"), aml.Attribute("keyed_name", "thingy"), "12345ABCDE12345612345ABCDE123456" };
      var item = aml.Item(aml.Type("Random Thing"),
        aml.Property("item_ref", parts)
      );
      Assert.AreEqual("<Item type=\"Random Thing\"><item_ref type=\"stuff\" keyed_name=\"thingy\">12345ABCDE12345612345ABCDE123456</item_ref></Item>", item.ToAml());
    }

    [TestMethod()]
    public void WhereUsedTest()
    {
      var aml = ElementFactory.Local;
      var result = aml.FromXml(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='Part' id='60664D33AEC245BBBEAEFC46612B87F5' icon='../images/customer/images/Part16.png' keyed_name='Part - 1 - D.2' loaded='1'>
        <relatedItems>
          <Item type='Affected Item' id='ACD4739883864B548FE0671634CB7670' keyed_name='Affected Item - 7 - A.1'></Item>
          <Item type='Part' id='F7C39CE1AB4245D4A1695075BC7F9B49' icon='../images/customer/images/Part16.png' keyed_name='Part - 7 - 2.2'></Item>
          <Item type='Part' id='F921E5F7576342698771C2539AFC23BD' icon='../images/customer/images/Part16.png' keyed_name='Part - 7 - 3.3'></Item>
        </relatedItems>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>");
      var items = result.AssertItem().Element("relatedItems").Elements().OfType<IReadOnlyItem>().ToArray();
      Assert.AreEqual(3, items.Length);
      Assert.AreEqual("ACD4739883864B548FE0671634CB7670", items[0].Id());
      Assert.AreEqual("Part", items[1].Type().Value);
    }

    [TestMethod()]
    public void CloneAsNew()
    {
      var aml = @"<Item type='PCO'>
  <Relationships>
    <Item type='PCO Task' typeId='34D0942644C04D6D975C871961559FAA' id='A68D766FBB974F03925D9AA468F5E61C'>
      <copied_from is_null='1' />
      <id keyed_name='A68D766FBB974F03925D9AA468F5E61C' type='PCO Task'>A68D766FBB974F03925D9AA468F5E61C</id>
      <related_id keyed_name='Confirm material handling and storage requirements' type='Task'>
        <Item type='Task' typeId='3B76D2C3A38142BBAA09995160D485A4' id='13E475EB99D44AC29C94A6F63DCBBD7C'>
          <config_id keyed_name='Confirm material handling and storage requirements' type='Task'>13E475EB99D44AC29C94A6F63DCBBD7C</config_id>
          <created_by_id keyed_name='Eric Domke' type='User'>2D246C5838644C1C8FD34F8D2796E327</created_by_id>
          <created_on>2018-04-06T08:32:35</created_on>
          <current_state name='Planning' keyed_name='Planning' type='Life Cycle State'>ED755CB822BD4D50A91E1219CD3599AC</current_state>
          <date_due_target>2018-04-06T08:32:35</date_due_target>
          <generation>1</generation>
          <id keyed_name='Confirm material handling and storage requirements' type='Task'>13E475EB99D44AC29C94A6F63DCBBD7C</id>
          <indent>0</indent>
          <is_complete>0</is_complete>
          <is_current>1</is_current>
          <is_released>0</is_released>
          <keyed_name>Confirm material handling and storage requirements</keyed_name>
          <modified_by_id keyed_name='Eric Domke' type='User'>2D246C5838644C1C8FD34F8D2796E327</modified_by_id>
          <modified_on>2018-04-06T08:32:35</modified_on>
          <new_version>0</new_version>
          <not_lockable>0</not_lockable>
          <owned_by_id keyed_name='* Owner' type='Identity'>
            <Item type='Identity' typeId='E582AB17663F4EF28460015B2BE9E094' id='538B300BB2A347F396C436E9EEE1976C'>
              <id keyed_name='* Owner' type='Identity'>538B300BB2A347F396C436E9EEE1976C</id>
              <is_alias>0</is_alias>
              <itemtype>E582AB17663F4EF28460015B2BE9E094</itemtype>
            </Item>
          </owned_by_id>
          <parent_project keyed_name='PCO-16549' type='Simple Project'>B0D545C34B9C449088394C164D846FAB</parent_project>
          <permission_id keyed_name='Task - Planning' type='Permission'>FA094E3BE840483895602050EBFC62CB</permission_id>
          <state>Planning</state>
          <state_image>../images/customer/images/Project_Planning22.png</state_image>
          <team_id keyed_name='B0D545C34B9C449088394C164D846FAB' type='Team'>B69BCA6C8B354CFBB9BB4CBBE6D51C56</team_id>
          <name>Confirm material handling and storage requirements</name>
          <itemtype>3B76D2C3A38142BBAA09995160D485A4</itemtype>
        </Item>
      </related_id>
      <source_id keyed_name='PCO-16549' type='Process Change Order'>B0D545C34B9C449088394C164D846FAB</source_id>
    </Item>
  </Relationships>
</Item>";
      var item = ElementFactory.Local.FromXml(aml).AssertItem();
      var settings = new CloneSettings()
      {
        DoCloneItem = (path, i) => !path.EndsWith("/Identity", StringComparison.OrdinalIgnoreCase)
      };


      var pco = item.CloneAsNew(settings);
      Assert.IsFalse(pco.Elements().OfType<IReadOnlyProperty>().Any());
      Assert.AreNotEqual(item.Id(), pco.Id());

      var pcoTask = pco.Relationships().Single();
      Assert.AreNotEqual(item.Relationships().Single().Id(), pcoTask.Id());
      Assert.AreEqual(1, pcoTask.Elements().OfType<IReadOnlyProperty>().Count());

      var task = pcoTask.RelatedItem();
      var origTask = item.Relationships().Single().RelatedItem();
      Assert.AreNotEqual(origTask.Id(), task.Id());

      var clonedProps = new string[]
      {
        "date_due_target",
        "indent",
        "is_complete",
        "name",
        "owned_by_id",
        "parent_project",
        "state_image",
        "team_id",
      };
      CollectionAssert.AreEqual(clonedProps, task.Elements().OfType<IReadOnlyProperty>().Select(p => p.Name).OrderBy(n => n).ToArray());
      Assert.AreEqual(origTask.OwnedById().Value, task.OwnedById().Value);
    }

    [TestMethod()]
    public void CloneTest()
    {
      var itemAml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <Result>
      <Item type=""ItemType"" typeId=""450906E86E304F55A34B3C0D65C097EA"" id=""F0834BBA6FB64394B78DF5BB725532DD"">
        <created_by_id keyed_name=""_Super User"" type=""User"">
          <Item type=""User"" typeId=""45E899CD2859442982EB22BB2DF683E5"" id=""AD30A6D8D3B642F5A2AFED1A4B02BEFA"">
            <id keyed_name=""_Super User"" type=""User"">AD30A6D8D3B642F5A2AFED1A4B02BEFA</id>
            <first_name>_Super</first_name>
            <itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype>
          </Item>
        </created_by_id>
        <id keyed_name=""Report"" type=""ItemType"">F0834BBA6FB64394B78DF5BB725532DD</id>
        <label xml:lang=""en"">Report</label>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      var item = ElementFactory.Local.FromXml(itemAml).AssertItem();
      var clone = item.Clone();
      var cloneAml = clone.ToAml();
      var expected = @"<Item type=""ItemType"" typeId=""450906E86E304F55A34B3C0D65C097EA"" id=""F0834BBA6FB64394B78DF5BB725532DD""><created_by_id keyed_name=""_Super User"" type=""User""><Item type=""User"" typeId=""45E899CD2859442982EB22BB2DF683E5"" id=""AD30A6D8D3B642F5A2AFED1A4B02BEFA""><id keyed_name=""_Super User"" type=""User"">AD30A6D8D3B642F5A2AFED1A4B02BEFA</id><first_name>_Super</first_name><itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype></Item></created_by_id><id keyed_name=""Report"" type=""ItemType"">F0834BBA6FB64394B78DF5BB725532DD</id><label xml:lang=""en"">Report</label></Item>";
      Assert.AreEqual(expected, cloneAml);
    }

    [TestMethod()]
    public void CloneNullItem()
    {
      var nullItem = Item.GetNullItem<IReadOnlyItem>();
      Assert.IsFalse(nullItem.Exists);
      var clone = nullItem.Clone();
      Assert.IsFalse(clone.Exists);
      nullItem = Item.GetNullItem<Model.File>();
      Assert.IsFalse(nullItem.Exists);
      clone = nullItem.Clone();
      Assert.IsFalse(clone.Exists);
    }

    [TestMethod()]
    public void ModelTypeTest()
    {
      var itemAml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <Result>
      <Item type=""ItemType"" typeId=""450906E86E304F55A34B3C0D65C097EA"" id=""F0834BBA6FB64394B78DF5BB725532DD"">
        <created_by_id keyed_name=""_Super User"" type=""User"">
          <Item type=""User"" typeId=""45E899CD2859442982EB22BB2DF683E5"" id=""AD30A6D8D3B642F5A2AFED1A4B02BEFA"">
            <id keyed_name=""_Super User"" type=""User"">AD30A6D8D3B642F5A2AFED1A4B02BEFA</id>
            <first_name>_Super</first_name>
            <itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype>
          </Item>
        </created_by_id>
        <id keyed_name=""Report"" type=""ItemType"">F0834BBA6FB64394B78DF5BB725532DD</id>
        <label xml:lang=""en"">Report</label>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      var item = ElementFactory.Local.FromXml(itemAml, "some query", null).AssertItem();
      Assert.AreEqual("ItemType", item.GetType().Name);
      Assert.AreEqual("User", item.CreatedById().AsItem().GetType().Name);

      var itemAml2 = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <Result>
      <Item type=""ItemType"" typeId=""450906E86E304F55A34B3C0D65C097EA"" id=""F0834BBA6FB64394B78DF5BB725532DD"">
        <created_by_id keyed_name=""_Super User"" type=""User"">AD30A6D8D3B642F5A2AFED1A4B02BEFA</created_by_id>
        <id keyed_name=""Report"" type=""ItemType"">F0834BBA6FB64394B78DF5BB725532DD</id>
        <label xml:lang=""en"">Report</label>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      item = ElementFactory.Local.FromXml(itemAml2, "some query", null).AssertItem();
      Assert.AreEqual("User", item.CreatedById().AsItem().GetType().Name);
    }

    [TestMethod()]
    public void ModelItemProperties()
    {
      var itemAml2 = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <Result>
      <Item type=""ItemType"" typeId=""450906E86E304F55A34B3C0D65C097EA"" id=""F0834BBA6FB64394B78DF5BB725532DD"">
        <created_by_id keyed_name=""_Super User"" type=""User"">AD30A6D8D3B642F5A2AFED1A4B02BEFA</created_by_id>
        <modified_by_id is_null=""1"" />
        <id keyed_name=""Report"" type=""ItemType"">F0834BBA6FB64394B78DF5BB725532DD</id>
        <label xml:lang=""en"">Report</label>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      var item = ElementFactory.Local.FromXml(itemAml2, "some query", null).AssertItem();
      var user = item.CreatedById().AsModel();
      Assert.AreEqual("User", user.GetType().Name);
      user = item.ModifiedById().AsModel();
      Assert.AreEqual("User", user.GetType().Name);
      Assert.AreEqual(false, user.Exists);
      user = item.ModifiedById().AsModel().CreatedById().AsModel();
      Assert.AreEqual("User", user.GetType().Name);
      Assert.AreEqual(false, user.Exists);
    }

    [TestMethod]
    public void ModelSourceRelated()
    {
      var itemAml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='Permission' typeId='C6A89FDE1294451497801DF78341B473' id='93AB06D37FC84328A314324DC45DB574'>
        <config_id keyed_name='World Can Get' type='Permission'>93AB06D37FC84328A314324DC45DB574</config_id>
        <created_by_id keyed_name='Super User' type='User'>AD30A6D8D3B642F5A2AFED1A4B02BEFA</created_by_id>
        <created_on>2004-08-04T17:12:08</created_on>
        <current_state keyed_name='Released' type='Life Cycle State' name='Released'>C363ABDADF8D485393BB89877DBDCFD0</current_state>
        <generation>1</generation>
        <id keyed_name='World Can Get' type='Permission'>93AB06D37FC84328A314324DC45DB574</id>
        <is_current>1</is_current>
        <is_private>0</is_private>
        <is_released>1</is_released>
        <keyed_name>World Can Get</keyed_name>
        <major_rev>A</major_rev>
        <modified_on>2004-08-04T17:12:10</modified_on>
        <new_version>0</new_version>
        <not_lockable>0</not_lockable>
        <permission_id keyed_name='Permission' type='Permission'>BBBFCF1164514B7B92C8BCA944498142</permission_id>
        <state>Released</state>
        <name>World Can Get</name>
        <Relationships>
          <Item type='Access' typeId='AEFCD3D2DC1D4E3EA126D49D68041EB6' id='05D159C4EE034DD7B8C18678FD1AFB84'>
            <behavior>float</behavior>
            <can_change_access>0</can_change_access>
            <can_delete>0</can_delete>
            <can_discover>1</can_discover>
            <can_get>1</can_get>
            <can_update>0</can_update>
            <config_id keyed_name='05D159C4EE034DD7B8C18678FD1AFB84' type='Access'>05D159C4EE034DD7B8C18678FD1AFB84</config_id>
            <created_by_id keyed_name='Super User' type='User'>AD30A6D8D3B642F5A2AFED1A4B02BEFA</created_by_id>
            <created_on>2004-08-04T17:12:09</created_on>
            <generation>1</generation>
            <is_current>1</is_current>
            <is_released>0</is_released>
            <keyed_name>05D159C4EE034DD7B8C18678FD1AFB84</keyed_name>
            <major_rev>A</major_rev>
            <new_version>0</new_version>
            <not_lockable>0</not_lockable>
            <permission_id keyed_name='Access' type='Permission'>D8D71D09802C475884D1AFE156AF92F3</permission_id>
            <related_id keyed_name='World' type='Identity'>
              <Item type='Identity' typeId='E582AB17663F4EF28460015B2BE9E094' id='A73B655731924CD0B027E4F4D5FCC0A9'>
                <config_id keyed_name='World' type='Identity'>A73B655731924CD0B027E4F4D5FCC0A9</config_id>
                <created_by_id keyed_name='Super User' type='User'>AD30A6D8D3B642F5A2AFED1A4B02BEFA</created_by_id>
                <created_on>2002-04-24T09:46:11</created_on>
                <current_state keyed_name='Released' type='Life Cycle State' name='Released'>C363ABDADF8D485393BB89877DBDCFD0</current_state>
                <description>All users are automatically members of the World.</description>
                <generation>1</generation>
                <id keyed_name='World' type='Identity'>A73B655731924CD0B027E4F4D5FCC0A9</id>
                <is_alias>0</is_alias>
                <is_current>1</is_current>
                <is_released>0</is_released>
                <keyed_name>World</keyed_name>
                <major_rev>A</major_rev>
                <modified_by_id keyed_name='Super User' type='User'>AD30A6D8D3B642F5A2AFED1A4B02BEFA</modified_by_id>
                <modified_on>2003-05-31T17:45:42</modified_on>
                <new_version>0</new_version>
                <not_lockable>0</not_lockable>
                <permission_id keyed_name='9A7C03AFA5E5453AAAC94FDB9006720B' type='Permission'>9A7C03AFA5E5453AAAC94FDB9006720B</permission_id>
                <state>Released</state>
                <name>World</name>
              </Item>
            </related_id>
            <show_permissions_warning>0</show_permissions_warning>
            <sort_order>128</sort_order>
            <source_id keyed_name='World Can Get' type='Permission'>93AB06D37FC84328A314324DC45DB574</source_id>
            <id keyed_name='05D159C4EE034DD7B8C18678FD1AFB84' type='Access'>05D159C4EE034DD7B8C18678FD1AFB84</id>
          </Item>
          <Item type='Access' typeId='AEFCD3D2DC1D4E3EA126D49D68041EB6' id='3861311B4E1C4BE6877074CA6D67A583'>
            <can_change_access>0</can_change_access>
            <can_delete>1</can_delete>
            <can_discover>1</can_discover>
            <can_get>1</can_get>
            <can_update>1</can_update>
            <config_id keyed_name='3861311B4E1C4BE6877074CA6D67A583' type='Access'>3861311B4E1C4BE6877074CA6D67A583</config_id>
            <created_by_id keyed_name='Super User' type='User'>AD30A6D8D3B642F5A2AFED1A4B02BEFA</created_by_id>
            <created_on>2004-08-04T17:26:04</created_on>
            <generation>1</generation>
            <is_current>1</is_current>
            <is_released>0</is_released>
            <keyed_name>3861311B4E1C4BE6877074CA6D67A583</keyed_name>
            <new_version>0</new_version>
            <not_lockable>0</not_lockable>
            <permission_id keyed_name='Access' type='Permission'>D8D71D09802C475884D1AFE156AF92F3</permission_id>
            <related_id keyed_name='Administrators' type='Identity'>
              <Item type='Identity' typeId='E582AB17663F4EF28460015B2BE9E094' id='2618D6F5A90949BAA7E920D1B04C7EE1'>
                <config_id keyed_name='Administrators' type='Identity'>2618D6F5A90949BAA7E920D1B04C7EE1</config_id>
                <created_by_id keyed_name='Super User' type='User'>AD30A6D8D3B642F5A2AFED1A4B02BEFA</created_by_id>
                <created_on>2005-07-15T11:33:48</created_on>
                <description>Users with access to system administrative aspects</description>
                <generation>1</generation>
                <id keyed_name='Administrators' type='Identity'>2618D6F5A90949BAA7E920D1B04C7EE1</id>
                <is_alias>0</is_alias>
                <is_current>1</is_current>
                <is_released>0</is_released>
                <keyed_name>Administrators</keyed_name>
                <major_rev>A</major_rev>
                <new_version>0</new_version>
                <not_lockable>0</not_lockable>
                <permission_id keyed_name='Identity' type='Permission'>761D6A3E3CA146138B47393D29FF6824</permission_id>
                <name>Administrators</name>
              </Item>
            </related_id>
            <show_permissions_warning>0</show_permissions_warning>
            <source_id keyed_name='World Can Get' type='Permission'>93AB06D37FC84328A314324DC45DB574</source_id>
            <id keyed_name='3861311B4E1C4BE6877074CA6D67A583' type='Access'>3861311B4E1C4BE6877074CA6D67A583</id>
          </Item>
        </Relationships>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      var item = ElementFactory.Local.FromXml(itemAml, "some query", null).AssertItem();
      var access = item.Relationships().OfType<Model.Access>().First();
      Assert.AreEqual("Permission", access.SourceModel().GetType().Name);
      Assert.AreEqual("World Can Get", access.SourceModel().NameProp().Value);
      Assert.AreEqual("All users are automatically members of the World.", access.RelatedModel().Description().Value);
    }

    [TestMethod()]
    public void TestItemCreation_Constructor()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Action("get"), aml.Type("Part"),
        aml.Property("is_active_rev", "1"),
        aml.Property("item_number", aml.Attribute("condition", "like"), "905-1954-*")
      );
      Assert.AreEqual("<Item action=\"get\" type=\"Part\"><is_active_rev>1</is_active_rev><item_number condition=\"like\">905-1954-*</item_number></Item>", item.ToString());

      Assert.AreEqual("get", item.Action().Value);
      Assert.AreEqual(true, item.ServerEvents().AsBoolean(true));
      Assert.AreEqual(false, item.ServerEvents().Exists);
      Assert.AreEqual(false, item.IsCurrent().AsBoolean().HasValue);
      Assert.AreEqual("905-1954-*", item.Property("item_number").Value);
      Assert.AreEqual(true, item.Property("is_active_rev").AsBoolean().Value);


      item = aml.Item(aml.Attribute("action", "get"),
        aml.Attribute("type", "Part"),
        aml.Property("is_active_rev", "1"),
        aml.Property("item_number", aml.Attribute("condition", "like"), "905-1954-*")
      );
      Assert.AreEqual("<Item action=\"get\" type=\"Part\"><is_active_rev>1</is_active_rev><item_number condition=\"like\">905-1954-*</item_number></Item>", item.ToString());

      //item = (Item)new Item().Action("get").Type("Part")
      //  .SetProperty("is_active_rev", "1")
      //  .Add(new Property("item_number", new Attribute("condition", "like"), "905-1954-*"));
      //Assert.AreEqual("<Item action=\"get\" type=\"Part\"><is_active_rev>1</is_active_rev><item_number condition=\"like\">905-1954-*</item_number></Item>", (string)item);
    }


    [TestMethod()]
    public void TestItemCreation_Relationships()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Type("Part"), aml.Action("get"),
        aml.Property("item_number", aml.Condition(Condition.Like), "905-1954-*"),
        aml.Relationships(
          aml.Item(aml.Type("Part BOM"), aml.Action("get"))
        )
      );

      Assert.AreEqual("<Item type=\"Part\" action=\"get\"><item_number condition=\"like\">905-1954-*</item_number><Relationships><Item type=\"Part BOM\" action=\"get\" /></Relationships></Item>", item.ToString());
    }

    [TestMethod]
    public void LanguageHandling()
    {
      var aml = new ElementFactory(new ServerContext(false) { LanguageCode = "fr" });
      var item = aml.FromXml(@"<Item type='Supplier' action='get' select='name' language='en,fr'>
<thing>All</thing>
<name xml:lang='fr'>Dell France</name>
<i18n:name xml:lang='en' xmlns:i18n='http://www.aras.com/I18N'>Dell US</i18n:name>
<i18n:name xml:lang='fr' xmlns:i18n='http://www.aras.com/I18N'>Dell France</i18n:name>
<description xml:lang='en'>Computers</description>
<i18n:description xml:lang='en' xmlns:i18n='http://www.aras.com/I18N'>Computers</i18n:description>
<i18n:description xml:lang='fr' is_null='1' xmlns:i18n='http://www.aras.com/I18N' />
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
      Assert.AreEqual("<Item type=\"Supplier\" action=\"get\" select=\"name\" language=\"en,fr\"><thing>All</thing><name xml:lang=\"fr\">Dell France</name><i18n:name xml:lang=\"en\" xmlns:i18n=\"http://www.aras.com/I18N\">Dell US</i18n:name><i18n:name xml:lang=\"fr\" xmlns:i18n=\"http://www.aras.com/I18N\">Dell France</i18n:name><description xml:lang=\"en\">Computers</description><i18n:description xml:lang=\"en\" xmlns:i18n=\"http://www.aras.com/I18N\">Computers</i18n:description><i18n:description xml:lang=\"fr\" is_null=\"1\" xmlns:i18n=\"http://www.aras.com/I18N\" /><test1>1</test1><i18n:test2 xml:lang=\"fr\" xmlns:i18n=\"http://www.aras.com/I18N\">2</i18n:test2><i18n:test3 xml:lang=\"fr\" xmlns:i18n=\"http://www.aras.com/I18N\">3</i18n:test3><i18n:test3 xml:lang=\"en\" xmlns:i18n=\"http://www.aras.com/I18N\">3.en</i18n:test3></Item>",
        item.ToAml());
    }

    [TestMethod()]
    public void LanguageHandling_v2()
    {
      const string aml = @"<AML xmlns:i18n='http://www.aras.com/I18N'><Item type='Property' id='1234'>
  <label xml:lang='en'>Finished Part Diameter</label>
  <i18n:label xml:lang='de'>Fertigteildurchmesser</i18n:label>
  <i18n:label xml:lang='en'>Finished Part Diameter</i18n:label>
</Item></AML>";
      var item = ElementFactory.Local.FromXml(aml).AssertItem();
      Assert.AreEqual("Finished Part Diameter", item.Property("label").Value);
      Assert.AreEqual("Finished Part Diameter", item.Property("label", "en").Value);
      Assert.AreEqual("Fertigteildurchmesser", item.Property("label", "de").Value);
    }

    [TestMethod]
    public void VerifyItemCount()
    {
      var aml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><Result><Item type='Document' typeId='B88C14B99EF449828C5D926E39EE8B89' id='9370ECBC57DD416A9465F69F1281DB74'><classification>Miscellaneous</classification><config_id keyed_name='heart-1239269 (DOC-171531)' type='Document'>9370ECBC57DD416A9465F69F1281DB74</config_id><copyright>0</copyright><created_by_id keyed_name='Eric Domke' type='User'>2D246C5838644C1C8FD34F8D2796E327</created_by_id><created_on>2016-03-04T14:34:56</created_on><current_state keyed_name='Released' type='Life Cycle State' name='Released'>C363ABDADF8D485393BB89877DBDCFD0</current_state><file_extensions>.jpg</file_extensions><generation>1</generation><has_change_pending>0</has_change_pending><has_files>1</has_files><id keyed_name='heart-1239269 (DOC-171531)' type='Document'>9370ECBC57DD416A9465F69F1281DB74</id><is_active_rev>1</is_active_rev><is_current>1</is_current><is_released>1</is_released><is_template>0</is_template><keyed_name>heart-1239269 (DOC-171531)</keyed_name><lab_controlled_document>0</lab_controlled_document><locked_by_id keyed_name='Eric Domke' type='User'>2D246C5838644C1C8FD34F8D2796E327</locked_by_id><major_rev>001</major_rev><modified_by_id keyed_name='Eric Domke' type='User'>2D246C5838644C1C8FD34F8D2796E327</modified_by_id><modified_on>2016-03-04T14:34:59</modified_on><new_version>1</new_version><not_lockable>0</not_lockable><permission_id keyed_name='New Document' type='Permission'>F0E3A6D242FC4889A9A119EEBC8EC79E</permission_id><release_date>2016-03-04T14:34:56</release_date><spec_regulation>0</spec_regulation><state>Released</state><team_id keyed_name='Owner: Public' type='Team'>2DEF50D558B44ECD9A603759D0B2D0DF</team_id><item_number>DOC-171531</item_number><name>heart-1239269</name><itemtype>B88C14B99EF449828C5D926E39EE8B89</itemtype><viewfile keyed_name='View' type='File'>F7584539F93F4F7F83A6EBF54072E6E4</viewfile></Item></Result><Message><Item id='F7584539F93F4F7F83A6EBF54072E6E4' type='File'><filename>f7584539f93f4f7f83a6ebf54072e6e4.jpg</filename></Item><event name='ids_modified' value='9370ECBC57DD416A9465F69F1281DB74|F7584539F93F4F7F83A6EBF54072E6E4|98F667F9CAB04528843D6D20738C46E6|527C835794B842A8B16E054E35B54F61' /></Message></SOAP-ENV:Body></SOAP-ENV:Envelope>";
      var result = ElementFactory.Local.FromXml(aml);
      Assert.AreEqual(1, result.Items().Count());
    }

    [TestMethod]
    public void VerifyIdMethod()
    {
      var aml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><Result><Item><classification>Miscellaneous</classification><id>9370ECBC57DD416A9465F69F1281DB74</id><keyed_name>heart-1239269 (DOC-171531)</keyed_name></Item></Result><Message><Item id='F7584539F93F4F7F83A6EBF54072E6E4' type='File'><filename>f7584539f93f4f7f83a6ebf54072e6e4.jpg</filename></Item><event name='ids_modified' value='9370ECBC57DD416A9465F69F1281DB74|F7584539F93F4F7F83A6EBF54072E6E4|98F667F9CAB04528843D6D20738C46E6|527C835794B842A8B16E054E35B54F61' /></Message></SOAP-ENV:Body></SOAP-ENV:Envelope>";
      var result = ElementFactory.Local.FromXml(aml);
      Assert.AreEqual("9370ECBC57DD416A9465F69F1281DB74", result.AssertItem().Id());
    }

    [TestMethod]
    public void AttributeValueOnNullItem()
    {
      var aml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><Result><Item><classification>Miscellaneous</classification><id>9370ECBC57DD416A9465F69F1281DB74</id><keyed_name>heart-1239269 (DOC-171531)</keyed_name></Item></Result><Message><Item id='F7584539F93F4F7F83A6EBF54072E6E4' type='File'><filename>f7584539f93f4f7f83a6ebf54072e6e4.jpg</filename></Item><event name='ids_modified' value='9370ECBC57DD416A9465F69F1281DB74|F7584539F93F4F7F83A6EBF54072E6E4|98F667F9CAB04528843D6D20738C46E6|527C835794B842A8B16E054E35B54F61' /></Message></SOAP-ENV:Body></SOAP-ENV:Envelope>";
      var result = ElementFactory.Local.FromXml(aml);
      Assert.AreEqual(null, result.AssertItem().CreatedById().AsItem().Type().Value);
    }

    [TestMethod]
    public void ValueOfItemPropertyIsId()
    {
      var aml = @"<Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='0E086FFA6C4646F6939B74C43D094182'>
  <created_by_id keyed_name='First Last' type='User'>
    <Item type='User' typeId='45E899CD2859442982EB22BB2DF683E5' id='8227040ABF0A46A8AF06C18ABD3967B3'>
      <id keyed_name='First Last' type='User'>8227040ABF0A46A8AF06C18ABD3967B3</id>
      <first_name>First</first_name>
      <itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype>
    </Item>
  </created_by_id>
  <id keyed_name='Another Company' type='Company'>0E086FFA6C4646F6939B74C43D094182</id>
  <permission_id keyed_name='Company' type='Permission'>
    <Item type='Permission' typeId='C6A89FDE1294451497801DF78341B473' id='A8FC3EC44ED0462B9A32D4564FAC0AD8'>
      <id keyed_name='Company' type='Permission'>A8FC3EC44ED0462B9A32D4564FAC0AD8</id>
      <name>Company</name>
    </Item>
  </permission_id>
  <itemtype>3E71E373FC2940B288760C915120AABE</itemtype>
</Item>";
      var item = ElementFactory.Local.FromXml(aml).AssertItem();
      Assert.AreEqual("8227040ABF0A46A8AF06C18ABD3967B3", item.CreatedById().Value);
    }

    [TestMethod]
    public void RoundTripMessageTag()
    {
      const string withMessage = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='File' typeId='8052A558B9084D41B9F11805E464F443' id='1CD793698353444CA6DF901A732A523B'>
        <classification>/*</classification>
      </Item>
    </Result>
    <Message>
      <event name='items_with_no_access_count' value='1' />
    </Message>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      const string noMessage = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='File' typeId='8052A558B9084D41B9F11805E464F443' id='1CD793698353444CA6DF901A732A523B'>
        <classification>/*</classification>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      const string outputWithMessage = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"File\" typeId=\"8052A558B9084D41B9F11805E464F443\" id=\"1CD793698353444CA6DF901A732A523B\"><classification>/*</classification></Item></Result><Message><event name=\"items_with_no_access_count\" value=\"1\" /></Message></SOAP-ENV:Body></SOAP-ENV:Envelope>";

      var aml = ElementFactory.Local;
      var result = aml.FromXml(withMessage);
      var str = result.ToAml();
      Assert.AreEqual(outputWithMessage, str);

      result = ElementFactory.Local.FromXml(noMessage);
      str = result.ToAml();
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"File\" typeId=\"8052A558B9084D41B9F11805E464F443\" id=\"1CD793698353444CA6DF901A732A523B\"><classification>/*</classification></Item></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", str);

      result.AddMessage(aml.Element("event", aml.Attribute("name", "items_with_no_access_count"), aml.Attribute("value", 1)));
      str = result.ToAml();
      Assert.AreEqual(outputWithMessage, str);
    }

    [TestMethod]
    public void GetItemsWithNoAccessCount()
    {
      const string exceptionXml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af='http://www.aras.com/InnovatorFault'>
      <faultcode>0</faultcode>
      <faultstring>No items of type File found.</faultstring>
      <detail>
        <af:legacy_detail>No items of type File found.</af:legacy_detail>
        <af:legacy_faultstring>No items of type 'File' found</af:legacy_faultstring>
        <af:legacy_faultactor>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</af:legacy_faultactor>
        <message key='items_with_no_access_count' value='83' />
      </detail>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      const string withMessage = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='File' typeId='8052A558B9084D41B9F11805E464F443' id='1CD793698353444CA6DF901A732A523B'>
        <classification>/*</classification>
      </Item>
    </Result>
    <Message>
      <event name='items_with_no_access_count' value='83' />
    </Message>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";

      var aml = ElementFactory.Local;
      var result = aml.FromXml(exceptionXml);
      Assert.AreEqual(83, result.ItemsWithNoAccessCount());

      var noItems = (NoItemsFoundException)result.Exception;
      Assert.AreEqual(83, noItems.ItemsWithNoAccessCount());

      result = aml.FromXml(withMessage);
      Assert.AreEqual(83, result.ItemsWithNoAccessCount());
    }


    [TestMethod()]
    public void RelationshipAdd()
    {
      var expected = @"<Item type=""List"" id=""D7D72BF68937462B947DAC6BE7E28322""><Relationships><Item type=""Value"" /></Relationships></Item>";

      var aml = ElementFactory.Local;
      var input = @"<Item type=""List"" id=""D7D72BF68937462B947DAC6BE7E28322""></Item>";
      var item = aml.FromXml(input).AssertItem();
      item.Relationships().Add(aml.Item(aml.Type("Value")));
      Assert.AreEqual(expected, item.ToAml());

      item = aml.Item(aml.Type("List"), aml.Id("D7D72BF68937462B947DAC6BE7E28322"),
        aml.Relationships(
          aml.Item(aml.Type("Value"))
        )
      );
      Assert.AreEqual(expected, item.ToAml());
    }

    [TestMethod()]
    public void ItemCopyProperty()
    {
      var conn = new TestConnection();
      var user = conn.Apply(@"<Item type='User' action='get' id='8227040ABF0A46A8AF06C18ABD3967B3' />").AssertItem();
      var aml = conn.AmlContext;
      var newItem = aml.Item(user.Property("first_name"), user.Property("owned_by_id"));
      Assert.AreEqual("<Item><first_name>First</first_name><owned_by_id is_null=\"1\" /></Item>", newItem.ToAml());
    }

    [TestMethod()]
    public void ParseAttributes()
    {
      var aml = ElementFactory.Local;
      const string xml = "<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><Result><Item id='81C7B50296DA460CAB9498F6A01FB568' type='ItemType' action='add' levels='0' isTemp='1' doGetItem='0' /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>";
      var item = aml.FromXml(xml).AssertItem();
      Assert.AreEqual("81C7B50296DA460CAB9498F6A01FB568", item.Id());
    }

    [TestMethod()]
    public void CompileMethodResponse()
    {
      var aml = ElementFactory.Local;
      var xml = "<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><CompileMethodResponse><Result><status>ERROR: method id was not provided.</status></Result></CompileMethodResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>";
      var result = aml.FromXml(xml);
      Assert.AreEqual("ERROR: method id was not provided.", result.Exception.Message);

      xml = "<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'><SOAP-ENV:Body><CompileMethodResponse><Result><status>OK: 02C0EEA10C084E14AA5D104CB35D3227.</status></Result></CompileMethodResponse></SOAP-ENV:Body></SOAP-ENV:Envelope>";
      result = aml.FromXml(xml);
      Assert.AreEqual("OK: 02C0EEA10C084E14AA5D104CB35D3227.", result.Value);
    }

    [TestMethod()]
    public void BuildingResult()
    {
      var aml = ElementFactory.Local;
      var items = new List<IReadOnlyItem>()
      {
        aml.Item(aml.Type("first")), aml.Item(aml.Type("second"))
      };

      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"first\" /><Item type=\"second\" /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result(items).ToAml());
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"first\" /><Item type=\"second\" /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result(aml.Item(aml.Type("first")), aml.Item(aml.Type("second"))).ToAml());

      var itemsArr = new[]
      {
        aml.Item(aml.Type("first")), aml.Item(aml.Type("second"))
      };
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"first\" /><Item type=\"second\" /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result(itemsArr).ToAml());

      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result>A string</Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result("A string").ToAml());
    }

    [TestMethod]
    public void CreateRelatedItem()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item(aml.Property("stuff", "thing"));
      var related = item.RelatedItem();
      var creator = related.CreatedById().AsItem();
      Assert.AreEqual(false, related.Exists);
      related.KeyedName().Set("related keyed name");
      Assert.AreEqual(true, related.Exists);
      Assert.AreEqual("<Item><stuff>thing</stuff><related_id><Item><keyed_name>related keyed name</keyed_name></Item></related_id></Item>", item.ToAml());


      item = aml.Item(aml.Property("stuff", "thing"));
      related = item.RelatedItem();
      Assert.AreEqual(false, related.Exists);
      related.Add(aml.KeyedName("related keyed name"));
      Assert.AreEqual(true, related.Exists);
      Assert.AreEqual("<Item><stuff>thing</stuff><related_id><Item><keyed_name>related keyed name</keyed_name></Item></related_id></Item>", item.ToAml());

      item = aml.FromXml("<Item><stuff>thing</stuff><related_id><Item><keyed_name>related keyed name</keyed_name></Item></related_id></Item>").AssertItem();
      Assert.AreEqual("related keyed name", item.RelatedItem().KeyedName().Value);
      item.RelatedItem().Property("name").Set("NAME");
      Assert.AreEqual("<Item><stuff>thing</stuff><related_id><Item><keyed_name>related keyed name</keyed_name><name>NAME</name></Item></related_id></Item>", item.ToAml());
    }

    [TestMethod()]
    public void AddInConditionProperty()
    {
      var aml = ElementFactory.Local;
      var item = aml.Item();
      var ids = new Dictionary<string, double>()
      {
        {"8227040ABF0A46A8AF06C18ABD3967B3", 1 },
        {"81C7B50296DA460CAB9498F6A01FB568", 2 }
      };
      item.Add(aml.Property("id", aml.Condition(Condition.In), ids.Keys));
      Assert.AreEqual("<Item><id condition=\"in\">'8227040ABF0A46A8AF06C18ABD3967B3','81C7B50296DA460CAB9498F6A01FB568'</id></Item>", item.ToAml());
    }

    [TestMethod()]
    public void ZeroListResult()
    {
      var aml = ElementFactory.Local;
      var list = new List<IReadOnlyItem>();
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result /></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result(list).ToAml());
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result /></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result(list.ToArray()).ToAml());
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result /></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result(Enumerable.Empty<IReadOnlyItem>()).ToAml());
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result /></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result(Enumerable.Empty<IItem>()).ToAml());
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result /></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result(Enumerable.Empty<IItem>().ToArray()).ToAml());
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result /></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result(Enumerable.Empty<IItem>().ToList()).ToAml());
      list.Add(aml.Item(aml.Type("type")));
      list.Add(aml.Item(aml.Type("type")));
      Assert.AreEqual("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><Result><Item type=\"type\" /><Item type=\"type\" /></Result></SOAP-ENV:Body></SOAP-ENV:Envelope>", aml.Result(list).ToAml());
    }

    [TestMethod()]
    public void PropertyType()
    {
      var item = ElementFactory.Local.FromXml(@"<Result>
  <Item type='Results'>
    <q0>
      <Item>
        <Relationships>
          <Item type='Part' typeId='5' id='5'>
            <major_rev>A</major_rev>
            <id keyed_name='999-9999-000' type='Part'>5</id>
            <keyed_name>999-9999-000</keyed_name>
            <itemtype>5</itemtype>
          </Item>
          <Item type='Part' typeId='5' id='5'>
            <id keyed_name='999-9999-000' type='Part'>5</id>
            <keyed_name>999-9999-000</keyed_name>
            <major_rev>AAB</major_rev>
            <itemtype>5</itemtype>
          </Item>
        </Relationships>
      </Item>
    </q0>
  </Item>
</Result>", new Command(), new TestConnection()).AssertItem();
      var firstRel = item.Property("q0").AsItem().Relationships().First();
      var allChildren = firstRel.Elements().ToArray();
      Assert.AreEqual(4, allChildren.Length);
      Assert.IsInstanceOfType(allChildren[0], typeof(Property));
      Assert.IsInstanceOfType(firstRel.Element("major_rev"), typeof(Property));
    }

    [TestMethod]
    public void AddSelectAttribute()
    {
      var item = ElementFactory.Local.FromXml("<Item type='Part' pagesize='250' action='get'></Item>").AssertItem();
      var selectNode = new SelectNode();
      selectNode.EnsurePath("id");
      selectNode.EnsurePath("state");
      selectNode.EnsurePath("keyed_name");

      item.Select().Set(selectNode);

      Assert.AreEqual("id,state,keyed_name", item.Select().Value);
    }

    [TestMethod]
    public void AddReadOnlyItemsToResult()
    {
      var result = ElementFactory.Local.Result();
      var resultItem = ElementFactory.Local.Item();
      result.Add(resultItem);

      var readonlyResultItem = resultItem.AsResult().AssertItem();
      result.Add(readonlyResultItem);

      Assert.AreEqual(2, result.Items().Count());
    }

    [TestMethod]
    public void LegacyElementNameRetrieval()
    {
      const string input = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af='http://www.aras.com/InnovatorFault'>
      <faultcode>0</faultcode>
      <faultstring>No items of type File found.</faultstring>
      <detail>
        <af:legacy_detail>No items of type File found.</af:legacy_detail>
      </detail>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";

      var result = ElementFactory.Local.FromXml(input);
      var detailElement = result.Exception.Fault.Element("detail");
      Assert.AreEqual("No items of type File found.", detailElement.Element("af:legacy_detail").Value);
      Assert.AreEqual("No items of type File found.", detailElement.Element("legacy_detail", "af").Value);
      Assert.AreEqual(null, detailElement.Element("legacy_detail").Value);
    }
  }
}
