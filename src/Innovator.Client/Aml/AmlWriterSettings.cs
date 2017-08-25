using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// Settings controlling how to serialize an AML structure
  /// </summary>
  public class AmlWriterSettings
  {
    /// <summary>
    /// Gets or sets a value indicating whether to expand property items.
    /// </summary>
    /// <value>
    ///   <c>true</c> to expand property items; otherwise, <c>false</c>.
    /// </value>
    public bool ExpandPropertyItems { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmlWriterSettings"/> class.
    /// </summary>
    public AmlWriterSettings()
    {
      this.ExpandPropertyItems = true;
    }
  }
}
