using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
#if TASKS
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Innovator.Client
{
  /// <summary>
  /// Useful extension methods
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    /// Renders a <see cref="Nullable{Guid}"/> using the default Aras ID format.
    /// </summary>
    /// <param name="guid"><see cref="Nullable{Guid}"/> to render as an ID</param>
    /// <returns><c>null</c> is <paramref name="guid"/> does not have a value, otherwise a 
    /// 32-character <see cref="String"/> representing the ID</returns>
    public static string ToArasId(this Guid? guid)
    {
      if (!guid.HasValue)
        return null;
      return guid.Value.ToArasId();
    }

    /// <summary>
    /// Renders a <see cref="Guid"/> using the default Aras ID format.
    /// </summary>
    /// <param name="guid"><see cref="Guid"/> to render as an ID</param>
    /// <returns>32-character <see cref="String"/> representing the ID</returns>
    public static string ToArasId(this Guid guid)
    {
      return guid.ToString("N").ToUpperInvariant();
    }

    /// <summary>
    /// Indicates if a <see cref="SecureToken"/> is null or empty
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>
    ///   <c>true</c> if <paramref name="token"/> is null or empty (with a <see cref="SecureToken.Length"/> &lt; 0; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty(this SecureToken token)
    {
      return token == null || token.Length < 1;
    }

    /// <summary>
    /// Render a <see cref="Stream"/> to a <see cref="String"/>
    /// </summary>
    /// <param name="data"><see cref="Stream"/> to render</param>
    /// <returns>A <see cref="String"/> representing the contents of <paramref name="data"/></returns>
    public static string AsString(this Stream data)
    {
      using (var reader = new StreamReader(data))
      {
        return reader.ReadToEnd();
      }
    }

    /// <summary>
    /// Render a <see cref="Stream"/> to a <see cref="Byte"/> array
    /// </summary>
    /// <param name="data"><see cref="Stream"/> to render</param>
    /// <returns>A <see cref="Byte"/> array representing the contents of <paramref name="data"/></returns>
    public static byte[] AsBytes(this Stream data)
    {
      var memStream = data as MemoryStream;
      if (memStream != null)
        return memStream.ToArray();

      var memTrib = data as MemoryTributary;
      if (memTrib != null)
        return memTrib.ToArray();

      using (memStream = new MemoryStream())
      {
        data.CopyTo(memStream);
        return memStream.ToArray();
      }
    }

    internal static Stream Seekable(this Stream data)
    {
      if (data.CanSeek)
      {
        data.Position = 0;
        return data;
      }

      var trib = new MemoryTributary();
      data.CopyTo(trib);
      trib.Position = 0;
      return trib;
    }

    /// <summary>
    /// Concatenate the string representation of a set of values to a single string separated by <paramref name="separator"/>
    /// </summary>
    /// <param name="values">Values to concatenate</param>
    /// <param name="separator"><see cref="string"/> to use as a separator</param>
    /// <param name="renderer">Function used to render a value as a string.  If not specified, <see cref="object.ToString"/> is used</param>
    /// <returns>A single string containing the string representation of each value in <paramref name="values"/></returns>
    /// <remarks>This performs a similar function to <see cref="String.Join(string, IEnumerable{string})"/> which is 
    /// available in .Net 4+</remarks>
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

    /// <summary>
    /// Returns a string from the HTTP response
    /// </summary>
    /// <param name="resp">The HTTP response.</param>
    /// <returns>A string from the HTTP response</returns>
    public static string AsString(this IHttpResponse resp)
    {
      if (resp.AsStream.CanSeek) resp.AsStream.Position = 0;
      using (var reader = new System.IO.StreamReader(resp.AsStream))
      {
        return reader.ReadToEnd();
      }
    }

    /// <summary>
    /// Creates an <see cref="XElement"/> from the data returned by the <see cref="IHttpResponse"/>
    /// </summary>
    /// <param name="resp"><see cref="IHttpResponse"/> returned by the server</param>
    /// <returns>An <see cref="XElement"/> representing the XML from the response</returns>
    public static XElement AsXml(this IHttpResponse resp)
    {
      var stream = resp.AsStream;
      if (stream.CanSeek) stream.Position = 0;

      var xmlStream = stream as IXmlStream;
      if (xmlStream == null)
      {
        using (var reader = new System.IO.StreamReader(stream))
        {
          return XElement.Load(reader);
        }
      }
      else
      {
        using (var reader = xmlStream.CreateReader())
        {
          return XElement.Load(reader);
        }
      }
    }

    /// <summary>
    /// Require use of writable session state.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="stateBehavior">Session state behavior to use</param>
    public static void SetSessionStateBehavior(this IHttpRequest request, SessionStateBehavior stateBehavior)
    {
      if (stateBehavior != SessionStateBehavior.Default)
        request.SetHeader(ArasHeaders.SessionStateBehaviorHeader, ArasHeaders.SessionStateBehaviorToString(stateBehavior));
    }

    /// <summary>
    /// Returns the service pack number based on the version revision
    /// </summary>
    /// <param name="version">The Aras version.</param>
    /// <returns>The service pack number</returns>
    public static int? ServicePack(this Version version)
    {
      if (version == null)
        return null;
      if (version.Build < 6296) // 11.0.0.6296 = SP5
        return 0;
      if (version.Build < 6549) // 11.0.0.6549 = SP9
        return 5;
      if (version.Build < 6920) // 11.0.0.6920 = SP12
        return 9;
      return 12;
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

    /// <summary>
    /// Returns a new stream where the length property returns the correct value.
    /// </summary>
    /// <param name="data">Stream data to annotate</param>
    /// <param name="length">The known length of the stream</param>
    /// <returns>An annotated stream</returns>
    public static Stream WithLength(this Stream data, long length)
    {
      return new LengthAnnotatedStream(data, length);
    }

    private class LengthAnnotatedStream : Stream
    {
      private readonly Stream _stream;
      private readonly long _length;

      public LengthAnnotatedStream(Stream stream, long length)
      {
        _stream = stream;
        _length = length;
      }

      public override bool CanRead => _stream.CanRead;

      public override bool CanSeek => _stream.CanSeek;

      public override bool CanWrite => _stream.CanWrite;

      public override long Length => _length;

      public override long Position { get => _stream.Position; set => _stream.Position = value; }

      public override void Flush()
      {
        _stream.Flush();
      }

#if HTTPSYNC && TASKS
      public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
      {
        return _stream.BeginRead(buffer, offset, count, callback, state);
      }

      public override int EndRead(IAsyncResult asyncResult)
      {
        return _stream.EndRead(asyncResult);
      }

      public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
      {
        return _stream.ReadAsync(buffer, offset, count, cancellationToken);
      }
#endif

      public override int Read(byte[] buffer, int offset, int count)
      {
        return _stream.Read(buffer, offset, count);
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
        return _stream.Seek(offset, origin);
      }

      public override void SetLength(long value)
      {
        _stream.SetLength(value);
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        _stream.Write(buffer, offset, count);
      }

      protected override void Dispose(bool disposing)
      {
        base.Dispose(disposing);
        if (disposing)
          _stream.Dispose();
      }
    }
  }
}
