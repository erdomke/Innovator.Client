using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
