using System.Net;

namespace Innovator.Client
{
  /// <summary>
  /// Provides metadata for an Aras file vault
  /// </summary>
  public class Vault : ILink<Vault>
  {
    private readonly SyncHttpClient _httpClient;

    /// <summary>
    /// Gets or sets the authentication scheme to use with the vault.
    /// </summary>
    /// <value>
    /// The authentication scheme to use with the vault.
    /// </value>
    public AuthenticationSchemes Authentication { get; set; }

    internal SyncHttpClient HttpClient { get { return _httpClient; } }

    /// <summary>
    /// Gets or sets the Aras ID of the vault.
    /// </summary>
    /// <value>
    /// The Aras ID of the vault.
    /// </value>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the URL.
    /// </summary>
    /// <value>
    /// The URL.
    /// </value>
    public string Url { get; set; }

    string ILink<Vault>.Name { get { return Id; } }
    Vault ILink<Vault>.Next { get; set; }

    internal Vault(IReadOnlyItem i)
    {
      this.Id = i.Id();
      this.Url = i.Property("vault_url").Value;
      this.Authentication = AuthenticationSchemes.None;

      var handler = new SyncClientHandler()
      {
        CookieContainer = new CookieContainer()
      };
      _httpClient = new SyncHttpClient(handler);
    }
  }
}
