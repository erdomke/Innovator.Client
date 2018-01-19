using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Interface for polymorphic item type xClassifiableItem_xClass </summary>
  public interface IxClassifiableItem_xClass : IItem
  {
    /// <summary>Retrieve the <c>behavior</c> property of the item</summary>
    IProperty_Text Behavior();
    /// <summary>Retrieve the <c>sort_order</c> property of the item</summary>
    IProperty_Number SortOrder();
  }
}