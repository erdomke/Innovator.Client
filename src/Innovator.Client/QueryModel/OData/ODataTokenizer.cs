using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  internal class ODataTokenizer : IEnumerator<ODataToken>
  {
    private readonly bool _decodeUri;
    private int _idx;
    private State _state;
    private readonly string _value;
    private readonly ODataVersion _versions;

    public ODataToken Current { get; private set; }
    object IEnumerator.Current { get { return Current; } }

    private enum State
    {
      PathStart,
      PathSeparator,
      ParamOpen,
      ParamName,
      ParamEq,
      ParamValue,
      ParamEnd,
      QueryName,
      QueryEq,
      QueryValue,
      UnknownQueryValue,
    }

    internal ODataTokenizer(string value, ODataVersion version = ODataVersion.All, bool decodeUri = true)
    {
      _value = UriDecode(value);
      _versions = version;
      _decodeUri = decodeUri;
      Reset();
    }

    private static string UriDecode(string value)
    {
      if (value == null)
        return null;

      var output = new char[value.Length];
      var o = 0;
      var inQuery = false;
      byte ascii;
      for (var i = 0; i < value.Length; i++)
      {
        inQuery = inQuery || value[i] == '?';
        if (value[i] == '%' && i + 2 < value.Length
          && byte.TryParse(value.Substring(i + 1, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ascii))
        {
          output[o++] = (char)ascii;
          i += 2;
        }
        else if (inQuery && value[i] == '+')
        {
          output[o++] = ' ';
        }
        else
        {
          output[o++] = value[i];
        }
      }
      return new string(output, 0, o);
    }

    public bool MoveNext()
    {
      if (_idx >= _value.Length)
        return false;

      for (var i = _idx; i < _value.Length; i++)
      {
        switch (_state)
        {
          case State.PathStart:
            switch (_value[i])
            {
              case ':':
                if (i == 0)
                  throw new ParseException(_value, i);

                if (_idx == 0 && (i + 2) < _value.Length && _value[i + 1] == '/' && _value[i + 2] == '/')
                {
                  Current = new ODataToken(ODataTokenType.Scheme, _value.Substring(_idx, i - _idx));
                  _state = State.PathSeparator;
                  _idx = i;
                  return true;
                }
                else if (Current?.Text == "://")
                {
                  Current = new ODataToken(ODataTokenType.Authority, _value.Substring(_idx, i - _idx));
                  _state = State.PathSeparator;
                  _idx = i;
                  return true;
                }
                else
                {
                  throw new ParseException(_value, i);
                }
              case '/':
              case '?':
                if (i > _idx)
                {
                  var type = ODataTokenType.Identifier;
                  if (Current?.Text == "://")
                    type = ODataTokenType.Authority;
                  else if (Current?.Text == ":")
                    type = ODataTokenType.Port;

                  Current = new ODataToken(type, _value.Substring(_idx, i - _idx));
                  _state = State.PathSeparator;
                  _idx = i;
                  return true;
                }
                else if (_value[i] == '?')
                {
                  Current = new ODataToken(ODataTokenType.Question, "?");
                  _state = State.QueryName;
                  _idx = i + 1;
                  return true;
                }
                else if (_value[i] == '/')
                {
                  Current = new ODataToken(ODataTokenType.PathSeparator, "/");
                  _state = State.PathStart;
                  _idx = i + 1;
                  return true;
                }
                break;
              case '=':
                if (i > _idx)
                {
                  Current = new ODataToken(ODataTokenType.QueryName, _value.Substring(_idx, i - _idx));
                  _state = State.QueryEq;
                  _idx = i;
                  return true;
                }
                break;
            }
            if (_value[i] == '(')
            {
              Current = new ODataToken(ODataTokenType.Identifier, _value.Substring(_idx, i - _idx));
              _state = State.ParamOpen;
              _idx = i;
              return true;
            }
            break;
          case State.PathSeparator:
            if (_value[i] == '?')
            {
              Current = new ODataToken(ODataTokenType.Question, "?");
              _state = State.QueryName;
              _idx = i + 1;
              return true;
            }
            else if (_value[i] != '/' && _value[i] != ':')
            {
              Current = new ODataToken(ODataTokenType.PathSeparator, _value.Substring(_idx, i - _idx));
              _state = State.PathStart;
              _idx = i;
              return true;
            }
            break;
          case State.ParamOpen:
            if (i == _idx && TryConsumeChar(ref i, '('))
            {
              Current = new ODataToken(ODataTokenType.OpenParen, _value.Substring(_idx, 1));
              _state = State.ParamName;
              _idx = i;
              return true;
            }
            throw new ParseException(_value, i);
          case State.ParamName:
            if (TryConsumeChar(ref i, ')'))
            {
              Current = new ODataToken(ODataTokenType.CloseParen, ")");
              _state = State.PathSeparator;
              _idx = i;
              return true;
            }
            Current = TryConsumeIdentifier()
              ?? TryConsumeLiteral();
            if (Current == null)
              throw new ParseException(_value, _idx);
            _state = Current.Type == ODataTokenType.Identifier ? State.ParamEq : State.ParamEnd;
            return true;
          case State.ParamEq:
            if (i == _idx && _value[i] == '=')
            {
              Current = new ODataToken(ODataTokenType.QueryAssign, _value.Substring(_idx, 1));
              _state = State.ParamValue;
              _idx = i + 1;
              return true;
            }
            throw new ParseException(_value, i);
          case State.ParamValue:
            Current = TryConsumeAlias()
              ?? TryConsumeLiteral();
            if (Current == null)
              throw new ParseException(_value, _idx);
            _state = State.ParamEnd;
            return true;
          case State.ParamEnd:
            if (TryConsumeChar(ref i, ','))
            {
              Current = new ODataToken(ODataTokenType.Comma, ",");
              _state = State.ParamName;
              _idx = i;
              return true;
            }
            else if (TryConsumeChar(ref i, ')'))
            {
              Current = new ODataToken(ODataTokenType.CloseParen, ")");
              _state = State.PathSeparator;
              _idx = i;
              return true;
            }
            throw new ParseException(_value, _idx);
          case State.QueryName:
            if (_value[i] == '@')
            {
              Current = TryConsumeAlias();
              if (Current == null)
                throw new ParseException(_value, _idx);
              _state = State.QueryEq;
              return true;
            }
            else if (_value[i] == '$')
            {
              _idx++;
              Current = TryConsumeIdentifier();
              Current.Text = "$" + Current.Text;
              Current.Type = ODataTokenType.QueryName;
              _state = State.QueryEq;
              return true;
            }
            else if (_value[i] == '=')
            {
              Current = new ODataToken(ODataTokenType.QueryName, _value.Substring(_idx, i - _idx));
              _state = State.QueryEq;
              _idx = i;
              return true;
            }
            break;
          case State.QueryEq:
            if (i == _idx && _value[i] == '=')
            {
              var known = Current.Text[0] == '$' || Current.Text[0] == '@';
              Current = new ODataToken(ODataTokenType.QueryAssign, _value.Substring(_idx, 1));
              _state = known ? State.QueryValue : State.UnknownQueryValue;
              _idx = i + 1;
              return true;
            }
            throw new ParseException(_value, i);
          case State.UnknownQueryValue:
            if (_value[i] == '&')
            {
              if (i == _idx)
              {
                Current = new ODataToken(ODataTokenType.Amperstand, _value.Substring(_idx, 1));
                _state = State.QueryName;
                _idx = i + 1;
                return true;
              }
              else
              {
                Current = new ODataToken(ODataTokenType.Identifier, _value.Substring(_idx, i - _idx));
                _idx = i;
                return true;
              }
            }
            break;
          case State.QueryValue:
            if (i == _idx)
            {
              if (_value[i] == '&')
              {
                Current = new ODataToken(ODataTokenType.Amperstand, _value.Substring(_idx, 1));
                _state = State.QueryName;
                _idx = i + 1;
                return true;
              }

              switch (_value[_idx])
              {
                case '*':
                  Current = new ODataToken(ODataTokenType.Star, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case '.':
                  Current = new ODataToken(ODataTokenType.Period, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case '/':
                  Current = new ODataToken(ODataTokenType.Navigation, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case ',':
                  Current = new ODataToken(ODataTokenType.Comma, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case '(':
                  Current = new ODataToken(ODataTokenType.OpenParen, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case ')':
                  Current = new ODataToken(ODataTokenType.CloseParen, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case ':':
                  Current = new ODataToken(ODataTokenType.Colon, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case '=':
                  Current = new ODataToken(ODataTokenType.QueryAssign, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case ';':
                  Current = new ODataToken(ODataTokenType.Semicolon, _value.Substring(_idx, 1));
                  _idx = i + 1;
                  return true;
                case '$':
                  _idx++;
                  Current = TryConsumeIdentifier();
                  if (Current == null)
                    throw new ParseException(_value, _idx);
                  Current.Text = "$" + Current.Text;
                  return true;
              }
            }

            Current = TryConsumeWhitespace()
              ?? TryConsumeKeyword()
              ?? TryConsumeLiteral()
              ?? TryConsumeAlias()
              ?? TryConsumeIdentifier();
            if (Current == null)
              throw new ParseException(_value, _idx);
            return true;
        }
      }

      if (_idx < _value.Length)
      {
        switch (_state)
        {
          case State.PathStart:
          case State.UnknownQueryValue:
            Current = new ODataToken(ODataTokenType.Identifier, _value.Substring(_idx));
            _idx = _value.Length;
            return true;
          case State.PathSeparator:
            Current = new ODataToken(ODataTokenType.PathSeparator, _value.Substring(_idx));
            _idx = _value.Length;
            return true;
        }

      }

      throw new ParseException(_value, _idx);
    }

    public ODataToken TryConsumeWhitespace()
    {
      var i = _idx;
      while (_value[i] == ' ')
        i++;

      if (i == _idx)
        return null;

      var result = new ODataToken(ODataTokenType.Whitespace, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    public bool TryConsumeChar(ref int index, char match)
    {
      if (index >= _value.Length)
        return false;

      if (_value[index] == match)
      {
        index += 1;
        return true;
      }

      return false;
    }

    public ODataToken TryConsumeAlias()
    {
      if (_value[_idx] != '@')
        return null;

      _idx++;
      var result = TryConsumeIdentifier();
      if (result == null)
      {
        _idx--;
        return null;
      }
      result.Text = "@" + result.Text;
      result.Type = ODataTokenType.Parameter;
      return result;
    }

    public ODataToken TryConsumeIdentifier()
    {
      if (!char.IsLetter(_value[_idx]) && _value[_idx] != '_')
        return null;

      var i = _idx + 1;
      while (i < _value.Length && (char.IsLetter(_value[i]) || char.IsDigit(_value[i]) || _value[i] == '_'))
        i++;

      var result = new ODataToken(ODataTokenType.Identifier, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    public ODataToken TryConsumeKeyword()
    {
      var i = _idx;
      while (i < _value.Length && char.IsLetter(_value[i]))
        i++;

      if (i == _idx)
        return null;

      var type = ODataTokenType.PathSeparator;
      switch (_value.Substring(_idx, i - _idx))
      {
        case "and":
          type = ODataTokenType.And;
          break;
        case "or":
          type = ODataTokenType.Or;
          break;
        case "eq":
          type = ODataTokenType.Equal;
          break;
        case "ne":
          type = ODataTokenType.NotEqual;
          break;
        case "lt":
          type = ODataTokenType.LessThan;
          break;
        case "le":
          type = ODataTokenType.LessThanOrEqual;
          break;
        case "gt":
          type = ODataTokenType.GreaterThan;
          break;
        case "ge":
          type = ODataTokenType.GreaterThanOrEqual;
          break;
        case "has":
          type = ODataTokenType.Has;
          break;
        case "in":
          type = ODataTokenType.In;
          break;
        case "add":
          type = ODataTokenType.Add;
          break;
        case "sub":
          type = ODataTokenType.Subtract;
          break;
        case "mul":
          type = ODataTokenType.Multiply;
          break;
        case "div":
        case "divby":
          type = ODataTokenType.Divide;
          break;
        case "mod":
          type = ODataTokenType.Modulo;
          break;
        case "not":
          type = ODataTokenType.Not;
          break;
        default:
          return null;
      }

      var result = new ODataToken(type, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    public ODataToken TryConsumeLiteral()
    {
      ODataToken result;

      switch (_value[_idx])
      {
        case 'n':
          if ((_idx + 4) <= _value.Length && _value.Substring(_idx, 4) == "null")
          {
            _idx += 4;
            return new ODataToken(ODataTokenType.Null, "null");
          }
          return null;
        case 'N':
          if ((_idx + 3) <= _value.Length && _value.Substring(_idx, 3) == "NaN")
          {
            _idx += 3;
            return new ODataToken(ODataTokenType.NaN, "NaN");
          }
          else if ((_idx + 4) <= _value.Length
            && _versions.SupportsV2OrV3()
            && (_value.Substring(_idx, 4) == "NaNd"
              || _value.Substring(_idx, 4) == "NaND"
              || _value.Substring(_idx, 4) == "NaNf"
              || _value.Substring(_idx, 4) == "NaNF"))
          {
            _idx += 4;
            return new ODataToken(ODataTokenType.NaN, _value.Substring(_idx, 4));
          }
          return null;
        case 't':
          if ((_idx + 4) <= _value.Length && _value.Substring(_idx, 4) == "true")
          {
            _idx += 4;
            return new ODataToken(ODataTokenType.True, "true");
          }
          return TryConsumeDuration();
        case 'a':
        case 'c':
        case 'e':
          return TryConsumeGuid();
        case 'b':
          return TryConsumeBinary()
            ?? TryConsumeGuid();
        case 'd':
          return TryConsumeDuration()
            ?? TryConsumeDateTime()
            ?? TryConsumeGuid();
        case 'f':
          if ((_idx + 5) <= _value.Length && _value.Substring(_idx, 5) == "false")
          {
            _idx += 5;
            return new ODataToken(ODataTokenType.False, "false");
          }
          else
          {
            return TryConsumeGuid();
          }
        case 'g':
          return TryConsumeGuid();
        case 'I':
          if ((_idx + 3) <= _value.Length && _value.Substring(_idx, 3) == "INF")
          {
            _idx += 3;
            return new ODataToken(ODataTokenType.PosInfinity, "INF");
          }
          else if ((_idx + 4) <= _value.Length
            && _versions.SupportsV2OrV3()
            && (_value.Substring(_idx, 4) == "INFd"
              || _value.Substring(_idx, 4) == "INFD"
              || _value.Substring(_idx, 4) == "INFf"
              || _value.Substring(_idx, 4) == "INFF"))
          {
            _idx += 4;
            return new ODataToken(ODataTokenType.PosInfinity, _value.Substring(_idx, 4));
          }
          return null;
        case 'X':
          return TryConsumeBinary();
        case '-':
        case '+':
          if ((_idx + 4) <= _value.Length && _value.Substring(_idx, 4) == "-INF")
          {
            _idx += 4;
            return new ODataToken(ODataTokenType.NegInfinity, "-INF");
          }
          else if ((_idx + 5) <= _value.Length
            && _versions.SupportsV2OrV3()
            && (_value.Substring(_idx, 5) == "-INFd"
              || _value.Substring(_idx, 5) == "-INFD"
              || _value.Substring(_idx, 5) == "-INFf"
              || _value.Substring(_idx, 5) == "-INFF"))
          {
            _idx += 5;
            return new ODataToken(ODataTokenType.NegInfinity, _value.Substring(_idx, 5));
          }
          return TryConsumeNumber();
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
          return TryConsumeGuid()
            ?? TryConsumeDateTime()
            ?? TryConsumeTimeOfDay()
            ?? TryConsumeNumber();
        case '\'':
          var i = _idx + 1;
          while (i < _value.Length)
          {
            if (TryConsumeChar(ref i, '\''))
            {
              if (!TryConsumeChar(ref i, '\''))
              {
                break;
              }
            }
            else
            {
              i++;
            }
          }
          if (_state == State.QueryValue || _state == State.UnknownQueryValue)
            result = new ODataToken(ODataTokenType.String, _value.Substring(_idx, i - _idx).Replace('+', ' '));
          else
            result = new ODataToken(ODataTokenType.String, _value.Substring(_idx, i - _idx));
          _idx = i;
          return result;
      }

      return null;
    }

    private ODataToken TryConsumeGuid()
    {
      if ((_idx + 36) > _value.Length)
        return null;

      if (_versions.SupportsV4()
          && IsHexPhrase(_idx, _idx + 8)
          && _value[_idx + 8] == '-'
          && IsHexPhrase(_idx + 9, _idx + 13)
          && _value[_idx + 13] == '-'
          && IsHexPhrase(_idx + 14, _idx + 18)
          && _value[_idx + 18] == '-'
          && IsHexPhrase(_idx + 19, _idx + 23)
          && _value[_idx + 23] == '-'
          && IsHexPhrase(_idx + 24, _idx + 36))
      {
        _idx += 36;
        return new ODataToken(ODataTokenType.Guid, _value.Substring(_idx - 36, 36));
      }

      if (_versions.SupportsV2OrV3()
        && (_idx + 42) <= _value.Length
        && _value.Substring(_idx, 5) == "guid'"
        && IsHexPhrase(_idx + 5, _idx + 13)
        && _value[_idx + 13] == '-'
        && IsHexPhrase(_idx + 14, _idx + 18)
        && _value[_idx + 18] == '-'
        && IsHexPhrase(_idx + 19, _idx + 23)
        && _value[_idx + 23] == '-'
        && IsHexPhrase(_idx + 24, _idx + 28)
        && _value[_idx + 28] == '-'
        && IsHexPhrase(_idx + 29, _idx + 41)
        && _value[_idx + 41] == '\'')
      {
        _idx += 42;
        return new ODataToken(ODataTokenType.Guid, _value.Substring(_idx - 42, 42));
      }

      return null;
    }

    private ODataToken TryConsumeBinary()
    {
      var i = _idx;
      var start = 0;
      var type = ODataTokenType.Base64;

      if ((_idx + 7) <= _value.Length
        && _value.Substring(_idx, 7) == "binary'")
      {
        start = 7;
        if (!_versions.SupportsV4())
          type = ODataTokenType.Binary;
      }
      else if ((_idx + 2) <= _value.Length
        && _value.Substring(_idx, 2) == "X'"
        && _versions.SupportsV2OrV3())
      {
        start = 2;
        type = ODataTokenType.Binary;
      }
      else
      {
        return null;
      }
      i += start;

      while (i < _value.Length && _value[i] != '\'')
      {
        if (type == ODataTokenType.Binary)
        {
          switch (_value[i])
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
            case 'a':
            case 'b':
            case 'c':
            case 'd':
            case 'e':
            case 'f':
            case 'A':
            case 'B':
            case 'C':
            case 'D':
            case 'E':
            case 'F':
              break;
            default:
              return null;
          }
        }
        else
        {
          if (!char.IsLetterOrDigit(_value[i])
            && _value[i] != '-'
            && _value[i] != '_'
            && _value[i] != '=')
            return null;
        }
        i++;
      }

      if (i >= _value.Length)
        return null;
      if (type == ODataTokenType.Binary
        && (i - _idx - start) % 2 != 0)
        return null;

      i++;
      var result = new ODataToken(type, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    private ODataToken TryConsumeDateTime()
    {
      // Just a date
      var length = 10;
      var start = _idx;
      if ((_idx + length) > _value.Length)
        return null;

      var startsWithPrefix = false;
      if (_value.Substring(_idx, 9) == "datetime'")
      {
        startsWithPrefix = true;
        start += 9;
      }
      else if ((_idx + 15) <= _value.Length
        && _value.Substring(_idx, 15) == "datetimeoffset'")
      {
        startsWithPrefix = true;
        start += 15;
      }

      // Prefix required for v2 & v3
      if (!_versions.SupportsV4() && !startsWithPrefix)
        return null;
      // Prefix should not be there for v4
      if (!_versions.SupportsV2OrV3() && startsWithPrefix)
        return null;
      // Make sure there are enough characters
      if (startsWithPrefix && (_idx + 10 + length) > _value.Length)
        return null;


      if (_value[start + 4] != '-'
        || _value[start + 7] != '-'
        || !int.TryParse(_value.Substring(start, 4), out int year)
        || !int.TryParse(_value.Substring(start + 5, 2), out int month)
        || !int.TryParse(_value.Substring(start + 8, 2), out int day)
        || month < 1 || month > 12 || day < 1 || day > 31)
      {
        return null;
      }

      // Date and Time
      if ((start + 16) <= _value.Length
        && _value[start + 10] == 'T' && _value[start + 13] == ':')
      {
        length += 6;
        if (!int.TryParse(_value.Substring(start + 11, 2), out int hour)
          || !int.TryParse(_value.Substring(start + 14, 2), out int minute)
          || hour < 0 || hour > 23 || minute < 0 || minute > 59)
        {
          return null;
        }

        // Fractional seconds
        if ((start + length + 3) <= _value.Length
          && _value[start + length] == ':'
          && int.TryParse(_value.Substring(start + length + 1, 2), out int second)
          && second >= 0 && second <= 59)
        {
          length += 3;

          if ((start + length + 2) > _value.Length
            && _value[start + length] == '.'
            && char.IsDigit(_value[start + length + 1]))
          {
            length += 2;
            while ((start + length) < _value.Length && char.IsDigit(_value[start + length]))
              length++;
          }
        }

        // Timezone
        if ((start + length) < _value.Length
          && _value[start + length] == 'Z')
        {
          length++;
        }
        else if ((start + length + 6) <= _value.Length
          && (_value[start + length] == '+' || _value[start + length] == '-')
          && int.TryParse(_value.Substring(start + length + 1, 2), out int tzHour)
          && _value[start + length + 3] == ':'
          && int.TryParse(_value.Substring(start + length + 4, 2), out int tzMinute)
          && tzHour >= 0 && tzHour <= 23
          && tzMinute >= 0 && tzMinute <= 59)
        {
          length += 6;
        }
      }

      if (startsWithPrefix && _value[start + length] != '\'')
        return null;
      if (startsWithPrefix)
        length += (start - _idx + 1);

      _idx += length;
      return new ODataToken(ODataTokenType.Date, _value.Substring(_idx - length, length));
    }

    private ODataToken TryConsumeDuration()
    {
      var i = _idx;
      if (_versions.SupportsV4()
        && (_idx + 9) <= _value.Length
        && _value.Substring(_idx, 9) == "duration'")
      {
        i += 9;
      }
      else if (_versions.SupportsV2OrV3()
        && (_idx + 5) <= _value.Length
        && _value.Substring(_idx, 5) == "time'")
      {
        i += 5;
      }
      else
      {
        return null;
      }

      if (i < _value.Length && (_value[i] == '-' || _value[i] == '+'))
        i++;
      if (i >= _value.Length || _value[i] != 'P')
        return null;
      i++;

      if (i < _value.Length && char.IsDigit(_value[i]))
      {
        while (i < _value.Length && char.IsDigit(_value[i]))
          i++;
        if (i >= _value.Length || _value[i] != 'D')
          return null;
        i++;
      }

      if (i < _value.Length && _value[i] == 'T')
      {
        i++;
        if (i >= _value.Length || !char.IsDigit(_value[i]))
          return null;

        while (i < _value.Length && char.IsDigit(_value[i]))
        {
          while (i < _value.Length && char.IsDigit(_value[i]))
            i++;

          if (i < _value.Length && _value[i] == '.')
          {
            i++;
            while (i < _value.Length && char.IsDigit(_value[i]))
              i++;
            if (i >= _value.Length || _value[i] != 'S')
              return null;
          }
          else if (i >= _value.Length || (_value[i] != 'H' && _value[i] != 'M'))
          {
            return null;
          }
          i++;
        }
      }

      if (i >= _value.Length || _value[i] != '\'')
        return null;
      i++;

      var result = new ODataToken(ODataTokenType.Duration, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    private ODataToken TryConsumeTimeOfDay()
    {
      var length = 5;
      if ((_idx + 5) > _value.Length)
        return null;

      if (_value[_idx + 2] != ':')
        return null;

      if (!int.TryParse(_value.Substring(_idx, 2), out int hour)
        || !int.TryParse(_value.Substring(_idx + 3, 2), out int minute))
      {
        return null;
      }

      if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
        return null;

      if ((_idx + length + 3) <= _value.Length
        && _value[_idx + length] == ':'
        && int.TryParse(_value.Substring(_idx + length + 1, 2), out int second)
        && second >= 0 && second <= 59)
      {
        length += 3;

        if ((_idx + length + 2) <= _value.Length
          && _value[_idx + length] == '.'
          && char.IsDigit(_value[_idx + length + 1]))
        {
          length += 2;
          while ((_idx + length) < _value.Length && char.IsDigit(_value[_idx + length]))
            length++;
        }
      }

      _idx += length;
      return new ODataToken(ODataTokenType.TimeOfDay, _value.Substring(_idx - length, length));
    }

    private ODataToken TryConsumeNumber()
    {
      var type = ODataTokenType.Integer;

      var i = _idx + 1;
      while (i < _value.Length && char.IsDigit(_value[i]))
        i++;
      if ((i - _idx) == 1 && (_value[i - 1] == '-' || _value[i - 1] == '+'))
        return null;

      if (i < _value.Length && _value[i] == '.')
      {
        type = ODataTokenType.Double;
        i++;
        while (i < _value.Length && char.IsDigit(_value[i]))
          i++;
      }
      else if (_versions.SupportsV2OrV3()
        && i < _value.Length
        && _value[i] == 'L')
      {
        i++;
        type = ODataTokenType.Long;
      }

      if ((i + 1) < _value.Length && _value[i] == 'e'
        && (_value[i + 1] == '-' || _value[i + 1] == '+' || char.IsDigit(_value[i + 1]))
        && type != ODataTokenType.Long)
      {
        if (char.IsDigit(_value[i + 1]) || ((i + 2) < _value.Length && char.IsDigit(_value[i + 2])))
        {
          type = ODataTokenType.Double;
          i += 2;
          while (i < _value.Length && char.IsDigit(_value[i]))
            i++;
        }
      }
      else if (_versions.SupportsV2OrV3()
        && i < _value.Length
        && (_value[i] == 'M' || _value[i] == 'm')
        && type != ODataTokenType.Long)
      {
        i++;
        type = ODataTokenType.Decimal;
      }

      if (_versions.SupportsV2OrV3()
        && i < _value.Length
        && (_value[i] == 'D' || _value[i] == 'd'))
      {
        i++;
        type = ODataTokenType.Double;
      }
      else if (_versions.SupportsV2OrV3()
        && i < _value.Length
        && (_value[i] == 'F' || _value[i] == 'f'))
      {
        i++;
        type = ODataTokenType.Single;
      }

      var result = new ODataToken(type, _value.Substring(_idx, i - _idx));
      _idx = i;
      return result;
    }

    private bool IsHexPhrase(int start, int end)
    {
      for (var i = start; i < end; i++)
      {
        switch (_value[i])
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
          case 'a':
          case 'b':
          case 'c':
          case 'd':
          case 'e':
          case 'f':
          case 'A':
          case 'B':
          case 'C':
          case 'D':
          case 'E':
          case 'F':
            break;
          default:
            return false;
        }
      }
      return true;
    }

    public void Reset()
    {
      _idx = 0;
      _state = State.PathStart;
    }

    public void Dispose()
    {
      // Do nothing
    }
  }
}
