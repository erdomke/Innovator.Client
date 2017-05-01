# Parameterized Queries

## Indexed Parameters

Sending AML queries to Aras is quite simple as you can craft an AML query string with parameters
that are replace in a system that is very analogous to string.Format.  However, unlike string.Format,
parameterized queries in Innovator.Client understand AML and SQL.  Core types (e.g. `int`, `bool`, 
`DateTime`, `double`, etc.) will all be serialized according to AML conventions.  (This includes 
converting time zones where necessary so that a local date is rendered in the corporate AML time 
zone.)  String will be properly escaped for including into XML and/or SQL.  Since Innovator Admin
supports parameters, these queries can be copied and pasted back and forth between your code
editor and Innovator Admin during debugging.

```csharp
var classification = "Component";
var date = DateTime.Now.AddMinutes(-20);
var components = conn.Apply(@"<Item type='Part' action='get'>
                                <classification>@0</classification>
                                <created_on condition='lt'>@1</created_on>
                                <state>Preliminary</state>
                              </Item>", classification, date).Items();
```

If you need control over the SOAP action used, you can do so by creating a Command object

```csharp
var classification = "Component";
var date = DateTime.Now.AddMinutes(-20);
var cmd = new Command(@"<Item type='Part' action='get'>
                          <classification>@0</classification>
                          <created_on condition='lt'>@1</created_on>
                          <state>Preliminary</state>
                        </Item>", classification, date)
              .WithAction(CommandAction.ApplyAML);
var components = conn.Apply(cmd).Items();
```

It is worth noting that a command object is implicity created with the first syntax.

## Named Parameters

If you aren't a fan of using zero-based indexed parameters, you can use named parameters with
a slightly more verbose syntax.

```csharp
var classification = "Component";
var date = DateTime.Now.AddMinutes(-20);
var cmd = new Command(@"<Item type='Part' action='get'>
                          <classification>@class</classification>
                          <created_on condition='lt'>@date</created_on>
                          <state>Preliminary</state>
                        </Item>")
              .WithParam("class", classification)
              .WithParam("date", date)
              .WithAction(CommandAction.ApplyAML);
var components = conn.Apply(cmd).Items();
```

## Interpolated Strings

If you are a fan of [interpolated strings](https://msdn.microsoft.com/en-us/library/dn961160.aspx) 
in C#/VB and are using .Net 4.6+, you also use them with parameterized queries.  Simply cast you
interpolated string as a FormattableString.

```csharp
var classification = "Component";
var date = DateTime.Now.AddMinutes(-20);

var components = conn.Apply((FormattableString)$"<Item type='Part' action='get'>
                              <classification>{classification}</classification>
                              <created_on condition='lt'>{date}</created_on>
                              <state>Preliminary</state>
                            </Item>", classification, date).Items();
```

## AML Objects

If you need more flexibility than simply inserting parameters, you can use create AML using an API
very similar to XElement.

```csharp
var classification = "Component";
var date = DateTime.Now.AddMinutes(-20);

var aml = conn.AmlContext;
var query = aml.Item(aml.Type("Part"), aml.Action("get")
  , aml.CreatedOn(aml.Condition(Condition.LessThan), date)
  , aml.State("Preliminary")
);
if (true)
{
  query.Classification().Set("Component");
}
var components = conn.Apply(query.ToAml()).Items();
```