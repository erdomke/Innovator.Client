using System.Net.Http;

namespace Innovator.Client
{
  internal class SyncHttpClient : HttpClient
  {
    private SyncClientHandler _handler;

    public SyncHttpClient() : this(new SyncClientHandler()) { }
    public SyncHttpClient(SyncClientHandler handler) : base(handler)
    {
      _handler = handler;
    }

#if HTTPSYNC
    public virtual HttpResponseMsg Send(HttpRequest request)
    {
      return _handler.Send(request);
    }
#endif
  }
}
