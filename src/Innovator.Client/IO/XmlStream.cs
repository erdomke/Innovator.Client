using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client
{
  internal class XmlStream : Stream, IXmlStream
  {
    private Func<XmlReader> _readerFactory;
    private MemoryTributary _stream;

    public XmlStream(Func<XmlReader> readerFactory)
    {
      _readerFactory = readerFactory;
    }

    public override bool CanRead { get { return true; } }

    public override bool CanSeek { get { return true; } }

    public override bool CanWrite { get { return true; } }

    public override long Length
    {
      get { return EnsureStream().Length; }
    }

    public override long Position
    {
      get { return EnsureStream().Position; }
      set { EnsureStream().Position = value; }
    }

    public XmlReader CreateReader()
    {
      return _readerFactory();
    }

    public override void Flush()
    {
      // Do nothing
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return EnsureStream().Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return EnsureStream().Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      EnsureStream().SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      EnsureStream().Write(buffer, offset, count);
    }

    private Stream EnsureStream()
    {
      if (_stream != null)
        return _stream;

      _stream = new MemoryTributary();
      using (var writer = XmlWriter.Create(_stream, new XmlWriterSettings() { OmitXmlDeclaration = true }))
      {
        var reader = _readerFactory();
        writer.WriteNode(reader, false);
      }
      return _stream;
    }
  }
}
