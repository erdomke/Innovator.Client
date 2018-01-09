using System.Diagnostics;

namespace Innovator.Client
{
  /// <summary>
  /// Credentials leveraged with Windows authentication (a.k.a. NTML or Kerberos)
  /// </summary>
  [DebuggerDisplay("Windows: {Database}")]
  public class WindowsCredentials : INetCredentials
  {
    /// <summary>
    /// An <see cref="System.Net.ICredentials"/> instance with the same user name and password
    /// </summary>
    public System.Net.ICredentials Credentials { get; }
    /// <summary>
    /// The database to connect to
    /// </summary>
    public string Database { get; }

    /// <summary>
    /// Instantiate a <c>WindowsCredentials</c> instance with the current Windows user credentials
    /// </summary>
    /// <param name="database">The database to connect to</param>
    public WindowsCredentials(string database)
    {
      Credentials = System.Net.CredentialCache.DefaultCredentials;
      Database = database;
    }
    /// <summary>
    /// Instantiate a <c>WindowsCredentials</c> instance with explicitly provided credentials
    /// </summary>
    /// <param name="database">The database to connect to</param>
    /// <param name="credentials">Explicit credentials</param>
    public WindowsCredentials(string database, System.Net.ICredentials credentials)
    {
      Credentials = credentials;
      Database = database;
    }
  }
}
