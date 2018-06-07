using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public class DateOffsetLiteral : DateTimeLiteral
  {
    public IServerContext Context { get; }

    public bool IsEnd { get; }

    public DateOffset Offset { get; }

    public override DateTime Value
    {
      get { return Offset.AsDate(Context.Now(), IsEnd); }
      set { throw new NotSupportedException(); }
    }

    public DateOffsetLiteral(IServerContext context, DateOffset offset, bool isEnd)
    {
      Context = context;
      Offset = offset;
      IsEnd = isEnd;
    }
  }
}
