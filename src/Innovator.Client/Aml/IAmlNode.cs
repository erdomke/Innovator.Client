using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// Represents a node of AML structure that can be rendered to AML
  /// </summary>
  public interface IAmlNode
  {
    /// <summary>
    /// Write the node to the specified <see cref="XmlWriter"/> as AML
    /// </summary>
    /// <param name="writer"><see cref="XmlWriter"/> to write the node to</param>
    /// <param name="settings">Settings controlling how the node is written</param>
    void ToAml(XmlWriter writer, AmlWriterSettings settings);
  }
}
