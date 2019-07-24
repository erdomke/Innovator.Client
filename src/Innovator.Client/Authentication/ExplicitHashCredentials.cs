using System;
using System.Diagnostics;

namespace Innovator.Client
{
  /// <summary>
  /// Credentials with a specified user name and password hash provided by the user
  /// </summary>
  [DebuggerDisplay("Hash: {Database}, {Username}")]
  public class ExplicitHashCredentials : IUserCredentials
  {
    /// <summary>
    /// The database to connect to
    /// </summary>
    public string Database { get; }
    /// <summary>
    /// The hash of the password to use
    /// </summary>
    public string PasswordHash { get; }
    /// <summary>
    /// The user name to use
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Instantiate an <c>ExplicitHashCredentials</c> instance
    /// </summary>
    /// <param name="database">The database to connect to</param>
    /// <param name="username">The hash of the password to use</param>
    /// <param name="passwordHash">The user name to use</param>
    public ExplicitHashCredentials(string database, string username, string passwordHash)
    {
      if (!passwordHash.IsGuid() && !passwordHash.IsSha256Hash())
        throw new ArgumentException($"Invalid format for password hash: `{new string('*', passwordHash?.Length ?? 0)}`", nameof(passwordHash));

      Database = database;
      Username = username;
      PasswordHash = passwordHash;
    }
  }
}
