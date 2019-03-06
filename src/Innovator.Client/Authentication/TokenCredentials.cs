using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  internal class TokenCredentials : ICredentials
  {
    public string AccessToken { get; }
    public DateTime Expires { get; }
    public string Database { get; }
    public string TokenType { get; }

    public TokenCredentials(Stream json, string database)
    {
      Database = database;
      using (var reader = new Json.Embed.JsonTextReader(json))
      {
        foreach (var kvp in reader.Flatten())
        {
          switch (kvp.Key)
          {
            case "$.access_token":
              AccessToken = kvp.Value.ToString();
              break;
            case "$.token_type":
              TokenType = kvp.Value.ToString();
              break;
            case "$.expires_in":
              Expires = DateTime.UtcNow.AddSeconds((int)kvp.Value);
              break;
          }
        }
      }

      if (string.IsNullOrEmpty(AccessToken))
        throw new InvalidOperationException("An access token must be specified.");

      var parts = AccessToken.Split('.');
      if (parts.Length > 1)
      {
        var body = parts[1];
        body.Replace('-', '+').Replace('_', '/');
        while ((body.Length % 4) != 0)
          body += "=";
        var bytes = Convert.FromBase64String(body);
        var jsonBody = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        using (var reader = new Json.Embed.JsonTextReader(jsonBody))
        {
          foreach (var kvp in reader.Flatten())
          {
            switch (kvp.Key)
            {
              case "$.exp":
                Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((int)kvp.Value);
                break;
              case "$.database":
                Database = kvp.Value?.ToString() ?? Database;
                break;
            }
          }
        }
      }
    }
  }
}
