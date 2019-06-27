using System.IO;
using System.Net.Http;

namespace Innovator.Client
{
  internal class ResponseContent : StreamContent
  {
    public Stream Stream { get; private set; }

    public ResponseContent(Stream content) : base(content)
    {
      Stream = content;
    }
  }
}
