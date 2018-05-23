using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public class Capture : IMatch
  {
    private Repetition _repeat = new Repetition();

    public Repetition Repeat
    {
      get
      {
        return _repeat;
      }
    }
    public PatternList Options { get; set; }

    public override string ToString()
    {
      return "(" + this.Options.ToString() + ")" + _repeat.ToString();
    }

    public void Visit(IPatternVisitor visitor)
    {
      visitor.Visit(this);
    }
    public bool ContentEquals(IMatch value)
    {
      return false;
    }
  }
}
