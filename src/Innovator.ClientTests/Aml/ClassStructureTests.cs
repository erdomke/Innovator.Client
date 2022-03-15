using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class ClassStructureTests
  {
    [TestMethod()]
    public void ParseItemTypeClassStructure()
    {
      const string aml = @"<Item type='ItemType' typeId='450906E86E304F55A34B3C0D65C097EA' id='4F1AC04A2B484F3ABA4E20DB63808A88'>
  <class_structure><![CDATA[<class id=""4F1AC04A2B484F3ABA4E20DB63808A88""><class name=""Component"" id=""0D83A764A1EA0CB66E34980C4B50F7E7"" /><class name=""Assembly"" id=""512FF7893DBCD6D1AC1F55B87A480934"" /><class name=""Material"" id=""64801C21172EEEB61692B77CB5E0ABF1""><class id=""2BDA1978BBA744E581F9B90F683EA939"" name=""Plastic"" /><class id=""2BE155A3B7C642BEABDB3980BFC8511F"" name=""Metal""><class id=""38603DB4AB534CEB9714DCB99FF14BEC"" name=""Steel"" /><class id=""79FE34C1987A49E9AA9B7022DDA99335"" name=""Aluminum"" /></class></class><class name=""Software"" id=""74DB7ADB15E0E70BD729E8837D0FF3C9"" /></class>]]></class_structure>
  <id keyed_name='Part' type='ItemType'>4F1AC04A2B484F3ABA4E20DB63808A88</id>
</Item>";
      var cStruct = new ClassStructure(aml);
      TestStructItemType(cStruct);
    }

    [TestMethod()]
    public void ParseItemTypeClassStructureOnly()
    {
      const string aml = @"<class id=""4F1AC04A2B484F3ABA4E20DB63808A88""><class name=""Component"" id=""0D83A764A1EA0CB66E34980C4B50F7E7"" /><class name=""Assembly"" id=""512FF7893DBCD6D1AC1F55B87A480934"" /><class name=""Material"" id=""64801C21172EEEB61692B77CB5E0ABF1""><class id=""2BDA1978BBA744E581F9B90F683EA939"" name=""Plastic"" /><class id=""2BE155A3B7C642BEABDB3980BFC8511F"" name=""Metal""><class id=""38603DB4AB534CEB9714DCB99FF14BEC"" name=""Steel"" /><class id=""79FE34C1987A49E9AA9B7022DDA99335"" name=""Aluminum"" /></class></class><class name=""Software"" id=""74DB7ADB15E0E70BD729E8837D0FF3C9"" /></class>";
      var cStruct = new ClassStructure(aml);
      TestStructItemType(cStruct);
    }

    [TestMethod()]
    public void ParseException()
    {
      const string aml = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
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
      var ex = Assert.ThrowsException<NoItemsFoundException>(() => new ClassStructure(aml));
      Assert.AreEqual("No items of type File found.", ex.Message);
    }

    private void TestStructItemType(ClassStructure cStruct)
    {
      var expectedPaths = new string[]
      {
        "Assembly",
        "Component",
        "Material",
        "Material/Metal",
        "Material/Metal/Aluminum",
        "Material/Metal/Steel",
        "Material/Plastic",
        "Software",
      };
      var actualPaths = cStruct.Descendants()
        .Select(n => n.Path)
        .OrderBy(p => p)
        .ToArray();
      CollectionAssert.AreEqual(expectedPaths, actualPaths);

      expectedPaths = new string[]
      {
        "Material/Metal",
        "Material/Metal/Aluminum",
        "Material/Metal/Steel",
      };
      actualPaths = cStruct["Material/Metal"]
        .DescendantsAndSelf()
        .Select(n => n.Path)
        .OrderBy(p => p)
        .ToArray();
      CollectionAssert.AreEqual(expectedPaths, actualPaths);

      var node = cStruct["38603DB4AB534CEB9714DCB99FF14BEC"];
      Assert.AreEqual("Steel", node.Name);
      Assert.AreEqual("Steel", node.Label);
      Assert.AreEqual("Material/Metal/Steel", node.Path);
      Assert.AreEqual(true, node.IsLeaf);

      node = cStruct["Material/Metal"];
      Assert.AreEqual("Metal", node.Name);
      Assert.AreEqual("Metal", node.Label);
      Assert.AreEqual("Material/Metal", node.Path);
      Assert.AreEqual(false, node.IsLeaf);

      node.Attributes["description"] = "Some help text";
      Assert.AreEqual(@"<class id=""4F1AC04A2B484F3ABA4E20DB63808A88""><class name=""Component"" id=""0D83A764A1EA0CB66E34980C4B50F7E7"" /><class name=""Assembly"" id=""512FF7893DBCD6D1AC1F55B87A480934"" /><class name=""Material"" id=""64801C21172EEEB61692B77CB5E0ABF1""><class id=""2BDA1978BBA744E581F9B90F683EA939"" name=""Plastic"" /><class id=""2BE155A3B7C642BEABDB3980BFC8511F"" name=""Metal"" description=""Some help text""><class id=""38603DB4AB534CEB9714DCB99FF14BEC"" name=""Steel"" /><class id=""79FE34C1987A49E9AA9B7022DDA99335"" name=""Aluminum"" /></class></class><class name=""Software"" id=""74DB7ADB15E0E70BD729E8837D0FF3C9"" /></class>", cStruct.ToItemTypeStructure());
    }

    [TestMethod()]
    public void ParseXClassTree()
    {
      const string aml = @"<Item type='xClassificationTree' typeId='BE722B4824314594AA4AFC3CC9A18CCA' id='95244ACFEBDB4E02BCD74D65EB9A5256'>
  <classification_hierarchy>[{""toRefId"":""95244ACFEBDB4E02BCD74D65EB9A5256""},{""fromRefId"":""95244ACFEBDB4E02BCD74D65EB9A5256"",""toRefId"":""60D882BF2BFF4177A984DEFEB2FF7665""},{""fromRefId"":""60D882BF2BFF4177A984DEFEB2FF7665"",""toRefId"":""E0977B54222D40A2B130B24599F39B02""},{""fromRefId"":""60D882BF2BFF4177A984DEFEB2FF7665"",""toRefId"":""983A6895474C4D9589D40A0A367B6626""},{""fromRefId"":""60D882BF2BFF4177A984DEFEB2FF7665"",""toRefId"":""7AFF0BD5DF604B0B894EA1507D01421B""},{""fromRefId"":""95244ACFEBDB4E02BCD74D65EB9A5256"",""toRefId"":""1AF5A30156BE4425A9BAFBD5C08D3CB4""},{""fromRefId"":""1AF5A30156BE4425A9BAFBD5C08D3CB4"",""toRefId"":""981F967D1BD44628B3A26F55746F89BF""},{""fromRefId"":""1AF5A30156BE4425A9BAFBD5C08D3CB4"",""toRefId"":""4EF7A860562B4045B1E842AF7D64E6B4""},{""fromRefId"":""95244ACFEBDB4E02BCD74D65EB9A5256"",""toRefId"":""D7F6F4839EF74DE09F918E35C333F32C""}]</classification_hierarchy>
  <id keyed_name='Part Types' type='xClassificationTree'>95244ACFEBDB4E02BCD74D65EB9A5256</id>
  <name>Part Types</name>
  <select_only_leaf_class>1</select_only_leaf_class>
  <select_only_single_class>1</select_only_single_class>
  <Relationships>
    <Item type='xClass' typeId='903F54E4E13D41528A561CD01BAF31F6' id='923DD8787F72467988194F5A4693D46B'>
      <id keyed_name='/Part Types' type='xClass'>923DD8787F72467988194F5A4693D46B</id>
      <label xml:lang='en'>Part Types</label>
      <name>Part Types</name>
      <ref_id>95244ACFEBDB4E02BCD74D65EB9A5256</ref_id>
      <source_id keyed_name='Part Types' type='xClassificationTree'>95244ACFEBDB4E02BCD74D65EB9A5256</source_id>
      <xproperties_sort_order>
      </xproperties_sort_order>
    </Item>
    <Item type='xClass' typeId='903F54E4E13D41528A561CD01BAF31F6' id='60D882BF2BFF4177A984DEFEB2FF7665'>
      <id keyed_name='/Part Types/*/Electronic' type='xClass'>60D882BF2BFF4177A984DEFEB2FF7665</id>
      <label xml:lang='en'>Electronic</label>
      <name>Electronic</name>
      <ref_id>60D882BF2BFF4177A984DEFEB2FF7665</ref_id>
      <source_id keyed_name='Part Types' type='xClassificationTree'>95244ACFEBDB4E02BCD74D65EB9A5256</source_id>
      <xproperties_sort_order>
      </xproperties_sort_order>
    </Item>
    <Item type='xClass' typeId='903F54E4E13D41528A561CD01BAF31F6' id='E0977B54222D40A2B130B24599F39B02'>
      <id keyed_name='/Part Types/*/Resistor' type='xClass'>E0977B54222D40A2B130B24599F39B02</id>
      <label xml:lang='en'>Resistor</label>
      <name>Resistor</name>
      <ref_id>E0977B54222D40A2B130B24599F39B02</ref_id>
      <source_id keyed_name='Part Types' type='xClassificationTree'>95244ACFEBDB4E02BCD74D65EB9A5256</source_id>
      <xproperties_sort_order>
      </xproperties_sort_order>
    </Item>
    <Item type='xClass' typeId='903F54E4E13D41528A561CD01BAF31F6' id='983A6895474C4D9589D40A0A367B6626'>
      <id keyed_name='/Part Types/*/Transistor' type='xClass'>983A6895474C4D9589D40A0A367B6626</id>
      <label xml:lang='en'>Transistor</label>
      <name>Transistor</name>
      <ref_id>983A6895474C4D9589D40A0A367B6626</ref_id>
      <source_id keyed_name='Part Types' type='xClassificationTree'>95244ACFEBDB4E02BCD74D65EB9A5256</source_id>
      <xproperties_sort_order>
      </xproperties_sort_order>
    </Item>
    <Item type='xClass' typeId='903F54E4E13D41528A561CD01BAF31F6' id='7AFF0BD5DF604B0B894EA1507D01421B'>
      <id keyed_name='/Part Types/*/Capacitor' type='xClass'>7AFF0BD5DF604B0B894EA1507D01421B</id>
      <label xml:lang='en'>Capacitor</label>
      <name>Capacitor</name>
      <ref_id>7AFF0BD5DF604B0B894EA1507D01421B</ref_id>
      <source_id keyed_name='Part Types' type='xClassificationTree'>95244ACFEBDB4E02BCD74D65EB9A5256</source_id>
      <xproperties_sort_order>
      </xproperties_sort_order>
    </Item>
    <Item type='xClass' typeId='903F54E4E13D41528A561CD01BAF31F6' id='1AF5A30156BE4425A9BAFBD5C08D3CB4'>
      <id keyed_name='/Part Types/*/Optical' type='xClass'>1AF5A30156BE4425A9BAFBD5C08D3CB4</id>
      <label is_null='1' xml:lang='en' />
      <name>Optical</name>
      <ref_id>1AF5A30156BE4425A9BAFBD5C08D3CB4</ref_id>
      <source_id keyed_name='Part Types' type='xClassificationTree'>95244ACFEBDB4E02BCD74D65EB9A5256</source_id>
      <xproperties_sort_order>
      </xproperties_sort_order>
    </Item>
    <Item type='xClass' typeId='903F54E4E13D41528A561CD01BAF31F6' id='981F967D1BD44628B3A26F55746F89BF'>
      <id keyed_name='/Part Types/*/Glass' type='xClass'>981F967D1BD44628B3A26F55746F89BF</id>
      <label xml:lang='en'>Glass</label>
      <name>Glass</name>
      <ref_id>981F967D1BD44628B3A26F55746F89BF</ref_id>
      <source_id keyed_name='Part Types' type='xClassificationTree'>95244ACFEBDB4E02BCD74D65EB9A5256</source_id>
      <xproperties_sort_order>
      </xproperties_sort_order>
    </Item>
    <Item type='xClass' typeId='903F54E4E13D41528A561CD01BAF31F6' id='4EF7A860562B4045B1E842AF7D64E6B4'>
      <id keyed_name='/Part Types/*/Lens' type='xClass'>4EF7A860562B4045B1E842AF7D64E6B4</id>
      <label xml:lang='en'>Lens</label>
      <name>Lens</name>
      <ref_id>4EF7A860562B4045B1E842AF7D64E6B4</ref_id>
      <source_id keyed_name='Part Types' type='xClassificationTree'>95244ACFEBDB4E02BCD74D65EB9A5256</source_id>
      <xproperties_sort_order>
      </xproperties_sort_order>
    </Item>
    <Item type='xClass' typeId='903F54E4E13D41528A561CD01BAF31F6' id='D7F6F4839EF74DE09F918E35C333F32C'>
      <id keyed_name='/Part Types/*/Component' type='xClass'>D7F6F4839EF74DE09F918E35C333F32C</id>
      <label xml:lang='en'>Component</label>
      <name>Component</name>
      <ref_id>D7F6F4839EF74DE09F918E35C333F32C</ref_id>
      <source_id keyed_name='Part Types' type='xClassificationTree'>95244ACFEBDB4E02BCD74D65EB9A5256</source_id>
      <xproperties_sort_order>
      </xproperties_sort_order>
    </Item>
  </Relationships>
</Item>";
      var cStruct = new ClassStructure(aml);

      var expectedPaths = new string[]
      {
        "Component",
        "Electronic",
        "Electronic/Capacitor",
        "Electronic/Resistor",
        "Electronic/Transistor",
        "Optical",
        "Optical/Glass",
        "Optical/Lens",
      };
      var actualPaths = cStruct.Descendants()
        .Select(n => n.Path)
        .OrderBy(p => p)
        .ToArray();
      CollectionAssert.AreEqual(expectedPaths, actualPaths);

      expectedPaths = new string[]
      {
        "Electronic",
        "Electronic/Capacitor",
        "Electronic/Resistor",
        "Electronic/Transistor",
      };
      actualPaths = cStruct["Electronic"]
        .DescendantsAndSelf()
        .Select(n => n.Path)
        .OrderBy(p => p)
        .ToArray();
      CollectionAssert.AreEqual(expectedPaths, actualPaths);

      var node = cStruct["4EF7A860562B4045B1E842AF7D64E6B4"];
      Assert.AreEqual("Lens", node.Name);
      Assert.AreEqual("Lens", node.Label);
      Assert.AreEqual("Optical/Lens", node.Path);
      Assert.AreEqual(true, node.IsLeaf);

      node = cStruct["Electronic"];
      Assert.AreEqual("Electronic", node.Name);
      Assert.AreEqual("Electronic", node.Label);
      Assert.AreEqual("Electronic", node.Path);
      Assert.AreEqual(false, node.IsLeaf);
    }
  }
}
