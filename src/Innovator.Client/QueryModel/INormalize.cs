using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal interface INormalize : IExpression
  {
    IExpression Normalize();
  }
}
