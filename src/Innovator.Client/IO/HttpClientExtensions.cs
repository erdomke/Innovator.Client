using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Innovator.Client
{
  internal static class HttpClientExtensions
  {
    private static IPromise<T> ToHttpPromise<T>(this Task<T> task, TimeoutSource timeout, LogData trace)
    {
      var promiseResult = new Promise<T>();
      promiseResult.CancelTarget(timeout);
      task
        .ContinueWith(t =>
        {
          if (t.IsFaulted)
          {
            Exception ex = t.Exception;
            while (ex != null && ex.InnerException != null && ex.GetType().Name == "AggregateException")
              ex = ex.InnerException;

            if (ex != null)
            {
              foreach (var kvp in trace)
              {
                ex.Data[kvp.Key] = kvp.Value;
              }
              trace.Add("exception", ex);
              if (ex is HttpException httpException)
              {
                var serverException = httpException.TryGetServerException();
                if (serverException != null)
                {
                  promiseResult.Reject(serverException);
                  return;
                }
              }
            }
            promiseResult.Reject(ex);
          }
          else if (t.IsCanceled)
          {
            promiseResult.Reject(new HttpTimeoutException(string.Format("A response was not received after waiting for {0:m' minutes, 's' seconds'}", TimeSpan.FromMilliseconds(timeout.TimeoutDelay))));
          }
          else
          {
            promiseResult.Resolve(t.Result);
          }
        });
      return promiseResult;
    }

    public static IPromise<IHttpResponse> PostPromise(this HttpClient service, Uri uri, bool async, HttpRequest req, LogData trace)
    {
      req.RequestUri = uri;
      req.Method = HttpMethod.Post;
      req.Async = async;

#if HTTPSYNC
      if (!async && req.Content is ISyncContent && service is SyncHttpClient)
        return SendSync((SyncHttpClient)service, req, trace);
#endif

      var timeout = new TimeoutSource();
      timeout.CancelAfter((int)req.Timeout.TotalMilliseconds);

      var result = service.SendAsync(req, timeout.Source.Token)
        .ContinueWith(HttpResponse.Create, TaskScheduler.Default)
        .Unwrap()
        .ToHttpPromise(timeout, trace);
      if (!async)
        result.WaitNoError();
      return result;
    }

    public static IPromise<IHttpResponse> GetPromise(this HttpClient service, Uri uri, bool async, LogData trace, HttpRequest req = null)
    {
      if (req == null)
        req = new HttpRequest();
      req.RequestUri = uri;
      req.Method = HttpMethod.Get;
      req.Async = async;

#if HTTPSYNC
      if (!async && service is SyncHttpClient)
        return SendSync((SyncHttpClient)service, req, trace);
#endif

      var timeout = new TimeoutSource();
      timeout.CancelAfter((int)req.Timeout.TotalMilliseconds);
      var respTask = service.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, timeout.Source.Token);

      var result = respTask
        .ContinueWith(HttpResponse.Create, TaskScheduler.Default)
        .Unwrap()
        .ToHttpPromise(timeout, trace);
      if (!async)
        result.WaitNoError();
      return result;
    }

#if HTTPSYNC
    private static IPromise<IHttpResponse> SendSync(SyncHttpClient service, HttpRequest req, LogData trace)
    {
      try
      {
        return Promises.Resolved((IHttpResponse)service.Send(req));
      }
      catch (System.Net.WebException webex)
      {
        switch (webex.Status)
        {
          case System.Net.WebExceptionStatus.RequestCanceled:
          case System.Net.WebExceptionStatus.Timeout:
            return Promises.Rejected<IHttpResponse>(new HttpTimeoutException(string.Format("A response was not received after waiting for {0:m' minutes, 's' seconds'}", req.Timeout)));
          default:
            foreach (var kvp in trace)
            {
              webex.Data[kvp.Key] = kvp.Value;
            }
            trace.Add("exception", webex);
            return Promises.Rejected<IHttpResponse>(webex);
        }
      }
      catch (HttpException httpException)
      {
        foreach (var kvp in trace)
        {
          httpException.Data[kvp.Key] = kvp.Value;
        }
        trace.Add("exception", httpException);

        var serverException = httpException.TryGetServerException();
        if (serverException != null)
        {
          return Promises.Rejected<IHttpResponse>(serverException);
        }

        return Promises.Rejected<IHttpResponse>(httpException);
      }
      catch (Exception ex)
      {
        foreach (var kvp in trace)
        {
          ex.Data[kvp.Key] = kvp.Value;
        }
        trace.Add("exception", ex);
        return Promises.Rejected<IHttpResponse>(ex);
      }
    }
#endif

    /// <summary>
    /// Try to get the Innovator server exception from the failed transaction response.
    /// The vault throws a 500 on a bad upload, but we want the fault from the aml response.
    /// </summary>
    /// <param name="httpException"></param>
    public static ServerException TryGetServerException(this HttpException httpException)
    {
      try
      {
        var result = ElementFactory.Local.FromXml(httpException.Response.AsXml());
        if (result.Exception != null)
        {
          return result.Exception;
        }
      }
      catch
      {
        // Do nothing as there are many scenarios where we won't have the data for a server exception
      }
      return null;
    }

    private class TimeoutSource : IDisposable, ICancelable
    {
      private CancellationTokenSource _source = new CancellationTokenSource();
      private int _timeoutDelay;

      public CancellationTokenSource Source { get { return _source; } }
      public int TimeoutDelay { get { return _timeoutDelay; } }

      public void CancelAfter(int millisecondsDelay)
      {
        _timeoutDelay = millisecondsDelay;

#if TASKS
        _source.CancelAfter(millisecondsDelay);
#else
        if (_source.IsCancellationRequested)
          return;

        if (_timer == null)
        {
          var timer = new Timer(_timerCallback, this, -1, -1);
          if (Interlocked.CompareExchange<Timer>(ref _timer, timer, null) != null)
          {
            timer.Dispose();
          }
        }
        try
        {
          this._timer.Change(millisecondsDelay, -1);
        }
        catch (ObjectDisposedException) { }
#endif
      }

#if !TASKS
      private Timer _timer;
      private static readonly TimerCallback _timerCallback = new TimerCallback(TimerCallbackLogic);

      private static void TimerCallbackLogic(object obj)
      {
        var source = (TimeoutSource)obj;
        try
        {
          source._source.Cancel();
        }
        catch (ObjectDisposedException) { }
      }
#endif

      public void Dispose()
      {
        if (_source != null)
        {
          _source.Dispose();
          _source = null;
        }
      }

      public void Cancel()
      {
        _source.Cancel();
      }
    }
  }
}
