using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public interface IMatch : IPatternSegment
  {
    Repetition Repeat { get; }
    IMatch Clone();
    bool ContentEquals(IMatch value);
  }
}
