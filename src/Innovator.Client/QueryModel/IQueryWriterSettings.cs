using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public interface IQueryWriterSettings
  {
    /// <summary>
    /// Gets the property metadata for an itemtype by name.
    /// </summary>
    /// <param name="itemType">Name of the itemtype</param>
    IDictionary<string, Model.IPropertyDefinition> GetProperties(string itemType);
  }
}
