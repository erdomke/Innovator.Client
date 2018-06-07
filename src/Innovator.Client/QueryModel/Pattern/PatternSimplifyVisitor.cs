using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal class PatternSimplifyVisitor : IPatternVisitor
  {
    public void Visit(Anchor value)
    {
      // Do Nothing
    }

    public void Visit(Capture value)
    {
      value.Options.Visit(this);
    }

    public void Visit(CharSet value)
    {
      // Do Nothing
    }

    public void Visit(Pattern value)
    {
      var i = 0;
      CharSet currSet;
      while (i < value.Matches.Count)
      {
        currSet = value.Matches[i] as CharSet;
        if (currSet != null && !currSet.InverseSet && currSet.Chars.Count == 1)
        {
          // Convert single character character sets into string matches
          if (i > 0 && value.Matches[i - 1] is StringMatch)
          {
            ((StringMatch)value.Matches[i - 1]).Match.Append(currSet.Chars[0]);
            value.Matches.RemoveAt(i);
          }
          else if ((i + 1) < value.Matches.Count && value.Matches[i + 1] is StringMatch)
          {
            ((StringMatch)value.Matches[i + 1]).Match.Insert(0, currSet.Chars[0]);
            value.Matches.RemoveAt(i);
          }
          else
          {
            value.Matches[i] = new StringMatch(currSet.Chars[0]);
            i++;
          }
        }
        else if (i > 0 && value.Matches[i - 1].ContentEquals(value.Matches[i]))
        {
          // Concatenate consecutive matches together as merely a repeat.
          value.Matches[i - 1].Repeat.MinCount++;
          if (value.Matches[i - 1].Repeat.MaxCount < int.MaxValue) value.Matches[i - 1].Repeat.MaxCount++;
          value.Matches.RemoveAt(i);
        }
        else if (i > 0 && value.Matches[i - 1] is StringMatch && value.Matches[i] is StringMatch)
        {
          // Concatenate consecutive string matches together.
          ((StringMatch)value.Matches[i - 1]).Match.Append(((StringMatch)value.Matches[i]).Match);
          value.Matches.RemoveAt(i);
        }
        else
        {
          i++;
        }
      }
    }

    public void Visit(PatternList value)
    {
      foreach (var pat in value.Patterns)
      {
        pat.Visit(this);
      }
    }

    public void Visit(Repetition value)
    {
      // Do Nothing
    }

    public void Visit(StringMatch value)
    {
      // Do Nothing
    }
  }
}
