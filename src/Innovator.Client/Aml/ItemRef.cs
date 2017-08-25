using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// A reference to an item containing just a type and ID
  /// </summary>
  /// <seealso cref="Innovator.Client.IItemRef" />
  public class ItemRef : IItemRef
  {
    private string _id;
    private string _type;

    /// <summary>
    /// The ID of the item as retrieved from either the attribute or the property
    /// </summary>
    /// <returns></returns>
    public string Id()
    {
      return _id;
    }

    /// <summary>
    /// The type of the item as retrieved from either the attribute or the property
    /// </summary>
    /// <returns></returns>
    public string TypeName()
    {
      return _type;
    }

#if DYNAMIC
    public ItemRef(dynamic value)
    {
      _id = value.Id;
      _type = value.Type;
    }
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemRef"/> class.
    /// </summary>
    /// <param name="type">The item type name.</param>
    /// <param name="id">The id.</param>
    public ItemRef(string type, string id)
    {
      _id = id;
      _type = type;
    }
  }
}
