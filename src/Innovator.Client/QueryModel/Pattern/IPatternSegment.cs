using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public interface IPatternSegment
  {
    void Visit(IPatternVisitor visitor);
  }
}
