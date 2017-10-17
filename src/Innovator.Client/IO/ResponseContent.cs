using System.IO;
using System.Net.Http;

namespace Innovator.Client
{
  internal class ResponseContent : StreamContent
  {
    private MemoryTributary _content;

    public Stream Stream { get { return _content; } }

    public ResponseContent(MemoryTributary content) : base(content)
    {
      _content = content;
    }
  }
}
