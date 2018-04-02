using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client.QueryModel
{
  public class Query : IAmlNode
  {
    private List<IExpression> _select = new List<IExpression>();
    private List<OrderByExpression> _orderBy = new List<OrderByExpression>();

    public IList<IExpression> Select { get { return _select; } }
    public ITableOperand From { get; set; }
    public IExpression Where { get; set; }
    public IList<OrderByExpression> OrderBy { get { return _orderBy; } }

    public void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      throw new NotImplementedException();
    }
  }
}
