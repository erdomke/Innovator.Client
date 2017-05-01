using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Innovator.Client
{
  public static class Extensions
  {
    public static string ToArasId(this Guid? guid)
    {
      if (!guid.HasValue)
        return null;
      return guid.Value.ToArasId();
    }
    public static string ToArasId(this Guid guid)
    {
      return guid.ToString("N").ToUpperInvariant();
    }

    public static bool IsNullOrEmpty(this SecureToken token)
    {
      return token == null || token.Length < 1;
    }

    public static string AsString(this Stream data)
    {
      using (var reader = new StreamReader(data))
      {
        return reader.ReadToEnd();
      }
    }
    public static byte[] AsBytes(this Stream data)
    {
      using (var memStream = new MemoryStream())
      {
        data.CopyTo(memStream);
        return memStream.ToArray();
      }
    }

    public static string GroupConcat<T>(this IEnumerable<T> values, string separator, Func<T, string> renderer = null)
    {
      if (values.Any())
      {
        if (renderer == null)
        {
          return values.Select(v => v.ToString()).Aggregate((p, c) => p + separator + c);
        }
        return values.Select(renderer).Aggregate((p, c) => p + separator + c);
      }
      else
      {
        return string.Empty;
      }
    }

    public static string AsString(this IHttpResponse resp)
    {
      if (resp.AsStream.CanSeek) resp.AsStream.Position = 0;
      using (var reader = new System.IO.StreamReader(resp.AsStream))
      {
        return reader.ReadToEnd();
      }
    }
    public static XElement AsXml(this IHttpResponse resp)
    {
      if (resp.AsStream.CanSeek) resp.AsStream.Position = 0;
      using (var reader = new System.IO.StreamReader(resp.AsStream))
      {
        return XElement.Load(reader);
      }
    }

#if FILEIO && MD5
    public static string Checksum(this FileInfo fileInfo)
    {
      if (!File.Exists(fileInfo.FullName))
        throw new ArgumentException("The spcecified file doesn't exist.", "fileInfo");

      if (Directory.Exists(fileInfo.FullName))
        throw new ArgumentException("The specified path is a directory and not a file.", "fileInfo");

      using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      using (var mD = System.Security.Cryptography.MD5.Create())
      {
        return mD.ComputeHash(fileStream).HexString();
      }
    }
#endif

#if FILEIO
    public static void Save(this Stream input, string path)
    {
      using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
      {
        input.CopyTo(fs);
      }
    }
#endif
  }
}
