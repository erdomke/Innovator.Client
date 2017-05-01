# Mapped Connections

Mapped connections allow Aras administrators to configure how `Innovator.Client` connects to a given
Aras installation.  In particular, it currently allows for

- Connections sent to a single URL can be redirected to other application servers based on the 
  database that is selected.
- An authentication service can be specified. If provided, credentials will first be sent to that 
  service.  The service will then respond with the credentials that the client will use to log in to 
  Innovator.

# Configuring

No changes have to be made to client-side code to start using mapped connections.  Rather, the 
@Innovator.Client.Factory.GetConnection code will look for a mapping file by default.  Mapping files 
should be deployed at `http://{web_server_location}/Server/Mapping.xml`.  A sample mapping file 
might look like

```xml
<Servers>
  <Server url="https://prod_app_server.company.com/address" auth="md5">
    <endpoint action="Auth">
      <uri>https://authentication_service.company.com/address</uri>
    </endpoint>
    <DB id="PROD" />
  </Server>
  <Server url="https://dev_app_server.company.com/address" auth="md5">
    <endpoint action="Auth">
      <uri>https://authentication_service.company.com/address</uri>
    </endpoint>
    <DB id="QA" />
    <DB id="DEV" />
  </Server>
</Servers>
```

> [!NOTE]
> The `auth` attribute of `md5` simply indicates that the server is a standard Aras Innovator server 
> which leverages the MD5 hash of the password as its authentication mechanism

For this file, the Innovator.Client code will present the databases `PROD`, `QA`, and `DEV` to the 
user.  Any connection to `PROD` will be sent to the URL `https://authentication_service.company.com/address` 
while connections to `QA` or `DEV` will go to `https://dev_app_server.company.com/address`.  In 
addition, all login requests will first be directed to `https://authentication_service.company.com/address`

# Authentication Service

If an authentication service is declared, it should be configured as follows:

- Credentials are sent to the authentication service using standard HTTP transport protocols.  
Therefore, your service should respond to unauthenticated requests with a standard HTTP 401 
authentication challenge looking for either Basic, Digest, or Windows (i.e. NTML/Kerberos) formatted 
credentials.  Additionally, the database that the user is attempting to connect to will be provided 
as a query string parameter.  The authentication service should respond to an authenticated request
with XML that indicates the user name and password that the client should use when authenticating
to Aras Innovator directly, as indicated below.  

```xml
<Result>
  <user>{username that will be sent to innovator}</user>
  <password>{username that will be sent to innovator}</password>
  <hash>{whether the password should be hashed 
         before sending it to Aras; either 'true' or 'false'}</hash>
</Result>
```

Therefore, a sample exchange might consist of:

**Client**

```
GET https://authentication_service.company.com/address?db=DEV
```

**Server**

```
HTTP/1.1 401 Access Denied
WWW-Authenticate: Basic realm="company.com"
Content-Length: 0
```

**Client**

```
GET https://authentication_service.company.com/address?db=DEV
Authorization: Basic bmFtZTpwYXNzd29yZA==
```

**Server**

```xml
<Result>
  <user>name</user>
  <password>password</password>
  <hash>true</hash>
</Result>
```