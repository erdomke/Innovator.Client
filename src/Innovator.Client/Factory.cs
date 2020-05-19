using Innovator.Client.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// Class for generating connection to an Aras Innovator instance
  /// </summary>
  /// <example>
  /// <code lang="C#">
  /// using Innovator.Client;
  ///
  /// var conn = Factory.GetConnection("URL", "USER_AGENT");
  /// conn.Login(new ExplicitCredentials("DATABASE", "USER_NAME", "PASSWORD"));
  /// </code>
  /// </example>
  public static class Factory
  {
    private static Action<int, string, IEnumerable<KeyValuePair<string, object>>> _logListener;
    private static MemoryCache<string, byte[]> _imageCache = new MemoryCache<string, byte[]>();
    
    /// <summary>
    /// Gets or sets the default <see cref="IItemFactory"/> to use with new connections
    /// </summary>
    /// <value>
    /// The default <see cref="IItemFactory"/> to use with new connections.
    /// </value>
    public static IItemFactory DefaultItemFactory { get; set; }

    /// <summary>
    /// Gets or sets the default log listener for all server communication
    /// </summary>
    /// <value>
    /// The default log listener for all server communication
    /// </value>
    /// <seealso cref="Command.LogListener"/>
    public static Action<int, string, IEnumerable<KeyValuePair<string, object>>> LogListener
    {
      get { return _logListener; }
      set { _logListener = value ?? DefaultLogListener; }
    }

    internal static MemoryCache<string, byte[]> ImageCache
    {
      get { return _imageCache; }
    }

    static Factory()
    {
      DefaultItemFactory = new DefaultItemFactory();
      _logListener = DefaultLogListener;
    }

    private static void DefaultLogListener(int level, string message, IEnumerable<KeyValuePair<string, object>> parameters)
    {
      // Do nothing
    }

    /// <summary>
    /// How many images to buffer in memory when downloading image files.  This cache is used
    /// by the <see cref="ItemExtensions.AsFile(IReadOnlyProperty, IConnection, bool)"/>
    /// extension method.
    /// </summary>
    public static long ImageBufferSize
    {
      get { return _imageCache.MaxSize; }
      set { _imageCache.MaxSize = value; }
    }

    /// <summary>
    /// Gets an HTTP connection to an innovator instance at the given URL
    /// </summary>
    /// <param name="preferences">Object containing preferences for the connection</param>
    /// <returns>A new <see cref="IRemoteConnection"/> object</returns>
    /// <example>
    /// Create a new connection using the default stored connection
    /// <code lang="C#">
    /// var pref = SavedConnections.Load().Default;
    /// var conn = Factory.GetConnection(pref);
    /// </code>
    /// </example>
    public static IRemoteConnection GetConnection(ConnectionPreferences preferences)
    {
      return GetConnection(preferences, false).Value;
    }

    /// <summary>
    /// Gets an HTTP connection to an innovator instance (or proxy) at the given URL
    /// </summary>
    /// <param name="url">URL of the innovator instance (or proxy)</param>
    /// <param name="userAgent">User agent string to use for the connection</param>
    /// <returns>A connection object</returns>
    /// <example>
    /// <code lang="C#">
    /// using Innovator.Client;
    ///
    /// var conn = Factory.GetConnection("URL", "USER_AGENT");
    /// conn.Login(new ExplicitCredentials("DATABASE", "USER_NAME", "PASSWORD"));
    /// </code>
    /// </example>
    public static IRemoteConnection GetConnection(string url, string userAgent)
    {
      var prefs = new ConnectionPreferences
      {
        Url = url
      };
      prefs.Headers.UserAgent = userAgent;
      return GetConnection(prefs, false).Value;
    }

    /// <summary>
    /// Gets an HTTP connection to an innovator instance (or proxy) at the given URL
    /// </summary>
    /// <param name="url">URL of the innovator instance (or proxy)</param>
    /// <param name="preferences">Object containing preferences for the connection</param>
    /// <returns>A connection object</returns>
    public static IRemoteConnection GetConnection(string url, ConnectionPreferences preferences)
    {
      preferences = preferences ?? new ConnectionPreferences();
      preferences.Url = url;
      return GetConnection(preferences, false).Value;
    }

    /// <summary>
    /// Asynchronously gets an HTTP connection to an innovator instance (or proxy) at the given URL
    /// </summary>
    /// <param name="url">URL of the innovator instance (or proxy)</param>
    /// <param name="preferences">Object containing preferences for the connection</param>
    /// <param name="async">Whether or not to return the connection asynchronously.  This is important
    /// as an HTTP request must be issued to determine the type of connection to create</param>
    /// <returns>A promise to return a connection object</returns>
    public static IPromise<IRemoteConnection> GetConnection(string url
      , ConnectionPreferences preferences, bool async)
    {
      preferences = preferences ?? new ConnectionPreferences();
      preferences.Url = url;
      return GetConnection(preferences, async);
    }

    /// <summary>
    /// Asynchronously gets an HTTP connection to an innovator instance (or proxy) at the given URL
    /// </summary>
    /// <param name="preferences">Object containing preferences for the connection</param>
    /// <param name="async">Whether or not to return the connection asynchronously.  This is important
    /// as an HTTP request must be issued to determine the type of connection to create</param>
    /// <returns>A promise to return a connection object</returns>
    public static IPromise<IRemoteConnection> GetConnection(ConnectionPreferences preferences, bool async)
    {
      preferences = preferences ?? new ConnectionPreferences();
      var url = preferences.Url;

      url = (url ?? "").TrimEnd('/');
      if (url.EndsWith("Server/InnovatorServer.aspx", StringComparison.OrdinalIgnoreCase))
        url = url.Substring(0, url.Length - 21);
      if (!url.EndsWith("/server", StringComparison.OrdinalIgnoreCase)) url += "/Server";
      var configUrl = url + "/mapping.xml";

      var service = preferences.HttpService ?? ConnectionPreferences.GetService();
      
      Func<ServerMapping, IRemoteConnection> connFactory = m =>
      {
        var uri = (m.Url ?? "").TrimEnd('/');
        if (!uri.EndsWith("/server", StringComparison.OrdinalIgnoreCase)) url += "/Server";
        switch (m.Type)
        {
          case ServerType.Proxy:
            throw new NotSupportedException();
          default:
            return ArasConn(service, uri, preferences);
        }
      };

      var result = new Promise<IRemoteConnection>();
      var req = new HttpRequest
      {
        UserAgent = preferences.Headers.UserAgent
      };
      req.SetHeader("Accept", "text/xml");
      foreach (var header in preferences.Headers.NonUserAgentHeaders())
      {
        req.SetHeader(header.Key, header.Value);
      }

      var trace = new LogData(4, "Innovator: Try to download mapping file", Factory.LogListener)
      {
        { "url", configUrl },
      };
      result.CancelTarget(service.GetPromise(new Uri(configUrl), async, trace, req)
        .Progress((p, m) => result.Notify(p, m))
        .Done(r =>
        {
          var data = r.AsString();
          if (string.IsNullOrEmpty(data))
          {
            result.Resolve(ArasConn(service, url, preferences));
          }
          else
          {
            try
            {
              var servers = ServerMapping.FromXml(data).ToArray();
              if (servers.Length < 1)
              {
                result.Resolve(ArasConn(service, url, preferences));
              }
              else if (servers.Length == 1)
              {
                result.Resolve(connFactory(servers.Single()));
              }
              else
              {
                foreach (var server in servers)
                {
                  server.Factory = connFactory;
                }
                result.Resolve(new MappedConnection(servers, preferences.AuthCallback));
              }
            }
            catch (XmlException)
            {
              result.Resolve(ArasConn(service, url, preferences));
            }
          }
        }).Fail(ex =>
        {
          result.Resolve(ArasConn(service, url, preferences));
        })).Always(trace.Dispose);


      if (preferences.Credentials != null)
      {
        IRemoteConnection conn = null;
        return result
          .Continue(c =>
          {
            conn = c;
            return c.Login(preferences.Credentials, async);
          })
          .Convert(u => conn);
      }

      return result;
    }

    private static IRemoteConnection ArasConn(HttpClient arasService, string url, ConnectionPreferences preferences)
    {
      var result = new Connection.ArasHttpConnection(arasService, url, preferences.ItemFactory);
      if (preferences.Headers.Any() || preferences.DefaultTimeout.HasValue)
      {
        result.DefaultSettings(r =>
        {
          if (preferences.DefaultTimeout.HasValue)
            r.Timeout = TimeSpan.FromMilliseconds(preferences.DefaultTimeout.Value);

          if (!string.IsNullOrEmpty(preferences.Headers.UserAgent))
            r.UserAgent = preferences.Headers.UserAgent;

          foreach (var header in preferences.Headers.NonUserAgentHeaders())
          {
            r.SetHeader(header.Key, header.Value);
          }
        });
      }
      return result;
    }
  }
}
