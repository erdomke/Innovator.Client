using Innovator.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

namespace Innovator.Client.Tests
{
  class TestConnection : IConnection
  {
    public Command LastRequest { get; set; }

    public ElementFactory AmlContext { get { return ElementFactory.Local; } }

    public string Database { get { return "Test"; } }

    public string UserId { get { return "2D246C5838644C1C8FD34F8D2796E327"; } }

    public UploadCommand CreateUploadCommand()
    {
      throw new NotImplementedException();
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
      if (AttrEquals(elem, "action", "get"))
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
  }
}
