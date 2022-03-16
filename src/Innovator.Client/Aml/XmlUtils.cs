using System.Collections.Generic;
using System.Text;

namespace Innovator.Client
{
  internal static class XmlUtils
  {
    public static StringBuilder AppendEscapedXml(this StringBuilder builder, string value)
    {
      if (value == null) return builder;
      builder.EnsureCapacity(builder.Length + value.Length + 10);
      for (var i = 0; i < value.Length; i++)
      {
        switch (value[i])
        {
          case '&':
            builder.Append("&amp;");
            break;
          case '<':
            builder.Append("&lt;");
            break;
          case '>':
            builder.Append("&gt;");
            break;
          case '"':
            builder.Append("&quot;");
            break;
          case '\'':
            builder.Append("&apos;");
            break;
          default:
            builder.Append(value[i]);
            break;
        }
      }
      return builder;
    }

    /// <summary>
    /// Split the xml name based on namespace prefix if available
    /// </summary>
    /// <param name="name">The xml name</param>
    /// <returns>KVP where the key is the prefix and the value is the adjusted name</returns>
    public static KeyValuePair<string, string> GetXmlNamePrefix(string name)
    {
      var prefix = "";
      var i = name?.IndexOf(':') ?? -1;
      if (i >= 0)
      {
        prefix = name.Substring(0, i);
        name = name.Substring(i + 1);
      }
      return new KeyValuePair<string, string>(prefix, name);
    }
  }
}
