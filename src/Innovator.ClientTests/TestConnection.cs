using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Innovator.Client.Tests
{
  internal class TestConnection : IConnection
  {
    public Command LastRequest { get; set; }

    public ElementFactory AmlContext { get { return ElementFactory.Local; } }

    public string Database { get { return "Test"; } }

    public string UserId { get { return "2D246C5838644C1C8FD34F8D2796E327"; } }

    public UploadCommand CreateUploadCommand()
    {
      throw new NotSupportedException();
    }

    public string MapClientUrl(string relativeUrl)
    {
      return relativeUrl;
    }

    public Stream Process(Command request)
    {
      LastRequest = request;
      var elem = XElement.Parse(request.ToNormalizedAml(this.AmlContext.LocalizationContext));
      var result = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <SOAP-ENV:Fault xmlns:af='http://www.aras.com/InnovatorFault'>
      <faultcode>0</faultcode>
      <faultstring>No items of type found.</faultstring>
      <detail>
        <af:legacy_detail>No items of type found.</af:legacy_detail>
        <af:legacy_faultstring>No items of type '' found using the criteria: </af:legacy_faultstring>
      </detail>
    </SOAP-ENV:Fault>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";

      if (request.Action == CommandAction.GetIdentityList)
      {
        result = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9,1942894C56164DF4AB400434FF5EFE3B,56A96DA9E981481688563E2D14D5D878,F18B4A8C523A4B3F88CFFEE3BC68F5D4,2618D6F5A90949BAA7E920D1B04C7EE1,43CC1A53BECA4364A22C136E98E185C7,5A8E8A27AA5A47238BD96AC60A6562F3,5E47C25D3FE6464688442ABE64478537,08C52237B1124FC4BB5D4E48F660C8E2,22DF5704DCA14D86B79BFDFF3CA030B9,2C5CF38CB45C4E1F848EA6DF969364EF,31387021B15E42548FC818E893A19D08,344632A16E8949CCA5CAD0A013D50655,3A0D61E9629141C8934B948C596B2E42,3A976285BB9A4D05AE1878004A33D440,484451EB7B524ECD92FFA372A4675E38,4CAAF215654F43098B415EC0EAB1829A,4CD258EF03834E7FAC9CD76876777EBC,5E12CD824411477AA56D9F539FA295EB,600B965EA3DB41D0B7F9D9D7BDD923EC,62E1A4258C764C5E814DDA3B4209A34F,66022731E81D4040A155F13CD083826A,66F68F2DC7D3410699EA302411608618,685069C4626B406BA5EEFBD87CCABBAC,7C63ED1CFD0A4E0DAAD27AAB0C90A58D,7F50E4A530A84C22935ED7E2231198E1,82F4AA3DD22648288D63938D648CD814,84D2CEC2D1E247DC9D2D28B78D1DF591,A11E92826CA44BEEA4A84E494D13F0B8,A4AFDB4EBD2A4130A4D0616E2E63F552,AD55DC9C04274E67A2C4C45AADC9D059,B3DC39013C13458F9D3A479046B25266,BFE82C27CAAB4667B51EEC64D818BEC8,C02E1CEE4C2B492496EAEC1C19F814B0,C3DB96EFB5EC47AE816CE16AE3E79959,C5C35BB6D73145DB8C88084BEA96BB1D,C6D220539A8D4458B27CA579B637967F,C8FE09099D8D4C25BED39BBFE3BF0476,D78B07BAF283495E9540115C00AD9CED,DE6E87399974415F948D4468C07C5134,E42675D6FC354C86880230D5DA79E4CD,FFB8C4C174054BD3AA17A00023AD49BC,25EAA8CA05254E1DB191F984D994751D,5D58B17DB5094950B93AE13202E46C5E,6A2F0C9740B74625B744E5FCE43879F6,7FA8E5852B7F4FE1817C24A11798A784,B32BD81D1AD04207BF1E61E39A4E0E13,FA8573065D534FBB8DA1E40C14FA54EF,0CDBE4C0F8A045159339D1C8BE8665FA,31E5C20B933D433CA8F509F7AED9E68A,5E7DC598214E4BBAA09CDB91D644FABD,78A5819F71E2414EB48C5984AB005ABB,988240C797D548A79A78B1AD4CDED7B3,3FCE6B2163FC4F5983BF00A13E6B047E</Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      }
      else if (AttrEquals(elem, "action", "get"))
      {
        switch (AttrValue(elem, "type"))
        {
          case "Company":
            if (AttrEquals(elem, "id", "0E086FFA6C4646F6939B74C43D094182"))
            {
              result = @"<Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='0E086FFA6C4646F6939B74C43D094182'>
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
            }
            break;
          case "User":
            if (AttrEquals(elem, "id", "8227040ABF0A46A8AF06C18ABD3967B3"))
            {
              result = @"<Result><Item type='User' typeId='45E899CD2859442982EB22BB2DF683E5' id='8227040ABF0A46A8AF06C18ABD3967B3'>
  <id keyed_name='First Last' type='User'>8227040ABF0A46A8AF06C18ABD3967B3</id>
  <first_name>First</first_name>
  <itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype>
</Item></Result>";
            }
            break;
          case "Permission":
            if (AttrEquals(elem, "id", "A8FC3EC44ED0462B9A32D4564FAC0AD8"))
            {
              result = @"<Result><Item type='Permission' typeId='C6A89FDE1294451497801DF78341B473' id='A8FC3EC44ED0462B9A32D4564FAC0AD8'>
  <id keyed_name='Company' type='Permission'>A8FC3EC44ED0462B9A32D4564FAC0AD8</id>
  <name>Company</name>
</Item></Result>";
            }
            break;
          case "ItemType":
            result = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>
      <Item type='ItemType' typeId='450906E86E304F55A34B3C0D65C097EA' id='4F1AC04A2B484F3ABA4E20DB63808A88'>
        <allow_private_permission>1</allow_private_permission>
        <auto_search>0</auto_search>
        <config_id keyed_name='Part' type='ItemType' name='Part'>4F1AC04A2B484F3ABA4E20DB63808A88</config_id>
        <core>0</core>
        <created_by_id keyed_name='Super User' type='User'>AD30A6D8D3B642F5A2AFED1A4B02BEFA</created_by_id>
        <created_on>2010-04-06T12:41:29</created_on>
        <default_page_size>30</default_page_size>
        <enforce_discovery>1</enforce_discovery>
        <generation>1</generation>
        <help_url xml:lang='en'>mergedProjects/ProductEngineering/plm_Parts.htm</help_url>
        <hide_where_used>0</hide_where_used>
        <history_template keyed_name='Default' type='History Template'>3BC16EF9E52B4F9792AB76BCE0492F29</history_template>
        <id keyed_name='Part' type='ItemType'>4F1AC04A2B484F3ABA4E20DB63808A88</id>
        <implementation_type>table</implementation_type>
        <instance_data>PART</instance_data>
        <is_cached>0</is_cached>
        <is_current>1</is_current>
        <is_dependent>0</is_dependent>
        <is_relationship>0</is_relationship>
        <is_released>0</is_released>
        <is_versionable>1</is_versionable>
        <keyed_name>Part</keyed_name>
        <label xml:lang='en'>Part</label>
        <label_plural xml:lang='en'>Parts</label_plural>
        <major_rev>A</major_rev>
        <manual_versioning>1</manual_versioning>
        <maxrecords>1000</maxrecords>
        <new_version>0</new_version>
        <not_lockable>0</not_lockable>
        <permission_id keyed_name='ItemType' type='Permission'>102D29B8CD9948BFB5F558341DF4C0F9</permission_id>
        <show_parameters_tab>1</show_parameters_tab>
        <structure_view>tab view</structure_view>
        <unlock_on_logout>0</unlock_on_logout>
        <use_src_access>0</use_src_access>
        <name>Part</name>
      </Item>
    </Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
            break;
        }
      }

      return new MemoryStream(Encoding.UTF8.GetBytes(result));
    }

    private string AttrValue(XElement elem, string name)
    {
      return elem.Attribute(name) == null ? null : elem.Attribute(name).Value;
    }
    private bool AttrEquals(XElement elem, string name, string value)
    {
      return elem.Attribute(name) != null && string.Equals(elem.Attribute(name).Value, value);
    }

    public ExplicitHashCredentials HashCredentials(ICredentials credentials)
    {
      throw new NotImplementedException();
    }
  }
}
