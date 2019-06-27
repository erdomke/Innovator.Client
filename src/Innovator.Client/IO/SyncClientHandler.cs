using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Innovator.Client
{
  internal class SyncClientHandler : HttpClientHandler
  {
#if HTTPSYNC
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      var req = request as HttpRequest;
      if (req == null || req.Async || (req.Content != null && !(req.Content is ISyncContent)))
        return base.SendAsync(request, cancellationToken).ContinueWith(t =>
        {
          request.Content?.Dispose();
          return t.Result;
        });

      var factory = new TaskCompletionSource<HttpResponseMessage>();
      try
      {
        factory.SetResult(Send(req));
      }
      catch (HttpTimeoutException)
      {
        factory.SetCanceled();
      }
      catch (Exception ex)
      {
        factory.SetException(ex);
      }

      return factory.Task;
    }

    public HttpResponseMsg Send(HttpRequest request)
    {
      var wReq = CreateAndPrepareWebRequest(request);
      wReq.Timeout = (int)request.Timeout.TotalMilliseconds;
      var syncContent = request.Content as ISyncContent;
      if (syncContent != null)
      {
        using (var stream = wReq.GetRequestStream())
        {
          syncContent.SerializeToStream(stream);
        }
      }

      try
      {
        return CreateResponseMessage((HttpWebResponse)wReq.GetResponse(), request);
      }
      catch (WebException webex)
      {
        switch (webex.Status)
        {
          case WebExceptionStatus.RequestCanceled:
          case WebExceptionStatus.Timeout:
            throw new HttpTimeoutException(string.Format("A response was not received after waiting for {0:m' minutes, 's' seconds'}", request.Timeout), webex);
          default:
            var resp = CreateResponseMessage((HttpWebResponse)webex.Response, request);
            throw new HttpException(resp);
        }
      }
    }

    private HttpResponseMsg CreateResponseMessage(HttpWebResponse webResponse, HttpRequestMessage request)
    {
      var httpResponseMessage = new HttpResponseMsg(webResponse.StatusCode)
      {
        ReasonPhrase = webResponse.StatusDescription,
        Version = webResponse.ProtocolVersion,
        RequestMessage = request
      };

      httpResponseMessage.Content = new ResponseContent(new ResponseStreamWrapper(webResponse));
      request.RequestUri = webResponse.ResponseUri;
      WebHeaderCollection headers = webResponse.Headers;
      HttpContentHeaders headers2 = httpResponseMessage.Content.Headers;
      HttpResponseHeaders headers3 = httpResponseMessage.Headers;
      if (webResponse.ContentLength >= 0L)
      {
        headers2.ContentLength = new long?(webResponse.ContentLength);
      }

      for (int i = 0; i < headers.Count; i++)
      {
        string key = headers.GetKey(i);
        if (string.Compare(key, "Content-Length", StringComparison.OrdinalIgnoreCase) != 0)
        {
          string[] values = headers.GetValues(i);
          if (!headers3.TryAddWithoutValidation(key, values))
          {
            bool flag = headers2.TryAddWithoutValidation(key, values);
          }
        }
      }
      return httpResponseMessage;
    }

    private class ResponseStreamWrapper : Stream
    {
      private readonly Stream _stream;
      private readonly HttpWebResponse _webResponse;
      private readonly long? _length;

      public ResponseStreamWrapper(HttpWebResponse webResponse)
      {
        _stream = webResponse.GetResponseStream();
        if (!_stream.CanSeek && webResponse.ContentLength > 0L)
          _length = webResponse.ContentLength;
        _webResponse = webResponse;
      }

      public override bool CanRead => _stream.CanRead;

      public override bool CanSeek => _stream.CanSeek;

      public override bool CanWrite => _stream.CanWrite;

      public override long Length => _length ?? _stream.Length;

      public override long Position { get => _stream.Position; set => _stream.Position = value; }

      public override void Flush()
      {
        _stream.Flush();
      }

      public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
      {
        return _stream.BeginRead(buffer, offset, count, callback, state);
      }

      public override int EndRead(IAsyncResult asyncResult)
      {
        return _stream.EndRead(asyncResult);
      }

#if TASKS
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
        {
          _stream.Dispose();
          _webResponse.Close();
        }
      }
    }


    private HttpWebRequest CreateAndPrepareWebRequest(HttpRequestMessage request)
    {
      var httpWebRequest = (HttpWebRequest)System.Net.WebRequest.Create(request.RequestUri);
      httpWebRequest.Method = request.Method.Method;
      httpWebRequest.ProtocolVersion = request.Version;
      this.SetDefaultOptions(httpWebRequest);
      SyncClientHandler.SetConnectionOptions(httpWebRequest, request);
      this.SetServicePointOptions(httpWebRequest, request);
      SyncClientHandler.SetRequestHeaders(httpWebRequest, request);
      SyncClientHandler.SetContentHeaders(httpWebRequest, request);
      return httpWebRequest;
    }

    private void SetDefaultOptions(HttpWebRequest webRequest)
    {
      webRequest.Timeout = -1;
      webRequest.AllowAutoRedirect = this.AllowAutoRedirect;
      webRequest.AutomaticDecompression = this.AutomaticDecompression;
      webRequest.PreAuthenticate = this.PreAuthenticate;
      if (this.UseDefaultCredentials)
      {
        webRequest.UseDefaultCredentials = true;
      }
      else
      {
        webRequest.Credentials = this.Credentials;
      }

      if (this.AllowAutoRedirect)
      {
        webRequest.MaximumAutomaticRedirections = this.MaxAutomaticRedirections;
      }

      if (this.UseProxy)
      {
        if (this.Proxy != null)
        {
          webRequest.Proxy = this.Proxy;
        }
      }
      else
      {
        webRequest.Proxy = null;
      }

      if (this.UseCookies)
      {
        webRequest.CookieContainer = this.CookieContainer;
      }
    }

    private static void SetConnectionOptions(HttpWebRequest webRequest, HttpRequestMessage request)
    {
      if (request.Version <= HttpVersion.Version10)
      {
        var keepAlive = false;
        foreach (var current in request.Headers.Connection)
        {
          if (string.Compare(current, "Keep-Alive", StringComparison.OrdinalIgnoreCase) == 0)
          {
            keepAlive = true;
            break;
          }
        }

        webRequest.KeepAlive = keepAlive;
        return;
      }

      if (request.Headers.ConnectionClose == true)
      {
        webRequest.KeepAlive = false;
      }
    }

    private void SetServicePointOptions(HttpWebRequest webRequest, HttpRequestMessage request)
    {
      var headers = request.Headers;
      var expectContinue = headers.ExpectContinue;
      if (expectContinue.HasValue)
      {
        var servicePoint = webRequest.ServicePoint;
        if (servicePoint != null)
          servicePoint.Expect100Continue = expectContinue.Value;
      }
    }

    private static void SetRequestHeaders(HttpWebRequest webRequest, HttpRequestMessage request)
    {
      var dest = webRequest.Headers;
      var origin = request.Headers;
      var hasExpect = origin.Contains("Expect");
      var hasTransferEncoding = origin.Contains("Transfer-Encoding");
      var hasConnection = origin.Contains("Connection");

#if !NET35
      var hasHost = origin.Contains("Host");
      if (hasHost)
      {
        string host = origin.Host;
        if (host != null)
          webRequest.Host = host;
      }
#endif

      if (hasExpect)
      {
        var headerValue = origin.Expect.ToString();
        if (!string.IsNullOrEmpty(headerValue))
          webRequest.Expect = headerValue;
      }

      if (hasTransferEncoding)
      {
        var headerValue = origin.TransferEncoding.ToString();
        if (string.Equals(headerValue, "chunked", StringComparison.OrdinalIgnoreCase))
          webRequest.SendChunked = true;
        else if (!string.IsNullOrEmpty(headerValue))
          webRequest.TransferEncoding = headerValue;
      }

      if (hasConnection)
      {
        var headerValue = origin.Connection.ToString();
        if (!string.IsNullOrEmpty(headerValue))
          webRequest.Connection = headerValue;
      }

      SetHeaders(webRequest, origin);
    }

    private static void SetContentHeaders(HttpWebRequest webRequest, HttpRequestMessage request)
    {
      if (request.Content != null)
      {
        // Store the length somewhere so that the header gets generated.
        var length = request.Content.Headers.ContentLength;
        SetHeaders(webRequest, request.Content.Headers);
      }
    }

    private static void SetHeaders(HttpWebRequest webRequest, IEnumerable<KeyValuePair<string, IEnumerable<string>>> origin)
    {
      foreach (var kvp in origin)
      {
        switch (kvp.Key.ToLowerInvariant())
        {
          case "accept":
            webRequest.Accept = kvp.Value.First();
            break;
          case "connection":
          case "expect":
          case "host":
          case "transfer-encoding":
            // Do nothing
            break;
          case "content-length":
            webRequest.ContentLength = long.Parse(kvp.Value.First());
            break;
          case "content-type":
            webRequest.ContentType = kvp.Value.First();
            break;
          case "if-modified-since":
            webRequest.IfModifiedSince = DateTime.Parse(kvp.Value.First());
            break;
          case "media-type":
            webRequest.MediaType = kvp.Value.First();
            break;
          case "referer":
            webRequest.Referer = kvp.Value.First();
            break;
          case "user-agent":
            webRequest.UserAgent = kvp.Value.First();
            break;
          default:
            foreach (var value in kvp.Value)
            {
              webRequest.Headers.Add(kvp.Key, value);
            }
            break;
        }
      }
    }

#endif
  }
}
