using Innovator.Client;
using Innovator.Client.QueryModel;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace QueryModelTests
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Starting...");
      var prefs = SavedConnections.Load().Default;
      var conn = Factory.GetConnection(prefs);
      var savedSearches = conn.Apply(@"<Item type='SavedSearch' action='get' select='criteria'>
        <is_email_subscription>0</is_email_subscription>
        <auto_saved>0</auto_saved>
        <criteria condition='is not null'></criteria>
      </Item>").Items().Select(i => i.Property("criteria").Value).ToArray();

      var settings = new ConnectedAmlSqlWriterSettings(conn)
      {
        PermissionOption = AmlSqlPermissionOption.None
      };
      var parser = new SimpleSearchParser()
      {
        Context = conn.AmlContext.LocalizationContext
      };
      parser.OrDelimiters.Add('\t');
      parser.OrDelimiters.Add('\r');
      parser.OrDelimiters.Add('\n');
      parser.String.DefaultSearchIsContains = true;
      parser.String.IsPercentWildcard = false;

      var ignoreActions = new[]
      {
        "ApplySubSelect",
        "ExcelOrgChartReport",
        "Finished Goods Open Concern Qry",
        "Gcs_DesignDateReport",
        "Gcs_Report_ConcernMfgOrg",
        "Gcs_Report_CustStagnantIssue",
        "Gcs_Report_EaConcernTracking",
        "Gcs_Report_SupplierQtyToReport",
        "Gcs_Report_SupplierQualityExport",
        "Get Where Used Parts",
        "GetChargeTimeEntered",
        "GetChargeTimeRaw",
        "GetByPerson",
        "GntxPartNumberXrefRpt",
        "LinkSearch",
        "Mco_Report_CreatedToday",
        "Mdm_Report_HtsCodes",
        "PcbProgramCostReport",
        "ProtoOrder_QueryNoRouting",
        "ProtoOrder_QueryOrderBuildDates",
        "ProtoOrder_QueryShouldBeClosed",
        "ProtoOrderQueryWeek",
        "QualityGcsPriorityReport",
        "SearchLumen",
        "TimeTrack_EmployeeAssignRpt",
        "TimeTrack_EscalationsReport",
      };

      var noErrorCnt = 0;
      var errorCnt = 0;
      var total = 0;
      Console.WriteLine("Testing queries...");
      foreach (var search in savedSearches)
      {
        try
        {
          if (ignoreActions.Any(a => search.IndexOf(a) > 0))
            continue;
          if (Regex.IsMatch(search, @"condition=""in"">\s*\(\s*SELECT", RegexOptions.IgnoreCase))
            continue;

          var query = QueryItem.FromXml(search);

          var countQuery = conn.AmlContext.FromXml(search).AssertItem();
          countQuery.Attribute("returnMode").Set("countOnly");
          var trueCount = conn.Apply(countQuery.ToAml()).ItemMax();

          var sql = query.ToArasSql(settings);
          if (conn.ApplySql(sql).Items().Count() != trueCount)
            throw new InvalidOperationException();
          var newAml = query.ToAml();
          if (conn.Apply(newAml).Items().Count() != trueCount)
            throw new InvalidOperationException();
          var oData = query.ToOData(settings, conn.AmlContext.LocalizationContext);
          var criteria = query.ToCriteria(parser);
          noErrorCnt++;
        }
        catch (Exception)
        {
          //Console.WriteLine(ex.ToString());
          errorCnt++;
        }
        total++;

        if ((total % 20) == 0)
          Console.WriteLine($"{total} queries tested");
      }

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine($"{errorCnt} errors");
      Console.WriteLine($"{noErrorCnt} successes");

      Console.ReadLine();
    }
  }
}
