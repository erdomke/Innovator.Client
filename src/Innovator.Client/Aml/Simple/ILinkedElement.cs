using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// A <see cref="IReadOnlyElement"/> which can be part of a linked list and has a parent reference
  /// </summary>
  public interface ILinkedElement : ILink<ILinkedElement>
  {
    /// <summary>
    /// Gets or sets the parent of the <see cref="IReadOnlyElement"/>.
    /// </summary>
    /// <value>
    /// The parent of the <see cref="IReadOnlyElement"/>.
    /// </value>
    IReadOnlyElement Parent { get; set; }
  }
}
