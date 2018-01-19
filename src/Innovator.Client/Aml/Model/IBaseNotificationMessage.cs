using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Interface for polymorphic item type Base Notification Message </summary>
  public interface IBaseNotificationMessage : IItem
  {
    /// <summary>Retrieve the <c>acknowledge</c> property of the item</summary>
    IProperty_Text Acknowledge();
    /// <summary>Retrieve the <c>priority</c> property of the item</summary>
    IProperty_Text Priority();
    /// <summary>Retrieve the <c>target</c> property of the item</summary>
    IProperty_Item<Identity> Target();
    /// <summary>Retrieve the <c>type</c> property of the item</summary>
    IProperty_Text Type();
  }
}