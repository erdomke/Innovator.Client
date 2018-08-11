#if XMLLEGACY
using System.Xml;

namespace Innovator.Client.IOM
{
  public class Logical : Item, ILogical
  {
    public override string Name => Xml?.LocalName;

    internal Logical(Innovator innovator, Element parent, XmlElement element) : base(innovator, element)
    {
      Parent = parent;
    }
  }
}
#endif
