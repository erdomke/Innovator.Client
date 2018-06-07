using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public class Capture : IMatch
  {
    public Repetition Repeat { get; } = new Repetition();
    public PatternList Options { get; } = new PatternList();

    public override string ToString()
    {
      return "(" + this.Options + ")" + Repeat;
    }

    public void Visit(IPatternVisitor visitor)
    {
      visitor.Visit(this);
    }

    public bool ContentEquals(IMatch value)
    {
      return false;
    }

    public IMatch Clone()
    {
      var result = new Capture();
      result.Options.Options = Options.Options;
      foreach (var pattern in Options.Patterns)
        result.Options.Patterns.Add(pattern.Clone());
      result.Repeat.Set(Repeat);
      return result;
    }
  }
}
