using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Innovator.Client.QueryModel
{
  internal class SqlPatternWriter : IPatternVisitor
  {
    private enum WriteState
    {
      NotStarted,
      InProgress,
      Finished
    }

    private readonly PatternParser _defn;
    private WriteState _state = WriteState.NotStarted;
    private readonly TextWriter _writer;

    public char EscapeUsed { get; private set; } = '\0';

    public SqlPatternWriter(PatternParser defn) : this(new StringWriter(), defn) { }
    public SqlPatternWriter(TextWriter writer, PatternParser defn)
    {
      _writer = writer;
      _defn = defn;
    }

    public void Visit(Anchor value)
    {
      switch (value.Type)
      {
        case AnchorType.End_Absolute:
        case AnchorType.End_BeforeNewline:
        case AnchorType.End_Line:
          if (_state == WriteState.InProgress)
          {
            _state = WriteState.Finished;
            return;
          }
          break;
        case AnchorType.Start_Absolute:
        case AnchorType.Start_Line:
          if (_state == WriteState.NotStarted)
          {
            _state = WriteState.InProgress;
            return;
          }
          break;
      }
      throw new NotSupportedException();
    }

    public void Visit(Capture value)
    {
      throw new NotSupportedException();
    }

    public void Visit(CharSet value)
    {
      StartWriting();
      if (value.InverseSet && value.Chars.Count == 0)
      {
        for (var i = 0; i < value.Repeat.MinCount; i++)
        {
          _writer.Write(_defn.Pattern_SingleChar);
        }
        if (value.Repeat.MaxCount == value.Repeat.MinCount)
        {
          // Do Nothing
        }
        else if (value.Repeat.MaxCount == int.MaxValue)
        {
          _writer.Write(_defn.Pattern_Anything);
        }
        else
        {
          throw new NotSupportedException();
        }
      }
      else if (_defn.Pattern_SingleDigit != '\0' && !value.InverseSet && Utils.ListEquals(value.Chars, CharSet.__digits) &&
        value.Repeat.MinCount >= 1 && value.Repeat.MinCount < int.MaxValue && value.Repeat.MinCount == value.Repeat.MaxCount)
      {
        for (var j = 0; j < value.Repeat.MinCount; j++)
        {
          _writer.Write(_defn.Pattern_SingleDigit);
        }
      }
      else if (_defn.AllowCharSet && value.Repeat.MinCount >= 1 && value.Repeat.MinCount < int.MaxValue && value.Repeat.MinCount == value.Repeat.MaxCount)
      {
        bool inRange = false;

        for (var j = 0; j < value.Repeat.MinCount; j++)
        {
          _writer.Write("[");
          if (value.InverseSet) _writer.Write(_defn.Pattern_InverseSet);
          inRange = false;
          for (var i = 0; i < value.Chars.Count; i++)
          {
            if (i > 0)
            {
              if (value.Chars[i] == value.Chars[i - 1] + 1)
              {
                if (!inRange) _writer.Write(_defn.Pattern_SetRange);
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
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    public void Visit(Pattern value)
    {
      _state = WriteState.NotStarted;
      foreach (var match in value.Matches)
      {
        match.Visit(this);
      }
      if (_state == WriteState.InProgress)
      {
        _writer.Write(_defn.Pattern_Anything);
        _state = WriteState.Finished;
      }
    }

    public void Visit(PatternList value)
    {
      if (value.Patterns.Count > 1) throw new NotSupportedException();
      if (value.Patterns.Count > 0) value.Patterns[0].Visit(this);
    }

    public void Visit(Repetition value)
    {
      throw new NotSupportedException();
    }

    public virtual void Visit(StringMatch value)
    {
      StartWriting();

      if (value.Repeat.MinCount != 1 || value.Repeat.MaxCount != 1) throw new NotSupportedException();
      char escape = _defn.Pattern_Escape;
      if (escape == '\0') escape = EscapeUsed;
      var str = value.Match.ToString();

      if (_defn.AllowCharSet)
      {
        str = str.Replace("[", "[[]")
          .Replace(_defn.Pattern_Anything.ToString(), "[" + _defn.Pattern_Anything.ToString() + "]");
        if (_defn.Pattern_SingleChar != '\0')
          str = str.Replace(_defn.Pattern_SingleChar.ToString(), "[" + _defn.Pattern_SingleChar.ToString() + "]");
        if (_defn.Pattern_SingleDigit != '\0')
          str = str.Replace(_defn.Pattern_SingleDigit.ToString(), "[" + _defn.Pattern_SingleDigit.ToString() + "]");
        _writer.Write(str);
      }
      else
      {
        // I need to do escaping if I have an escape character
        bool needEscape = (escape != '\0');

        // If I don't think I need to escape, think again
        if (!needEscape)
        {
          needEscape = (str.IndexOf(_defn.Pattern_Anything) >= 0);
          needEscape = needEscape || (str.IndexOf(_defn.Pattern_SingleChar) >= 0);
          needEscape = needEscape || (_defn.Pattern_SingleDigit != '\0' && str.IndexOf(_defn.Pattern_SingleDigit) >= 0);
          if (needEscape)
          {
            EscapeUsed = '`';
            escape = '`';
          }
        }

        if (needEscape)
        {
          str = str.Replace(_defn.Pattern_Anything.ToString(), escape + _defn.Pattern_Anything.ToString()).
                    Replace(_defn.Pattern_SingleChar.ToString(), escape + _defn.Pattern_SingleChar.ToString()).
                    Replace(escape.ToString(), escape.ToString() + escape);
          if (_defn.Pattern_SingleDigit != '\0') str = str.Replace(_defn.Pattern_SingleDigit.ToString(), escape + _defn.Pattern_SingleDigit.ToString());
        }
        _writer.Write(str);
      }
    }

    private void StartWriting()
    {
      switch (_state)
      {
        case WriteState.NotStarted:
          _writer.Write(_defn.Pattern_Anything);
          _state = WriteState.InProgress;
          break;
        case WriteState.Finished:
          throw new NotSupportedException();
      }
    }

    public override string ToString()
    {
      return _writer.ToString();
    }
  }
}
