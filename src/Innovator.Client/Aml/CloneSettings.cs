using System;
using System.Collections.Generic;

namespace Innovator.Client
{
  public class CloneSettings
  {
    public Dictionary<string, string> OldIdToNewGeneratedId = new Dictionary<string, string>();

    /// <summary>
    /// Return true/false on whether the passed system property path should be removed.
    /// </summary>
    public Func<string, bool> DoRemoveSystemProperty { get; set; }

    /// <summary>
    /// Return true on whether the passed nested item and path should be cloned as well,
    /// or false if it should be reduced to a link
    /// </summary>
    public Func<string, IReadOnlyItem, bool> DoCloneItem { get; set; }

    /// <summary>
    /// Action used to do any post processing on each item in the tree.
    /// The old id is supplied if the item had a value.
    /// </summary>
    public Action<string, IItem, string> PostProcessClonedItem { get; set; }
  }
}
