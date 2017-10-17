using System.IO;

namespace Innovator.Client
{
  interface ISyncContent
  {
    void SerializeToStream(Stream stream);
  }
}
