using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
