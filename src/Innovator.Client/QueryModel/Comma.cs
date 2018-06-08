using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal class Comma : BinaryOperator
  {
    public override int Precedence => (int)PrecedenceLevel.Comma;

    public override void Visit(IExpressionVisitor visitor)
    {
      throw new NotSupportedException();
    }
  }
}
