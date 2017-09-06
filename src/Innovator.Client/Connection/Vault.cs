using Innovator.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// Provides metadata for an Aras file vault
  /// </summary>
  public class Vault : ILink<Vault>
  {
    private readonly CookieContainer _cookies = new CookieContainer();

    /// <summary>
    /// Gets or sets the authentication scheme to use with the vault.
    /// </summary>
    /// <value>
    /// The authentication scheme to use with the vault.
    /// </value>
    public AuthenticationSchemes Authentication { get; set; }

    /// <summary>
    /// Gets the cookies used for vault communication (e.g. session state or authentication cookies).
    /// </summary>
    /// <value>
    /// The cookies used for vault communication.
    /// </value>
    public CookieContainer Cookies { get { return _cookies; } }

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
    }
  }
}
