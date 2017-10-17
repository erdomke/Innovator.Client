using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// A stream with a custom approach for reading XML
  /// </summary>
  public interface IXmlStream
  {
    /// <summary>
    /// Creates an <see cref="XmlReader"/> for reading XML from the stream
    /// </summary>
    /// <returns>An <see cref="XmlReader"/> for reading XML</returns>
    XmlReader CreateReader();
  }
}
