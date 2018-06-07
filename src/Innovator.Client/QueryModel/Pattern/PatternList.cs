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
      try
      {
        return PatternParser.SqlServer.Render(this);
      }
      catch (Exception)
      {
        var builder = new StringBuilder("'");
        for (var i = 0; i < Patterns.Count; i++)
        {
          if (i > 0) builder.Append("|");
          builder.Append(Patterns[i]);
        }
        builder.Append("'");
        return builder.ToString();
      }
    }

    public void Visit(IPatternVisitor visitor)
    {
      visitor.Visit(this);
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    public bool? AsBoolean()
    {
      throw new InvalidCastException();
    }

    public DateTime? AsDateTime()
    {
      throw new InvalidCastException();
    }

    public DateTime? AsDateTimeUtc()
    {
      throw new InvalidCastException();
    }

    public double? AsDouble()
    {
      throw new InvalidCastException();
    }

    public Guid? AsGuid()
    {
      throw new InvalidCastException();
    }

    public int? AsInt()
    {
      throw new InvalidCastException();
    }

    public long? AsLong()
    {
      throw new InvalidCastException();
    }

    public string AsString(string defaultValue)
    {
      return ToString() ?? defaultValue;
    }

    public PatternList Clone()
    {
      var result = new PatternList() { Options = Options };
      foreach (var pattern in Patterns)
        result.Patterns.Add(pattern.Clone());
      return result;
    }
  }
}
