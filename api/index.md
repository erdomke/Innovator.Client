# Innovator.Client

API documentation for the Innovator.Client library can be found here.  As a general pattern for 
how to use the library, consider the following example:

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
    
    // If the part was created after 2016-01-01, put the name of the creator in 
    // the description
    if (comp.CreatedOn().AsDateTime(DateTime.MaxValue) > new DateTime(2016, 1, 1))
    {
      edits.Property("description")
        .Set("Created by: " + comp.CreatedById().KeyedName().Value);
    }
    
    // Apply the changes. Throw an exception if an error occurs.
    edits.Apply(conn).AssertNoError();

    // Promote the item. Throw an exception if an error occurs.
    comp.Promote(conn, "Released").AssertNoError();
  }
}
```
