using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Json.Embed
{
  internal class JsonTextReader : IDisposable
  {
    private readonly TextReader _reader;
    private int _column;
    private int _line;
    private readonly Stack<JsonToken> _groups = new Stack<JsonToken>();
    private AllowedToken _expected = AllowedToken.GroupStart;

    [Flags()]
    private enum AllowedToken
    {
      Nothing = 0x00,
      Object = 0x01,
      Array = 0x02,
      GroupStart = Object | Array,
      Comma = 0x04,
      Colon = 0x08,
      Property = 0x10,
      String = 0x20,
      Number = 0x40,
      Boolean = 0x80,
      Null = 0x100,
      Value = String | Number | Object | Array | Boolean | Null,
      EndObject = 0x200,
      EndArray = 0x400,
      EndValue = Comma | EndObject | EndArray
    }

    /// <summary>
    /// Gets or sets a value indicating whether the source should be closed when this reader is closed.
    /// </summary>
    /// <value>
    /// <c>true</c> to close the source when this reader is closed; otherwise <c>false</c>. The default is <c>true</c>.
    /// </value>
    public bool CloseInput { get; set; }
    /// <summary>
    /// Gets the depth of the current token in the JSON document.
    /// </summary>
    /// <value>The depth of the current token in the JSON document.</value>
    public int Depth { get { return _groups.Count - (TokenType == JsonToken.StartObject || TokenType == JsonToken.StartArray ? 1 : 0); } }
    /// <summary>
    /// Gets the current line number.
    /// </summary>
    /// <value>
    /// The current line number or 0 if no line information is available.
    /// </value>
    public int LineNumber { get { return _line; } }
    /// <summary>
    /// Gets the current line position.
    /// </summary>
    /// <value>
    /// The current line position or 0 if no line information is available.
    /// </value>
    public int LinePosition { get { return _column; } }
    /// <summary>
    /// Gets the type of the current JSON token. 
    /// </summary>
    public JsonToken TokenType { get; set; } = JsonToken.None;
    /// <summary>
    /// Gets the text value of the current JSON token.
    /// </summary>
    public string Value { get; set; }

    public JsonTextReader(string text)
    {
      _reader = new StringReader(text);
    }

    public JsonTextReader(TextReader reader)
    {
      _reader = reader;
    }

    public JsonTextReader(Stream stream)
    {
      CloseInput = true;
      _reader = new StreamReader(stream);
    }

    public bool Read()
    {
      SkipWhitespace();

      var next = Peek();

      if (next == ':' && (_expected & AllowedToken.Colon) != 0)
      {
        ReadChar();
        _expected = AllowedToken.Value;
        SkipWhitespace();
        next = Peek();
      }
      else if (next == ',' && (_expected & AllowedToken.Comma) != 0 && _groups.Count > 0)
      {
        ReadChar();
        _expected = _groups.Peek() == JsonToken.StartObject ? AllowedToken.Property : AllowedToken.Value;
        SkipWhitespace();
        next = Peek();
      }

      switch (next)
      {
        case '{':
          AssertExpected(AllowedToken.Object);
          _expected = AllowedToken.Property | AllowedToken.EndObject;
          TokenType = JsonToken.StartObject;
          Value = null;
          ReadChar();
          _groups.Push(TokenType);
          return true;
        case '[':
          AssertExpected(AllowedToken.Array);
          _expected = AllowedToken.Value | AllowedToken.EndArray;
          TokenType = JsonToken.StartArray;
          Value = null;
          ReadChar();
          _groups.Push(TokenType);
          return true;
        case '}':
          if (_groups.Count < 1)
            ParseException($"Too many closing `{next}`");
          var open = _groups.Pop();
          if (open != JsonToken.StartObject)
            ParseException($"Closing character `{next}` does not match the last opening character `{Definition(open)}`");

          AssertExpected(AllowedToken.EndObject);
          _expected = AllowedToken.EndValue;
          TokenType = JsonToken.EndObject;
          Value = null;
          ReadChar();
          return true;
        case ']':
          if (_groups.Count < 1)
            ParseException($"Too many closing `{next}`");
          var open2 = _groups.Pop();
          if (open2 != JsonToken.StartArray)
            ParseException($"Closing character `{next}` does not match the last opening character `{Definition(open2)}`");

          AssertExpected(AllowedToken.EndArray);
          _expected = AllowedToken.EndValue;
          TokenType = JsonToken.EndArray;
          Value = null;
          ReadChar();
          return true;
        case '"':
          AssertExpected(AllowedToken.Property | AllowedToken.String);
          Value = ReadString();
          if ((_expected & AllowedToken.Property) != 0)
          {
            TokenType = JsonToken.PropertyName;
            _expected = AllowedToken.Colon;
          }
          else
          {
            TokenType = JsonToken.String;
            _expected = AllowedToken.EndValue;
          }
          return true;
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
        case '-':
          AssertExpected(AllowedToken.Number);
          _expected = AllowedToken.EndValue;
          TokenType = JsonToken.Number;
          Value = ReadNumber();
          return true;
        case 't':
        case 'f':
          AssertExpected(AllowedToken.Boolean);
          _expected = AllowedToken.EndValue;
          TokenType = JsonToken.Boolean;
          Value = ReadBoolean();
          return true;
        case 'n':
          AssertExpected(AllowedToken.Null);
          _expected = AllowedToken.EndValue;
          TokenType = JsonToken.Null;
          Value = ReadNull();
          return true;
        case '\0':
          if (_groups.Count > 0)
            ParseException($"The last group `{Definition(_groups.Peek())}` is not closed");
          TokenType = JsonToken.None;
          Value = null;
          return false;
        default:
          ParseException();
          return false;
      }
    }

    private void AssertExpected(AllowedToken match)
    {
      if ((_expected & match) == 0)
      {
        ParseException("Expected a " + string.Join(", ", Definition(_expected).ToArray())
          + ", but found a "
          + string.Join(", ", Definition(match).ToArray()));
      }
    }

    private static char Definition(JsonToken token)
    {
      switch (token)
      {
        case JsonToken.EndArray:
          return ']';
        case JsonToken.EndObject:
          return '}';
        case JsonToken.StartArray:
          return '[';
        case JsonToken.StartObject:
          return '{';
        default:
          return ' ';
      }
    }

    private static IEnumerable<string> Definition(AllowedToken match)
    {
      var parts = new List<string>();

      if ((match & AllowedToken.Array) != 0)
        parts.Add("`[`");
      if ((match & AllowedToken.Boolean) != 0)
        parts.Add("boolean");
      if ((match & AllowedToken.Colon) != 0)
        parts.Add("`:`");
      if ((match & AllowedToken.Comma) != 0)
        parts.Add("`,`");
      if ((match & AllowedToken.EndObject) != 0)
        parts.Add("`}`");
      if ((match & AllowedToken.EndArray) != 0)
        parts.Add("`]`");
      if ((match & AllowedToken.Null) != 0)
        parts.Add("`null`");
      if ((match & AllowedToken.Number) != 0)
        parts.Add("number");
      if ((match & AllowedToken.Object) != 0)
        parts.Add("`{`");
      if ((match & AllowedToken.Property) != 0)
        parts.Add("property name");
      if ((match & AllowedToken.String) != 0)
        parts.Add("string");

      return parts;
    }

    private void ParseException(string message = null)
    {
      throw new JsonReaderException(this, message ?? "Invalid or unexpected character");
    }

    private string ReadNumber()
    {
      var builder = new StringBuilder();

      if (Peek() == '-')
      {
        builder.Append(ReadChar());
      }

      if (Peek() == '0')
      {
        builder.Append(ReadChar());
      }
      else
      {
        ReadDigits(builder);
      }

      if (Peek() == '.')
      {
        builder.Append(ReadChar());
        ReadDigits(builder);
      }

      if (Peek() == 'e' || Peek() == 'E')
      {
        builder.Append(ReadChar());

        switch (Peek())
        {
          case '+':
          case '-':
            builder.Append(ReadChar());
            break;
        }

        ReadDigits(builder);
      }

      return builder.ToString();
    }

    private void ReadDigits(StringBuilder builder)
    {
      var foundDigits = false;
      while (char.IsDigit(Peek()))
      {
        builder.Append(ReadChar());
        foundDigits = true;
      }
      if (!foundDigits)
        ParseException("Insufficient digits found when parsing a number");
    }

    private string ReadBoolean()
    {
      switch (Peek())
      {
        case 't':
          return Assert("true");
        case 'f':
          return Assert("false");
        default:
          ParseException();
          return null;
      }
    }

    private string ReadNull()
    {
      return Assert("null");
    }

    private string ReadString()
    {
      var builder = new StringBuilder();

      Assert("\"");

      while (true)
      {
        var c = ReadChar();

        if (c == '\\')
        {
          c = ReadChar();

          switch (c)
          {
            case '"':  // "
            case '\\': // \
            case '/':  // /
              builder.Append(c);
              break;
            case 'b':
              builder.Append('\b');
              break;
            case 'f':
              builder.Append('\f');
              break;
            case 'n':
              builder.Append('\n');
              break;
            case 'r':
              builder.Append('\r');
              break;
            case 't':
              builder.Append('\t');
              break;
            case 'u':
              builder.Append(ReadUnicodeLiteral());
              break;
            default:
              ParseException();
              break;
          }
        }
        else if (c == '"')
        {
          break;
        }
        else if (c == '\0')
        {
          ParseException("Unterminated string literal");
        }
        else
        {
          if (char.IsControl(c))
            ParseException("Control character present in string literal");
          else
            builder.Append(c);
        }
      }

      return builder.ToString();
    }

    private char ReadUnicodeLiteral()
    {
      int value = 0;

      value += ReadHexDigit() * 4096; // 16^3
      value += ReadHexDigit() * 256;  // 16^2
      value += ReadHexDigit() * 16;   // 16^1
      value += ReadHexDigit();        // 16^0

      return (char)value;
    }

    private int ReadHexDigit()
    {
      switch (ReadChar())
      {
        case '0':
          return 0;

        case '1':
          return 1;

        case '2':
          return 2;

        case '3':
          return 3;

        case '4':
          return 4;

        case '5':
          return 5;

        case '6':
          return 6;

        case '7':
          return 7;

        case '8':
          return 8;

        case '9':
          return 9;

        case 'a':
        case 'A':
          return 10;

        case 'b':
        case 'B':
          return 11;

        case 'c':
        case 'C':
          return 12;

        case 'd':
        case 'D':
          return 13;

        case 'e':
        case 'E':
          return 14;

        case 'f':
        case 'F':
          return 15;

        default:
          ParseException();
          return -1;
      }
    }

    #region Read Characters
    private char ReadChar()
    {
      var next = _reader.Read();
      if (next == -1)
        return '\0';
      if (_line == 0)
        _line = 1;

      switch (next)
      {
        case '\r':
          // Normalize '\r\n' line encoding to '\n'.
          if (_reader.Peek() == '\n')
          {
            _reader.Read();
          }
          goto case '\n';

        case '\n':
          _line++;
          _column = 0;
          return '\n';

        default:
          _column++;
          return (char)next;
      }
    }

    private char Peek()
    {
      var next = _reader.Peek();
      if (next == -1)
        return '\0';
      return (char)next;
    }

    private void SkipWhitespace()
    {
      while (char.IsWhiteSpace(Peek()))
        ReadChar();
    }

    /// <summary>
    /// Verifies that the given string matches the next characters in the stream.
    /// If the strings do not match, an exception will be thrown.
    /// </summary>
    /// <param name="next">The expected string.</param>
    public string Assert(string next)
    {
      for (var i = 0; i < next.Length; i += 1)
      {
        if (Peek() == next[i])
          ReadChar();
        else
          ParseException($"Parser expected '{next}'");
      }
      return next;
    }

    public void Dispose()
    {
      if (CloseInput)
        _reader.Dispose();
    }
    #endregion

    public IEnumerable<KeyValuePair<string, object>> Flatten()
    {
      var path = new Stack<object>();

      while (this.Read())
      {
        switch (this.TokenType)
        {
          case JsonToken.StartArray:
            if (path.Count == 0)
              path.Push("$");
            path.Push(0);
            break;
          case JsonToken.StartObject:
            if (path.Count == 0)
              path.Push("$");
            break;
          case JsonToken.PropertyName:
            path.Push(this.Value);
            break;
          case JsonToken.Null:
            yield return new KeyValuePair<string, object>(ToJsonPath(path), null);
            ConsumePath(path);
            break;
          case JsonToken.Number:
            if (int.TryParse(Value, out var intValue))
              yield return new KeyValuePair<string, object>(ToJsonPath(path), intValue);
            else if (double.TryParse(Value, out var dblValue))
              yield return new KeyValuePair<string, object>(ToJsonPath(path), dblValue);
            else
              throw new InvalidOperationException();
            ConsumePath(path);
            break;
          case JsonToken.Boolean:
            if (string.Equals(Value, "true", StringComparison.OrdinalIgnoreCase))
              yield return new KeyValuePair<string, object>(ToJsonPath(path), true);
            else if (string.Equals(Value, "false", StringComparison.OrdinalIgnoreCase))
              yield return new KeyValuePair<string, object>(ToJsonPath(path), false);
            else
              throw new InvalidOperationException();
            ConsumePath(path);
            break;
          case JsonToken.String:
            yield return new KeyValuePair<string, object>(ToJsonPath(path), Value);
            ConsumePath(path);
            break;
          case JsonToken.EndArray:
            path.Pop();
            break;
          case JsonToken.EndObject:
            ConsumePath(path);
            break;
        }
      }
    }

    private void ConsumePath(Stack<object> path)
    {
      if (path.Peek() is string)
      {
        path.Pop();
      }
      else if (path.Peek() is int index)
      {
        path.Pop();
        path.Push(index + 1);
      }

    }

    //https://goessner.net/articles/JsonPath/index.html#e2
    private string ToJsonPath(Stack<object> path)
    {
      var builder = new StringBuilder();
      foreach (var part in path.Reverse())
      {
        if (part is int)
        {
          builder.Append('[').Append(part).Append(']');
        }
        else
        {
          if (builder.Length > 0)
            builder.Append('.');
          builder.Append(part);
        }
      }
      return builder.ToString();
    }
  }
}
