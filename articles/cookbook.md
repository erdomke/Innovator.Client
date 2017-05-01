# Innovator.Client Cookbook

Following many of the same examples as the [Aras Programmers Guide](http://www.aras.com/support/documentation/11.0%20SP5/Other%20Documentation/Aras%20Innovator%2011.0%20-%20Programmers%20Guide.pdf),
this document describes how to perform a number of different actions in Aras using the 
Innovator.Client library.

## Connecting to Innovator

Connections are created from a factory which determine the best connection object to create for a 
given URL.  Once the connection is established, you can login using one of the credential patterns
 of `AnonymousCredentials`, `ExplicitCredentials`, `TokenCredentials`, or `WindowsCredentials`.  
`ExplicitCredentials` is universally supported while the other types currently have limited 
support.

**C#**

```cs
using Innovator.Client;

var conn = Factory.GetConnection("URL", "USER_AGENT");
conn.Login(new ExplicitCredentials("DATABASE", "USER_NAME", "PASSWORD"));
```

## Create an Item Object

All Aras objects (items, results, exception, etc.) are created through an `ElementFactory`.  This 
class handles the serialization of base data types relative to a particular connection (e.g. 
handling timezone conversions for `DateTime` values).  An `ElementFactory` can be obtained for a 
connection via the `AmlContext` property of the `IConnection` interface.  Alternatively, if you 
want to manipulate AML outside of the context of a server connection, you can assume the local 
timezone and use the local factory by calling `ElementFactory.Local`.

**C#**

```cs
var aml = conn.AmlContext;
IItem myItem = aml.Item(aml.Type(myType), aml.Action(myAction));
IResult myResult = aml.Result(resultText);
ServerException = aml.ServerException(errorMessage); 
```

OR

```cs
var aml = ElementFactory.Local;
IItem myItem = aml.Item(aml.Type(myType), aml.Action(myAction));
IResult myResult = aml.Result(resultText);
ServerException = aml.ServerException(errorMessage); 
```


## Query for an Item

There are a few ways to get an `IReadOnlyItem` when you know its id and type, the simplest being 
the `IConnection.ItemById()` extension method. However, overuse of this method can lead to poor 
performance as it select more data than you likely need.  Therefore, If you need to be granular 
about your request then building the AML is required. This provides the ability to include 
controls to limit the results and define the structure to be returned for the Items found.

**C#**

```cs
var results = conn.Apply(@"<Item type='@0' action='@1' id='@2' />", myType, myAction, myId);
var item = results.AssertItem();
// NOTE: An exception will be thrown if there is not a single item in the result
```

OR

```cs
var aml = conn.AmlContext;
IItem qryItem = aml.Item(aml.Type(myType), aml.Action(myAction), aml.Id(myId));
var results = qryItem.Apply(conn);
var item = results.AssertItem();
// NOTE: An exception will be thrown if there is not a single item in the result
```

OR 

```cs
var item = conn.ItemById(myType, myId); 
// NOTE: An exception will be thrown if there is not a single item in the result
```

## Query and iterate over a set of items

There is no difference in setting up a query for a single Item or for many. Only the criteria 
define the set size returned. In this recipe you apply an AML statement and iterate over the Items
 returned producing a HTML `<TABLE>` fragment.

**C#**

```cs
// Configure and execute the query using one of the two methods below:
var results = conn.Apply(@"<Item type='Part' action='get' select='item_number,description,cost'>
                              <cost condition='gt'>100</cost>
                           </Item>");

// Build the table
var content = new System.Text.StringBuilder("<table>");
foreach (var item in results.Items())
{
  content.Append("<tr>");
  content.Append("<td>").Append(item.Property("item_number").Value).Append("</td>");
  content.Append("<td>").Append(item.Property("description").Value).Append("</td>");
  content.Append("<td>").Append(item.Property("cost").Value).Append("</td>");
  content.Append("</tr>");
}
content.Append("</table>");

return aml.Result(content.ToString());
```

## Query for an Item and return its configuration

To query for an Item and retrieve its structure you build the query as the structure you want 
returned. Use the methods to add the relationships you want and build the structure in the Item. 
The server returns the structure that follows the request structure. 

This recipe illustrates several related concepts together, which are how to get a set of Items 
from an Item and how to iterate over the set, plus how to get the related Item from the 
relationship Item.

**C#**

```cs
// Configure and execute the query using one of the two methods below:
var results = conn.Apply(@"<Item type='Part' action='get' select='item_number,description,cost' id='@0'>
  <Relationships>
    <Item type='Part BOM' action='get' select='quantity,related_id(item_number,description,cost)' />
  </Relationships>
</Item>", myId);

// NOTE: No need to check for an error.  If there is not a single item in the result, an useful 
// exception will be thrown which will tell you the error returned by the server (if there was 
// one). However, if you are exception-adverse, you can always check to see if the
// results.Exception property is null
var bomItems = results.AssertItem().Relationships();

// Create the results content
var content = new System.Text.StringBuilder(@"<table border='1'> 
  <tr>
    <td>Part Number</td>
    <td>Description</td>
    <td>Cost</td>
    <td>Quantity</td>
  </tr>");

// Iterate over the BOM Items
foreach (var bom in bomItems)
{
  var bomPart = bom.RelatedItem();
  content.Append("<tr>");
  content.Append("<td>").Append(bomPart.Property("item_number").Value).Append("</td>");
  content.Append("<td>").Append(bomPart.Property("description").Value).Append("</td>");
  content.Append("<td>").Append(bomPart.Property("cost").Value).Append("</td>");
  content.Append("<td>").Append(bom.Property("quantity").Value).Append("</td>");
  content.Append("</tr>");
}
content.Append("</table>");

return aml.Result(content.ToString());
```

## Apply a Generic Method

Generic methods can be applied by observing that all you need to do is set the `action` attribute 
to the name of your method.  In this example, assume a server-side method named "Reverse String" 
exists, and that it returns a result item containing the reversed contents of the `<string>` tag.

**C#**

```cs
var results = conn.Apply(@"<Item type='Method' action='Reverse String'>
                             <string>abc</string>
                           </Item>");
return aml.Result(results.Value);
```

## Need for Speed: ApplySQL

Use the `IConnection.ApplySql()` extension method to submit SQL direct to the database. The format
 of the XML returned by the `IConnection.ApplySql()` method when the SQL statement is a `select` 
statement is:

```xml
<SOAP-ENV:Envelope xmlns:SOAP-ENV=...>
  <SOAP-ENV:Body>
    <ApplySQLResponse>
      <Item>
        <A>aval</A>
        <B>bval</B>
        …
      </Item>
      <Item>
      …
      </Item>
      …
    </ApplySQLResponse>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>
```

In case executed SQL statement doesn’t return a record set (e.g. update [table] …), the returned 
AML either contains a `<Fault>` if SQL statement failed or looks like:

```xml
<SOAP-ENV:Envelope xmlns:SOAP-ENV=...>
<SOAP-ENV:Body>
 <ApplySQLResponse>
 OK
 </ApplySQLResponse>
</SOAP-ENV:Body>
</SOAP-ENV:Envelope>
```

This recipe returns the XML from the `ApplySQL()` method and forms HTML for a table to display the
 data.

**C#**

```cs
var results = conn.ApplySQL(@"select login_name,first_name,last_name,email 
  from [user] 
  order by last_name,first_name");
var content = new System.Text.StringBuilder(@"<style type='text/css'>
  table {background:##000000;}
  th {font:bold 10pt Verdana; background:##0000FF; color:##FFFFFF;}
  td {font:normal 10pt Verdana; background:##FFFFFF;}
  caption {font:bold 14pt Verdana; text-align:left;}
  </style>
  <table id='tbl' border='0' cellspacing='1' cellpadding='2' datasrc='##itemData'>
  <caption>User Directory</caption>
  <thead>
  <tr>
  <th>Login Name</th>
  <th>First Name</th>
  <th>Last Name</th>
  <th>EMail</th>
  </tr>
  </thead>
  <tbody>");

foreach (var user in results.Items())
{
  content.Append("<tr>");
  content.Append("<td>").Append(user.Property("login_name").Value).Append("</td>");
  content.Append("<td>").Append(user.Property("first_name").Value).Append("</td>");
  content.Append("<td>").Append(user.Property("last_name").Value).Append("</td>");
  content.Append("<td>").Append(user.Property("email").Value).Append("</td>");
  content.Append("</tr>");
}
content.Append("</tbody>");
content.Append("</table>");

return conn.AmlContext.Result(content.ToString());
```

## Want to Vault a File

In order to have the AML submitted to the Vault server, you cannot use a simple `Command` object 
(which was implicitly created from strings in all of the previous examples).  Rather, you need to 
create an `UploadCommand` as shown in the code below.

**C#**

```cs
var upload = conn.CreateUploadCommand();
var fileAml = upload.AddFile(@"C:\My Document.doc");
// Note, the ! after the @0! tells the library to inject the raw XML without encoding it.
upload.WithAml(@"<Item type='Document' action='add'>
                   <item_number>123</item_number>
                   <Relationships>
                     <Item type='Document File' action='add'>
                       <related_id>@0!</related_id>
                     </Item>
                   </Relationships>
                </Item>", fileAml);
conn.Apply(upload).AssertNoError();

// You can also upload a file from a stream if it does not exist on disk:
var memStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes("FILE_CONTENT"));
var upload = conn.CreateUploadCommand();
var fileAml = upload.AddFile(@"C:\My Document.doc", memStream);
upload.WithAml(@"<Item type='Document' action='add'>
                   <item_number>123</item_number>
                   <Relationships>
                     <Item type='Document File' action='add'>
                       <related_id>@0!</related_id>
                     </Item>
                   </Relationships>
                </Item>", fileAml);
conn.Apply(upload).AssertNoError();
```

## Download a file

To download a file, use the custom SOAP action of `DownloadFile`.

**C#**

```cs
var stream = conn.Process(new Command("<Item type='File' action='get' id='@0' />", fileId)
    .WithAction(CommandAction.DownloadFile));
```


## Want to get an existing Vaulted File and save it with a new Document

This is similar to the last recipe, but uses the `ItemByKeyedName()` extension method to get an 
existing File Item and `copyAsNew` action to create it as a new File Item.

**C#**

```cs
// Create the Document item
var aml = conn.AmlContext;
var docItem = aml.Item(aml.Type("Document"), aml.Action("add"), aml.Property("item_number", "456"));

// Get the File Item.  An except will be thrown if necessary.
var fileItem = conn.ItemByKeyedName("File", "My Document.doc").AssertItem();

// Duplicate File Item as files should be 1 to 1
// You need to clone the item to get an editable one
var fileCopyQuery = fileItem.Clone();
fileCopyQuery.Action().Set("copyAsNew");
// Create the Relationship 
docItem.Add(aml.Relationships(
  aml.Item(aml.Type("Document File"), aml.Action("add"), fileCopyQuery)
));

var results = docItem.Apply(conn);
// Act on the results
```

## Need to reject an Item Promote

Use the Pre Server Method on the Life Cycle Transition to call a server side Method to validate the 
Item before it is promoted and if invalid rejects the Promote by returning an Error Item.

**C#**

```cs
var item = arg.Item;
if (item.Property("cost").AsDouble(0.0) > 500) 
{
  throw arg.Conn.AmlContext.ServerException("Error promoting: Item costs more than $500.00");
}
return item;
```

## How to handle multilingual properties

** Not fully supported yet: In progress **

## How to handle date properties

Use the library's ability to convert value for you.

**C#**

```cs
// Get yesterday's date
var myDate = DateTime.Now.AddDays(-1);

// Find all methods edited in the past 24 hours
var results = conn.Apply(@"<Item type='Method' action='get' select='name'>
                             <modified_on condition='gt'>@0</modified_on>
                           </Item>", myDate);

// loop through the returned methods and return the list
var methodList = new System.Text.StringBuilder();
foreach (var method in results.Items())
{
  // Yes, this if statement isn't necessary, but it does demonstrate
  // use of the code
  if (method.ModifiedOn().AsDateTime(DateTime.MinValue) > myDate)
  {
     methodList.Append(method.Property("name").Value).Append(", ");
  }
}
```

## Execute AML Asynchronously

In order to support .Net 3.5 without any external dependencies and to better match JQuery, the 
library implements a Promise API for implementing asynchronous calls.  However, for better 
compatibility with other async code, the .Net library also supports converting promises to Tasks. 

**C#**

```cs
// .Net 3.5
var promise = conn.ApplyAsync("MY_QUERY", true, false)
  .Done(result => {})
  .Fail(ex => {});

// .Net 4
var result = await conn.ApplyAsync("MY_QUERY", true, false);
```

## Avoiding NullReferenceExceptions

When traversing a long tree, it nice not to have to deal with null reference exceptions.  
Therefore, the library implements the null-object pattern. Simply check the Exists property to 
determine if a retrieved property actually exists.

**C#**

```cs
var firstNameProp = part.CreatedById().AsItem().Property("first_name");
if (!firstNameProp.Exists)
{
    // Get the first_name by other means
}
```

## Modifying the HTTP Headers

You have control over the HTTP headers sent with each request to the server.  To modify particular
 headers, follow a pattern similar to

**C#**

```cs
var conn = Factory.GetConnection("URL", "USER_AGENT");
conn.DefaultSettings(r => r.SetHeader("X-CUSTOM-HEADER", "MY_VALUE"));
conn.Login(new ExplicitCredentials("DATABASE", "USER_NAME", "PASSWORD"));
```