#if REFLECTION
using Innovator.Client.QueryModel;
using System.Linq.Expressions;
using System.Xml;

namespace Innovator.Client.Queryable
{
  internal class AmlQuery : IAmlNode
  {
    public ElementFactory AmlContext { get; }
    public LambdaExpression Projection { get; }
    public QueryItem QueryItem { get; }
    public QuerySettings Settings { get; set; }

    public AmlQuery(QueryItem query, ElementFactory context, LambdaExpression projection)
    {
      QueryItem = query;
      AmlContext = context;
      Projection = projection;
    }

    public void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      if (Settings?.ModifyQuery == null)
      {
        QueryItem.ToAml(writer, settings);
      }
      else
      {
        var item = AmlContext.FromXml(QueryItem).AssertItem();
        Settings.ModifyQuery.Invoke(item);
        item.ToAml(writer, settings);
      }
    }
  }
}
#endif
