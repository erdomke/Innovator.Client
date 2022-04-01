using Innovator.Client.Connection;
using Innovator.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Innovator.Client.Tests
{
  internal class TestConnection : IArasConnection, IServerConnection
  {
    private readonly ArasVaultConnection _vaultConn;

    public Command LastRequest { get; set; }

    public ElementFactory AmlContext { get { return ElementFactory.Local; } }

    public string Database { get { return "Test"; } }

    public string UserId { get { return "2D246C5838644C1C8FD34F8D2796E327"; } }

    public List<Action<IHttpRequest>> DefaultSettings => new List<Action<IHttpRequest>>();

    public CompressionType Compression => CompressionType.none;

    public Version Version => new Version(11, 0);

    public Action<string, string> QueryCallback { get; set; }

    public string DefaultResponse { get; set; } = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
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

    public IServerCache ApplicationCache => throw new NotImplementedException();

    public string OriginalRequest => throw new NotImplementedException();

    public IServerPermissions Permissions => throw new NotImplementedException();

    public IServerCache RequestState => throw new NotImplementedException();

    public string RequestUrl => throw new NotImplementedException();

    public IServerCache SessionCache => throw new NotImplementedException();

    public TestConnection()
    {
      _vaultConn = new ArasVaultConnection(this);
      _vaultConn.InitializeStrategy();
    }

    public UploadCommand CreateUploadCommand()
    {
      return _vaultConn.CreateUploadCommand();
    }

    public string MapClientUrl(string relativeUrl)
    {
      return relativeUrl;
    }

    public Stream Process(Command request)
    {
      if (request is UploadCommand)
        return _vaultConn.Upload((UploadCommand)request, false).Wait();

      LastRequest = request;
      var elem = XElement.Parse(request.ToNormalizedAml(this.AmlContext.LocalizationContext));
      QueryCallback?.Invoke(request.ActionString, elem.ToString(SaveOptions.DisableFormatting));
      var result = DefaultResponse;

      if (request.Action == CommandAction.GetIdentityList)
      {
        result = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
  <SOAP-ENV:Body>
    <Result>F13AF7BC3D7A4084AF67AB7BF938C409,A73B655731924CD0B027E4F4D5FCC0A9</Result>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>";
      }
      else if (AttrEquals(elem, "action", "get"))
      {
        switch (AttrValue(elem, "type"))
        {
          case "Company":
            if (AttrEquals(elem, "id", "0E086FFA6C4646F6939B74C43D094182")
              || elem.Element("keyed_name")?.Value == "Another Company")
            {
              result = @"<Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='0E086FFA6C4646F6939B74C43D094182'>
  <keyed_name>Another Company</keyed_name>
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
  <owned_by_id>44CC39EB107F4C02884AFF66A478202D</owned_by_id>
</Item>";
            }
            else if (AttrEquals(elem, "id", "1470B001142748A5BB39CECB72CD83C8"))
            {
              result = @"<Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='1470B001142748A5BB39CECB72CD83C8'>
  <created_by_id keyed_name='First Last' type='User'>
    <Item type='User' typeId='45E899CD2859442982EB22BB2DF683E5' id='8227040ABF0A46A8AF06C18ABD3967B3'>
      <id keyed_name='First Last' type='User'>8227040ABF0A46A8AF06C18ABD3967B3</id>
      <itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype>
      <first_name>First</first_name>
    </Item>
  </created_by_id>
  <id keyed_name='Best Company' type='Company'>1470B001142748A5BB39CECB72CD83C8</id>
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
            if (AttrEquals(elem, "id", "2D246C5838644C1C8FD34F8D2796E327") || elem.Element("id")?.Value == "2D246C5838644C1C8FD34F8D2796E327")
            {
              result = @"<Result>
  <Item type='User' typeId='45E899CD2859442982EB22BB2DF683E5' id='2D246C5838644C1C8FD34F8D2796E327'>
    <default_vault keyed_name='Default' type='Vault'>
      <Item type='Vault' typeId='8FC29FEF933641A09CEE13A604A9DC74' id='51C2A8877C4D4038A5A0C3071A863706'>
        <id keyed_name='Default' type='Vault'>51C2A8877C4D4038A5A0C3071A863706</id>
        <keyed_name>Default</keyed_name>
        <vault_url_pattern>$[HTTP_PREFIX_SERVER]$[HTTP_HOST_SERVER]$[HTTP_PORT_SERVER]$[HTTP_PATH_SERVER]/vault/vaultserver.aspx</vault_url_pattern>
        <name>Default</name>
        <vault_url>http://server/innovator11sp12/vault/vaultserver.aspx</vault_url>
      </Item>
    </default_vault>
    <id keyed_name='First Last' type='User'>2D246C5838644C1C8FD34F8D2796E327</id>
    <itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype>
    <Relationships>
      <Item type='ReadPriority' typeId='8CFAF78BCFFB41E6A3ED838D9EC2FD7C' id='749A3C52A15247F4AB7EDE406AA19AF8'>
        <id keyed_name='749A3C52A15247F4AB7EDE406AA19AF8' type='ReadPriority'>749A3C52A15247F4AB7EDE406AA19AF8</id>
        <priority>10</priority>
        <related_id keyed_name='Default' type='Vault'>
          <Item type='Vault' typeId='8FC29FEF933641A09CEE13A604A9DC74' id='51C2A8877C4D4038A5A0C3071A863706'>
            <id keyed_name='Default' type='Vault'>51C2A8877C4D4038A5A0C3071A863706</id>
            <keyed_name>Default</keyed_name>
            <vault_url_pattern>$[HTTP_PREFIX_SERVER]$[HTTP_HOST_SERVER]$[HTTP_PORT_SERVER]$[HTTP_PATH_SERVER]/vault/vaultserver.aspx</vault_url_pattern>
            <name>Default</name>
            <vault_url>http://server/innovator11sp12/vault/vaultserver.aspx</vault_url>
          </Item>
        </related_id>
        <source_id keyed_name='First Last' type='User'>
          <Item type='User' typeId='45E899CD2859442982EB22BB2DF683E5' id='2D246C5838644C1C8FD34F8D2796E327'>
            <id keyed_name='First Last' type='User'>2D246C5838644C1C8FD34F8D2796E327</id>
            <itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype>
          </Item>
        </source_id>
      </Item>
    </Relationships>
  </Item>
</Result>";
            }
            else if (AttrEquals(elem, "id", "8227040ABF0A46A8AF06C18ABD3967B3"))
            {
              result = @"<Result><Item type='User' typeId='45E899CD2859442982EB22BB2DF683E5' id='8227040ABF0A46A8AF06C18ABD3967B3'>
  <id keyed_name='First Last' type='User'>8227040ABF0A46A8AF06C18ABD3967B3</id>
  <first_name>First</first_name>
  <itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype>
</Item></Result>";
            }
            else if (AttrEquals(elem, "id", "49403709D9F847ECA1A2DE9ADE68660F"))
            {
              result = @"<Result><Item type='User' typeId='45E899CD2859442982EB22BB2DF683E5' id='49403709D9F847ECA1A2DE9ADE68660F'>
  <id keyed_name='John Doe' type='User'>49403709D9F847ECA1A2DE9ADE68660F</id>
  <first_name>John</first_name>
  <last_name>Doe</last_name>
  <itemtype>45E899CD2859442982EB22BB2DF683E5</itemtype>
</Item></Result>";
            }
            break;
          case "Identity":
            if (AttrEquals(elem, "id", "44CC39EB107F4C02884AFF66A478202D"))
            {
              result = @"<Result>
  <Item type='Identity' typeId='E582AB17663F4EF28460015B2BE9E094' id='44CC39EB107F4C02884AFF66A478202D'>
    <id keyed_name='First Last' type='Identity'>44CC39EB107F4C02884AFF66A478202D</id>
    <is_alias>1</is_alias>
    <name>First Last</name>
    <itemtype>E582AB17663F4EF28460015B2BE9E094</itemtype>
  </Item>
</Result>";
            }
            else if (AttrEquals(elem, "id", "384C0326D719419F897C34163B8C5B2E"))
            {
              result = @"<Result>
  <Item type='Identity' typeId='E582AB17663F4EF28460015B2BE9E094' id='384C0326D719419F897C34163B8C5B2E'>
    <id keyed_name='John Doe' type='Identity'>384C0326D719419F897C34163B8C5B2E</id>
    <is_alias>1</is_alias>
    <name>John Doe</name>
    <itemtype>E582AB17663F4EF28460015B2BE9E094</itemtype>
  </Item>
</Result>";
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
            else if (AttrEquals(elem, "id", "F8BAD68CCADB43DF901FDCA693A22705"))
            {
              result = @"<Result><Item type='Permission' typeId='C6A89FDE1294451497801DF78341B473' id='F8BAD68CCADB43DF901FDCA693A22705'>
  <id keyed_name='Vault' type='Permission'>F8BAD68CCADB43DF901FDCA693A22705</id>
  <name>Vault</name>
</Item></Result>";
            }
            break;
          case "ItemType":
            if (AttrEquals(elem, "id", "4F1AC04A2B484F3ABA4E20DB63808A88") || elem.Value == "Part")
            {
              result = @"<Result>
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
    </Result>";
            }
            break;
          case "Property":
            if (elem.Element("source_id")?.Value == "Company")
            {
              result = @"<Result>
  <Item type='Property' typeId='26D7CD4E033242148E2724D3D054B4D3' id='264EA3A8B7D14D0587EFB79365FF28A2'>
    <data_source keyed_name='Permission' type='ItemType' name='Permission'>C6A89FDE1294451497801DF78341B473</data_source>
    <data_type>item</data_type>
    <name>permission_id</name>
  </Item>
  <Item type='Property' typeId='26D7CD4E033242148E2724D3D054B4D3' id='E6B16DA893644CFDBC62A4EAF68C61A1'>
    <data_source keyed_name='Identity' type='ItemType' name='Identity'>E582AB17663F4EF28460015B2BE9E094</data_source>
    <data_type>item</data_type>
    <name>owned_by_id</name>
  </Item>
</Result>";
            }
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

    public void SetDefaultHeaders(Action<string, string> writer)
    {
      writer.Invoke("AUTHUSER", "User");
      writer.Invoke("AUTHPASSWORD", "Password");
      writer.Invoke("DATABASE", "Database");
      writer.Invoke("LOCALE", "Locale");
      writer.Invoke("TIMEZONE_NAME", "TimeZone");
    }

    public IPromise<Stream> Process(Command request, bool async)
    {
      return Promises.Resolved(Process(request));
    }

    public IPromise<ExplicitHashCredentials> HashCredentials(ICredentials credentials, bool async)
    {
      throw new NotImplementedException();
    }

    public string GetHeader(string name)
    {
      throw new NotImplementedException();
    }
  }
}
