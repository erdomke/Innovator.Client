# Innovator.Client

[![innovator-client MyGet Build Status](https://www.myget.org/BuildSource/Badge/innovator-client?identifier=cb689f79-0412-4d27-b7ca-e6be3283d4e9)](https://www.myget.org/)
[![NuGet](https://img.shields.io/nuget/v/Innovator.Client.svg)](https://www.nuget.org/packages/Innovator.Client/)

Innovator.Client is a library for connecting to [Aras Innovator](http://www.aras.com/) installations.
It is a replacement for the IOM library provided by Aras.  It is not a drop-in replacement, but rather
aims to provide an API which is easier to use.

# API Example

```csharp
/// <summary>
/// Edit and release components of a given classification
/// </summary>
/// <param name="classification">Classification of parts to release</param>
private void ReleaseByType(string classification)
{
  var conn = Factory.GetConnection("URL", "USER_AGENT");
  conn.Login(new ExplicitCredentials("DATABASE", "USER_NAME", "PASSWORD"));

  // Get preliminary parts which have existed for a little bit of time
  var components = conn.Apply(@"<Item type='Part' action='get'>
                                  <classification>@0</classification>
                                  <created_on condition='lt'>@1</created_on>
                                  <state>Preliminary</state>
                                </Item>", classification, DateTime.Now.AddMinutes(-20)).Items();

  var aml = conn.AmlContext;
  // Iterate through the components
  foreach (var comp in components)
  {
    // Append the value " - Released" to the end of the name
    var edits = aml.Item(aml.Type("Part"), aml.Action("edit"), aml.Id(comp.Id()),
      aml.Property("name", comp.Property("name").AsString("") + " - Released")
    );
    // If the part was created after 2016-01-01, put the name of the creator in the description
    if (comp.CreatedOn().AsDateTime(DateTime.MaxValue) > new DateTime(2016, 1, 1))
    {
        edits.Property("description").Set("Created by: " + comp.CreatedById().KeyedName().Value);
    }
    // Apply the changes. Throw an exception if an error occurs.
    edits.Apply(conn).AssertNoError();

    // Promote the item. Throw an exception if an error occurs.
    comp.Promote(conn, "Released").AssertNoError();
  }
}
```

# Installing

Get it via [NuGet](https://www.nuget.org/packages/Innovator.Client/) using the command

    PM> Install-Package Innovator.Client
    
This will install the package in your Visual Studio solution
  
# Compatibility

## .Net

Innovator.Client has builds for .Net 3.5+ and .Net Standard 1.1+. In particular, there are builds
for 

- .Net 3.5, 4.0, 4.5, 4.6
- .Net Standard 1.1, 1.3, 2.0

It is worth noting that the .Net Standard builds are largely untested while the full .Net builds 
are currently used in production deployments

## Aras

The IOM is not backwards-compatible.  For example, v11.0 of the IOM cannot be used with a v9.3
Aras Innovator vault.  Innovator.Client strives to be compatible with all versions >= 9.3.  It has
been used in production deployments against Aras 9.3 and 11sp12.

# Documentation

- [API Documentation](https://erdomke.github.io/Innovator.Client/api/index.html) is hosted at https://erdomke.github.io/Innovator.Client/api/index.html
- For additional documentation, check out the various pages within the [wiki](https://github.com/erdomke/Innovator.Client/wiki)

# Why a new IOM?

Some use cases could not be addressed without a complete rewrite of the client (e.g. truly asynchronous 
AML calls, support for all Innovator versions, support for .Net Core, etc.).  Furthermore, a rewrite
provided the opportunity to address a number of frustrations with the current API (e.g. 'Not a single
item' errors obscuring errors returned by the server).  A more complete list of the goals/reasons are 
given below:

## AML in code should look like AML

In `Innovator.Client`, there are multiple ways to include AML in code.  In the first (and preferred)
way, AML is embedded directly as a multi-line string:

```csharp
// Get preliminary parts which have existed for a little bit of time
var components = conn.Apply(@"<Item type='Part' action='get'>
                                <classification>@0</classification>
                                <created_on condition='lt'>@1</created_on>
                                <state>Preliminary</state>
                              </Item>", classification, DateTime.Now.AddMinutes(-20)).Items();
```

Notice that parameters can be prefixed with an `@` symbol.  Parameters can of any .Net supported
type.  Core types (e.g. `int`, `bool`, `DateTime`, `double`, etc.) will all be serialized 
according to AML conventions.  (This includes converting time zones where necessary so that a
local date is rendered in the corporate AML time zone.)  This prevents having to remember AML
conventions and also helps prevent "AML-injection" attacks on your code.  Since Innovator Admin
supports parameters, these queries can be copied and pasted back and forth between your code
editor and Innovator Admin during debugging.

In the second method, modifiable AML is composed using a method very similar to Linq-to-XML:

```csharp
// Append the value " - Released" to the end of the name
var aml = conn.AmlContext;
var edits = aml.Item(aml.Type("Part"), aml.Action("edit"), aml.Id(comp.Id()),
    aml.Property("name", comp.Property("name").AsString("") + " - Released")
);
```

This coding style allows the nesting and structure of the AML to be preserved as it is being
composed.

## Parsing and serialization should be handled by the library

Besides the handling of .Net types as describe above, AML can also be parsed by the library
using culture and time zone aware methods. For example, the `created_on` property can be returned
as either a `Nullable<DateTime>` or a `DateTime` based on which overload of the `AsDateTime` 
method is called:

```csharp
comp.CreatedOn().AsDateTime(); // Get Nullable<DateTime>
comp.CreatedOn().AsDateTime(DateTime.MinValue); // Get DateTime
```

## Full support for asynchronous calls

The IOM does not come with generic support for asynchronous calls (e.g. of AML statements) or
single file uploads or downloads.  In this library, asynchronous calls can be performed in 
.Net 3.5+ using the Promise pattern or in .Net 4+ using the async pattern.  For example:

```csharp
// .Net 3.5
var promise = conn.ApplyAsync("MY_QUERY", true, false)
    .Done(result => {});
    
// .Net 4
var result = await conn.ApplyAsync("MY_QUERY", true, false);
```

## Support vault access via streams

Requiring that files be read from/written to disk requires accessing the slowest, most error-prone
memory accessible on a computer.  This is particular a problem for server-based code.

```csharp
// Upload a file
var memStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes("FILE_CONTENT"));
var upload = conn.CreateUploadCommand();
var sourceId = "DOCUMENT_ID";
var fileAml = upload.AddFile(@"C:\DUMMY_FILE_NAME.EXTENSION", memStream);
upload.WithAml(@"<Item type='Document File' action='add'>
                    <source_id>@0</source_id>
                    <related_id>@1!</related_id>
                </Item>", sourceId, fileAml);
var fileId = conn.Apply(upload).AssertItem().RelatedId().Value;

// Download a file
var stream = conn.Process(new Command("<Item type='File' action='get' id='@0' />", fileId)
    .WithAction(CommandAction.DownloadFile));
```

## Stop 'Not a single item' exceptions

Results come back from the server as an `IReadOnlyResult` object.  From this object, you can 
assert that the server should have returned a single item, graph any items returned by the server,
access the returned error, etc.  However, once you obtain and `IReadOnlyItem` object, you are
guaranteed throughout the code that it actually represents an `<Item />` tag.

```csharp
var result = conn.Apply("MY_QUERY");
result.AssertNoError(); // Do nothing other than throw an exception if there is an error
                        // other than 'No Items Found'
result.AssertItem();    // Return a single item.  If that is not possible, throw an appropriate
                        // exception (e.g. the exception returned by the server where possible)
result.Items();         // Return an enumerable of items.  Throw an exception if there is 
                        // an error other than 'No Items Found'
result.AssertItems();   // Return an enumerable of items.  Throw an exception for any error
```

## Stop NullReferenceExceptions

When traversing a long tree, it nice not to have to deal with null reference exceptions.  
Therefore, the library implements the null-object pattern.  Simply check the `Exists` property
to determine if a retrieved property actually exists.

```csharp
var firstNameProp = part.CreatedById().AsItem().Property("first_name");
if (!firstNameProp.Exists)
{
    // Get the first_name by other means
}
```

## HTTP Headers

The developer does not have control over HTTP headers when making calls to the server.  Use
cases for this might include:

  - Including a custom User-Agent so that the server knows about custom clients and their versions
    which are in use
  - Changing the TIMEZONE_NAME and LOCALE headers used by Aras for testing and development
    purposes
  - Including additional authentication headers used by proxies within an organization

The code to achive this looks like

```csharp
var conn = Factory.GetConnection("URL", "USER_AGENT");
conn.DefaultSettings(r => r.SetHeader("X-CUSTOM-HEADER", "MY_VALUE"));
conn.Login(new ExplicitCredentials("DATABASE", "USER_NAME", "PASSWORD"));
```

## A .Net library should support .Net features

### LINQ and IEnumerable<T>

Enumerating through items is as simple as

```csharp
var result = conn.Apply("MY_QUERY");

foreach (var item in result.Items())
{
}
```

Alternatively, leverage LINQ:

```csharp
var result = conn.Apply("MY_QUERY")
    .Items()
    .Where(i => i.IsReleased().AsBoolean(false));
```

### async and await

When using the .Net 4 version of the library, leverage the `async` and `await` keywords

```csharp
var result = await conn.ApplyAsync("MY_QUERY", true, false);
```

### SecureString

For security reasons, passwords are automatically stored in memory as `SecureString`.  In addition,
passwords stored in this way can be passed to the `Login` methods

# Developing

To build the project, clone or fork the repository and run the `build.ps1` file.  The 
`Innovator.Client.sln` solution file can be used to debug and develop the .Net Standard 1.1 build
of the library.  To debug specific features of the other builds, consider using the 
`Innovator.Client.Net35.sln` or the `Innovator.Client.Net45.sln` file.