using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  public static class ODataExtensions
  {
    public static bool SupportsV4(this ODataVersion version)
    {
      return (version & ODataVersion.v4) == ODataVersion.v4;
    }

    public static bool SupportsV3(this ODataVersion version)
    {
      return (version & ODataVersion.v3) == ODataVersion.v3;
    }

    public static bool SupportsV2(this ODataVersion version)
    {
      return (version & ODataVersion.v2) == ODataVersion.v2;
    }

    public static bool SupportsV2OrV3(this ODataVersion version)
    {
      return (version & ODataVersion.v3) == ODataVersion.v3
        || (version & ODataVersion.v2) == ODataVersion.v2;
    }

    public static bool OnlySupportsV2OrV3(this ODataVersion version)
    {
      return version != 0 && (version & (ODataVersion.v3 | ODataVersion.v2)) == version;
    }
  }
}
