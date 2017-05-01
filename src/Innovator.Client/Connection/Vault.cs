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
    private CookieContainer _cookies = new CookieContainer();

    public AuthenticationSchemes Authentication { get; set; }
    public CookieContainer Cookies { get { return _cookies; } }
    public string Id { get; set; }
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
