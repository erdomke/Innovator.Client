using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  [DebuggerDisplay("{Text} {Type}")]
  internal class ODataToken
  {
    public ODataTokenType Type { get; internal set; }
    public string Text { get; internal set; }

    private ODataToken() { }
    public ODataToken(ODataTokenType type, string value)
    {
      this.Type = type;
      this.Text = value;
    }

    public object AsPrimitive()
    {
      switch (Type)
      {
        case ODataTokenType.Date:
        case ODataTokenType.TimeOfDay:
          if (Text.StartsWith("datetime'"))
            return DateTime.Parse(Text.Substring(9).TrimEnd('\''));
          return DateTime.Parse(Text);
        case ODataTokenType.Decimal:
          return decimal.Parse(Text.TrimEnd(new char[] { 'M', 'm' }));
        case ODataTokenType.Double:
          return double.Parse(Text.TrimEnd(new char[] { 'd', 'D' }));
        case ODataTokenType.Duration:
          return System.Xml.XmlConvert.ToTimeSpan(Text);
        case ODataTokenType.False:
          return false;
        case ODataTokenType.Guid:
          if (Text.StartsWith("guid'"))
            return new Guid(Text.Substring(5, 36));
          return new Guid(Text);
        case ODataTokenType.Integer:
          return int.Parse(Text);
        case ODataTokenType.Long:
          return long.Parse(Text.TrimEnd('L'));
        case ODataTokenType.NaN:
          return double.NaN;
        case ODataTokenType.NegInfinity:
          return double.NegativeInfinity;
        case ODataTokenType.Null:
          return null;
        case ODataTokenType.PosInfinity:
          return double.PositiveInfinity;
        case ODataTokenType.Single:
          return float.Parse(Text.TrimEnd(new char[] { 'f', 'F' }));
        case ODataTokenType.String:
          return CleanString(Text);
        case ODataTokenType.True:
          return true;
      }
      throw new InvalidOperationException();
    }

    private string CleanString(string value)
    {
      var buffer = new char[value.Length - 2];
      var o = 0;
      for (var i = 1; i < value.Length - 1; i++)
      {
        switch (value[i])
        {
          case '\'':
            buffer[o++] = value[i];
            i++;
            break;
          default:
            buffer[o++] = value[i];
            break;
        }
      }
      return new string(buffer, 0, o);
    }

    public static ODataToken FromPrimative(object value, ODataVersion version = ODataVersion.All)
    {
      var writer = new StringBuilder();
      var result = new ODataToken();

      if (value == null
#if DBDATA
            || value is DBNull
#endif
      )
      {
        writer.Append("null");
        result.Type = ODataTokenType.Null;
      }
      else if (value is byte[])
      {
        if (version.SupportsV4())
        {
          writer.Append("binary'");
          var str = Convert.ToBase64String((byte[])value);
          writer.Append(str.Replace('+', '-').Replace('/', '_'));
          writer.Append("'");
        }
        else
        {
          writer.Append("X'");
          foreach (var b in (byte[])value)
          {
            writer.Append(b.ToString("X2"));
          }
          writer.Append("'");
        }
        result.Type = ODataTokenType.Binary;
      }
      else if (value is bool)
      {
        if ((bool)value)
        {
          writer.Append("true");
          result.Type = ODataTokenType.True;
        }
        else
        {
          writer.Append("false");
          result.Type = ODataTokenType.False;
        }
      }
      else if (value is DateTime)
      {
        var date = (DateTime)value;
        if (version.SupportsV4())
        {
          var time = date.TimeOfDay;
          if (time.TotalMilliseconds > 0)
          {
            writer.Append(new DateTimeOffset(date).ToUniversalTime().ToString("s"));
            writer.Append("Z");
          }
          else
          {
            writer.Append(date.ToString("yyyy-MM-dd"));
          }
        }
        else
        {
          writer.Append("datetime'");
          writer.Append(date.ToString("s"));
          writer.Append("'");
        }
        result.Type = ODataTokenType.Date;
      }
      else if (value is DateTimeOffset)
      {
        var offset = (DateTimeOffset)value;
        if (version.SupportsV4())
        {
          writer.Append(offset.ToUniversalTime().ToString("s"));
          writer.Append("Z");
        }
        else
        {
          writer.Append("datetimeoffset'");
          writer.Append(offset.ToUniversalTime().ToString("s"));
          writer.Append("Z'");
        }
        result.Type = ODataTokenType.Date;
      }
      else if (value is decimal)
      {
        writer.Append(value);
        if (version.SupportsV2OrV3() && !version.SupportsV4())
        {
          writer.Append("m");
        }
        result.Type = ODataTokenType.Decimal;
      }
      else if (value is double)
      {
        if (double.IsPositiveInfinity((double)value))
        {
          writer.Append("INF");
          result.Type = ODataTokenType.PosInfinity;
        }
        else if (double.IsNegativeInfinity((double)value))
        {
          writer.Append("-INF");
          result.Type = ODataTokenType.NegInfinity;
        }
        else
        {
          writer.Append(value);
          if (version.SupportsV2OrV3() && !version.SupportsV4())
          {
            writer.Append("d");
          }
          result.Type = double.IsNaN((double)value) ? ODataTokenType.NaN : ODataTokenType.Double;
        }
      }
      else if (value is float)
      {
        if (float.IsPositiveInfinity((float)value))
        {
          writer.Append("INF");
          result.Type = ODataTokenType.PosInfinity;
        }
        else if (float.IsNegativeInfinity((float)value))
        {
          writer.Append("-INF");
          result.Type = ODataTokenType.NegInfinity;
        }
        else
        {
          writer.Append(value);
          if (version.SupportsV2OrV3() && !version.SupportsV4())
          {
            writer.Append("f");
          }
          result.Type = float.IsNaN((float)value) ? ODataTokenType.NaN : ODataTokenType.Single;
        }
      }
      else if (value is Guid)
      {
        if (version.SupportsV4())
        {
          writer.Append(value.ToString());
        }
        else
        {
          writer.Append("guid'");
          writer.Append(value.ToString());
          writer.Append("'");
        }
        result.Type = ODataTokenType.Guid;
      }
      else if (value is int || value is uint
        || value is short || value is ushort
        || value is byte || value is sbyte)
      {
        writer.Append(value);
        result.Type = ODataTokenType.Integer;
      }
      else if (value is long || value is ulong)
      {
        writer.Append(value);
        if (version.SupportsV2OrV3() && !version.SupportsV4())
        {
          writer.Append("L");
        }
        result.Type = ODataTokenType.Long;
      }
      else if (value is TimeSpan)
      {
        var time = (TimeSpan)value;
        var dur = time.Duration();
        writer.Append("duration'");
        if (time.TotalMilliseconds < 0)
          writer.Append('-');
        writer.Append("P");
        if (dur.Days > 0)
        {
          writer.Append(dur.Days);
          writer.Append("D");
        }
        if (dur.Hours > 0 || dur.Minutes > 0 || dur.Seconds > 0 || dur.Milliseconds > 0)
        {
          writer.Append("T");
          writer.Append(dur.Hours);
          writer.Append("H");
          if (dur.Minutes > 0 || dur.Seconds > 0 || dur.Milliseconds > 0)
          {
            writer.Append(dur.Minutes);
            writer.Append("M");
            if (dur.Seconds > 0)
            {
              writer.Append(dur.Seconds);
              writer.Append("S");
              if (dur.Milliseconds > 0)
              {
                writer.Append(".");
                writer.Append(dur.Minutes.ToString("d3"));
              }
            }
          }
        }
        writer.Append("'");
        result.Type = ODataTokenType.Duration;
      }
      else
      {
        writer.Append("'");
        writer.Append(value.ToString().Replace("'", "''"));
        writer.Append("'");
        result.Type = ODataTokenType.String;
      }

      result.Text = writer.ToString();
      return result;
    }
  }
}
