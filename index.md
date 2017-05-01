# Innovator.Client

[![innovator-client MyGet Build Status](https://www.myget.org/BuildSource/Badge/innovator-client?identifier=cb689f79-0412-4d27-b7ca-e6be3283d4e9)](https://www.myget.org/)
[![NuGet](https://img.shields.io/nuget/v/Innovator.Client.svg)](https://www.nuget.org/packages/Innovator.Client/)

Innovator.Client is a library for connecting to [Aras Innovator](http://www.aras.com/) installations.
It is a replacement for the IOM library provided by Aras.  It is not a drop-in replacement, but rather
aims to provide an API which is easier to use.

# First, show me the code

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

# I want it

Get it via [NuGet](https://www.nuget.org/packages/Innovator.Client/) using the command

    PM> Install-Package Innovator.Client
    
# Compatibility

## .Net

Innovator.Client has builds for .Net 3.5+ and .Net Standard 1.1+.  It is worth noting that the .Net 
Standard builds are largely untested while the full .Net builds are currently used in production
deployments

## Aras

The IOM is not backwards-compatible.  For example, v11.0 of the IOM cannot be used with a v9.3
Aras Innovator vault.  Innovator.Client strives to be compatible with all versions >= 9.3