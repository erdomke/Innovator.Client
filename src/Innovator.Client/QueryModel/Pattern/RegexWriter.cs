using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Innovator.Client.QueryModel
{
  public class RegexWriter : IPatternVisitor
  {
    private TextWriter _writer;

    public RegexWriter() : this(new StringWriter()) { }
    public RegexWriter(TextWriter writer)
    {
      _writer = writer;
    }

    public void Visit(Anchor value)
    {
      switch (value.Type)
      {
        case AnchorType.End_Absolute:
          _writer.Write(@"\z");
          break;
        case AnchorType.End_BeforeNewline:
          _writer.Write(@"\Z");
          break;
        case AnchorType.End_Line:
          _writer.Write(@"$");
          break;
        case AnchorType.Start_Absolute:
          _writer.Write(@"\A");
          break;
        case AnchorType.Start_Line:
          _writer.Write(@"^");
          break;
        case AnchorType.WordBoundary:
          _writer.Write(@"\b");
          break;
      }
    }

    public void Visit(Capture value)
    {
      _writer.Write("(");
      value.Options.Visit(this);
      _writer.Write(")");
      value.Repeat.Visit(this);
    }

    public void Visit(CharSet value)
    {
      if (value.Chars.Count == 0 && value.InverseSet)
      {
        _writer.Write(".");
      }
      else if (Utils.ListEquals(value.Chars, CharSet.__digits))
      {
        _writer.Write("\\");
        _writer.Write(value.InverseSet ? "D" : "d");
      }
      else if (Utils.ListEquals(value.Chars, CharSet.__spaceChars))
      {
        _writer.Write("\\");
        _writer.Write(value.InverseSet ? "S" : "s");
      }
      else if (Utils.ListEquals(value.Chars, CharSet.__wordChars))
      {
        _writer.Write("\\");
        _writer.Write(value.InverseSet ? "W" : "w");
      }
      else
      {
        bool inRange = false;

        _writer.Write("[");
        if (value.InverseSet) _writer.Write("^");
        for (var i = 0; i < value.Chars.Count; i++)
        {
          if (i > 0)
          {
            if (value.Chars[i] == value.Chars[i - 1] + 1)
            {
              if (!inRange) _writer.Write("-");
              inRange = true;
            }
            else if (inRange)
            {
              _writer.Write(value.Chars[i - 1]);
              inRange = false;
            }
          }

          if (!inRange) _writer.Write(value.Chars[i]);
        }
        if (inRange) _writer.Write(value.Chars[value.Chars.Count - 1]);

        _writer.Write("]");
      }
      value.Repeat.Visit(this);
    }

    public void Visit(Pattern value)
    {
      foreach (var match in value.Matches)
      {
        match.Visit(this);
      }
    }

    public void Visit(PatternList value)
    {
      for (var i = 0; i < value.Patterns.Count; i++)
      {
        if (i > 0) _writer.Write("|");
        value.Patterns[i].Visit(this);
      }
    }

    public void Visit(Repetition value)
    {
      if (value.MinCount == 1 && value.MaxCount == 1)
      {
        // Do Nothing
      }
      else if (value.MinCount == 0 && value.MaxCount == 1)
      {
        _writer.Write("?");
      }
      else if (value.MinCount == 1 && value.MaxCount == int.MaxValue)
      {
        _writer.Write("+");
      }
      else if (value.MinCount == 0 && value.MaxCount == int.MaxValue)
      {
        _writer.Write("*");
      }
      else if (value.MinCount == value.MaxCount)
      {
        _writer.Write("{");
        _writer.Write(value.MinCount);
        _writer.Write("}");
      }
      else if (value.MaxCount == int.MaxValue)
      {
        _writer.Write("{");
        _writer.Write(value.MinCount);
        _writer.Write(",}");
      }
      else
      {
        _writer.Write("{");
        _writer.Write(value.MinCount);
        _writer.Write(",");
        _writer.Write(value.MaxCount);
        _writer.Write("}");
      }

      if (!value.Greedy) _writer.Write("?");
    }

    public void Visit(StringMatch value)
    {
      for (var i = 0; i < value.Match.Length; i++)
      {
        switch (value.Match[i])
        {
          case '\\':
          case '.':
          case '?':
          case '+':
          case '*':
          case '(':
          case ')':
          case '[':
          case ']':
          case '{':
          case '}':
          case '|':
            _writer.Write('\\');
            break;
        }
        _writer.Write(value.Match[i]);
      }
      value.Repeat.Visit(this);
    }

    public override string ToString()
    {
      return _writer.ToString();
    }
  }
}
