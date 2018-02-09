using System.Net;

namespace Innovator.Client
{
  /// <summary>
  /// Provides metadata for an Aras file vault
  /// </summary>
  public class Vault : ILink<Vault>
  {

    /// <summary>
    /// Gets or sets the authentication scheme to use with the vault.
    /// </summary>
    /// <value>
    /// The authentication scheme to use with the vault.
    /// </value>
    public AuthenticationSchemes Authentication { get; set; }

    internal SyncHttpClient HttpClient { get; }

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

    /// <summary>
    /// Transforms the vault URL replacing any <c>$[]</c>-style parameters.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>A promise to return the transformed URL</returns>
    public IPromise<string> TransformUrl(IAsyncConnection conn, bool async)
    {
      return Url.IndexOf("$[") < 0 ?
        Promises.Resolved(Url) :
        conn.Process(new Command("<url>@0</url>", Url)
          .WithAction(CommandAction.TransformVaultServerURL), async)
          .Convert(s =>
          {
            Url = s.AsString();
            return Url;
          });
    }

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
      HttpClient = new SyncHttpClient(handler);
    }
  }
}
