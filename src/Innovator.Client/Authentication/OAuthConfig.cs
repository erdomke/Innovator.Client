using System;
using System.IO;

namespace Innovator.Client
{
  internal class OAuthConfig
  {
    public Uri TokenEndpoint { get; }
    public Uri AuthorizeEndpoint { get; }
    public Version ProtocolVersion { get; } = new Version(0, 0);
    public ProtocolType ProtocolType { get; }

    public OAuthConfig(string json, Uri baseUri)
    {
      AuthorizeEndpoint = new Uri(baseUri, "connect/authorize");
      TokenEndpoint = new Uri(baseUri, "connect/token");

      using (var reader = new Json.Embed.JsonTextReader(json))
      {
        foreach (var kvp in reader.Flatten())
        {
          switch (kvp.Key)
          {
            case "$.authorization_endpoint":
              //AuthorizeEndpoint = new Uri(kvp.Value.ToString());
              break;
            case "$.token_endpoint":
              //TokenEndpoint = new Uri(kvp.Value.ToString());
              break;
            case "$.protocol_info.protocol_type":
              if (string.Equals(kvp.Value?.ToString(), "Standard", StringComparison.OrdinalIgnoreCase))
                ProtocolType = ProtocolType.Standard;
              else
                ProtocolType = ProtocolType.Custom;
              break;
            case "$.protocol_version":
#if NET35
              try
              {
                ProtocolVersion = new Version(kvp.Value?.ToString());
              }
              catch (Exception)
              {
                ProtocolVersion = new Version(0, 0);
              }
#else
              if (Version.TryParse(kvp.Value?.ToString(), out var version))
                ProtocolVersion = version;
              else
                ProtocolVersion = new Version(0, 0);
#endif
              break;
          }
        }
      }
    }
  }
  internal enum ProtocolType
  {
    Standard,
    Custom
  }
}
