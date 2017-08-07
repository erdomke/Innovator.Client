using System;

namespace Innovator.Client.Tests
{
  class Scratchpad
  {
    public static void Test()
    {
      var conn = Factory.GetConnection("", "");

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

      var aml = conn.AmlContext;
      var query = aml.Item(aml.Type("Part"), aml.Action("get")
        , aml.CreatedOn(aml.Condition(Condition.LessThan), date)
        , aml.State("Preliminary")
      );
      if (true)
      {
        query.Classification().Set("Component");
      }
      conn.Apply(query.ToAml());
    }
  }
}
