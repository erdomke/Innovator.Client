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
      this.AllowCharSet = inverseSet != '\0' || setRange != '\0';
      this.Pattern_InverseSet = inverseSet;
      this.Pattern_SetRange = setRange;
    }

    public virtual PatternList Parse(string str)
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
        else if (str[i] == '[' && !inRange && AllowCharSet)
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
      patOpts.Visit(RegexParser.Simplify);
      return patOpts;
    }

    public virtual string Render(PatternList pattern)
    {
      var writer = new SqlPatternWriter(this);
      pattern.Visit(writer);
      return writer.ToString();
    }

    private void FinishStringMatch()
    {
      if (_strMatch.Match.Length > 0)
      {
        _pat.Matches.Add(_strMatch);
        _strMatch = new StringMatch();
      }
    }

    //public static PatternMatchDefinition DefaultDefn = new PatternMatchDefinition('%', '_', '\0', '\0');
    //public static PatternMatchDefinition AccessDefn = new PatternMatchDefinition('*', '?', '#', '\0', '!', '-');
    //public static PatternMatchDefinition WqlDefn = new PatternMatchDefinition('%', '_', '\0', '\0', '^', '=');
    internal static PatternParser SqlServer = new PatternParser('%', '_', '\0', '\0', '^', '-');
    //public static PatternMatchDefinition MySqlDefn = new PatternMatchDefinition('%', '_', '\0', '\\');
  }
}
