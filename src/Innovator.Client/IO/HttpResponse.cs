using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Innovator.Client
{
  internal class HttpResponse : Stream, IHttpResponse
  {
    private readonly Stream _stream;
    private readonly long? _length;

    public Stream AsStream { get { return this; } }
    public IDictionary<string, string> Headers { get; }
    public HttpStatusCode StatusCode { get; }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => _length ?? _stream.Length;

    public override long Position { get => _stream.Position; set => _stream.Position = value; }

    internal HttpResponse(HttpStatusCode statusCode, IDictionary<string, string> headers, Stream stream)
    {
      StatusCode = statusCode;
      Headers = headers;
      _stream = stream;
      if (!_stream.CanSeek
        && Headers.TryGetValue("Content-Length", out var contentLength)
        && long.TryParse(contentLength, out var length))
        _length = length;
    }

    public static Task<IHttpResponse> Create(Task<HttpResponseMessage> task)
    {
      var factory = new TaskCompletionSource<IHttpResponse>();

      if (task.IsCanceled)
      {
        factory.SetCanceled();
        //factory.SetException(new HttpTimeoutException());
      }
      else if (task.IsFaulted)
      {
        factory.SetException(task.Exception);
      }
      else
      {
        task.Result.Content.ReadAsStreamAsync().ContinueWith(t =>
        {
          if (t.IsCanceled)
          {
            factory.SetCanceled();
          }
          else if (t.IsFaulted)
          {
            factory.SetException(t.Exception);
          }
          else
          {
            var result = new HttpResponse(task.Result.StatusCode
              , task.Result.Headers.Concat(task.Result.Content.Headers)
                .ToDictionary(k => k.Key, k => k.Value.First(), StringComparer.OrdinalIgnoreCase)
              , t.Result);
            if (task.Result.IsSuccessStatusCode)
              factory.SetResult(result);
            else
              factory.SetException(new HttpException(result));
          }
        }, TaskScheduler.Default);
      }

      return factory.Task;
    }

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
