# Connecting to Aras

To connect to Aras, use the @Innovator.Client.Factory class to create a new connection.  All you 
need to specify is the URL of the Aras instance and a `User-Agent` string.  The use of unique
`User-Agents` is strongly encouraged when using Innovator.Client so that administrators can track 
the tools that are using their Aras installation.  After the connection is created, you can log in
by specifying the database, user name, and password.  If you need a list of databases, you can
retrieve such a list using the @Innovator.Client.IRemoteConnection.GetDatabases method

```csharp
//using Innovator.Client;

var conn = Factory.GetConnection("URL", "USER_AGENT");
conn.Login(new ExplicitCredentials("DATABASE", "USER_NAME", "PASSWORD"));
```

## Credentials

Several different credentials classes are available:

* @Innovator.Client.AnonymousCredentials: Represents an attempt to log in to Aras anonymously.
  Currently, no connection in Innovator.Client actually supports using these credentials
* @Innovator.Client.ExplicitCredentials: (**Recommended**) Allows you to log in to Aras using
  a database, user name, and password.
* @Innovator.Client.ExplicitHashCredentials: Allows you to log in to Aras using a database, user 
  name, and the MD5 hash of a password.
* @Innovator.Client.WindowsCredentials: Allows you to log in to Aras using the current account 
  which is logged in to Windows.  Alternatively, you can specify an System.Net.ICredentials object
  specifying the credentials to use.
  
## Customizing Authentication

If you would like to customize how authentication is handled, consider implementing 
[Mapped Connections](mappedconnections.html)

## Additional Control

For more control over how requests are sent, use the @Innovator.Client.ConnectionPreferences 
parameter.  For example, perhaps you want to specify a default timeout and add a new HTTP header to
every request.  The following code configures these preferences.  It also creates the connection
and logs in to Aras in a single step.

```csharp
var prefs = new ConnectionPreferences() { Url = "URL" };
prefs.DefaultTimeout = 200000; // in milliseconds
prefs.Headers.UserAgent = "USER_AGENT";
prefs.Headers.Add("Header-Name", "Value");
prefs.Credentials = new ExplicitCredentials("DATABASE", "USER_NAME", "PASSWORD");
var conn = Factory.GetConnection(prefs);
```

## Saving Connection Preferences

Connection preferences can be saved for later use in an XML file or blob.  Using the default 
settings on a Windows computer, you can save preferences for later use:

```csharp
var prefs = new ConnectionPreferences() { Url = "URL" };
prefs.DefaultTimeout = 200000 // in milliseconds
prefs.Headers.UserAgent = "USER_AGENT";
prefs.Headers.Add("Header-Name", "Value");
prefs.Credentials = new ExplicitCredentials("DATABASE", "USER_NAME", "PASSWORD");

var saved = SavedConnections.Load();
saved.Default = prefs;
saved.Save();
```

This makes authenticating at a later date very simple as you can create a connection with
the following single line of code

```csharp
var conn = Factory.GetConnection(SavedConnections.Load().Default);
```