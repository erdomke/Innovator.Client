using System;
using System.IO;
using System.Text;

namespace Innovator.Client
{
  public class TokenCredentials : IUserCredentials
  {
    public string AccessToken { get; }
    public DateTime Expires { get; private set; }
    public string Database { get; private set; }
    public string TokenType { get; }
    public string Username { get; private set; }

    public TokenCredentials(string token)
    {
      TokenType = "Bearer";
      AccessToken = token;

      InitAccessToken();
    }

    internal TokenCredentials(Stream json, string database)
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

      InitAccessToken();
    }

    private void InitAccessToken()
    {
      var parts = AccessToken.Split('.');
      if (parts.Length < 2)
        throw new ArgumentException("Invalid JWT token");

      var body = parts[1];
      body = body.Replace('-', '+').Replace('_', '/');
      var diff = body.Length % 4;
      if (diff > 0)
        body += new string('=', 4 - diff);

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
            case "$.username":
              Username = kvp.Value?.ToString();
              break;
          }
        }
      }
    }
  }
}
