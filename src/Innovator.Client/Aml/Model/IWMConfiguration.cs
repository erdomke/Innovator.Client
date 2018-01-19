using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Interface for polymorphic item type WMConfiguration </summary>
  public interface IWMConfiguration : IItem
  {
    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    IProperty_Text Description();
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    IProperty_Text NameProp();
    /// <summary>Retrieve the <c>watermark_context</c> property of the item</summary>
    IProperty_Text WatermarkContext();
  }
}