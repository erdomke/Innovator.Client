using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  public class CloneSettings
  {
    public Func<string, bool> DoRemoveSystemProperty { get; set; }
    public Func<string, IItem, bool> DoCloneItem { get; set; }
  }
}
