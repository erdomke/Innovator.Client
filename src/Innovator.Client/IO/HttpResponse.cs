using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Innovator.Client
{
  internal class HttpResponse : IHttpResponse
  {
    private HttpStatusCode _statusCode;
    private IDictionary<string, string> _headers;
    private Stream _stream;

    public Stream AsStream { get { return _stream; } }
    public IDictionary<string, string> Headers { get { return _headers; } }
    public HttpStatusCode StatusCode { get { return _statusCode; } }

    private HttpResponse() { }

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
        var result = new HttpResponse
        {
          _statusCode = task.Result.StatusCode,
          _headers = task.Result.Headers.ToDictionary(k => k.Key, k => k.Value.First())
        };
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
            result._stream = t.Result;
            if (!result._stream.CanSeek && task.Result.Content.Headers.ContentLength.HasValue)
              result._stream = result._stream.WithLength(task.Result.Content.Headers.ContentLength.Value);
            if (task.Result.IsSuccessStatusCode)
            {
              factory.SetResult(result);
            }
            else
            {
              factory.SetException(new HttpException(result));
            }
          }
        }, TaskScheduler.Default);
      }

      return factory.Task;
    }
  }
}
