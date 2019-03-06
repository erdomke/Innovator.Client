using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  internal class LegacyAuthenticator : IAuthenticator
  {
    private readonly Uri _innovatorClientBin;
    private readonly ICredentials _credentials;
    private IPromise<ExplicitHashCredentials> _hashCreds;
    private Func<SecureToken, string> _hashFunc = ElementFactory.Local.CalcMd5;

    public Func<SecureToken, string> HashFunction
    {
      get { return _hashFunc; }
      set
      {
        _hashFunc = value;
        _hashCreds = null;
      }
    }

    public LegacyAuthenticator(Uri innovatorServerBaseUrl, ICredentials creds)
    {
      _innovatorClientBin = new Uri(innovatorServerBaseUrl, "../Client/cbin/");
      _credentials = creds;
    }

    private IPromise<ExplicitHashCredentials> ValidCredentials(bool async)
    {
      if (_hashCreds == null)
        _hashCreds = HashCredentials(_credentials, async);
      return _hashCreds;
    }

    public IPromise<IEnumerable<KeyValuePair<string, string>>> GetAuthHeaders(bool async)
    {
      return ValidCredentials(async)
        .Convert(h => (IEnumerable<KeyValuePair<string, string>>)new[]
        {
          new KeyValuePair<string, string>("AUTHUSER", h.Username),
          new KeyValuePair<string, string>("AUTHPASSWORD", h.PasswordHash),
          new KeyValuePair<string, string>("DATABASE", h.Database)
        });
    }

    /// <summary>
    /// Hashes the credentials for use with logging in or workflow voting
    /// </summary>
    /// <param name="credentials">The credentials.</param>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>
    /// A promise to return hashed credentials
    /// </returns>
    public IPromise<ExplicitHashCredentials> HashCredentials(ICredentials credentials, bool async)
    {
      var explicitCred = credentials as ExplicitCredentials;
      var hashCred = credentials as ExplicitHashCredentials;
      var winCred = credentials as WindowsCredentials;

      if (explicitCred != null)
      {
        return Promises.Resolved(new ExplicitHashCredentials(explicitCred.Database, explicitCred.Username, _hashFunc(explicitCred.Password)));
      }
      else if (hashCred != null)
      {
        return Promises.Resolved(hashCred);
      }
      else if (winCred != null)
      {
        var waLoginUrl = new Uri(this._innovatorClientBin, "../scripts/IOMLogin.aspx");
        var handler = new SyncClientHandler()
        {
          Credentials = winCred.Credentials,
          PreAuthenticate = true
        };
        var http = new SyncHttpClient(handler);
        var req = new HttpRequest()
        {
          Content = new SimpleContent("<?xml version=\"1.0\" encoding=\"utf-8\" ?><Item />", "text/xml")
        };

        var context = ElementFactory.Local.LocalizationContext;
        req.SetHeader("DATABASE", winCred.Database);
        req.SetHeader("LOCALE", context.Locale);
        req.SetHeader("TIMEZONE_NAME", context.TimeZone);

        return http.PostPromise(waLoginUrl, async, req, new LogData(4
          , "Innovator: Execute query"
          , Factory.LogListener)
        {
          { "database", winCred.Database },
          { "url", waLoginUrl },
        }).Convert(r =>
        {
          var res = r.AsXml().DescendantsAndSelf("Result").FirstOrDefault();
          var username = res.Element("user").Value;
          var pwd = res.Element("password").Value;
          if (pwd.IsNullOrWhiteSpace())
            throw new ArgumentException("Failed to authenticate with Innovator server '" + _innovatorClientBin, "credentials");

          var needHash = res.Element("hash").Value;
          var password = default(string);
          if (string.Equals(needHash.Trim(), "false", StringComparison.OrdinalIgnoreCase))
            password = pwd;
          else
            password = _hashFunc(pwd);

          return new ExplicitHashCredentials(winCred.Database, username, password);
        });
      }
      else
      {
        throw new NotSupportedException("This connection implementation does not support the specified credential type");
      }
    }

    /// <summary>
    /// Hashes the credentials for use with logging in or workflow voting
    /// </summary>
    /// <param name="credentials">The credentials.</param>
    /// <returns>
    /// Hashed credentials
    /// </returns>
    public ExplicitHashCredentials HashCredentials(ICredentials credentials)
    {
      return HashCredentials(credentials, false).Value;
    }
  }
}
