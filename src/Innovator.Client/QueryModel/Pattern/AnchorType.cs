using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client.QueryModel
{
  public enum AnchorType
  {
    // \A
    Start_Absolute,
    // ^
    Start_Line,
    // \b
    WordBoundary,
    // \z
    End_Absolute,
    // \Z
    End_BeforeNewline,
    // $
    End_Line
  }
}
