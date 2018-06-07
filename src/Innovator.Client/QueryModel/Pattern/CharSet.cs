using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public class CharSet : IMatch
  {
    public IList<char> Chars { get; } = new List<char>();
    public bool InverseSet { get; set; }
    public Repetition Repeat { get; } = new Repetition();

    public CharSet()
    {
      this.InverseSet = false;
    }

    public CharSet(char defined)
    {
      switch (defined)
      {
        case '.':
          this.InverseSet = true;
          break;
        case 'd':
          AddRange(__digits);
          this.InverseSet = false;
          break;
        case 'D':
          AddRange(__digits);
          this.InverseSet = true;
          break;
        case 's':
          AddRange(__spaceChars);
          this.InverseSet = false;
          break;
        case 'S':
          AddRange(__spaceChars);
          this.InverseSet = true;
          break;
        case 'w':
          AddRange(__wordChars);
          this.InverseSet = false;
          break;
        case 'W':
          AddRange(__wordChars);
          this.InverseSet = true;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public void Add(char ch)
    {
      Chars.Add(ch);
    }

    public void AddRange(char start, char end)
    {
      var curr = start;
      while (curr < end)
      {
        Chars.Add(curr);
        curr++;
      }
      Chars.Add(curr);
    }

    public void AddRange(IEnumerable<char> chars)
    {
      foreach (var ch in chars)
        Chars.Add(ch);
    }

    public override string ToString()
    {
      var builder = new StringBuilder();

      if (Chars.Count == 0 && this.InverseSet)
      {
        builder.Append(".");
      }
      else if (Utils.ListEquals(Chars, __digits))
      {
        builder.Append("\\");
        builder.Append(this.InverseSet ? "D" : "d");
      }
      else if (Utils.ListEquals(Chars, __spaceChars))
      {
        builder.Append("\\");
        builder.Append(this.InverseSet ? "S" : "s");
      }
      else if (Utils.ListEquals(Chars, __wordChars))
      {
        builder.Append("\\");
        builder.Append(this.InverseSet ? "W" : "w");
      }
      else
      {
        builder.Append("[");
        if (this.InverseSet) builder.Append("^");
        for (var i = 0; i < Chars.Count; i++)
        {
          builder.Append(Chars[i]);
        }
        builder.Append("]");
      }
      builder.Append(Repeat.ToString());
      return builder.ToString();
    }

    public void Visit(IPatternVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal static char[] __digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
    internal static char[] __spaceChars = new char[] { '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u0020', '\u0085', '\u00A0', '\u1680', '\u180E', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F', '\u3000' };
    internal static char[] __wordChars = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '_' };

    public bool ContentEquals(IMatch value)
    {
      if (!(value is CharSet set))
        return false;

      return this.InverseSet == set.InverseSet && Utils.ListEquals(this.Chars, set.Chars);
    }

    public IMatch Clone()
    {
      var result = new CharSet() { InverseSet = InverseSet };
      result.AddRange(Chars);
      result.Repeat.Set(Repeat);
      return result;
    }
  }
}
