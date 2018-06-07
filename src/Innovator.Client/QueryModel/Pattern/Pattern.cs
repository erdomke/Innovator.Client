using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public class Pattern : IPatternSegment
  {
    public IList<IMatch> Matches { get; } = new List<IMatch>();

    public override string ToString()
    {
      var builder = new StringBuilder();
      foreach (var match in Matches)
      {
        builder.Append(match.ToString());
      }
      return builder.ToString();
    }

    public void Visit(IPatternVisitor visitor)
    {
      visitor.Visit(this);
    }

    public Pattern Clone()
    {
      var result = new Pattern();
      foreach (var match in Matches)
        result.Matches.Add(match.Clone());
      return result;
    }
  }
}
