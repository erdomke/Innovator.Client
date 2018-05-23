using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class InOperator : IOperator, IBooleanOperator, INormalize
  {
    public IExpression Left { get; set; }
    public ListExpression Right { get; set; }

    public int Precedence => (int)PrecedenceLevel.Comparison;
    QueryItem ITableProvider.Table { get; set; }

    public IExpression ToConditional()
    {
      var list = Right;
      if (list.Values.Count < 1)
        throw new NotSupportedException();
      var result = (IExpression)new EqualsOperator()
      {
        Left = Left,
        Right = list.Values[0]
      }.Normalize();
      foreach (var value in list.Values.Skip(1))
      {
        result = new OrOperator()
        {
          Left = result,
          Right = new EqualsOperator()
          {
            Left = Left,
            Right = value
          }.Normalize()
        }.Normalize();
      }
      return result;
    }

    public virtual void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public override string ToString()
    {
      return this.ToSqlString();
    }

    public virtual IExpression Normalize()
    {
      if (Left is ITableProvider tbl)
        ((ITableProvider)this).Table = tbl.Table;
      return this;
    }
  }
}
