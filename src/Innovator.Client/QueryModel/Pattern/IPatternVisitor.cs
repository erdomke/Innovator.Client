using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public interface IPatternVisitor
  {
    void Visit(Anchor value);
    void Visit(Capture value);
    void Visit(CharSet value);
    void Visit(Pattern value);
    void Visit(PatternList value);
    void Visit(Repetition value);
    void Visit(StringMatch value);
  }
}
