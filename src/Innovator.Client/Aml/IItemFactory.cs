using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// Creates an instance of a strongly-typed class (which inherits from <see cref="Item"/>) that 
  /// represents a specific item type
  /// </summary>
  public interface IItemFactory
  {
    /// <summary>
    /// Creates an instance of a strongly-typed class (which inherits from <see cref="Item"/>) that 
    /// represents the item type <paramref name="type"/>
    /// </summary>
    /// <param name="factory">Factory to be passed to the <see cref="Item"/> constructor</param>
    /// <param name="type">Item type name</param>
    Item NewItem(ElementFactory factory, string type);
  }
}
