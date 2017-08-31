using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Innovator.Client
{
  internal static class Utils
  {
    internal const string VaultPicturePrefix = "vault:///?fileId=";

    internal static string HexString(this byte[] value, int offset = 0, int length = -1)
    {
      if (length < 0) length = value.Length;
      var builder = new StringBuilder(value.Length * 2);
      int b;
      for (int i = 0; i < value.Length; i++)
      {
        b = value[i] >> 4;
        builder.Append((char)(55 + b + (((b - 10) >> 31) & -7)));
        b = value[i] & 0xF;
        builder.Append((char)(55 + b + (((b - 10) >> 31) & -7)));
      }
      return builder.ToString();
    }

    internal static T[] ToArray<T>(this ArraySegment<T> data)
    {
      return data.Array.ToArray(data.Offset, data.Count);
    }
    internal static T[] ToArray<T>(this T[] data, int offset, int length)
    {
      var result = new T[length];
      Array.Copy(data, offset, result, 0, result.Length);
      return result;
    }

    internal static bool IsNullOrWhiteSpace(this string value)
    {
      return value == null || value.Trim().Length == 0;
    }

    internal static bool EndsWith<T>(this IEnumerable<T> values, params T[] compare)
    {
      return EndsWith(values, (IEnumerable<T>)compare);
    }
    internal static bool EndsWith<T>(this IEnumerable<T> values, IEnumerable<T> compare)
    {
      using (var compareEnum = compare.Reverse().GetEnumerator())
      {
        using (var valueEnum = values.Reverse().GetEnumerator())
        {
          while (compareEnum.MoveNext() && valueEnum.MoveNext())
          {
            if (!valueEnum.Current.Equals(compareEnum.Current)) return false;
          }
          if (compareEnum.MoveNext()) return false;
          return true;
        }
      }
    }

    internal static byte[] AsciiGetBytes(string value)
    {
#if MD5
      return System.Text.Encoding.ASCII.GetBytes(value);
#else
      var result = new byte[value.Length];
      int ch;
      for (var i = 0; i < value.Length; i++)
      {
        ch = (int)value[i];
        if (ch > 127)
          result[i] = 63;
        else
          result[i] = (byte)ch;
      }
      return result;
#endif
    }

    internal static byte[] AsciiGetBytes(char[] value)
    {
#if MD5
      return System.Text.Encoding.ASCII.GetBytes(value);
#else
      var result = new byte[value.Length];
      int ch;
      for (var i = 0; i < value.Length; i++)
      {
        ch = (int)value[i];
        if (ch > 127)
          result[i] = 63;
        else
          result[i] = (byte)ch;
      }
      return result;
#endif
    }

    internal static byte[] Compress(byte[] data, CompressionType type)
    {
      byte[] result;
      using (var compressedStream = new MemoryStream())
      {
        switch (type)
        {
          case CompressionType.gzip:
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
              zipStream.Write(data, 0, data.Length);
            }
            break;
          case CompressionType.deflate:
            using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress))
            {
              deflateStream.Write(data, 0, data.Length);
            }
            break;
          default:
            result = data;
            return result;
        }
        result = compressedStream.ToArray();
      }
      return result;
    }

    internal static bool EnumTryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct
    {
#if NET35
      try
      {
        result = (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
        return true;
      }
      catch (ArgumentNullException)
      {
        result = default(TEnum);
        return false;
      }
      catch (ArgumentException)
      {
        result = default(TEnum);
        return false;
      }
#else
      return Enum.TryParse<TEnum>(value, ignoreCase, out result);
#endif
    }

#if NET35
    public static void CopyTo(this Stream input, Stream output)
    {
      byte[] buffer = new byte[4096];
      int read;
      while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
      {
        output.Write(buffer, 0, read);
      }
    }
#endif

#if NET35
    /// <summary>
    /// Used for fixing a bug with cookie domain handling
    /// </summary>
    /// <remarks>See http://stackoverflow.com/questions/1047669/cookiecontainer-bug</remarks>
    public static void BugFix_CookieDomain(this CookieContainer cookieContainer)
    {
      var _ContainerType = typeof(CookieContainer);
      var table = (Hashtable)_ContainerType.InvokeMember("m_domainTable",
                                 System.Reflection.BindingFlags.NonPublic |
                                 System.Reflection.BindingFlags.GetField |
                                 System.Reflection.BindingFlags.Instance,
                                 null,
                                 cookieContainer,
                                 new object[] { });
      var keys = new ArrayList(table.Keys);
      foreach (var keyObj in keys)
      {
        var key = (keyObj as string);
        if (key[0] == '.')
        {
          var newKey = key.Remove(0, 1);
          table[newKey] = table[keyObj];
        }
      }
    }
#endif

    internal static bool ShouldClose(this Stream input)
    {
      return !(input is MemoryStream || input is MemoryTributary);
    }

    internal static IPromise<T> AsyncInvoke<T>(Func<T> method)
    {
      var result = new Promise<T>();
      method.BeginInvoke(iar =>
      {
        try
        {
          result.Resolve(((Func<T>)iar.AsyncState).EndInvoke(iar));
        }
        catch (Exception ex)
        {
          result.Reject(ex);
        }
      }, method);
      return result;
    }
    internal static IPromise<T> AsyncInvoke<T1, T>(Func<T1, T> method, T1 arg1)
    {
      var result = new Promise<T>();
      method.BeginInvoke(arg1, iar =>
      {
        try
        {
          result.Resolve(((Func<T1, T>)iar.AsyncState).EndInvoke(iar));
        }
        catch (Exception ex)
        {
          result.Reject(ex);
        }
      }, method);
      return result;
    }
    internal static IPromise<T> AsyncInvoke<T1, T2, T>(Func<T1, T2, T> method, T1 arg1, T2 arg2)
    {
      var result = new Promise<T>();
      method.BeginInvoke(arg1, arg2, iar =>
      {
        try
        {
          result.Resolve(((Func<T1, T2, T>)iar.AsyncState).EndInvoke(iar));
        }
        catch (Exception ex)
        {
          result.Reject(ex);
        }
      }, method);
      return result;
    }
    internal static IPromise<T> AsyncInvoke<T1, T2, T3, T>(Func<T1, T2, T3, T> method, T1 arg1, T2 arg2, T3 arg3)
    {
      var result = new Promise<T>();
      method.BeginInvoke(arg1, arg2, arg3, iar =>
      {
        try
        {
          result.Resolve(((Func<T1, T2, T3, T>)iar.AsyncState).EndInvoke(iar));
        }
        catch (Exception ex)
        {
          result.Reject(ex);
        }
      }, method);
      return result;
    }

    internal static void AsyncInvoke(Action method)
    {
      method.BeginInvoke(iar =>
      {
        ((Action)iar.AsyncState).EndInvoke(iar);
      }, method);
    }
    internal static void AsyncInvoke<T>(Action<T> method, T arg1)
    {
      method.BeginInvoke(arg1, iar =>
      {
        ((Action<T>)iar.AsyncState).EndInvoke(iar);
      }, method);
    }
    internal static void AsyncInvoke<T1, T2>(Action<T1, T2> method, T1 arg1, T2 arg2)
    {
      method.BeginInvoke(arg1, arg2, iar =>
      {
        ((Action<T1, T2>)iar.AsyncState).EndInvoke(iar);
      }, method);
    }

    internal static XElement EnsureElement(this XElement parent, XName name)
    {
      var result = parent.Element(name);
      if (result == null)
      {
        result = new XElement(name);
        parent.Add(result);
      }
      return result;
    }


    public static string Left(this string str, int count)
    {
      if (string.IsNullOrEmpty(str) || count < 1)
        return string.Empty;
      else
        return str.Substring(0, Math.Min(count, str.Length));
    }

    internal static void Rethrow(this Exception ex)
    {
      if (!string.IsNullOrEmpty(ex.StackTrace))
      {
#if REFLECTION
        typeof(Exception).GetMethod("PrepForRemoting",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(ex, new object[0]);
        throw ex;
#else
        ex.Source = ex.StackTrace;
#endif
      }
      throw ex;
    }

    internal static T Single<T>(this IEnumerable<T> source, Func<T, bool> predicate, Func<int, Exception> errorHandler)
    {
      if (source == null) throw new ArgumentNullException("source");
      if (predicate == null) throw new ArgumentNullException("predicate");
      if (errorHandler == null) throw new ArgumentNullException("errorHandler");

      var found = false;
      var result = default(T);
      foreach (var current in source)
      {
        if (predicate(current))
        {
          if (found)
          {
            throw errorHandler(2);
          }
          else
          {
            result = current;
            found = true;
          }
        }
      }

      if (found) return result;
      throw errorHandler(0);
    }

    public static bool IsGuid(this string value)
    {
      if (value == null || value.Length != 32) return false;
      for (var i = 0; i < value.Length; i++)
      {
        switch (value[i])
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
          case 'A':
          case 'B':
          case 'C':
          case 'D':
          case 'E':
          case 'F':
          case 'a':
          case 'b':
          case 'c':
          case 'd':
          case 'e':
          case 'f':
            break;
          default:
            return false;
        }
      }
      return true;
    }

    internal static TextWriter Append(this TextWriter writer, string value)
    {
      writer.Write(value);
      return writer;
    }
    internal static TextWriter Append(this TextWriter writer, char value)
    {
      writer.Write(value);
      return writer;
    }
    internal static TextWriter Append(this TextWriter writer, int value)
    {
      writer.Write(value);
      return writer;
    }

    internal static void CopyTo(this XmlReader xml, XmlWriter writer)
    {
      var num = (xml.NodeType == XmlNodeType.None) ? -1 : xml.Depth;
      do
      {
        switch (xml.NodeType)
        {
          case XmlNodeType.Element:
            writer.WriteStartElement(xml.Prefix, xml.LocalName, xml.NamespaceURI);
            var empty = xml.IsEmptyElement;
            if (xml.MoveToFirstAttribute())
            {
              do
              {
                writer.WriteStartAttribute(xml.Prefix, xml.LocalName, xml.NamespaceURI);
                while (xml.ReadAttributeValue())
                {
                  if (xml.NodeType == XmlNodeType.EntityReference)
                  {
                    writer.WriteEntityRef(xml.Name);
                  }
                  else
                  {
                    writer.WriteString(xml.Value);
                  }
                }
                writer.WriteEndAttribute();
              }
              while (xml.MoveToNextAttribute());
            }
            if (empty)
            {
              writer.WriteEndElement();
            }
            break;
          case XmlNodeType.Text:
            writer.WriteString(xml.Value);
            break;
          case XmlNodeType.CDATA:
            writer.WriteCData(xml.Value);
            break;
          case XmlNodeType.EntityReference:
            writer.WriteEntityRef(xml.Name);
            break;
          case XmlNodeType.SignificantWhitespace:
            writer.WriteWhitespace(xml.Value);
            break;
          case XmlNodeType.EndElement:
            writer.WriteFullEndElement();
            break;

            //Just ignore the following
            //case XmlNodeType.Whitespace:
            //case XmlNodeType.ProcessingInstruction:
            //case XmlNodeType.XmlDeclaration:
            //case XmlNodeType.Comment:
            //case XmlNodeType.DocumentType:
        }
      }
      while (xml.Read() && (num < xml.Depth || (num == xml.Depth && xml.NodeType == XmlNodeType.EndElement)));
    }

    internal static Stream WriteUtf8(this Stream stream, string value)
    {
      var bytes = Encoding.UTF8.GetBytes(value);
      stream.Write(bytes, 0, bytes.Length);
      return stream;
    }
  }
}
