using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  internal class FormContent : MultipartFormDataContent, ISyncContent
  {
    private CompressionType _compression;
    private string _boundary;

    public CompressionType Compression
    {
      get { return _compression; }
      set
      {
        _compression = value;
        Headers.ContentEncoding.Clear();
        if (_compression != CompressionType.none)
          Headers.ContentEncoding.Add(_compression.ToString());
      }
    }

    public FormContent() : this(GetBoundaryLine()) { }
    public FormContent(string boundary) : base(boundary)
    {
      _boundary = boundary;
      var mediaTypeHeaderValue = new MediaTypeHeaderValue("multipart/form-data");
      mediaTypeHeaderValue.Parameters.Add(new NameValueHeaderValue("boundary", boundary));
      base.Headers.ContentType = mediaTypeHeaderValue;
    }

    public void Add(string name, string value)
    {
      var content = new SimpleContent(Encoding.UTF8.GetBytes(value));
      content.Headers.Add("Content-Disposition", "form-data; name=\"" + name + "\"");
      base.Add(content);
    }

    protected override bool TryComputeLength(out long length)
    {
      if (Compression == CompressionType.none)
        return base.TryComputeLength(out length);

      length = -1;
      return false;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
      Stream compressedStream = null;
      if (Compression == CompressionType.none)
        return base.SerializeToStreamAsync(stream, context);
      else if (Compression == CompressionType.gzip)
        compressedStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
      else if (Compression == CompressionType.deflate)
        compressedStream = new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true);
      else
        throw new NotSupportedException();

      return base.SerializeToStreamAsync(compressedStream, context).ContinueWith(tsk =>
      {
        if (compressedStream != null)
          compressedStream.Dispose();
      });
    }

    private const int SessionHashLen = 14;
    private const string Symbols = "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM";
    private static readonly Random _rnd = new Random();
    private static string GetBoundaryLine()
    {
      var sb = new StringBuilder("---------------------------", 27 + SessionHashLen);
      for (int i = 0; i < SessionHashLen; i++)
      {
        sb.Append(Symbols[(int)(_rnd.NextDouble() * (double)(Symbols.Length - 1))]);
      }
      return sb.ToString();
    }

    public void SerializeToStream(Stream stream)
    {
      var first = true;
      stream.WriteUtf8("--" + _boundary + "\r\n");
      foreach (var child in this)
      {
        var builder = new StringBuilder();
        if (!first)
          builder.Append("\r\n--").Append(_boundary).Append("\r\n");
        foreach (var header in child.Headers)
        {
          builder.Append(header.Key)
            .Append(": ")
            .Append(header.Value.GroupConcat(","))
            .Append("\r\n");
        }
        builder.Append("\r\n");
        stream.WriteUtf8(builder.ToString());

        ((ISyncContent)child).SerializeToStream(stream);
        first = false;
      }
      stream.WriteUtf8("\r\n--" + _boundary + "--\r\n");
    }
  }
}
