using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal class RegexParser
  {
    internal static PatternSimplifyVisitor Simplify { get; } = new PatternSimplifyVisitor();

    public static PatternList Parse(string pattern)
    {
      var stack = new Stack<PatternList>();
      stack.Push(new PatternList()
      {
        Patterns = { new Pattern() }
      });

      var last = "";
      Repetition repeat;
      var inCharSet = false;
      foreach (var token in Tokenizer(pattern))
      {
        if (inCharSet)
        {
          if (token.Value == "]" && token.IsOperator)
          {
            inCharSet = false;
          }
          else
          {
            var set = new CharSet();
            for (var i = 0; i < token.Value.Length; i++)
            {
              if (i < token.Value.Length - 2 && token.Value[i + 1] == '-')
              {
                set.AddRange(token.Value[i], token.Value[i + 2]);
                i += 2;
              }
              else
              {
                set.Add(token.Value[i]);
              }
            }
            set.InverseSet = last == "[^";
            stack.Peek().Patterns.Last().Matches.Add(set);
          }
        }
        else if (token.IsOperator)
        {
          switch (token.Value)
          {
            case "^":
            case "$":
            case @"\A":
            case @"\b":
            case @"\B":
            case @"\G":
            case @"\Z":
            case @"\z":
              stack.Peek().Patterns.Last().Matches.Add(new Anchor(token.Value[token.Value.Length - 1]));
              break;
            case "(":
              var capture = new Capture();
              capture.Options.Patterns.Add(new Pattern());
              stack.Peek().Patterns.Last().Matches.Add(capture);
              stack.Push(capture.Options);
              break;
            case ")":
              stack.Pop();
              break;
            case "|":
              stack.Peek().Patterns.Add(new Pattern());
              break;
            case "{":
            case "}":
              // Do nothing
              break;
            case "[":
            case "[^":
              inCharSet = true;
              break;
            case ".":
            case @"\d":
            case @"\D":
            case @"\s":
            case @"\S":
            case @"\w":
            case @"\W":
              stack.Peek().Patterns.Last().Matches.Add(new CharSet(token.Value[token.Value.Length - 1]));
              break;
            case "*":
            case "*?":
              repeat = stack.Peek().Patterns.Last().Matches.Last().Repeat;
              repeat.MinCount = 0;
              repeat.MaxCount = int.MaxValue;
              repeat.Greedy = token.Value.Length == 1;
              break;
            case "+":
            case "+?":
              repeat = stack.Peek().Patterns.Last().Matches.Last().Repeat;
              repeat.MinCount = 1;
              repeat.MaxCount = int.MaxValue;
              repeat.Greedy = token.Value.Length == 1;
              break;
            case "?":
              repeat = stack.Peek().Patterns.Last().Matches.Last().Repeat;
              if (last == "}")
              {
                repeat.Greedy = false;
              }
              else
              {
                repeat.MinCount = 0;
                repeat.MaxCount = 1;
                repeat.Greedy = true;
              }
              break;
            case "??":
              repeat = stack.Peek().Patterns.Last().Matches.Last().Repeat;
              repeat.MinCount = 0;
              repeat.MaxCount = 1;
              repeat.Greedy = false;
              break;
            default:
              throw new NotSupportedException();
          }
        }
        else
        {
          if (last == "{")
          {
            var times = token.Value.Split(',').ToArray();
            repeat = stack.Peek().Patterns.Last().Matches.Last().Repeat;
            repeat.MinCount = int.Parse(times[0]);
            if (times.Length == 1)
              repeat.MaxCount = repeat.MinCount;
            else
              repeat.MaxCount = string.IsNullOrEmpty(times[1]) ? int.MaxValue : int.Parse(times[1]);
          }
          else
          {
            stack.Peek().Patterns.Last().Matches.Add(new StringMatch(token.Value));
          }
        }
        last = token.Value;
      }

      if (stack.Count != 1)
        throw new InvalidOperationException();
      var result = stack.Pop();
      result.Visit(Simplify);
      return result;
    }

    private static IEnumerable<Token> Tokenizer(string pattern)
    {
      var buffer = new StringBuilder();
      var inCharGroup = false;

      for (var i = 0; i < pattern.Length; i++)
      {
        switch (pattern[i])
        {
          case '\\':
            i++;
            switch (pattern[i])
            {
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
                buffer.Append((char)Convert.ToInt32(pattern.Substring(i, 3), 8));
                i += 2;
                break;
              case 'a':
                buffer.Append('\a');
                break;
              case 'A':
              case 'B':
              case 'd':
              case 'D':
              case 'G':
              case 's':
              case 'S':
              case 'w':
              case 'W':
              case 'z':
              case 'Z':
                if (buffer.Length > 0)
                  yield return new Token(buffer.ToString(), false);
                buffer.Length = 0;
                yield return new Token("\\" + pattern[i], true);
                break;
              case 'b':
                if (inCharGroup)
                {
                  buffer.Append('\b');
                }
                else
                {
                  if (buffer.Length > 0)
                    yield return new Token(buffer.ToString(), false);
                  buffer.Length = 0;
                  yield return new Token("\\" + pattern[i], true);
                }
                break;
              case 'c':
                throw new NotSupportedException();
              case 'e':
                buffer.Append('\u001B');
                break;
              case 'f':
                buffer.Append('\f');
                break;
              case 'n':
                buffer.Append('\n');
                break;
              case 'p':
              case 'P':
                if (pattern[i + 1] != '{')
                  throw new InvalidOperationException();
                if (buffer.Length > 0)
                  yield return new Token(buffer.ToString(), false);
                buffer.Length = 0;
                var idx = pattern.IndexOf('}', i);
                if (idx < 0)
                  throw new InvalidOperationException();
                yield return new Token(pattern.Substring(i - 1, idx - i + 2), true);
                i = idx;
                break;
              case 'r':
                buffer.Append('\r');
                break;
              case 't':
                buffer.Append('\t');
                break;
              case 'u':
                buffer.Append((char)Convert.ToInt32(pattern.Substring(i, 4), 16));
                i += 4;
                break;
              case 'v':
                buffer.Append('\v');
                break;
              case 'x':
                buffer.Append((char)Convert.ToInt32(pattern.Substring(i, 2), 16));
                i += 2;
                break;
              default:
                buffer.Append(pattern[i]);
                break;
            }
            break;
          case '[':
            inCharGroup = true;
            if (buffer.Length > 0)
              yield return new Token(buffer.ToString(), false);
            buffer.Length = 0;
            if (i < pattern.Length - 1 && pattern[i + 1] == '^')
            {
              yield return new Token(pattern.Substring(i, 2), true);
              i++;
            }
            else
            {
              yield return new Token(pattern[i].ToString(), true);
            }
            break;
          case ']':
            inCharGroup = false;
            if (buffer.Length > 0)
              yield return new Token(buffer.ToString(), false);
            buffer.Length = 0;
            yield return new Token(pattern[i].ToString(), true);
            break;
          case '^':
            if (buffer.Length > 0)
              yield return new Token(buffer.ToString(), false);
            buffer.Length = 0;
            yield return new Token(pattern[i].ToString(), true);
            break;
          case '.':
          case '$':
          case '(':
          case ')':
          case '{':
          case '}':
          case '|':
            if (inCharGroup)
            {
              buffer.Append(pattern[i]);
            }
            else
            {
              if (buffer.Length > 0)
                yield return new Token(buffer.ToString(), false);
              buffer.Length = 0;
              yield return new Token(pattern[i].ToString(), true);
            }
            break;
          case '+':
          case '*':
            if (inCharGroup)
            {
              buffer.Append(pattern[i]);
            }
            else
            {
              if (buffer.Length > 0)
                yield return new Token(buffer.ToString(), false);
              buffer.Length = 0;
              if (i < pattern.Length - 1 && pattern[i + 1] == '?')
              {
                yield return new Token(pattern.Substring(i, 2), true);
                i++;
              }
              else
              {
                yield return new Token(pattern[i].ToString(), true);
              }
            }
            break;
          case '?':
            if (inCharGroup)
            {
              buffer.Append(pattern[i]);
            }
            else if (i > 0 && pattern[i - 1] == '(')
            {
              i++;
              switch (pattern[i])
              {
                case '<':
                  i++;
                  switch (pattern[i])
                  {
                    case '=':
                    case '!':
                      if (buffer.Length > 0)
                        yield return new Token(buffer.ToString(), false);
                      buffer.Length = 0;
                      yield return new Token(pattern.Substring(i - 2, 3), true);
                      break;
                    default:
                      var idx2 = pattern.IndexOf('>', i);
                      if (idx2 < 0)
                        throw new NotSupportedException();
                      yield return new Token(pattern.Substring(i - 2, idx2 - i + 3), true);
                      i = idx2;
                      break;
                  }
                  break;
                case '\'':
                  var idx = pattern.IndexOf('\'', i);
                  if (idx < 0)
                    throw new NotSupportedException();
                  yield return new Token(pattern.Substring(i - 1, idx - i + 2), true);
                  i = idx;
                  break;
                case ':':
                case '=':
                case '!':
                case '>':
                  if (buffer.Length > 0)
                    yield return new Token(buffer.ToString(), false);
                  buffer.Length = 0;
                  yield return new Token(pattern.Substring(i - 1, 2), true);
                  break;
                default:
                  var idx3 = pattern.IndexOf(':', i);
                  if (idx3 < 0)
                    throw new NotSupportedException();
                  yield return new Token(pattern.Substring(i - 1, idx3 - i + 2), true);
                  i = idx3;
                  break;
              }
            }
            else
            {
              if (buffer.Length > 0)
                yield return new Token(buffer.ToString(), false);
              buffer.Length = 0;
              if (i < pattern.Length - 1 && pattern[i + 1] == '?')
              {
                yield return new Token(pattern.Substring(i, 2), true);
                i++;
              }
              else
              {
                yield return new Token(pattern[i].ToString(), true);
              }
            }
            break;
          default:
            buffer.Append(pattern[i]);
            break;
        }
      }

      if (buffer.Length > 0)
        yield return new Token(buffer.ToString(), false);
    }

    private struct Token
    {
      public bool IsOperator { get; set; }
      public string Value { get; set; }

      public Token(string value, bool isOperator)
      {
        Value = value;
        IsOperator = isOperator;
      }
    }
  }
}
