using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public class PatternParser
  {
    internal char Pattern_Anything { get; }
    internal char Pattern_SingleChar { get; }
    internal char Pattern_SingleDigit { get; }
    internal char Pattern_InverseSet { get; }
    internal char Pattern_SetRange { get; }
    internal char Pattern_Escape { get; }
    internal bool AllowCharSet { get; }

    private Pattern _pat;
    private StringMatch _strMatch;
    private CharSet _set;
    private readonly PatternSimplifyVisitor _simplify = new PatternSimplifyVisitor();

    public PatternParser() { }
    public PatternParser(char anything, char singleChar, char singleDigit, char escape)
    {
      this.Pattern_Anything = anything;
      this.Pattern_SingleChar = singleChar;
      this.Pattern_SingleDigit = singleDigit;
      this.Pattern_Escape = escape;
      this.AllowCharSet = false;
      this.Pattern_InverseSet = '\0';
      this.Pattern_SetRange = '\0';
    }
    public PatternParser(char anything, char singleChar, char singleDigit, char escape, char inverseSet, char setRange)
    {
      this.Pattern_Anything = anything;
      this.Pattern_SingleChar = singleChar;
      this.Pattern_SingleDigit = singleDigit;
      this.Pattern_Escape = escape;
      this.AllowCharSet = true;
      this.Pattern_InverseSet = inverseSet;
      this.Pattern_SetRange = setRange;
    }

    public PatternList Parse(string str)
    {
      _pat = new Pattern();
      _strMatch = new StringMatch();
      Anchor endAnchor = null;

      if (string.IsNullOrEmpty(str)) return new PatternList();

      if (str[0] == this.Pattern_Anything)
      {
        str = str.TrimStart(this.Pattern_Anything);
      }
      else
      {
        _pat.Matches.Add(new Anchor() { Type = AnchorType.Start_Absolute });
      }
      if (!string.IsNullOrEmpty(str) && str[str.Length - 1] == this.Pattern_Anything)
      {
        str = str.TrimEnd(this.Pattern_Anything);
      }
      else
      {
        endAnchor = new Anchor() { Type = AnchorType.End_Absolute };
      }

      var i = 0;
      bool inRange = false;
      while (i < str.Length)
      {
        if (inRange)
        {
          if (str[i] == ']')
          {
            inRange = false;
            _pat.Matches.Add(_set);
            _set = null;
          }
          else if (_set.Chars.Count > 0 && str[i] == this.Pattern_SetRange && (i + 1) < str.Length && str[i + 1] != ']')
          {
            _set.AddRange((char)(_set.Chars.Last() + 1), str[i + 1]);
            i++;
          }
          else
          {
            _set.Chars.Add(str[i]);
          }
        }
        else if (str[i] == this.Pattern_Anything)
        {
          FinishStringMatch();
          _set = new CharSet('.');
          _set.Repeat.MinCount = 0;
          _set.Repeat.MaxCount = int.MaxValue;
          _pat.Matches.Add(_set);
          _set = null;
        }
        else if (str[i] == this.Pattern_SingleChar)
        {
          FinishStringMatch();
          _set = new CharSet('.');
          _set.Repeat.MinCount = 1;
          _set.Repeat.MaxCount = 1;
          _pat.Matches.Add(_set);
          _set = null;
        }
        else if (str[i] == this.Pattern_SingleDigit)
        {
          FinishStringMatch();
          _set = new CharSet('d');
          _set.Repeat.MinCount = 1;
          _set.Repeat.MaxCount = 1;
          _pat.Matches.Add(_set);
          _set = null;
        }
        else if (str[i] == '[' && !inRange)
        {
          FinishStringMatch();
          inRange = true;
          _set = new CharSet();
          if ((i + 1) < str.Length && str[i + 1] == this.Pattern_InverseSet)
          {
            _set.InverseSet = true;
            i++;
          }
        }
        else if (str[i] == this.Pattern_Escape)
        {
          i++;
          _strMatch.Match.Append(str[i]);
        }
        else
        {
          _strMatch.Match.Append(str[i]);
        }
        i++;
      }

      if (_strMatch.Match.Length > 0) _pat.Matches.Add(_strMatch);
      if (endAnchor != null) _pat.Matches.Add(endAnchor);

      var patOpts = new PatternList();
      patOpts.Patterns.Add(_pat);
      patOpts.Visit(_simplify);
      return patOpts;
    }

    private void FinishStringMatch()
    {
      if (_strMatch.Match.Length > 0)
      {
        _pat.Matches.Add(_strMatch);
        _strMatch = new StringMatch();
      }
    }

    private class PatternSimplifyVisitor : IPatternVisitor
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

    //public static PatternMatchDefinition DefaultDefn = new PatternMatchDefinition('%', '_', '\0', '\0');
    //public static PatternMatchDefinition AccessDefn = new PatternMatchDefinition('*', '?', '#', '\0', '!', '-');
    //public static PatternMatchDefinition WqlDefn = new PatternMatchDefinition('%', '_', '\0', '\0', '^', '=');
    internal static PatternParser SqlServer = new PatternParser('%', '_', '\0', '\0', '^', '-');
    //public static PatternMatchDefinition MySqlDefn = new PatternMatchDefinition('%', '_', '\0', '\\');
  }
}
