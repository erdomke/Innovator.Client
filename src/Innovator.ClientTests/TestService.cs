using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Innovator.Client.Tests
{
  class TestService : SyncHttpClient
  {
    public Func<HttpRequestMessage, HttpResponseMessage> ResponseGenerator { get; set; }

    public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      return Task.FromResult(ResponseGenerator?.Invoke(request) ?? new HttpResponseMessage());
    }
  }
}
