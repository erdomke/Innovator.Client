using System;
using System.Linq;
using System.Net.Http;

namespace Innovator.Client
{
  internal class HttpRequest : HttpRequestMessage, IHttpRequest
  {
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100);

    public bool Async { get; set; }
    public TimeSpan Timeout { get; set; }
    public string UserAgent
    {
      get { return Headers.GetValues("User-Agent").FirstOrDefault(); }
      set { SetHeader("User-Agent", value); }
    }

    public HttpRequest() : base()
    {
      this.Async = true;
      this.Version = new Version(1, 1);
      this.Timeout = DefaultTimeout;
    }

    public void SetHeader(string name, string value)
    {
      if (Headers.Contains(name))
        Headers.Remove(name);
      Headers.TryAddWithoutValidation(name, value);
    }
  }
}
