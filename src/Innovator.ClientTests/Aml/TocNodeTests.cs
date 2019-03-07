using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Tests
{
  [TestClass]
  public class TocNodeTests
  {
    [TestMethod]
    public void TocMainTreeItems()
    {
      var node = TocNode.FromXml(_mainTreeToc);
      Assert.AreEqual(8, node.Children.Count());

      var firstChild = node.Children.First();
      Assert.AreEqual("Administration", firstChild.Label);

      var itemTypeChild = firstChild.Children.FirstOrDefault(n => n.Label == "ItemTypes");
      Assert.IsNotNull(itemTypeChild);
      Assert.AreEqual("450906E86E304F55A34B3C0D65C097EA", itemTypeChild.References.First().Id());

      var savedSearch = itemTypeChild.Children.Single();
      var savedSearchRef = savedSearch.References.FirstOrDefault(r => r.TypeName() == "SavedSearch");
      Assert.IsNotNull(savedSearchRef);
      Assert.AreEqual("89D9ECA77A0642F6B7B5D830AB455452", savedSearchRef.Id());
    }

    [TestMethod]
    public void TocFromCui()
    {
      var node = TocNode.FromXml(_cuiToc);
      Assert.AreEqual(8, node.Children.Count());
      var discussionNode = node.Children.Single(n => n.Label == "My Innovator").Children.Single(n => n.Label == "My Discussions");
      var viewNode = discussionNode.References.Single(r => r.TypeName() == "TOC View");
      Assert.AreEqual("03399AF3DF31469CBAB324F55C5A2139", viewNode.Id());
      Assert.AreEqual("../Modules/aras.innovator.SSVC/Views/MyDiscussions.html", discussionNode.AdditionalData["startPage"]);

      var inbasketNode = node.Children.Single(n => n.Label == "My Innovator").Children.Single(n => n.Label == "My InBasket");
      Assert.AreEqual("BC7977377FFF40D59FF14205914E9C71", inbasketNode.References.Single(r => r.TypeName() == "ItemType").Id());

      var formNode = node.Children.Single(n => n.Label == "Administration").Children.Single(n => n.Label == "Enterprise Search").Children.Single(n => n.Label == "Enterprise Search");
      Assert.AreEqual("901B4A7BB60540D4B914A9B5CB66713D", formNode.References.Single(r => r.TypeName() == "Form").Id());
    }

    const string _mainTreeToc = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='Tree'>
        <root>
          <Item type='Tree Node'>
            <itemtype_id></itemtype_id>
            <name>Administration</name>
            <label>Administration</label>
            <classification>Tree Node/TocCategory</classification>
            <Relationships>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Access Control</name>
                    <label>Access Control</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>4DDF9A4566C24F8E9E22632FD7F08A75</itemtype_id>
                            <open_icon>../Images/EnvironmentAttribute.svg</open_icon>
                            <close_icon>../Images/EnvironmentAttribute.svg</close_icon>
                            <name>Administration/Access Control/mp_PolicyAccessEnvAttribute</name>
                            <label>Environment Attributes</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>92C2C067865545BEA572C62FA896AD3D</itemtype_id>
                            <open_icon>../images/MACPolicy.svg</open_icon>
                            <close_icon>../images/MACPolicy.svg</close_icon>
                            <name>Administration/Access Control/mp_MacPolicy</name>
                            <label>MAC Policies</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Configuration</name>
                    <label>Configuration</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>FD63F33DC00E45A7913F585D0FFEC202</itemtype_id>
                            <open_icon>../Images/dashboard-line.svg</open_icon>
                            <close_icon>../Images/dashboard-line.svg</close_icon>
                            <name>Administration/Configuration/Dashboard</name>
                            <label>Ansys Dashboard</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>CC16953D192F4FF6AEF34B425E1A8D27</itemtype_id>
                            <open_icon>../Images/dashboard-line.svg</open_icon>
                            <close_icon>../Images/dashboard-line.svg</close_icon>
                            <name>Administration/Configuration/Dashboard_Widget</name>
                            <label>Ansys Dashboard Widget</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>CBDBEDBB001F439FBEF2DBC460C77799</itemtype_id>
                            <open_icon>../Images/dashboard-line.svg</open_icon>
                            <close_icon>../Images/dashboard-line.svg</close_icon>
                            <name>Administration/Configuration/Dashboard_Widget_Type</name>
                            <label>Ansys Dashboard Widget Type</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>7641AC8B5D2044F3B8C9A0885A3FA1D1</itemtype_id>
                            <open_icon>../images/BusinessCalendarYear.svg</open_icon>
                            <close_icon>../images/BusinessCalendarYear.svg</close_icon>
                            <name>Administration/Configuration/Business Calendar Year</name>
                            <label>Calendars</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>8AB5CEB9824B4CF7920AFED29F662C66</itemtype_id>
                            <open_icon>../Images/ClientPresentation.svg</open_icon>
                            <close_icon>../Images/ClientPresentation.svg</close_icon>
                            <name>Administration/Configuration/GlobalPresentationConfig</name>
                            <label>Client Presentation</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>645774D6072F41FD8F998C861E211741</itemtype_id>
                            <open_icon>../images/Dashboard.svg</open_icon>
                            <close_icon>../images/Dashboard.svg</close_icon>
                            <name>Administration/Configuration/Dashboard</name>
                            <label>Dashboards</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>4DAD707F62B54823AE2E4730BB00C649</itemtype_id>
                            <open_icon>../images/DatabaseUpgrade.svg</open_icon>
                            <close_icon>../images/DatabaseUpgrade.svg</close_icon>
                            <name>Administration/Configuration/DatabaseUpgrade</name>
                            <label>Database Upgrades</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>F312562D6AD948DCBCCCCF6A615EE0EA</itemtype_id>
                            <open_icon>../images/PackageDefinition.svg</open_icon>
                            <close_icon>../images/PackageDefinition.svg</close_icon>
                            <name>Administration/Configuration/PackageDefinition</name>
                            <label>PackageDefinitions</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>0421B558114B4B22886C0F4206C923EE</itemtype_id>
                            <open_icon>../Images/QueryDefinition.svg</open_icon>
                            <close_icon>../Images/QueryDefinition.svg</close_icon>
                            <name>Administration/Configuration/qry_QueryDefinition</name>
                            <label>Query Definitions</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>6AFE8A9127ED48FFB2F9183B9922981B</itemtype_id>
                            <open_icon>../Images/TreeGridView.svg</open_icon>
                            <close_icon>../Images/TreeGridView.svg</close_icon>
                            <name>Administration/Configuration/rb_TreeGridViewDefinition</name>
                            <label>Tree Grid Views</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Content Modeling</name>
                    <label>Content Modeling</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>94E345F15EB94D86ADE8FF1B6AE2B439</itemtype_id>
                            <open_icon>../Images/ContentStyle.svg</open_icon>
                            <close_icon>../Images/ContentStyle.svg</close_icon>
                            <name>Administration/Content Modeling/cmf_Style</name>
                            <label>Content Styles</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>3538F62649F3477EA1F8990EB20F88B9</itemtype_id>
                            <open_icon>../Images/ContentType.svg</open_icon>
                            <close_icon>../Images/ContentType.svg</close_icon>
                            <name>Administration/Content Modeling/cmf_ContentType</name>
                            <label>Content Types</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Enterprise Search</name>
                    <label>Enterprise Search</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>FAFACB0BC40E4C3DBFC282F058505C7F</itemtype_id>
                            <open_icon>../images/SearchAgent.svg</open_icon>
                            <close_icon>../images/SearchAgent.svg</close_icon>
                            <name>Administration/Enterprise Search/ES_Agent</name>
                            <label>Agents</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>9D136E339A6743E28C0974FA5D61549B</itemtype_id>
                            <open_icon>../images/SearchCrawler.svg</open_icon>
                            <close_icon>../images/SearchCrawler.svg</close_icon>
                            <name>Administration/Enterprise Search/ES_Crawler</name>
                            <label>Crawlers</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>5591005E6A904383A073FF3B209FC3BB</itemtype_id>
                            <open_icon>../images/SearchIndexedConfiguration.svg</open_icon>
                            <close_icon>../images/SearchIndexedConfiguration.svg</close_icon>
                            <name>Administration/Enterprise Search/ES_IndexedConfiguration</name>
                            <label>Indexed Configuration</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Extended Classification</name>
                    <label>Extended Classification</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>03435AC2C3D844FC87DA9CAF1899C753</itemtype_id>
                            <open_icon>../Images/ExplicitPermission.svg</open_icon>
                            <close_icon>../Images/ExplicitPermission.svg</close_icon>
                            <name>Administration/Extended Classification/Permission_ExplicitDefine</name>
                            <label>Explicit Permissions</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>599A06DD161348CD84734DBF5829574C</itemtype_id>
                            <open_icon>../Images/ItemClassPermission.svg</open_icon>
                            <close_icon>../Images/ItemClassPermission.svg</close_icon>
                            <name>Administration/Extended Classification/Permission_ItemClassification</name>
                            <label>Item Classification Permissions</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>BE722B4824314594AA4AFC3CC9A18CCA</itemtype_id>
                            <open_icon>../Images/xClassification.svg</open_icon>
                            <close_icon>../Images/xClassification.svg</close_icon>
                            <name>Administration/Extended Classification/xClassificationTree</name>
                            <label>xClassification Trees</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>C81D09F315EF4099B4A2F597C37EEEC0</itemtype_id>
                            <open_icon>../Images/xProperty.svg</open_icon>
                            <close_icon>../Images/xProperty.svg</close_icon>
                            <name>Administration/Extended Classification/xPropertyDefinition</name>
                            <label>xProperties</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>749B727E891F4005A82F84ACD6A803AB</itemtype_id>
                            <open_icon>../Images/xPropertyValuePermission.svg</open_icon>
                            <close_icon>../Images/xPropertyValuePermission.svg</close_icon>
                            <name>Administration/Extended Classification/Permission_PropertyValue</name>
                            <label>xProperty Value Permissions</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/File Handling</name>
                    <label>File Handling</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>05DF56FF833542F98251528F3FFE2FA0</itemtype_id>
                            <open_icon>../images/ConversionRule.svg</open_icon>
                            <close_icon>../images/ConversionRule.svg</close_icon>
                            <name>Administration/File Handling/ConversionRule</name>
                            <label>Conversion Rules</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>02FA838247DF47C2BB85AAB299E646B2</itemtype_id>
                            <open_icon>../images/ConversionServer.svg</open_icon>
                            <close_icon>../images/ConversionServer.svg</close_icon>
                            <name>Administration/File Handling/ConversionServer</name>
                            <label>Conversion Servers</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>0300B828CBEE4610B77C41377209C900</itemtype_id>
                            <open_icon>../images/ConversionTask.svg</open_icon>
                            <close_icon>../images/ConversionTask.svg</close_icon>
                            <name>Administration/File Handling/ConversionTask</name>
                            <label>Conversion Tasks</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>0DE14E76AA794A039DA8D2CDC34E6B1D</itemtype_id>
                            <open_icon>../images/ConverterType.svg</open_icon>
                            <close_icon>../images/ConverterType.svg</close_icon>
                            <name>Administration/File Handling/ConverterType</name>
                            <label>Conversion Types</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>D441339CBF2644D9A231FE952C627322</itemtype_id>
                            <open_icon>../Images/storage-line.svg</open_icon>
                            <close_icon>../Images/storage-line.svg</close_icon>
                            <name>Administration/File Handling/Data</name>
                            <label>Data</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>784E4A704B5A4E5E840DB3A11B8B9F50</itemtype_id>
                            <open_icon>../Images/file-group-line.svg</open_icon>
                            <close_icon>../Images/file-group-line.svg</close_icon>
                            <name>Administration/File Handling/FileTransferDefinition</name>
                            <label>File Transfer Definitions</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>8052A558B9084D41B9F11805E464F443</itemtype_id>
                            <open_icon>../images/File.svg</open_icon>
                            <close_icon>../images/File.svg</close_icon>
                            <name>Administration/File Handling/File</name>
                            <label>Files</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>13EC84A626F1457BB5F60A13DA03580B</itemtype_id>
                            <open_icon>../images/FileType.svg</open_icon>
                            <close_icon>../images/FileType.svg</close_icon>
                            <name>Administration/File Handling/FileType</name>
                            <label>FileTypes</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>2DAC2B407B0043A692905CF6A94296A8</itemtype_id>
                            <open_icon>../images/ReplicationTxn.svg</open_icon>
                            <close_icon>../images/ReplicationTxn.svg</close_icon>
                            <name>Administration/File Handling/ReplicationTxn</name>
                            <label>Replication Transactions</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>98FF7C1BFDFA43448B1EC5A95EA13AEA</itemtype_id>
                            <open_icon>../images/ReplicationTxnLog.svg</open_icon>
                            <close_icon>../images/ReplicationTxnLog.svg</close_icon>
                            <name>Administration/File Handling/ReplicationTxnLog</name>
                            <label>Replication Transactions Log</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>8FC29FEF933641A09CEE13A604A9DC74</itemtype_id>
                            <open_icon>../images/Vault.svg</open_icon>
                            <close_icon>../images/Vault.svg</close_icon>
                            <name>Administration/File Handling/Vault</name>
                            <label>Vaults</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>602D9828174C48EBA648B1D261C54E43</itemtype_id>
                            <open_icon>../images/Viewer.svg</open_icon>
                            <close_icon>../images/Viewer.svg</close_icon>
                            <name>Administration/File Handling/Viewer</name>
                            <label>Viewers</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Job Management</name>
                    <label>Job Management</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>523D3C18B6F948BB8809A434610FE751</itemtype_id>
                            <open_icon>../Images/terminal-line.svg</open_icon>
                            <close_icon>../Images/terminal-line.svg</close_icon>
                            <name>Administration/Job Management/Application</name>
                            <label>Applications</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>5A51450C88904CF18CC70FA0E4B9F769</itemtype_id>
                            <open_icon>../Images/ConversionRule.svg</open_icon>
                            <close_icon>../Images/ConversionRule.svg</close_icon>
                            <name>Administration/Job Management/SimApp</name>
                            <label>Job Definitions</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>29DA909DF00D4C9CAF1B7E564E010924</itemtype_id>
                            <open_icon>../Images/server-line.svg</open_icon>
                            <close_icon>../Images/server-line.svg</close_icon>
                            <name>Administration/Job Management/JobServer</name>
                            <label>Job Servers</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>93A9ABD1A513450296C98073F11BA994</itemtype_id>
                            <open_icon>../Images/variable-line.svg</open_icon>
                            <close_icon>../Images/variable-line.svg</close_icon>
                            <name>Administration/Job Management/JobVariable</name>
                            <label>Job Variables</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Localization</name>
                    <label>Localization</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>030019FA30FE40FEB5E32AD2FC9B1F20</itemtype_id>
                            <open_icon>../images/Language.svg</open_icon>
                            <close_icon>../images/Language.svg</close_icon>
                            <name>Administration/Localization/Language</name>
                            <label>Languages</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>137D24DFD9AC4D0CA2ABF8D90346AABB</itemtype_id>
                            <open_icon>../images/Locale.svg</open_icon>
                            <close_icon>../images/Locale.svg</close_icon>
                            <name>Administration/Localization/Locale</name>
                            <label>Locales</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Mass Operations</name>
                    <label>Mass Operations</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>5A549B2EDD3C4CBB9F8797902EE2EBE2</itemtype_id>
                            <open_icon>../Images/MassPromote.svg</open_icon>
                            <close_icon>../Images/MassPromote.svg</close_icon>
                            <name>Administration/Mass Operations/mpo_MassPromotion</name>
                            <label>Mass Promotions</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Notification</name>
                    <label>Notification</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>F91DE6AE038A4CC8B53D47D5A7FA49FC</itemtype_id>
                            <open_icon>../images/EMailMessage.svg</open_icon>
                            <close_icon>../images/EMailMessage.svg</close_icon>
                            <name>Administration/Notification/EMail Message</name>
                            <label>E-Mail Message</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>F8F19489113C487B860733E7F7D5B12D</itemtype_id>
                            <open_icon>../images/Message.svg</open_icon>
                            <close_icon>../images/Message.svg</close_icon>
                            <name>Administration/Notification/Message</name>
                            <label>Notification Messages</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Secure Social</name>
                    <label>Secure Social</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>2E25B49E218A45D28D0C7D3C0633710C</itemtype_id>
                            <open_icon>../images/Forum.svg</open_icon>
                            <close_icon>../images/Forum.svg</close_icon>
                            <name>Administration/Secure Social/Forum</name>
                            <label>Forums</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>6AC851D83E4A465CAB4E06E164B905D8</itemtype_id>
                            <open_icon>../Images/MeasureDistanceBetweenFaces.svg</open_icon>
                            <close_icon>../Images/MeasureDistanceBetweenFaces.svg</close_icon>
                            <name>Administration/Secure Social/Measurement Unit</name>
                            <label>Measurement Units</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/System Events</name>
                    <label>System Events</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>B30032741C894BB086148DDB551D3BEE</itemtype_id>
                            <open_icon>../images/SystemEvent.svg</open_icon>
                            <close_icon>../images/SystemEvent.svg</close_icon>
                            <name>Administration/System Events/SystemEvent</name>
                            <label>System Events</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>2F4E2B53BFBA4351BFFCEE0E438ECF97</itemtype_id>
                            <open_icon>../images/SystemEventLog.svg</open_icon>
                            <close_icon>../images/SystemEventLog.svg</close_icon>
                            <name>Administration/System Events/SystemEventLog</name>
                            <label>System Events Log</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>FF53A19A424D4B2F80938A5A5C1A29EA</itemtype_id>
                            <open_icon>../images/SystemEventLogDescriptor.svg</open_icon>
                            <close_icon>../images/SystemEventLogDescriptor.svg</close_icon>
                            <name>Administration/System Events/SystemEventLogDescriptor</name>
                            <label>System Events Log Descriptor</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Templates</name>
                    <label>Templates</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>B47D2E8E30484C66A4E9449E394F9456</itemtype_id>
                            <open_icon>../images/ProjectTemplate.svg</open_icon>
                            <close_icon>../images/ProjectTemplate.svg</close_icon>
                            <name>Administration/Templates/Project Template</name>
                            <label>Project Templates</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id></itemtype_id>
                    <name>Administration/Watermarking</name>
                    <label>Watermarking</label>
                    <classification>Tree Node/TocCategory</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>327998A4BAFF4CE4B29AD5AC4D37C0C1</itemtype_id>
                            <open_icon>../Images/PDF.svg</open_icon>
                            <close_icon>../Images/PDF.svg</close_icon>
                            <name>Administration/Watermarking/PDFWMConfiguration</name>
                            <label>PDF Collections</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>14479E1ADA444096B3116018A0012EB3</itemtype_id>
                            <open_icon>../Images/WatermarkRule.svg</open_icon>
                            <close_icon>../Images/WatermarkRule.svg</close_icon>
                            <name>Administration/Watermarking/WMSettings</name>
                            <label>Watermark Rules</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>08CC14292189425B9FBC68937CC5BD33</itemtype_id>
                            <open_icon>../Images/WatermarkSet.svg</open_icon>
                            <close_icon>../Images/WatermarkSet.svg</close_icon>
                            <name>Administration/Watermarking/WMSet</name>
                            <label>Watermark Sets</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>C8BC1FE465374CE290C61FCFEA67F4DB</itemtype_id>
                            <open_icon>../Images/WatermarkType.svg</open_icon>
                            <close_icon>../Images/WatermarkType.svg</close_icon>
                            <name>Administration/Watermarking/WMType</name>
                            <label>Watermark Types</label>
                            <classification>Tree Node/ItemTypeInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>483228BE6B9A4C0E99ACD55FDF328DEC</itemtype_id>
                    <open_icon>../images/Action.svg</open_icon>
                    <close_icon>../images/Action.svg</close_icon>
                    <name>Administration/Action</name>
                    <label>Actions</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>CE0C4143D35E46CDA3874C4339F159BE</itemtype_id>
                    <open_icon>../images/FeatureLicense.svg</open_icon>
                    <close_icon>../images/FeatureLicense.svg</close_icon>
                    <name>Administration/Feature License</name>
                    <label>Feature Licenses</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>47573682FB7549F59ADECD4BFE04F1DE</itemtype_id>
                    <open_icon>../images/Form.svg</open_icon>
                    <close_icon>../images/Form.svg</close_icon>
                    <name>Administration/Form</name>
                    <label>Forms</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>8EABD91B465443F0A4995418F483DC51</itemtype_id>
                    <open_icon>../images/Grid.svg</open_icon>
                    <close_icon>../images/Grid.svg</close_icon>
                    <name>Administration/Grid</name>
                    <label>Grids</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>E582AB17663F4EF28460015B2BE9E094</itemtype_id>
                    <open_icon>../images/Identity.svg</open_icon>
                    <close_icon>../images/Identity.svg</close_icon>
                    <name>Administration/Identity</name>
                    <label>Identities</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>450906E86E304F55A34B3C0D65C097EA</itemtype_id>
                    <open_icon>../images/ItemType.svg</open_icon>
                    <close_icon>../images/ItemType.svg</close_icon>
                    <name>Administration/ItemType</name>
                    <label>ItemTypes</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                    <Relationships>
                      <Item type='Tree Node Child'>
                        <related_id>
                          <Item type='Tree Node'>
                            <itemtype_id>450906E86E304F55A34B3C0D65C097EA</itemtype_id>
                            <open_icon>../images/ItemType.svg</open_icon>
                            <close_icon>../images/ItemType.svg</close_icon>
                            <name>Administration/ItemType</name>
                            <saved_search_id>89D9ECA77A0642F6B7B5D830AB455452</saved_search_id>
                            <label>Test saved search</label>
                            <classification>Tree Node/SavedSearchInToc</classification>
                          </Item>
                        </related_id>
                      </Item>
                    </Relationships>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>AC32527D85604A4D9FC9107C516AEF47</itemtype_id>
                    <open_icon>../images/LifeCycleMap.svg</open_icon>
                    <close_icon>../images/LifeCycleMap.svg</close_icon>
                    <name>Administration/Life Cycle Map</name>
                    <label>Life Cycle Maps</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>5736C479A8CB49BCA20138514C637266</itemtype_id>
                    <open_icon>../images/List.svg</open_icon>
                    <close_icon>../images/List.svg</close_icon>
                    <name>Administration/List</name>
                    <label>Lists</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>87879A09B8044DE380D59DF22DE1867F</itemtype_id>
                    <open_icon>../images/Method.svg</open_icon>
                    <close_icon>../images/Method.svg</close_icon>
                    <name>Administration/Method</name>
                    <label>Methods</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>61F0B16FD9BF4D57BFF7E50ABF28CC19</itemtype_id>
                    <open_icon>../Images/OfficeDocumentReference.svg</open_icon>
                    <close_icon>../Images/OfficeDocumentReference.svg</close_icon>
                    <name>Administration/MSO_Reference</name>
                    <label>Office Document References</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>C6A89FDE1294451497801DF78341B473</itemtype_id>
                    <open_icon>../images/Permission.svg</open_icon>
                    <close_icon>../images/Permission.svg</close_icon>
                    <name>Administration/Permission</name>
                    <label>Permissions</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>7C63771EBC8D46FE8E902C5188033515</itemtype_id>
                    <open_icon>../images/Preference.svg</open_icon>
                    <close_icon>../images/Preference.svg</close_icon>
                    <name>Administration/Preference</name>
                    <label>Preferences</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>471932C33B604C3099070F4106EE5024</itemtype_id>
                    <open_icon>../images/RelationshipType.svg</open_icon>
                    <close_icon>../images/RelationshipType.svg</close_icon>
                    <name>Administration/RelationshipType</name>
                    <label>RelationshipTypes</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>F0834BBA6FB64394B78DF5BB725532DD</itemtype_id>
                    <open_icon>../images/Report.svg</open_icon>
                    <close_icon>../images/Report.svg</close_icon>
                    <name>Administration/Report</name>
                    <label>Reports</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>85D5BDBAED9643D28409F30FD3C3501B</itemtype_id>
                    <open_icon>../Images/work-request-line.svg</open_icon>
                    <close_icon>../Images/work-request-line.svg</close_icon>
                    <name>Administration/SimulationTemplate</name>
                    <label>Request Templates</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>F81CDEF9FE324D01947CC9023BC38317</itemtype_id>
                    <open_icon>../images/Revision.svg</open_icon>
                    <close_icon>../images/Revision.svg</close_icon>
                    <name>Administration/Revision</name>
                    <label>Revisions</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>18C15AB147F84834874F2E0CB6B8B4C0</itemtype_id>
                    <open_icon>../images/SavedSearch.svg</open_icon>
                    <close_icon>../images/SavedSearch.svg</close_icon>
                    <name>Administration/SavedSearch</name>
                    <label>Saved Searches</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>A46890D3535C41D4A5D79240B8C373B0</itemtype_id>
                    <open_icon>../Images/SelfServiceReporting.svg</open_icon>
                    <close_icon>../Images/SelfServiceReporting.svg</close_icon>
                    <name>Administration/SelfServiceReport</name>
                    <label>Self-Service Reports</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>2B46201802CE46708C269667DB4798AC</itemtype_id>
                    <open_icon>../images/Sequence.svg</open_icon>
                    <close_icon>../images/Sequence.svg</close_icon>
                    <name>Administration/Sequence</name>
                    <label>Sequences</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>FC3E32F18F804FD9BE4B175973D29112</itemtype_id>
                    <open_icon>../images/SQL.svg</open_icon>
                    <close_icon>../images/SQL.svg</close_icon>
                    <name>Administration/SQL</name>
                    <label>SQLs</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>CC23F9130F574E7D99DF9659F27590A6</itemtype_id>
                    <open_icon>../images/Team.svg</open_icon>
                    <close_icon>../images/Team.svg</close_icon>
                    <name>Administration/Team</name>
                    <label>Teams</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>45E899CD2859442982EB22BB2DF683E5</itemtype_id>
                    <open_icon>../images/User.svg</open_icon>
                    <close_icon>../images/User.svg</close_icon>
                    <name>Administration/User</name>
                    <label>Users</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>6DAB4ACC09E6471DB4BDD15F36C3482B</itemtype_id>
                    <open_icon>../images/Variable.svg</open_icon>
                    <close_icon>../images/Variable.svg</close_icon>
                    <name>Administration/Variable</name>
                    <label>Variables</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>B19D349CC6FC44BC97D50A6D70AE79CB</itemtype_id>
                    <open_icon>../images/WorkflowMap.svg</open_icon>
                    <close_icon>../images/WorkflowMap.svg</close_icon>
                    <name>Administration/Workflow Map</name>
                    <label>Workflow Maps</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
              <Item type='Tree Node Child'>
                <related_id>
                  <Item type='Tree Node'>
                    <itemtype_id>261EAC08AE9144FC95C49182ACE0D3FE</itemtype_id>
                    <open_icon>../images/WorkflowProcess.svg</open_icon>
                    <close_icon>../images/WorkflowProcess.svg</close_icon>
                    <name>Administration/Workflow Process</name>
                    <label>Workflow Processes</label>
                    <classification>Tree Node/ItemTypeInToc</classification>
                  </Item>
                </related_id>
              </Item>
            </Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
            <Relationships></Relationships>
          </Item>
          <Item type='Tree Node'>
            <itemtype_id>48BF847EAEFA42BBB0F50F86A07B637E</itemtype_id>
            <open_icon>../Images/pinboard-line.svg</open_icon>
            <close_icon>../Images/pinboard-line.svg</close_icon>
            <name>SimChangeNotice</name>
            <label>Change Notices</label>
            <classification>Tree Node/ItemTypeInToc</classification>
          </Item>
          <Item type='Tree Node'>
            <itemtype_id>717606D0FD314DBA915FA653DF297A73</itemtype_id>
            <open_icon>../Images/storage-line.svg</open_icon>
            <close_icon>../Images/storage-line.svg</close_icon>
            <name>Data_UI</name>
            <label>Data</label>
            <classification>Tree Node/ItemTypeInToc</classification>
          </Item>
          <Item type='Tree Node'>
            <itemtype_id>3E85D9DD379643F8A207B99A9DFB72C2</itemtype_id>
            <open_icon>../Images/talk-bubbles-line.svg</open_icon>
            <close_icon>../Images/talk-bubbles-line.svg</close_icon>
            <name>Discussion</name>
            <label>Discussions</label>
            <classification>Tree Node/ItemTypeInToc</classification>
          </Item>
          <Item type='Tree Node'>
            <itemtype_id>0B9D641B40D24036A117D911558CBDCE</itemtype_id>
            <open_icon>../Images/bar-chart-line.svg</open_icon>
            <close_icon>../Images/bar-chart-line.svg</close_icon>
            <name>MyReports</name>
            <label>Reports</label>
            <classification>Tree Node/ItemTypeInToc</classification>
          </Item>
          <Item type='Tree Node'>
            <itemtype_id>BC7977377FFF40D59FF14205914E9C71</itemtype_id>
            <open_icon>../Images/inbox-line.svg</open_icon>
            <close_icon>../images/InBasketTask.svg</close_icon>
            <name>InBasket Task</name>
            <label>Task List</label>
            <classification>Tree Node/ItemTypeInToc</classification>
          </Item>
          <Item type='Tree Node'>
            <itemtype_id>2CFE6D5B341947668654545F031810ED</itemtype_id>
            <open_icon>../Images/tasks-line.svg</open_icon>
            <close_icon>../Images/tasks-line.svg</close_icon>
            <name>SimulationTask</name>
            <label>Tasks</label>
            <classification>Tree Node/ItemTypeInToc</classification>
          </Item>
          <Item type='Tree Node'>
            <itemtype_id>4AE047D8E67A4FD59DB03CD74D64C430</itemtype_id>
            <open_icon>../Images/work-request-line.svg</open_icon>
            <close_icon>../Images/work-request-line.svg</close_icon>
            <name>SimulationRequest</name>
            <label>Work Requests</label>
            <classification>Tree Node/ItemTypeInToc</classification>
          </Item>
        </root>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";

    const string _cuiToc = @"<AML>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='6C773D9EE99446969C87E4DA10F8DF13'>
    <id keyed_name='com.aras.innovator.mp.toc_Access Control' type='CommandBarMenu'>6C773D9EE99446969C87E4DA10F8DF13</id>
    <label xml:lang='en'>Access Control</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.mp.toc_Access Control</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>128</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='ED137389E93048819E68DD364EDA1322'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</id>
    <label xml:lang='en'>Administration</label>
    <name>com.aras.innovator.cui_default.toc_Administration</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>256</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='64DF4B14BED84AF7B2EDE0EF3764EFBE'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Change Management' type='CommandBarMenu'>64DF4B14BED84AF7B2EDE0EF3764EFBE</id>
    <label xml:lang='en'>Change Management</label>
    <name>com.aras.innovator.cui_default.toc_Change Management</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>384</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='C599EA936834459F83CC0D3B09BF5856'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Configuration' type='CommandBarMenu'>C599EA936834459F83CC0D3B09BF5856</id>
    <label xml:lang='en'>Configuration</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Configuration</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>512</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='677A28DAA58D443093069C7B7C23F287'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Content Modeling' type='CommandBarMenu'>677A28DAA58D443093069C7B7C23F287</id>
    <label xml:lang='en'>Content Modeling</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Content Modeling</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>640</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='ED37E2A4E866447088D6EA16B3340FBD'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Design' type='CommandBarMenu'>ED37E2A4E866447088D6EA16B3340FBD</id>
    <label xml:lang='en'>Design</label>
    <name>com.aras.innovator.cui_default.toc_Design</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>768</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='15DDAB021C704306881BAF3CE6B6CE17'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Documents' type='CommandBarMenu'>15DDAB021C704306881BAF3CE6B6CE17</id>
    <label xml:lang='en'>Documents</label>
    <name>com.aras.innovator.cui_default.toc_Documents</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>896</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='740CAE5357F64959A6969D0909C66988'>
    <id keyed_name='com.aras.innovator.effs.toc_Effectivity Services' type='CommandBarMenu'>740CAE5357F64959A6969D0909C66988</id>
    <label xml:lang='en'>Effectivity Services</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.effs.toc_Effectivity Services</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>1024</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='E5D4D2F396404BADB48DFA7431AB836D'>
    <id keyed_name='com.aras.innovator.es.toc_Enterprise Search' type='CommandBarMenu'>E5D4D2F396404BADB48DFA7431AB836D</id>
    <label xml:lang='en'>Enterprise Search</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.es.toc_Enterprise Search</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>1152</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='4D83ED9DD681459A94590FE16F64B962'>
    <id keyed_name='com.aras.innovator.xpxc.toc_Extended Classification' type='CommandBarMenu'>4D83ED9DD681459A94590FE16F64B962</id>
    <label xml:lang='en'>Extended Classification</label>
    <name>com.aras.innovator.xpxc.toc_Extended Classification</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>1280</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='D857FFD536F741DEA5778F4F8252A762'>
    <id keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</id>
    <label xml:lang='en'>File Handling</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_File Handling</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>1408</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='324E4BAF108344D999C36AC6F0EB0FE7'>
    <id keyed_name='com.aras.innovator.gn.toc_GraphNavigation' type='CommandBarMenu'>324E4BAF108344D999C36AC6F0EB0FE7</id>
    <label xml:lang='en'>Graph Navigation</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Configuration' type='CommandBarMenu'>C599EA936834459F83CC0D3B09BF5856</parent_menu>
    <name>com.aras.innovator.gn.toc_GraphNavigation</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>1536</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='D99D74772743438F938FB14C1710A84B'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Localization' type='CommandBarMenu'>D99D74772743438F938FB14C1710A84B</id>
    <label xml:lang='en'>Localization</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Localization</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>1664</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='69E7F9371B8C4731815FA9618F6953C3'>
    <id keyed_name='com.aras.innovator.mpo.toc_Mass Operations' type='CommandBarMenu'>69E7F9371B8C4731815FA9618F6953C3</id>
    <label xml:lang='en'>Mass Operations</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.mpo.toc_Mass Operations</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>1792</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='FA93FDCB24D245DE8A82B2234B1C95DB'>
    <id keyed_name='com.aras.innovator.cui_default.toc_My Innovator' type='CommandBarMenu'>FA93FDCB24D245DE8A82B2234B1C95DB</id>
    <label xml:lang='en'>My Innovator</label>
    <name>com.aras.innovator.cui_default.toc_My Innovator</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>1920</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='A7B3994B55FB409B887BD708ED4F856F'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Notification' type='CommandBarMenu'>A7B3994B55FB409B887BD708ED4F856F</id>
    <label xml:lang='en'>Notification</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Notification</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>2048</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='718FA406416E44369BE81C45C6C50360'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Portfolio' type='CommandBarMenu'>718FA406416E44369BE81C45C6C50360</id>
    <label xml:lang='en'>Portfolio</label>
    <name>com.aras.innovator.cui_default.toc_Portfolio</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>2176</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='6592CC9829F641ADB926EB4766B393A2'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Secure Social' type='CommandBarMenu'>6592CC9829F641ADB926EB4766B393A2</id>
    <label xml:lang='en'>Secure Social</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Secure Social</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>2304</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='E0A77E86F2F6419AB7DEE7CFB0B28FC7'>
    <id keyed_name='com.aras.innovator.cui_default.toc_Sourcing' type='CommandBarMenu'>E0A77E86F2F6419AB7DEE7CFB0B28FC7</id>
    <label xml:lang='en'>Sourcing</label>
    <name>com.aras.innovator.cui_default.toc_Sourcing</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>2432</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='BE0A63FCB9034FFB84A2FE4867AC0382'>
    <id keyed_name='com.aras.innovator.cui_default.toc_System Events' type='CommandBarMenu'>BE0A63FCB9034FFB84A2FE4867AC0382</id>
    <label xml:lang='en'>System Events</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_System Events</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>2560</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenu' typeId='B5175846B27145EEA5653DA35ED78BE4' id='5DA51B9736D04B399B8839FD75D07F2A'>
    <id keyed_name='com.aras.innovator.wm.toc_Watermarking' type='CommandBarMenu'>5DA51B9736D04B399B8839FD75D07F2A</id>
    <label xml:lang='en'>Watermarking</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.wm.toc_Watermarking</name>
    <itemtype>B5175846B27145EEA5653DA35ED78BE4</itemtype>
    <sort_order>2688</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='0A2F5F4C36E44BEC8233665F472CA26D'>
    <additional_data>{
  ""itemTypeId"": ""483228BE6B9A4C0E99ACD55FDF328DEC"",
  ""tocAccessId"": ""0A2F5F4C36E44BEC8233665F472CA26D""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Action_9482ACC3C88EE1834A899BBDDC2171C3' type='CommandBarMenuButton'>0A2F5F4C36E44BEC8233665F472CA26D</id>
    <image>../images/Action.svg</image>
    <label xml:lang='en'>Actions</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Action_9482ACC3C88EE1834A899BBDDC2171C3</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>2816</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='428616D754514A86B1F9C761C8C0A5E2'>
    <additional_data>{
  ""itemTypeId"": ""FAFACB0BC40E4C3DBFC282F058505C7F"",
  ""tocAccessId"": ""428616D754514A86B1F9C761C8C0A5E2""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_ES_Agent_B8AB86CC36EC0F08102416FD6096DF50' type='CommandBarMenuButton'>428616D754514A86B1F9C761C8C0A5E2</id>
    <image>../images/SearchAgent.svg</image>
    <label xml:lang='en'>Agents</label>
    <parent_menu keyed_name='com.aras.innovator.es.toc_Enterprise Search' type='CommandBarMenu'>E5D4D2F396404BADB48DFA7431AB836D</parent_menu>
    <name>com.aras.innovator.cui_default.toc_ES_Agent_B8AB86CC36EC0F08102416FD6096DF50</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>2944</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='3F4D845E525C4DD1A40FDBD6F17B51A5'>
    <additional_data>{
  ""itemTypeId"": ""CCF205347C814DD1AF056875E0A880AC"",
  ""tocAccessId"": ""3F4D845E525C4DD1A40FDBD6F17B51A5""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_CAD_D38AD887C6644F8EF3F330DB27A37363' type='CommandBarMenuButton'>3F4D845E525C4DD1A40FDBD6F17B51A5</id>
    <image>../images/CAD.svg</image>
    <label xml:lang='en'>CAD Documents</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Documents' type='CommandBarMenu'>15DDAB021C704306881BAF3CE6B6CE17</parent_menu>
    <name>com.aras.innovator.plm.toc_CAD_D38AD887C6644F8EF3F330DB27A37363</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>3072</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='506A6850A14346909899622C5AD1617E'>
    <additional_data>{
  ""itemTypeId"": ""8AB5CEB9824B4CF7920AFED29F662C66"",
  ""tocAccessId"": ""506A6850A14346909899622C5AD1617E""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_GlobalPresentationConfig_9826202A0C905E2B8A63CDD313F0BC87' type='CommandBarMenuButton'>506A6850A14346909899622C5AD1617E</id>
    <image>../Images/ClientPresentation.svg</image>
    <label xml:lang='en'>Client Presentation</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Configuration' type='CommandBarMenu'>C599EA936834459F83CC0D3B09BF5856</parent_menu>
    <name>com.aras.innovator.cui_default.toc_GlobalPresentationConfig_9826202A0C905E2B8A63CDD313F0BC87</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>3200</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='D26D4714C7A94C46B5EC3FBF46AF0828'>
    <additional_data>{
  ""itemTypeId"": ""17A499D45EF34ACBA65914A8F57238F1"",
  ""tocAccessId"": ""D26D4714C7A94C46B5EC3FBF46AF0828""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_gn_ConnectorViewDefinition_8A0B9CFDE17291275A019A9AF2CEDEEC' type='CommandBarMenuButton'>D26D4714C7A94C46B5EC3FBF46AF0828</id>
    <image>../Images/ConnectorView.svg</image>
    <label xml:lang='en'>Connector View Definitions</label>
    <parent_menu keyed_name='com.aras.innovator.gn.toc_GraphNavigation' type='CommandBarMenu'>324E4BAF108344D999C36AC6F0EB0FE7</parent_menu>
    <name>com.aras.innovator.cui_default.toc_gn_ConnectorViewDefinition_8A0B9CFDE17291275A019A9AF2CEDEEC</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>3328</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='7FF508FC10D647BB908FD89B75E5D107'>
    <additional_data>{
  ""itemTypeId"": ""94E345F15EB94D86ADE8FF1B6AE2B439"",
  ""tocAccessId"": ""7FF508FC10D647BB908FD89B75E5D107""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_cmf_Style_F579264F69723B74E0C46F7B0180C7C4' type='CommandBarMenuButton'>7FF508FC10D647BB908FD89B75E5D107</id>
    <image>../Images/ContentStyle.svg</image>
    <label xml:lang='en'>Content Styles</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Content Modeling' type='CommandBarMenu'>677A28DAA58D443093069C7B7C23F287</parent_menu>
    <name>com.aras.innovator.cui_default.toc_cmf_Style_F579264F69723B74E0C46F7B0180C7C4</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>3456</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='2B5C4A6487BA4E07B58EFAEDFEB5C284'>
    <additional_data>{
  ""itemTypeId"": ""3538F62649F3477EA1F8990EB20F88B9"",
  ""tocAccessId"": ""2B5C4A6487BA4E07B58EFAEDFEB5C284""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_cmf_ContentType_3E67873D179E27753CB1A88C0DC395C5' type='CommandBarMenuButton'>2B5C4A6487BA4E07B58EFAEDFEB5C284</id>
    <image>../Images/ContentType.svg</image>
    <label xml:lang='en'>Content Types</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Content Modeling' type='CommandBarMenu'>677A28DAA58D443093069C7B7C23F287</parent_menu>
    <name>com.aras.innovator.cui_default.toc_cmf_ContentType_3E67873D179E27753CB1A88C0DC395C5</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>3584</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='7EC658DB4C8544598861163296F1402F'>
    <additional_data>{
  ""itemTypeId"": ""05DF56FF833542F98251528F3FFE2FA0"",
  ""tocAccessId"": ""7EC658DB4C8544598861163296F1402F""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_ConversionRule_0B8E269DF9CAD14553DE2B83BF85E2D9' type='CommandBarMenuButton'>7EC658DB4C8544598861163296F1402F</id>
    <image>../images/ConversionRule.svg</image>
    <label xml:lang='en'>Conversion Rules</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</parent_menu>
    <name>com.aras.innovator.cui_default.toc_ConversionRule_0B8E269DF9CAD14553DE2B83BF85E2D9</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>3712</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='757F3319358B452B9125FBE6620CBA1D'>
    <additional_data>{
  ""itemTypeId"": ""02FA838247DF47C2BB85AAB299E646B2"",
  ""tocAccessId"": ""757F3319358B452B9125FBE6620CBA1D""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_ConversionServer_C3AD2FFA33F46CBF94F14A217B8A8202' type='CommandBarMenuButton'>757F3319358B452B9125FBE6620CBA1D</id>
    <image>../images/ConversionServer.svg</image>
    <label xml:lang='en'>Conversion Servers</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</parent_menu>
    <name>com.aras.innovator.cui_default.toc_ConversionServer_C3AD2FFA33F46CBF94F14A217B8A8202</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>3840</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='0A946C2BC33C4254B4935D9028DA101E'>
    <additional_data>{
  ""itemTypeId"": ""0300B828CBEE4610B77C41377209C900"",
  ""tocAccessId"": ""0A946C2BC33C4254B4935D9028DA101E""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_ConversionTask_EE18E8BC08ED6444DD09DEF257EC4BC8' type='CommandBarMenuButton'>0A946C2BC33C4254B4935D9028DA101E</id>
    <image>../images/ConversionTask.svg</image>
    <label xml:lang='en'>Conversion Tasks</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</parent_menu>
    <name>com.aras.innovator.cui_default.toc_ConversionTask_EE18E8BC08ED6444DD09DEF257EC4BC8</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>3968</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='AC21F97B83674149A48253D99C3DE3D4'>
    <additional_data>{
  ""itemTypeId"": ""0DE14E76AA794A039DA8D2CDC34E6B1D"",
  ""tocAccessId"": ""AC21F97B83674149A48253D99C3DE3D4""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_ConverterType_1707929FD5F63ED5C487869226E7CEDC' type='CommandBarMenuButton'>AC21F97B83674149A48253D99C3DE3D4</id>
    <image>../images/ConverterType.svg</image>
    <label xml:lang='en'>Conversion Types</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</parent_menu>
    <name>com.aras.innovator.cui_default.toc_ConverterType_1707929FD5F63ED5C487869226E7CEDC</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>4096</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='039C570782F54360A540B9168C2B9B9A'>
    <additional_data>{
  ""itemTypeId"": ""9D136E339A6743E28C0974FA5D61549B"",
  ""tocAccessId"": ""039C570782F54360A540B9168C2B9B9A""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_ES_Crawler_7992C195470754E0C6F04913ADABD3B5' type='CommandBarMenuButton'>039C570782F54360A540B9168C2B9B9A</id>
    <image>../images/SearchCrawler.svg</image>
    <label xml:lang='en'>Crawlers</label>
    <parent_menu keyed_name='com.aras.innovator.es.toc_Enterprise Search' type='CommandBarMenu'>E5D4D2F396404BADB48DFA7431AB836D</parent_menu>
    <name>com.aras.innovator.cui_default.toc_ES_Crawler_7992C195470754E0C6F04913ADABD3B5</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>4224</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='CB2E68372BDE443A99B47E044B53C51D'>
    <additional_data>{
  ""itemTypeId"": ""E4847F4A706B4285A395C0228A867E0F"",
  ""tocAccessId"": ""CB2E68372BDE443A99B47E044B53C51D""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_Customer_18B8AA3A73F6EB6D2D25EC305A2535B4' type='CommandBarMenuButton'>CB2E68372BDE443A99B47E044B53C51D</id>
    <image>../images/Customer.svg</image>
    <label xml:lang='en'>Customers</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Portfolio' type='CommandBarMenu'>718FA406416E44369BE81C45C6C50360</parent_menu>
    <name>com.aras.innovator.plm.toc_Customer_18B8AA3A73F6EB6D2D25EC305A2535B4</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>4352</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='822A0A803BD04825AA86656E234824FF'>
    <additional_data>{
  ""itemTypeId"": ""62ECDC442CAD4ECAA7E6721475FC86E0"",
  ""tocAccessId"": ""822A0A803BD04825AA86656E234824FF""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_dac_DomainDefinition_80C67E8764593167EC96DDA43E110F30' type='CommandBarMenuButton'>822A0A803BD04825AA86656E234824FF</id>
    <image>../Images/DomainAccessControlDefinition.svg</image>
    <label xml:lang='en'>DAC Definitions</label>
    <parent_menu keyed_name='com.aras.innovator.mp.toc_Access Control' type='CommandBarMenu'>6C773D9EE99446969C87E4DA10F8DF13</parent_menu>
    <name>com.aras.innovator.cui_default.toc_dac_DomainDefinition_80C67E8764593167EC96DDA43E110F30</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>4480</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='27C22BF54BE746C4A25E9289709E7233'>
    <additional_data>{
  ""itemTypeId"": ""645774D6072F41FD8F998C861E211741"",
  ""tocAccessId"": ""27C22BF54BE746C4A25E9289709E7233""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Dashboard_4A124B07531573A7ED9417761617989F' type='CommandBarMenuButton'>27C22BF54BE746C4A25E9289709E7233</id>
    <image>../images/Dashboard.svg</image>
    <label xml:lang='en'>Dashboards</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Configuration' type='CommandBarMenu'>C599EA936834459F83CC0D3B09BF5856</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Dashboard_4A124B07531573A7ED9417761617989F</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>4608</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='13CE0CC847554D63A7C385408F4D8CE7'>
    <additional_data>{
  ""itemTypeId"": ""4DAD707F62B54823AE2E4730BB00C649"",
  ""tocAccessId"": ""13CE0CC847554D63A7C385408F4D8CE7""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_DatabaseUpgrade_0132095D5B389DC187A8B952699B2E8C' type='CommandBarMenuButton'>13CE0CC847554D63A7C385408F4D8CE7</id>
    <image>../images/DatabaseUpgrade.svg</image>
    <label xml:lang='en'>Database Upgrades</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Configuration' type='CommandBarMenu'>C599EA936834459F83CC0D3B09BF5856</parent_menu>
    <name>com.aras.innovator.cui_default.toc_DatabaseUpgrade_0132095D5B389DC187A8B952699B2E8C</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>4736</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='11E0D964E56B45FEBAA29960FB20D818'>
    <additional_data>{
  ""itemTypeId"": ""B678FAF40F7246A7925FB5B116988915"",
  ""tocAccessId"": ""11E0D964E56B45FEBAA29960FB20D818""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_Express DCO_364D60822FCFA143F2C531C7F456CE20' type='CommandBarMenuButton'>11E0D964E56B45FEBAA29960FB20D818</id>
    <image>../images/ExpressDCO.svg</image>
    <label xml:lang='en'>DCOs</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Change Management' type='CommandBarMenu'>64DF4B14BED84AF7B2EDE0EF3764EFBE</parent_menu>
    <name>com.aras.innovator.plm.toc_Express DCO_364D60822FCFA143F2C531C7F456CE20</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>4864</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='6CF81C3D2FF04057A191D370D6A91805'>
    <additional_data>{
  ""itemTypeId"": ""CC8231836D2F40A0984D1E01AEC749DF"",
  ""tocAccessId"": ""6CF81C3D2FF04057A191D370D6A91805""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_dr_RelationshipFamily_CBE441D5876CDDEC0E0B05BDBCD28E9C' type='CommandBarMenuButton'>6CF81C3D2FF04057A191D370D6A91805</id>
    <image>../images/DerivedRelationshipFamily.svg</image>
    <label xml:lang='en'>Derived Relationship Families</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Configuration' type='CommandBarMenu'>C599EA936834459F83CC0D3B09BF5856</parent_menu>
    <name>com.aras.innovator.cui_default.toc_dr_RelationshipFamily_CBE441D5876CDDEC0E0B05BDBCD28E9C</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>4992</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='B8CFC4379A674EED9B158C4A80F0AC7D'>
    <additional_data>{
  ""itemTypeId"": ""C3317E65105D44C88C1603FB919146BC"",
  ""tocAccessId"": ""B8CFC4379A674EED9B158C4A80F0AC7D""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_PE_Deviation_E6C26E4F5AA28A0BBB12CDCFB3B96A11' type='CommandBarMenuButton'>B8CFC4379A674EED9B158C4A80F0AC7D</id>
    <image>../Images/Deviation.svg</image>
    <label xml:lang='en'>Deviations</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Change Management' type='CommandBarMenu'>64DF4B14BED84AF7B2EDE0EF3764EFBE</parent_menu>
    <name>com.aras.innovator.plm.toc_PE_Deviation_E6C26E4F5AA28A0BBB12CDCFB3B96A11</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>5120</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='84FD7C09821A401581BE13AEC072BCCC'>
    <additional_data>{
  ""itemTypeId"": ""B88C14B99EF449828C5D926E39EE8B89"",
  ""tocAccessId"": ""84FD7C09821A401581BE13AEC072BCCC""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_Document_34BA970B4879EAAAF7372504DC7BE8AD' type='CommandBarMenuButton'>84FD7C09821A401581BE13AEC072BCCC</id>
    <image>../images/Document.svg</image>
    <label xml:lang='en'>Documents</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Documents' type='CommandBarMenu'>15DDAB021C704306881BAF3CE6B6CE17</parent_menu>
    <name>com.aras.innovator.plm.toc_Document_34BA970B4879EAAAF7372504DC7BE8AD</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>5248</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='87BA1FD47B2B4B0885CFE17231677CA8'>
    <additional_data>{
  ""itemTypeId"": ""F91DE6AE038A4CC8B53D47D5A7FA49FC"",
  ""tocAccessId"": ""87BA1FD47B2B4B0885CFE17231677CA8""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_EMail Message_8FEDA2533E81678A1CB2FAF9F115C097' type='CommandBarMenuButton'>87BA1FD47B2B4B0885CFE17231677CA8</id>
    <image>../images/EMailMessage.svg</image>
    <label xml:lang='en'>E-Mail Message</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Notification' type='CommandBarMenu'>A7B3994B55FB409B887BD708ED4F856F</parent_menu>
    <name>com.aras.innovator.cui_default.toc_EMail Message_8FEDA2533E81678A1CB2FAF9F115C097</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>5376</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='7197E8100DE941F8B007B6FB335113EF'>
    <additional_data>{
  ""itemTypeId"": ""85F86794FACF42599F07019F7BEA1D46"",
  ""tocAccessId"": ""7197E8100DE941F8B007B6FB335113EF""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_ECN_301205467B730F3559B7FDA38064B651' type='CommandBarMenuButton'>7197E8100DE941F8B007B6FB335113EF</id>
    <image>../images/ECN.svg</image>
    <label xml:lang='en'>ECNs</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Change Management' type='CommandBarMenu'>64DF4B14BED84AF7B2EDE0EF3764EFBE</parent_menu>
    <name>com.aras.innovator.plm.toc_ECN_301205467B730F3559B7FDA38064B651</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>5504</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='762F6A213BAA44408431C46BB00B8700'>
    <additional_data>{
  ""itemTypeId"": ""CBA93BEFFB4F499CAF122CB79E204983"",
  ""tocAccessId"": ""762F6A213BAA44408431C46BB00B8700""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_Express ECO_CD2FE652398350F27D03E50482881C43' type='CommandBarMenuButton'>762F6A213BAA44408431C46BB00B8700</id>
    <image>../images/ExpressECO.svg</image>
    <label xml:lang='en'>ECOs</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Change Management' type='CommandBarMenu'>64DF4B14BED84AF7B2EDE0EF3764EFBE</parent_menu>
    <name>com.aras.innovator.plm.toc_Express ECO_CD2FE652398350F27D03E50482881C43</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>5632</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='17BBFB48FAC34450B622C4FE0D3B5940'>
    <additional_data>{
  ""itemTypeId"": ""2D4799413792453DA08C1EDE6AA22A76"",
  ""tocAccessId"": ""17BBFB48FAC34450B622C4FE0D3B5940""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_ECR_F1BD78A1E0EE3B98E7990034A487DC37' type='CommandBarMenuButton'>17BBFB48FAC34450B622C4FE0D3B5940</id>
    <image>../images/ECR.svg</image>
    <label xml:lang='en'>ECRs</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Change Management' type='CommandBarMenu'>64DF4B14BED84AF7B2EDE0EF3764EFBE</parent_menu>
    <name>com.aras.innovator.plm.toc_ECR_F1BD78A1E0EE3B98E7990034A487DC37</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>5760</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='7F06C10F802A44719804D1EC5EE65519'>
    <additional_data>{
  ""itemTypeId"": ""EA86051664F44D48AE48226ABC4E570E"",
  ""tocAccessId"": ""7F06C10F802A44719804D1EC5EE65519""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_Express EDR_3A5DD91CE17D539D1CC9E4FC6D687855' type='CommandBarMenuButton'>7F06C10F802A44719804D1EC5EE65519</id>
    <image>../images/ExpressEDR.svg</image>
    <label xml:lang='en'>EDRs</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Change Management' type='CommandBarMenu'>64DF4B14BED84AF7B2EDE0EF3764EFBE</parent_menu>
    <name>com.aras.innovator.plm.toc_Express EDR_3A5DD91CE17D539D1CC9E4FC6D687855</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>5888</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='139AAE432D0E49D09BDEC3130751735B'>
    <additional_data>{
  ""itemTypeId"": ""63449FB1A2EE411ABFEE16109A5576C3"",
  ""tocAccessId"": ""139AAE432D0E49D09BDEC3130751735B""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_effs_scope_95DD864B44C4FCC8F096F28C540B84C9' type='CommandBarMenuButton'>139AAE432D0E49D09BDEC3130751735B</id>
    <image>../images/EffScope.svg</image>
    <label xml:lang='en'>Effectivity Scope</label>
    <parent_menu keyed_name='com.aras.innovator.effs.toc_Effectivity Services' type='CommandBarMenu'>740CAE5357F64959A6969D0909C66988</parent_menu>
    <name>com.aras.innovator.cui_default.toc_effs_scope_95DD864B44C4FCC8F096F28C540B84C9</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>6016</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='6362250603E24689BA4A6400EBDB5E24'>
    <additional_data>{
  ""itemTypeId"": ""0B477D812B35400DB4F27C6067783B29"",
  ""tocAccessId"": ""6362250603E24689BA4A6400EBDB5E24""
}</additional_data>
    <id keyed_name='com.aras.innovator.effs.toc_effs_variable_D97CF1E3941DE5E836B0AC0B39536C8B' type='CommandBarMenuButton'>6362250603E24689BA4A6400EBDB5E24</id>
    <image>../images/EffectivityVariable.svg</image>
    <label xml:lang='en'>Effectivity Variables</label>
    <parent_menu keyed_name='com.aras.innovator.effs.toc_Effectivity Services' type='CommandBarMenu'>740CAE5357F64959A6969D0909C66988</parent_menu>
    <name>com.aras.innovator.effs.toc_effs_variable_D97CF1E3941DE5E836B0AC0B39536C8B</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>6144</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' id='80AE87B8B2A5456FA42B5349637C7135'>
    <parent_menu>E5D4D2F396404BADB48DFA7431AB836D</parent_menu>
    <name>Enterprise Search_191196ACFF4F440BBD0524A97693B35F</name>
    <image>../images/DefaultItemType.svg</image>
    <label>Enterprise Search</label>
    <additional_data>{
  ""formId"": ""901B4A7BB60540D4B914A9B5CB66713D""
}</additional_data>
    <sort_order>6272</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='54C0FB1493484F4983D822AAA23D449F'>
    <additional_data>{
  ""itemTypeId"": ""4DDF9A4566C24F8E9E22632FD7F08A75"",
  ""tocAccessId"": ""54C0FB1493484F4983D822AAA23D449F""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_mp_PolicyAccessEnvAttribute_648CD1A4603E37E488FA3EF1E8B5E005' type='CommandBarMenuButton'>54C0FB1493484F4983D822AAA23D449F</id>
    <image>../Images/EnvironmentAttribute.svg</image>
    <label xml:lang='en'>Environment Attributes</label>
    <parent_menu keyed_name='com.aras.innovator.mp.toc_Access Control' type='CommandBarMenu'>6C773D9EE99446969C87E4DA10F8DF13</parent_menu>
    <name>com.aras.innovator.cui_default.toc_mp_PolicyAccessEnvAttribute_648CD1A4603E37E488FA3EF1E8B5E005</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>6400</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='FADDCF054682431E86E5267B2031E487'>
    <additional_data>{
  ""itemTypeId"": ""03435AC2C3D844FC87DA9CAF1899C753"",
  ""tocAccessId"": ""FADDCF054682431E86E5267B2031E487""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Permission_ExplicitDefine_B90A5521A3BF97858E2A2BFD4313FB79' type='CommandBarMenuButton'>FADDCF054682431E86E5267B2031E487</id>
    <image>../Images/ExplicitPermission.svg</image>
    <label xml:lang='en'>Explicit Permissions</label>
    <parent_menu keyed_name='com.aras.innovator.xpxc.toc_Extended Classification' type='CommandBarMenu'>4D83ED9DD681459A94590FE16F64B962</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Permission_ExplicitDefine_B90A5521A3BF97858E2A2BFD4313FB79</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>6528</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='5E612514BE334AC2BEA3481B2077DB7A'>
    <additional_data>{
  ""itemTypeId"": ""2073428E99384916938E3519AF1C0A44"",
  ""tocAccessId"": ""5E612514BE334AC2BEA3481B2077DB7A""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_xPropertyContainerItem_07AAD3875C0813B188957B283168156E' type='CommandBarMenuButton'>5E612514BE334AC2BEA3481B2077DB7A</id>
    <image>../Images/ExtendedPropertySearch.svg</image>
    <label xml:lang='en'>Extended Property Search</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_My Innovator' type='CommandBarMenu'>FA93FDCB24D245DE8A82B2234B1C95DB</parent_menu>
    <name>com.aras.innovator.cui_default.toc_xPropertyContainerItem_07AAD3875C0813B188957B283168156E</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>6656</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='5F8B80820D294E1EB053E019A61DD6CE'>
    <additional_data>{
  ""itemTypeId"": ""CE0C4143D35E46CDA3874C4339F159BE"",
  ""tocAccessId"": ""5F8B80820D294E1EB053E019A61DD6CE""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Feature License_AAF2DAFEF1D42A1BD395DB5A1AAAB088' type='CommandBarMenuButton'>5F8B80820D294E1EB053E019A61DD6CE</id>
    <image>../images/FeatureLicense.svg</image>
    <label xml:lang='en'>Feature Licenses</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Feature License_AAF2DAFEF1D42A1BD395DB5A1AAAB088</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>6784</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='5BD352FB1BDA4B5AAF050BA1C07BD1CF'>
    <additional_data>{
  ""itemTypeId"": ""8052A558B9084D41B9F11805E464F443"",
  ""tocAccessId"": ""5BD352FB1BDA4B5AAF050BA1C07BD1CF""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_File_3A76B290694694BB4A763095FDF5C02F' type='CommandBarMenuButton'>5BD352FB1BDA4B5AAF050BA1C07BD1CF</id>
    <image>../images/File.svg</image>
    <label xml:lang='en'>Files</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</parent_menu>
    <name>com.aras.innovator.cui_default.toc_File_3A76B290694694BB4A763095FDF5C02F</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>6912</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='A303673F228F45E8A670DD89250FB988'>
    <additional_data>{
  ""itemTypeId"": ""13EC84A626F1457BB5F60A13DA03580B"",
  ""tocAccessId"": ""A303673F228F45E8A670DD89250FB988""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_FileType_DEE086A016D0B21FE1C5D92DA092A152' type='CommandBarMenuButton'>A303673F228F45E8A670DD89250FB988</id>
    <image>../images/FileType.svg</image>
    <label xml:lang='en'>FileTypes</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</parent_menu>
    <name>com.aras.innovator.cui_default.toc_FileType_DEE086A016D0B21FE1C5D92DA092A152</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>7040</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='C2D5C31B4A354AA09796F8C0A441812C'>
    <additional_data>{
  ""itemTypeId"": ""47573682FB7549F59ADECD4BFE04F1DE"",
  ""tocAccessId"": ""C2D5C31B4A354AA09796F8C0A441812C""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Form_D6BBDA7573BA60F132B1D5547168D62C' type='CommandBarMenuButton'>C2D5C31B4A354AA09796F8C0A441812C</id>
    <image>../images/Form.svg</image>
    <label xml:lang='en'>Forms</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Form_D6BBDA7573BA60F132B1D5547168D62C</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>7168</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='42A2695111794836A7C6CB8E7BDD4591'>
    <additional_data>{
  ""itemTypeId"": ""2E25B49E218A45D28D0C7D3C0633710C"",
  ""tocAccessId"": ""42A2695111794836A7C6CB8E7BDD4591""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Forum_0B959231B41A00622115705993B48761' type='CommandBarMenuButton'>42A2695111794836A7C6CB8E7BDD4591</id>
    <image>../images/Forum.svg</image>
    <label xml:lang='en'>Forums</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Secure Social' type='CommandBarMenu'>6592CC9829F641ADB926EB4766B393A2</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Forum_0B959231B41A00622115705993B48761</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>7296</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='D877969696B34949A902A98F37F4F701'>
    <additional_data>{
  ""itemTypeId"": ""6DAD626079124665A0B219FFE673052C"",
  ""tocAccessId"": ""D877969696B34949A902A98F37F4F701""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_gn_GraphViewDefinition_A1EDF625F29DF6D70CAEE1C52960E9D8' type='CommandBarMenuButton'>D877969696B34949A902A98F37F4F701</id>
    <image>../Images/GraphView.svg</image>
    <label xml:lang='en'>Graph View Definitions</label>
    <parent_menu keyed_name='com.aras.innovator.gn.toc_GraphNavigation' type='CommandBarMenu'>324E4BAF108344D999C36AC6F0EB0FE7</parent_menu>
    <name>com.aras.innovator.cui_default.toc_gn_GraphViewDefinition_A1EDF625F29DF6D70CAEE1C52960E9D8</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>7424</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='4CA2761C63F349D9BDB5FAF714A8C5D8'>
    <additional_data>{
  ""itemTypeId"": ""8EABD91B465443F0A4995418F483DC51"",
  ""tocAccessId"": ""4CA2761C63F349D9BDB5FAF714A8C5D8""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Grid_83AB5521DB6638BDFCD06483FC8D8C45' type='CommandBarMenuButton'>4CA2761C63F349D9BDB5FAF714A8C5D8</id>
    <image>../images/Grid.svg</image>
    <label xml:lang='en'>Grids</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Grid_83AB5521DB6638BDFCD06483FC8D8C45</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>7552</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='FDE0E3CCE7C84001B62CF9B712F67643'>
    <additional_data>{
  ""itemTypeId"": ""E582AB17663F4EF28460015B2BE9E094"",
  ""tocAccessId"": ""FDE0E3CCE7C84001B62CF9B712F67643""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Identity_5A7D0F36E0DED0D54E6C6D7A40AAF61A' type='CommandBarMenuButton'>FDE0E3CCE7C84001B62CF9B712F67643</id>
    <image>../images/Identity.svg</image>
    <label xml:lang='en'>Identities</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Identity_5A7D0F36E0DED0D54E6C6D7A40AAF61A</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>7680</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='372AD44DB5D5424181C9CDABE1719F03'>
    <additional_data>{
  ""itemTypeId"": ""5591005E6A904383A073FF3B209FC3BB"",
  ""tocAccessId"": ""372AD44DB5D5424181C9CDABE1719F03""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_ES_IndexedConfiguration_BCAC7DB65EB1A21EF063452D7103581C' type='CommandBarMenuButton'>372AD44DB5D5424181C9CDABE1719F03</id>
    <image>../images/SearchIndexedConfiguration.svg</image>
    <label xml:lang='en'>Indexed Configuration</label>
    <parent_menu keyed_name='com.aras.innovator.es.toc_Enterprise Search' type='CommandBarMenu'>E5D4D2F396404BADB48DFA7431AB836D</parent_menu>
    <name>com.aras.innovator.cui_default.toc_ES_IndexedConfiguration_BCAC7DB65EB1A21EF063452D7103581C</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>7808</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='5E88743218834F248DEB2826D7531B67'>
    <additional_data>{
  ""itemTypeId"": ""599A06DD161348CD84734DBF5829574C"",
  ""tocAccessId"": ""5E88743218834F248DEB2826D7531B67""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Permission_ItemClassification_2A8DF42309D2CD0F4F8CD577889C9394' type='CommandBarMenuButton'>5E88743218834F248DEB2826D7531B67</id>
    <image>../Images/ItemClassPermission.svg</image>
    <label xml:lang='en'>Item Classification Permissions</label>
    <parent_menu keyed_name='com.aras.innovator.xpxc.toc_Extended Classification' type='CommandBarMenu'>4D83ED9DD681459A94590FE16F64B962</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Permission_ItemClassification_2A8DF42309D2CD0F4F8CD577889C9394</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>7936</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='7519094A7BB646A2B195A2A069B64AFE'>
    <additional_data>{
  ""itemTypeId"": ""450906E86E304F55A34B3C0D65C097EA"",
  ""tocAccessId"": ""7519094A7BB646A2B195A2A069B64AFE""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_ItemType_C20E7D942C1A3FE7AB3D175E28434563' type='CommandBarMenuButton'>7519094A7BB646A2B195A2A069B64AFE</id>
    <image>../images/ItemType.svg</image>
    <label xml:lang='en'>ItemTypes</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_ItemType_C20E7D942C1A3FE7AB3D175E28434563</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>8064</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='5C2F61906CE3401EB8290E9197C0E5BE'>
    <additional_data>{
  ""itemTypeId"": ""030019FA30FE40FEB5E32AD2FC9B1F20"",
  ""tocAccessId"": ""5C2F61906CE3401EB8290E9197C0E5BE""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Language_8862B7367D7EDE109C7616727C2D07CE' type='CommandBarMenuButton'>5C2F61906CE3401EB8290E9197C0E5BE</id>
    <image>../images/Language.svg</image>
    <label xml:lang='en'>Languages</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Localization' type='CommandBarMenu'>D99D74772743438F938FB14C1710A84B</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Language_8862B7367D7EDE109C7616727C2D07CE</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>8192</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='69526C5AA7DB4B058904FD2F36261DB7'>
    <additional_data>{
  ""itemTypeId"": ""AC32527D85604A4D9FC9107C516AEF47"",
  ""tocAccessId"": ""69526C5AA7DB4B058904FD2F36261DB7""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Life Cycle Map_3B3EA119F0BA5D7537293D0AF65F542B' type='CommandBarMenuButton'>69526C5AA7DB4B058904FD2F36261DB7</id>
    <image>../images/LifeCycleMap.svg</image>
    <label xml:lang='en'>Life Cycle Maps</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Life Cycle Map_3B3EA119F0BA5D7537293D0AF65F542B</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>8320</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='8FE001A0D6254423A87A5D61453D8583'>
    <additional_data>{
  ""itemTypeId"": ""5736C479A8CB49BCA20138514C637266"",
  ""tocAccessId"": ""8FE001A0D6254423A87A5D61453D8583""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_List_7F63BDCDC4DF0E992A5E9C46E4CD94DF' type='CommandBarMenuButton'>8FE001A0D6254423A87A5D61453D8583</id>
    <image>../images/List.svg</image>
    <label xml:lang='en'>Lists</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_List_7F63BDCDC4DF0E992A5E9C46E4CD94DF</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>8448</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='115D215B9B8648E4BF0B7916E3A54FA6'>
    <additional_data>{
  ""itemTypeId"": ""137D24DFD9AC4D0CA2ABF8D90346AABB"",
  ""tocAccessId"": ""115D215B9B8648E4BF0B7916E3A54FA6""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Locale_B22DAD585205DC705FFB8DF892C31C37' type='CommandBarMenuButton'>115D215B9B8648E4BF0B7916E3A54FA6</id>
    <image>../images/Locale.svg</image>
    <label xml:lang='en'>Locales</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Localization' type='CommandBarMenu'>D99D74772743438F938FB14C1710A84B</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Locale_B22DAD585205DC705FFB8DF892C31C37</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>8576</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='14E68331884B4BACADBEEA3AA40D5908'>
    <additional_data>{
  ""itemTypeId"": ""CC2E5B8519EC48C5A3A0BDAC2A0842C3"",
  ""tocAccessId"": ""14E68331884B4BACADBEEA3AA40D5908""
}</additional_data>
    <id keyed_name='com.aras.innovator.mpp.toc_mpp_Location_FBA65C0530FEB51BB36CD608CB20E0CD' type='CommandBarMenuButton'>14E68331884B4BACADBEEA3AA40D5908</id>
    <image>../images/Location.svg</image>
    <label xml:lang='en'>Locations</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Portfolio' type='CommandBarMenu'>718FA406416E44369BE81C45C6C50360</parent_menu>
    <name>com.aras.innovator.mpp.toc_mpp_Location_FBA65C0530FEB51BB36CD608CB20E0CD</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>8704</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='B6F62BA3A1E94B18A29D40D319EAF49E'>
    <additional_data>{
  ""itemTypeId"": ""92C2C067865545BEA572C62FA896AD3D"",
  ""tocAccessId"": ""B6F62BA3A1E94B18A29D40D319EAF49E""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_mp_MacPolicy_CF9A2B136C389F4E34EDB50C8A002FBC' type='CommandBarMenuButton'>B6F62BA3A1E94B18A29D40D319EAF49E</id>
    <image>../images/MACPolicy.svg</image>
    <label xml:lang='en'>MAC Policies</label>
    <parent_menu keyed_name='com.aras.innovator.mp.toc_Access Control' type='CommandBarMenu'>6C773D9EE99446969C87E4DA10F8DF13</parent_menu>
    <name>com.aras.innovator.cui_default.toc_mp_MacPolicy_CF9A2B136C389F4E34EDB50C8A002FBC</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>8832</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='55195E148C3A4F548E8309E1755BBFD2'>
    <additional_data>{
  ""itemTypeId"": ""FDE9DA6F52C642EA807287C340EE0B72"",
  ""tocAccessId"": ""55195E148C3A4F548E8309E1755BBFD2""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_Manufacturer Part_6911FEF6DFC9C372B5BB6618BE2F6BFB' type='CommandBarMenuButton'>55195E148C3A4F548E8309E1755BBFD2</id>
    <image>../images/ManufacturerPart.svg</image>
    <label xml:lang='en'>Manufacturer Parts</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Sourcing' type='CommandBarMenu'>E0A77E86F2F6419AB7DEE7CFB0B28FC7</parent_menu>
    <name>com.aras.innovator.plm.toc_Manufacturer Part_6911FEF6DFC9C372B5BB6618BE2F6BFB</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>8960</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='D7194BE0426347DAA46C73F73D15A48C'>
    <additional_data>{
  ""itemTypeId"": ""3E71E373FC2940B288760C915120AABE"",
  ""tocAccessId"": ""D7194BE0426347DAA46C73F73D15A48C""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_Manufacturer_64F6A50BC4D19C99258FE5DC7C3B5E4F' type='CommandBarMenuButton'>D7194BE0426347DAA46C73F73D15A48C</id>
    <image>../images/Manufacturer.svg</image>
    <label xml:lang='en'>Manufacturers</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Sourcing' type='CommandBarMenu'>E0A77E86F2F6419AB7DEE7CFB0B28FC7</parent_menu>
    <name>com.aras.innovator.plm.toc_Manufacturer_64F6A50BC4D19C99258FE5DC7C3B5E4F</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>9088</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='1DFC992FA79F4CCD9C5C851678660828'>
    <additional_data>{
  ""itemTypeId"": ""5A549B2EDD3C4CBB9F8797902EE2EBE2"",
  ""tocAccessId"": ""1DFC992FA79F4CCD9C5C851678660828""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_mpo_MassPromotion_565CE5BA547C3A83E0D6B6B1EC0F3941' type='CommandBarMenuButton'>1DFC992FA79F4CCD9C5C851678660828</id>
    <image>../Images/MassPromote.svg</image>
    <label xml:lang='en'>Mass Promotions</label>
    <parent_menu keyed_name='com.aras.innovator.mpo.toc_Mass Operations' type='CommandBarMenu'>69E7F9371B8C4731815FA9618F6953C3</parent_menu>
    <name>com.aras.innovator.cui_default.toc_mpo_MassPromotion_565CE5BA547C3A83E0D6B6B1EC0F3941</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>9216</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='71A8F762E40A4D3F9CA42015CCC076ED'>
    <additional_data>{
  ""itemTypeId"": ""6AC851D83E4A465CAB4E06E164B905D8"",
  ""tocAccessId"": ""71A8F762E40A4D3F9CA42015CCC076ED""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Measurement Unit_AE13E0C85739262641DEAB25A7C67A6E' type='CommandBarMenuButton'>71A8F762E40A4D3F9CA42015CCC076ED</id>
    <image>../Images/MeasureDistanceBetweenFaces.svg</image>
    <label xml:lang='en'>Measurement Units</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Secure Social' type='CommandBarMenu'>6592CC9829F641ADB926EB4766B393A2</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Measurement Unit_AE13E0C85739262641DEAB25A7C67A6E</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>9344</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='DA4ED2ECD0EE423FAC4FCE75BDEF6BD3'>
    <additional_data>{
  ""itemTypeId"": ""87879A09B8044DE380D59DF22DE1867F"",
  ""tocAccessId"": ""DA4ED2ECD0EE423FAC4FCE75BDEF6BD3""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Method_50EFF4C94A43A9C84085F46F6D98472E' type='CommandBarMenuButton'>DA4ED2ECD0EE423FAC4FCE75BDEF6BD3</id>
    <image>../images/Method.svg</image>
    <label xml:lang='en'>Methods</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Method_50EFF4C94A43A9C84085F46F6D98472E</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>9472</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='6F09240485C74AC8BEBA3ADD0ED15EA7'>
    <additional_data>{
  ""itemTypeId"": ""A8C33F5606104501899F94D378878626"",
  ""tocAccessId"": ""6F09240485C74AC8BEBA3ADD0ED15EA7""
}</additional_data>
    <id keyed_name='com.aras.innovator.effs.toc_effs_model_5136CE7849E989DDA5506FFE5A48E157' type='CommandBarMenuButton'>6F09240485C74AC8BEBA3ADD0ED15EA7</id>
    <image>../images/DefaultItemType.svg</image>
    <label xml:lang='en'>Models</label>
    <parent_menu keyed_name='com.aras.innovator.effs.toc_Effectivity Services' type='CommandBarMenu'>740CAE5357F64959A6969D0909C66988</parent_menu>
    <name>com.aras.innovator.effs.toc_effs_model_5136CE7849E989DDA5506FFE5A48E157</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>9600</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='E951FB1AD5EBDFF4B0B100CF2210BFB9'>
    <additional_data>{
  ""itemTypeId"": ""3E85D9DD379643F8A207B99A9DFB72C2"",
  ""tocViewId"": ""03399AF3DF31469CBAB324F55C5A2139"",
  ""relatedTocAccessId"": ""D4E0B9DFA9024B3DA5A3C490083BF112"",
  ""startPage"": ""../Modules/aras.innovator.SSVC/Views/MyDiscussions.html""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Discussion_15467FD51B387080C5DA318616632001' type='CommandBarMenuButton'>E951FB1AD5EBDFF4B0B100CF2210BFB9</id>
    <image>../images/MyDiscussions.svg</image>
    <label xml:lang='en'>My Discussions</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_My Innovator' type='CommandBarMenu'>FA93FDCB24D245DE8A82B2234B1C95DB</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Discussion_15467FD51B387080C5DA318616632001</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>9728</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='9E11457725674139BD29F70D00E0BBDD'>
    <additional_data>{
  ""itemTypeId"": ""BC7977377FFF40D59FF14205914E9C71"",
  ""tocAccessId"": ""9E11457725674139BD29F70D00E0BBDD""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_InBasket Task_D0885536E2AC18CEEA9791FAD40A8BF0' type='CommandBarMenuButton'>9E11457725674139BD29F70D00E0BBDD</id>
    <image>../images/InBasketTask.svg</image>
    <label xml:lang='en'>My InBasket</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_My Innovator' type='CommandBarMenu'>FA93FDCB24D245DE8A82B2234B1C95DB</parent_menu>
    <name>com.aras.innovator.cui_default.toc_InBasket Task_D0885536E2AC18CEEA9791FAD40A8BF0</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>9856</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='67BC12DC7BBF9432C2A09F208659DE34'>
    <additional_data>{
  ""itemTypeId"": ""0B9D641B40D24036A117D911558CBDCE"",
  ""tocViewId"": ""2AE64118FE414EDBAE7FFB5109B2FA5D"",
  ""relatedTocAccessId"": ""864D6391ADAE4475AEB44AD994627505"",
  ""startPage"": ""../Modules/aras.innovator.izenda/MyReports""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_MyReports_A0C1FF1E6ED21FD182D78C48EB518C84' type='CommandBarMenuButton'>67BC12DC7BBF9432C2A09F208659DE34</id>
    <image>../Images/SelfServiceReporting.svg</image>
    <label xml:lang='en'>My Reports</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_My Innovator' type='CommandBarMenu'>FA93FDCB24D245DE8A82B2234B1C95DB</parent_menu>
    <name>com.aras.innovator.cui_default.toc_MyReports_A0C1FF1E6ED21FD182D78C48EB518C84</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>9984</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='7DB3750634474492BDE600935D873F52'>
    <additional_data>{
  ""itemTypeId"": ""E3B78447EB16474BBCCC55785B030828"",
  ""tocAccessId"": ""7DB3750634474492BDE600935D873F52""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_gn_NodeViewDefinition_76DE9DC5CAC2D9A8A9FB8CCCA5CED099' type='CommandBarMenuButton'>7DB3750634474492BDE600935D873F52</id>
    <image>../Images/NodeView.svg</image>
    <label xml:lang='en'>Node View Definitions</label>
    <parent_menu keyed_name='com.aras.innovator.gn.toc_GraphNavigation' type='CommandBarMenu'>324E4BAF108344D999C36AC6F0EB0FE7</parent_menu>
    <name>com.aras.innovator.cui_default.toc_gn_NodeViewDefinition_76DE9DC5CAC2D9A8A9FB8CCCA5CED099</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>10112</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='D3BB2787FEA24281B6A188600D30C2F0'>
    <additional_data>{
  ""itemTypeId"": ""F8F19489113C487B860733E7F7D5B12D"",
  ""tocAccessId"": ""D3BB2787FEA24281B6A188600D30C2F0""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Message_FB5D9956E2077CC09CD332CD17528B67' type='CommandBarMenuButton'>D3BB2787FEA24281B6A188600D30C2F0</id>
    <image>../images/Message.svg</image>
    <label xml:lang='en'>Notification Messages</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Notification' type='CommandBarMenu'>A7B3994B55FB409B887BD708ED4F856F</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Message_FB5D9956E2077CC09CD332CD17528B67</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>10240</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='FCA9E80EF2494B56B428BE89499C7701'>
    <additional_data>{
  ""itemTypeId"": ""F312562D6AD948DCBCCCCF6A615EE0EA"",
  ""tocAccessId"": ""FCA9E80EF2494B56B428BE89499C7701""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_PackageDefinition_3A1514099A70CA524D7097BD9850811D' type='CommandBarMenuButton'>FCA9E80EF2494B56B428BE89499C7701</id>
    <image>../images/PackageDefinition.svg</image>
    <label xml:lang='en'>PackageDefinitions</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Configuration' type='CommandBarMenu'>C599EA936834459F83CC0D3B09BF5856</parent_menu>
    <name>com.aras.innovator.cui_default.toc_PackageDefinition_3A1514099A70CA524D7097BD9850811D</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>10368</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='44BDF6CC677342CB9BA8E0D05ED88960'>
    <additional_data>{
  ""itemTypeId"": ""4F1AC04A2B484F3ABA4E20DB63808A88"",
  ""tocAccessId"": ""44BDF6CC677342CB9BA8E0D05ED88960""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_Part_F2A5F5BC0973025E292556EFFC7A9A11' type='CommandBarMenuButton'>44BDF6CC677342CB9BA8E0D05ED88960</id>
    <image>../images/Part.svg</image>
    <label xml:lang='en'>Parts</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Design' type='CommandBarMenu'>ED37E2A4E866447088D6EA16B3340FBD</parent_menu>
    <name>com.aras.innovator.plm.toc_Part_F2A5F5BC0973025E292556EFFC7A9A11</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>10496</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='0D70CB075E9F4F78B5770A992B77E13C'>
    <additional_data>{
  ""itemTypeId"": ""327998A4BAFF4CE4B29AD5AC4D37C0C1"",
  ""tocAccessId"": ""0D70CB075E9F4F78B5770A992B77E13C""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_PDFWMConfiguration_BCF9B682550D5B2105750CBF08D0C9D2' type='CommandBarMenuButton'>0D70CB075E9F4F78B5770A992B77E13C</id>
    <image>../Images/PDF.svg</image>
    <label xml:lang='en'>PDF Collections</label>
    <parent_menu keyed_name='com.aras.innovator.wm.toc_Watermarking' type='CommandBarMenu'>5DA51B9736D04B399B8839FD75D07F2A</parent_menu>
    <name>com.aras.innovator.cui_default.toc_PDFWMConfiguration_BCF9B682550D5B2105750CBF08D0C9D2</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>10624</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='9FB82FE0B9AC4B75856FE982D22E9B21'>
    <additional_data>{
  ""itemTypeId"": ""C6A89FDE1294451497801DF78341B473"",
  ""tocAccessId"": ""9FB82FE0B9AC4B75856FE982D22E9B21""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Permission_3919720D64E6C11DECA04358E391CE3E' type='CommandBarMenuButton'>9FB82FE0B9AC4B75856FE982D22E9B21</id>
    <image>../images/Permission.svg</image>
    <label xml:lang='en'>Permissions</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Permission_3919720D64E6C11DECA04358E391CE3E</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>10752</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='00D1FA735D474D4AAA3BC3ECE4FBE42B'>
    <additional_data>{
  ""itemTypeId"": ""7C63771EBC8D46FE8E902C5188033515"",
  ""tocAccessId"": ""00D1FA735D474D4AAA3BC3ECE4FBE42B""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Preference_1F1D069CBE674F92DABAC92B5004A43E' type='CommandBarMenuButton'>00D1FA735D474D4AAA3BC3ECE4FBE42B</id>
    <image>../images/Preference.svg</image>
    <label xml:lang='en'>Preferences</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Preference_1F1D069CBE674F92DABAC92B5004A43E</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>10880</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='F7DF98798D95417E8B25D9DE79B2C5DF'>
    <additional_data>{
  ""itemTypeId"": ""DF373D3D2CC1462D852C2BD420CCF5E2"",
  ""tocAccessId"": ""F7DF98798D95417E8B25D9DE79B2C5DF""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_Product_B4B0A963E90AB71F571EDF03E6894384' type='CommandBarMenuButton'>F7DF98798D95417E8B25D9DE79B2C5DF</id>
    <image>../images/Product.svg</image>
    <label xml:lang='en'>Products</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Design' type='CommandBarMenu'>ED37E2A4E866447088D6EA16B3340FBD</parent_menu>
    <name>com.aras.innovator.plm.toc_Product_B4B0A963E90AB71F571EDF03E6894384</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>11008</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='F8754FA4CBC34A89B5C32EEA02AC7406'>
    <additional_data>{
  ""itemTypeId"": ""881803D90C0B4F98A49A4732C16277F2"",
  ""tocAccessId"": ""F8754FA4CBC34A89B5C32EEA02AC7406""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_PR_3743EBFCEF473741B8F26F930B6AD165' type='CommandBarMenuButton'>F8754FA4CBC34A89B5C32EEA02AC7406</id>
    <image>../images/PR.svg</image>
    <label xml:lang='en'>PRs</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Change Management' type='CommandBarMenu'>64DF4B14BED84AF7B2EDE0EF3764EFBE</parent_menu>
    <name>com.aras.innovator.plm.toc_PR_3743EBFCEF473741B8F26F930B6AD165</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>11136</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='160801F2D6034518AB50B1233FF81A9B'>
    <additional_data>{
  ""itemTypeId"": ""0421B558114B4B22886C0F4206C923EE"",
  ""tocAccessId"": ""160801F2D6034518AB50B1233FF81A9B""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_qry_QueryDefinition_2F59B6EB4C7680D89015D29615D5D073' type='CommandBarMenuButton'>160801F2D6034518AB50B1233FF81A9B</id>
    <image>../Images/QueryDefinition.svg</image>
    <label xml:lang='en'>Query Definitions</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Configuration' type='CommandBarMenu'>C599EA936834459F83CC0D3B09BF5856</parent_menu>
    <name>com.aras.innovator.cui_default.toc_qry_QueryDefinition_2F59B6EB4C7680D89015D29615D5D073</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>11264</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='787EE646CE9A4A1BB2B9277C5B791FC5'>
    <additional_data>{
  ""itemTypeId"": ""471932C33B604C3099070F4106EE5024"",
  ""tocAccessId"": ""787EE646CE9A4A1BB2B9277C5B791FC5""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_RelationshipType_112B1B8933E1484B5F0FE140D0C2EA14' type='CommandBarMenuButton'>787EE646CE9A4A1BB2B9277C5B791FC5</id>
    <image>../images/RelationshipType.svg</image>
    <label xml:lang='en'>RelationshipTypes</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_RelationshipType_112B1B8933E1484B5F0FE140D0C2EA14</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>11392</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='C645CD822CEAB9BEE21C98B3AE50E3F3'>
    <additional_data>{
  ""itemTypeId"": ""2DAC2B407B0043A692905CF6A94296A8"",
  ""tocViewId"": ""F7CF6509800F4526B97A054EBC7A1A1C"",
  ""relatedTocAccessId"": ""0F63132D8B354553B0B83AF1D21AD86A""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_ReplicationTxn_8D6022B35C8ADFD8184943B88717E95E' type='CommandBarMenuButton'>C645CD822CEAB9BEE21C98B3AE50E3F3</id>
    <image>../images/ReplicationTxn.svg</image>
    <label xml:lang='en'>Replication Transactions</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</parent_menu>
    <name>com.aras.innovator.cui_default.toc_ReplicationTxn_8D6022B35C8ADFD8184943B88717E95E</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>11520</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='86F4F7F880CB00CA8BA612DD9BD79F1E'>
    <additional_data>{
  ""itemTypeId"": ""98FF7C1BFDFA43448B1EC5A95EA13AEA"",
  ""tocViewId"": ""2198E7776D67411DA3DA71144941DE99"",
  ""relatedTocAccessId"": ""879FF81F93F84C7396785113438452F2""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_ReplicationTxnLog_D545C0A3D4AFF64CD3378F1EC86501EB' type='CommandBarMenuButton'>86F4F7F880CB00CA8BA612DD9BD79F1E</id>
    <image>../images/ReplicationTxnLog.svg</image>
    <label xml:lang='en'>Replication Transactions Log</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</parent_menu>
    <name>com.aras.innovator.cui_default.toc_ReplicationTxnLog_D545C0A3D4AFF64CD3378F1EC86501EB</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>11648</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='FA6AAA517C914F8294CC0969F8451C9C'>
    <additional_data>{
  ""itemTypeId"": ""F0834BBA6FB64394B78DF5BB725532DD"",
  ""tocAccessId"": ""FA6AAA517C914F8294CC0969F8451C9C""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Report_B0865CC959BE9EFB6071A1E80C922810' type='CommandBarMenuButton'>FA6AAA517C914F8294CC0969F8451C9C</id>
    <image>../images/Report.svg</image>
    <label xml:lang='en'>Reports</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Report_B0865CC959BE9EFB6071A1E80C922810</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>11776</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='67707D7C4A2F4F2F8C175F09D2697345'>
    <additional_data>{
  ""itemTypeId"": ""F81CDEF9FE324D01947CC9023BC38317"",
  ""tocAccessId"": ""67707D7C4A2F4F2F8C175F09D2697345""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Revision_CC30B0206CFAF516BA5E0776F7691847' type='CommandBarMenuButton'>67707D7C4A2F4F2F8C175F09D2697345</id>
    <image>../images/Revision.svg</image>
    <label xml:lang='en'>Revisions</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Revision_CC30B0206CFAF516BA5E0776F7691847</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>11904</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='3E610CFE071B41678ECFF240869B57C1'>
    <additional_data>{
  ""itemTypeId"": ""255AACBE895E4ADFA79BE85F3C8BCDEC"",
  ""tocAccessId"": ""3E610CFE071B41678ECFF240869B57C1""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_PE_Rework Order_DDC6BB6A6C6FC1424C0436FBD7D515DE' type='CommandBarMenuButton'>3E610CFE071B41678ECFF240869B57C1</id>
    <image>../Images/ReworkOrder.svg</image>
    <label xml:lang='en'>Rework Orders</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Change Management' type='CommandBarMenu'>64DF4B14BED84AF7B2EDE0EF3764EFBE</parent_menu>
    <name>com.aras.innovator.plm.toc_PE_Rework Order_DDC6BB6A6C6FC1424C0436FBD7D515DE</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>12032</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='D5F9C27D426049CDA78C134E34A83A8D'>
    <additional_data>{
  ""itemTypeId"": ""18C15AB147F84834874F2E0CB6B8B4C0"",
  ""tocAccessId"": ""D5F9C27D426049CDA78C134E34A83A8D""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_SavedSearch_0026FE3193532450E1803EAC13E98620' type='CommandBarMenuButton'>D5F9C27D426049CDA78C134E34A83A8D</id>
    <image>../images/SavedSearch.svg</image>
    <label xml:lang='en'>Saved Searches</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_SavedSearch_0026FE3193532450E1803EAC13E98620</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>12160</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='C368E034B5B0413F804CBB5BFB028C6A'>
    <additional_data>{
  ""itemTypeId"": ""A46890D3535C41D4A5D79240B8C373B0"",
  ""tocAccessId"": ""C368E034B5B0413F804CBB5BFB028C6A""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_SelfServiceReport_E2542A037FB8128E9250ECDB1921EB84' type='CommandBarMenuButton'>C368E034B5B0413F804CBB5BFB028C6A</id>
    <image>../Images/SelfServiceReporting.svg</image>
    <label xml:lang='en'>Self-Service Reports</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_SelfServiceReport_E2542A037FB8128E9250ECDB1921EB84</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>12288</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='F0E403BE703042C391A675FF184A65D3'>
    <additional_data>{
  ""itemTypeId"": ""2B46201802CE46708C269667DB4798AC"",
  ""tocAccessId"": ""F0E403BE703042C391A675FF184A65D3""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Sequence_37CF802F22BFA518EDBFC97C40C4141D' type='CommandBarMenuButton'>F0E403BE703042C391A675FF184A65D3</id>
    <image>../images/Sequence.svg</image>
    <label xml:lang='en'>Sequences</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Sequence_37CF802F22BFA518EDBFC97C40C4141D</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>12416</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='8544109E5DB649BAA1AF30EAF0D2D4A5'>
    <additional_data>{
  ""itemTypeId"": ""FC3E32F18F804FD9BE4B175973D29112"",
  ""tocAccessId"": ""8544109E5DB649BAA1AF30EAF0D2D4A5""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_SQL_91703A3BD4A4913895416234FD90BB20' type='CommandBarMenuButton'>8544109E5DB649BAA1AF30EAF0D2D4A5</id>
    <image>../images/SQL.svg</image>
    <label xml:lang='en'>SQLs</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_SQL_91703A3BD4A4913895416234FD90BB20</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>12544</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='19CA7EC57ED4411D8A0256DB6295F536'>
    <additional_data>{
  ""itemTypeId"": ""BB9C415B65E0480C96C99C07EB3FCA88"",
  ""tocAccessId"": ""19CA7EC57ED4411D8A0256DB6295F536""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_gn_SystemDefaultCard_1B4679EBAF4177A164BC5B612A6713DA' type='CommandBarMenuButton'>19CA7EC57ED4411D8A0256DB6295F536</id>
    <image>../Images/SystemDefaultCard.svg</image>
    <label xml:lang='en'>System Default Cards</label>
    <parent_menu keyed_name='com.aras.innovator.gn.toc_GraphNavigation' type='CommandBarMenu'>324E4BAF108344D999C36AC6F0EB0FE7</parent_menu>
    <name>com.aras.innovator.cui_default.toc_gn_SystemDefaultCard_1B4679EBAF4177A164BC5B612A6713DA</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>12672</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='85CAF1D138314958977D3030FEBA7A94'>
    <additional_data>{
  ""itemTypeId"": ""B30032741C894BB086148DDB551D3BEE"",
  ""tocAccessId"": ""85CAF1D138314958977D3030FEBA7A94""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_SystemEvent_8AB3D9B6F99C2C4A0DE85606135BAF07' type='CommandBarMenuButton'>85CAF1D138314958977D3030FEBA7A94</id>
    <image>../images/SystemEvent.svg</image>
    <label xml:lang='en'>System Events</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_System Events' type='CommandBarMenu'>BE0A63FCB9034FFB84A2FE4867AC0382</parent_menu>
    <name>com.aras.innovator.cui_default.toc_SystemEvent_8AB3D9B6F99C2C4A0DE85606135BAF07</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>12800</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='7F914E1F521A4B358F851AA14357E7FE'>
    <additional_data>{
  ""itemTypeId"": ""2F4E2B53BFBA4351BFFCEE0E438ECF97"",
  ""tocAccessId"": ""7F914E1F521A4B358F851AA14357E7FE""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_SystemEventLog_E4CA4A63710B72A336429415815F82A3' type='CommandBarMenuButton'>7F914E1F521A4B358F851AA14357E7FE</id>
    <image>../images/SystemEventLog.svg</image>
    <label xml:lang='en'>System Events Log</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_System Events' type='CommandBarMenu'>BE0A63FCB9034FFB84A2FE4867AC0382</parent_menu>
    <name>com.aras.innovator.cui_default.toc_SystemEventLog_E4CA4A63710B72A336429415815F82A3</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>12928</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='A85B75C2CA9A4CCAB2BE29E2DBDFA708'>
    <additional_data>{
  ""itemTypeId"": ""FF53A19A424D4B2F80938A5A5C1A29EA"",
  ""tocAccessId"": ""A85B75C2CA9A4CCAB2BE29E2DBDFA708""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_SystemEventLogDescriptor_C3595E3B394F6F5DA37746E16372374C' type='CommandBarMenuButton'>A85B75C2CA9A4CCAB2BE29E2DBDFA708</id>
    <image>../images/SystemEventLogDescriptor.svg</image>
    <label xml:lang='en'>System Events Log Descriptor</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_System Events' type='CommandBarMenu'>BE0A63FCB9034FFB84A2FE4867AC0382</parent_menu>
    <name>com.aras.innovator.cui_default.toc_SystemEventLogDescriptor_C3595E3B394F6F5DA37746E16372374C</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>13056</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='1116CDDEEFC747139BD2AF4290B76F7C'>
    <additional_data>{
  ""itemTypeId"": ""CC23F9130F574E7D99DF9659F27590A6"",
  ""tocAccessId"": ""1116CDDEEFC747139BD2AF4290B76F7C""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Team_6307F26260FFCB6BE775E93DF527F9DA' type='CommandBarMenuButton'>1116CDDEEFC747139BD2AF4290B76F7C</id>
    <image>../images/Team.svg</image>
    <label xml:lang='en'>Teams</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Team_6307F26260FFCB6BE775E93DF527F9DA</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>13184</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='C81974F4A1AB4109973761F6D8BCE389'>
    <additional_data>{
  ""itemTypeId"": ""6AFE8A9127ED48FFB2F9183B9922981B"",
  ""tocAccessId"": ""C81974F4A1AB4109973761F6D8BCE389""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_rb_TreeGridViewDefinition_4B7181E69DE555EDF44F2D7167052885' type='CommandBarMenuButton'>C81974F4A1AB4109973761F6D8BCE389</id>
    <image>../Images/TreeGridView.svg</image>
    <label xml:lang='en'>Tree Grid Views</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Configuration' type='CommandBarMenu'>C599EA936834459F83CC0D3B09BF5856</parent_menu>
    <name>com.aras.innovator.cui_default.toc_rb_TreeGridViewDefinition_4B7181E69DE555EDF44F2D7167052885</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>13312</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='B980495A990945F796F953CFA19CC367'>
    <additional_data>{
  ""itemTypeId"": ""CBAADF17BF494B62AA5604AFEB9AF7C2"",
  ""tocAccessId"": ""B980495A990945F796F953CFA19CC367""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_uvf_UserVisibilityFilter_091F1A16531C2C192D5773114BA35806' type='CommandBarMenuButton'>B980495A990945F796F953CFA19CC367</id>
    <image>../Images/UserVisibilityRule.svg</image>
    <label xml:lang='en'>User Visibility Rules</label>
    <parent_menu keyed_name='com.aras.innovator.mp.toc_Access Control' type='CommandBarMenu'>6C773D9EE99446969C87E4DA10F8DF13</parent_menu>
    <name>com.aras.innovator.cui_default.toc_uvf_UserVisibilityFilter_091F1A16531C2C192D5773114BA35806</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>13440</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='2A24A48D26EB490FB8E1F5F5B7B376F2'>
    <additional_data>{
  ""itemTypeId"": ""45E899CD2859442982EB22BB2DF683E5"",
  ""tocAccessId"": ""2A24A48D26EB490FB8E1F5F5B7B376F2""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_User_BE8EC3B98AA73EDF1A63F5634C7976F9' type='CommandBarMenuButton'>2A24A48D26EB490FB8E1F5F5B7B376F2</id>
    <image>../images/User.svg</image>
    <label xml:lang='en'>Users</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_User_BE8EC3B98AA73EDF1A63F5634C7976F9</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>13568</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='F6F0D67828BD4E6E90834901955B50DE'>
    <additional_data>{
  ""itemTypeId"": ""6DAB4ACC09E6471DB4BDD15F36C3482B"",
  ""tocAccessId"": ""F6F0D67828BD4E6E90834901955B50DE""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Variable_2EE4CCF1B8D80D5841281643BFC62E65' type='CommandBarMenuButton'>F6F0D67828BD4E6E90834901955B50DE</id>
    <image>../images/Variable.svg</image>
    <label xml:lang='en'>Variables</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Variable_2EE4CCF1B8D80D5841281643BFC62E65</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>13696</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='1F4D94CF0F3844DC87604656D5968313'>
    <additional_data>{
  ""itemTypeId"": ""8FC29FEF933641A09CEE13A604A9DC74"",
  ""tocAccessId"": ""1F4D94CF0F3844DC87604656D5968313""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Vault_487806C4643A429CDEB8C3A42931D709' type='CommandBarMenuButton'>1F4D94CF0F3844DC87604656D5968313</id>
    <image>../images/Vault.svg</image>
    <label xml:lang='en'>Vaults</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Vault_487806C4643A429CDEB8C3A42931D709</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>13824</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='192F3C0AA5E142888DDC300657E297A4'>
    <additional_data>{
  ""itemTypeId"": ""9F23A02D1E464F44A3A93A1EFB732B5C"",
  ""tocAccessId"": ""192F3C0AA5E142888DDC300657E297A4""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_Vendor_60610D6E25FCBAB3B88AAB36AC3312FF' type='CommandBarMenuButton'>192F3C0AA5E142888DDC300657E297A4</id>
    <image>../images/Vendor.svg</image>
    <label xml:lang='en'>Vendors</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Sourcing' type='CommandBarMenu'>E0A77E86F2F6419AB7DEE7CFB0B28FC7</parent_menu>
    <name>com.aras.innovator.plm.toc_Vendor_60610D6E25FCBAB3B88AAB36AC3312FF</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>13952</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='D36539B1A334403496A9DA60D6667677'>
    <additional_data>{
  ""itemTypeId"": ""B5B1E4E180CF43A986D05ED063EA7D67"",
  ""tocAccessId"": ""D36539B1A334403496A9DA60D6667677""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_gn_ViewCard_0CDAB973163B7632C458D7ABE6116448' type='CommandBarMenuButton'>D36539B1A334403496A9DA60D6667677</id>
    <image>../Images/ViewCard.svg</image>
    <label xml:lang='en'>View Cards</label>
    <parent_menu keyed_name='com.aras.innovator.gn.toc_GraphNavigation' type='CommandBarMenu'>324E4BAF108344D999C36AC6F0EB0FE7</parent_menu>
    <name>com.aras.innovator.cui_default.toc_gn_ViewCard_0CDAB973163B7632C458D7ABE6116448</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>14080</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='F3B07129B2B84565800105369A3D1A21'>
    <additional_data>{
  ""itemTypeId"": ""602D9828174C48EBA648B1D261C54E43"",
  ""tocAccessId"": ""F3B07129B2B84565800105369A3D1A21""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Viewer_47C8C546F3D3058252221EA105C2BEC2' type='CommandBarMenuButton'>F3B07129B2B84565800105369A3D1A21</id>
    <image>../images/Viewer.svg</image>
    <label xml:lang='en'>Viewers</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_File Handling' type='CommandBarMenu'>D857FFD536F741DEA5778F4F8252A762</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Viewer_47C8C546F3D3058252221EA105C2BEC2</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>14208</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='15DC660B50B6433F8CFC5ABE21C14815'>
    <additional_data>{
  ""itemTypeId"": ""CD84AA40803F444CBF06843D26F6EC6A"",
  ""tocAccessId"": ""15DC660B50B6433F8CFC5ABE21C14815""
}</additional_data>
    <id keyed_name='com.aras.innovator.plm.toc_PE_Waiver_B2172EC8D11C6FCD64724640A84C6862' type='CommandBarMenuButton'>15DC660B50B6433F8CFC5ABE21C14815</id>
    <image>../Images/Waiver.svg</image>
    <label xml:lang='en'>Waivers</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Change Management' type='CommandBarMenu'>64DF4B14BED84AF7B2EDE0EF3764EFBE</parent_menu>
    <name>com.aras.innovator.plm.toc_PE_Waiver_B2172EC8D11C6FCD64724640A84C6862</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>14336</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='7BFAFD1F718D41C7B3F8EC4ADFC6B3F4'>
    <additional_data>{
  ""itemTypeId"": ""14479E1ADA444096B3116018A0012EB3"",
  ""tocAccessId"": ""7BFAFD1F718D41C7B3F8EC4ADFC6B3F4""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_WMSettings_68B480C508D9D23D3C7261180E7140AF' type='CommandBarMenuButton'>7BFAFD1F718D41C7B3F8EC4ADFC6B3F4</id>
    <image>../Images/WatermarkRule.svg</image>
    <label xml:lang='en'>Watermark Rules</label>
    <parent_menu keyed_name='com.aras.innovator.wm.toc_Watermarking' type='CommandBarMenu'>5DA51B9736D04B399B8839FD75D07F2A</parent_menu>
    <name>com.aras.innovator.cui_default.toc_WMSettings_68B480C508D9D23D3C7261180E7140AF</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>14464</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='8242F6B2AD514849B0936C02CFF33658'>
    <additional_data>{
  ""itemTypeId"": ""08CC14292189425B9FBC68937CC5BD33"",
  ""tocAccessId"": ""8242F6B2AD514849B0936C02CFF33658""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_WMSet_B5CDB6407E9329FEF88C5BD929E36F14' type='CommandBarMenuButton'>8242F6B2AD514849B0936C02CFF33658</id>
    <image>../Images/WatermarkSet.svg</image>
    <label xml:lang='en'>Watermark Sets</label>
    <parent_menu keyed_name='com.aras.innovator.wm.toc_Watermarking' type='CommandBarMenu'>5DA51B9736D04B399B8839FD75D07F2A</parent_menu>
    <name>com.aras.innovator.cui_default.toc_WMSet_B5CDB6407E9329FEF88C5BD929E36F14</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>14592</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='75A2A5E6EDD649C3B6E031A00C500D9E'>
    <additional_data>{
  ""itemTypeId"": ""C8BC1FE465374CE290C61FCFEA67F4DB"",
  ""tocAccessId"": ""75A2A5E6EDD649C3B6E031A00C500D9E""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_WMType_7ACCF04D78F155EFE5DD84BFED7401A4' type='CommandBarMenuButton'>75A2A5E6EDD649C3B6E031A00C500D9E</id>
    <image>../Images/WatermarkType.svg</image>
    <label xml:lang='en'>Watermark Types</label>
    <parent_menu keyed_name='com.aras.innovator.wm.toc_Watermarking' type='CommandBarMenu'>5DA51B9736D04B399B8839FD75D07F2A</parent_menu>
    <name>com.aras.innovator.cui_default.toc_WMType_7ACCF04D78F155EFE5DD84BFED7401A4</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>14720</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='633CA39561254C18ADE8BB0CC0F9F218'>
    <additional_data>{
  ""itemTypeId"": ""B19D349CC6FC44BC97D50A6D70AE79CB"",
  ""tocAccessId"": ""633CA39561254C18ADE8BB0CC0F9F218""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Workflow Map_71950009E01A556ED3758A878356DB5B' type='CommandBarMenuButton'>633CA39561254C18ADE8BB0CC0F9F218</id>
    <image>../images/WorkflowMap.svg</image>
    <label xml:lang='en'>Workflow Maps</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Workflow Map_71950009E01A556ED3758A878356DB5B</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>14848</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='80186A97BAC34BC499509BB3000E32D9'>
    <additional_data>{
  ""itemTypeId"": ""261EAC08AE9144FC95C49182ACE0D3FE"",
  ""tocAccessId"": ""80186A97BAC34BC499509BB3000E32D9""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Workflow Process_9C96AE4CA4126E7FB5279D059B3BE02B' type='CommandBarMenuButton'>80186A97BAC34BC499509BB3000E32D9</id>
    <image>../images/WorkflowProcess.svg</image>
    <label xml:lang='en'>Workflow Processes</label>
    <parent_menu keyed_name='com.aras.innovator.cui_default.toc_Administration' type='CommandBarMenu'>ED137389E93048819E68DD364EDA1322</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Workflow Process_9C96AE4CA4126E7FB5279D059B3BE02B</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>14976</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='75181059737F444AA1B06F289AE5443A'>
    <additional_data>{
  ""itemTypeId"": ""BE722B4824314594AA4AFC3CC9A18CCA"",
  ""tocAccessId"": ""75181059737F444AA1B06F289AE5443A""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_xClassificationTree_9A09A08854AA82494033366B1F155D0C' type='CommandBarMenuButton'>75181059737F444AA1B06F289AE5443A</id>
    <image>../Images/xClassification.svg</image>
    <label xml:lang='en'>xClassification Trees</label>
    <parent_menu keyed_name='com.aras.innovator.xpxc.toc_Extended Classification' type='CommandBarMenu'>4D83ED9DD681459A94590FE16F64B962</parent_menu>
    <name>com.aras.innovator.cui_default.toc_xClassificationTree_9A09A08854AA82494033366B1F155D0C</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>15104</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='43D97743C5A2452597EBC1BB5C27219D'>
    <additional_data>{
  ""itemTypeId"": ""C81D09F315EF4099B4A2F597C37EEEC0"",
  ""tocAccessId"": ""43D97743C5A2452597EBC1BB5C27219D""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_xPropertyDefinition_48D4EEA620E2DBE46793015B6AD21EA3' type='CommandBarMenuButton'>43D97743C5A2452597EBC1BB5C27219D</id>
    <image>../Images/xProperty.svg</image>
    <label xml:lang='en'>xProperties</label>
    <parent_menu keyed_name='com.aras.innovator.xpxc.toc_Extended Classification' type='CommandBarMenu'>4D83ED9DD681459A94590FE16F64B962</parent_menu>
    <name>com.aras.innovator.cui_default.toc_xPropertyDefinition_48D4EEA620E2DBE46793015B6AD21EA3</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>15232</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
  <Item type='CommandBarMenuButton' typeId='51F1815885E3477D8B6385A69236C5AA' id='91F9A0F03C1F40B7963EC31F8F8D4DDD'>
    <additional_data>{
  ""itemTypeId"": ""749B727E891F4005A82F84ACD6A803AB"",
  ""tocAccessId"": ""91F9A0F03C1F40B7963EC31F8F8D4DDD""
}</additional_data>
    <id keyed_name='com.aras.innovator.cui_default.toc_Permission_PropertyValue_7A3685210702B33209803AD7B4FA6CC2' type='CommandBarMenuButton'>91F9A0F03C1F40B7963EC31F8F8D4DDD</id>
    <image>../Images/xPropertyValuePermission.svg</image>
    <label xml:lang='en'>xProperty Value Permissions</label>
    <parent_menu keyed_name='com.aras.innovator.xpxc.toc_Extended Classification' type='CommandBarMenu'>4D83ED9DD681459A94590FE16F64B962</parent_menu>
    <name>com.aras.innovator.cui_default.toc_Permission_PropertyValue_7A3685210702B33209803AD7B4FA6CC2</name>
    <itemtype>51F1815885E3477D8B6385A69236C5AA</itemtype>
    <sort_order>15360</sort_order>
    <section keyed_name='com.aras.innovator.cui_default.toc'>0AEAF671F8E548C494CCEC8E13F99562</section>
    <item_classification>
    </item_classification>
  </Item>
</AML>";
  }
}
