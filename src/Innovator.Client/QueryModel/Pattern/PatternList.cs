using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Innovator.Client.QueryModel
{
  public class PatternList : IPatternSegment, ILiteral
  {
    public RegexOptions Options { get; set; }
    public IList<Pattern> Patterns { get; } = new List<Pattern>();

    public override string ToString()
    {
      var builder = new StringBuilder("'");
      for (var i = 0; i < Patterns.Count; i++)
      {
        if (i > 0) builder.Append("|");
        builder.Append(Patterns[i]).ToString();
      }
      builder.Append("'");
      return builder.ToString();
    }

    public void Visit(IPatternVisitor visitor)
    {
      visitor.Visit(this);
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
