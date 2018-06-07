using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public class StringMatch : IMatch
  {
    public StringBuilder Match { get; }
    public Repetition Repeat { get; } = new Repetition();

    public StringMatch() : this("") { }
    public StringMatch(string value)
    {
      Match = new StringBuilder(value);
    }
    public StringMatch(char value)
    {
      Match = new StringBuilder(value);
    }

    public override string ToString()
    {
      return Match.ToString() + Repeat;
    }

    public void Visit(IPatternVisitor visitor)
    {
      visitor.Visit(this);
    }

    public bool ContentEquals(IMatch value)
    {
      if (!(value is StringMatch str))
        return false;
      return str.Match.ToString() == Match.ToString();
    }

    public IMatch Clone()
    {
      var result = new StringMatch();
      result.Match.Append(Match.ToString());
      result.Repeat.Set(Repeat);
      return result;
    }
  }
}
