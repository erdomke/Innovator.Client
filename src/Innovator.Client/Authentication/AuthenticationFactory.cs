using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Innovator.Client
{
  internal class AuthenticationFactory
  {

    public static IPromise<IAuthenticator> GetAuthenticator(Uri innovatorServerBaseUrl, HttpClient service, ICredentials creds, bool async)
    {
      var discovery = new Uri(innovatorServerBaseUrl, "OAuthServerDiscovery.aspx");
      var req = new HttpRequest();
      req.Headers.Add("Aras-Set-HttpSessionState-Behavior", "switch_to_initial");
      var oauthBaseUri = default(Uri);

      var result = new Promise<IAuthenticator>();
      result.CancelTarget(
        service.GetPromise(discovery, async, new LogData(4
          , "Innovator: Get OAuth URL"
          , Factory.LogListener)
        {
          { "url", discovery },
        }, req)
        .Continue(r =>
        {
          if (r.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("OAuth not found");

          var serverUrls = default(List<Uri>);
          using (var json = new Json.Embed.JsonTextReader(r.AsStream))
          {
            serverUrls = json.Flatten()
              .Where(k => k.Key.StartsWith("$.locations[")
                && k.Key.EndsWith("].uri")
                && k.Value is string str
                && !string.IsNullOrEmpty(str))
              .Select(k => new Uri(k.Value.ToString()))
              .OrderBy(u => string.Equals(u.Host, innovatorServerBaseUrl.Host, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
              .ToList();
          }

          if (serverUrls?.Count < 1)
            throw new InvalidOperationException("OAuth server URL could not be found");

          oauthBaseUri = serverUrls[0];
          var oauthUri = new Uri(oauthBaseUri, ".well-known/openid-configuration");
          return service.GetPromise(oauthUri, async, new LogData(4
            , "Innovator: Get OAuth Config"
            , Factory.LogListener)
          {
            { "url", oauthUri },
          });
        })
        .Progress(result.Notify)
        .Done(r =>
        {
          try
          {
            var config = new OAuthConfig(r.AsString(), oauthBaseUri);
            if (config.ProtocolVersion >= new Version("1.0"))
              result.Resolve(new OAuthAuthenticator(service, config, creds));
            else
              result.Resolve(new LegacyAuthenticator(innovatorServerBaseUrl, creds));
          }
          catch (Exception ex)
          {
            result.Reject(ex);
          }
        })
        .Fail(e =>
        {
          result.Resolve(new LegacyAuthenticator(innovatorServerBaseUrl, creds));
        }));
      return result;
    }
  }
}
