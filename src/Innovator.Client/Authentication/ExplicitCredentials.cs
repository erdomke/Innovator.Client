using System.Diagnostics;
using System.Net;

namespace Innovator.Client
{
  /// <summary>
  /// Credentials with a specified user name and password provided by the user
  /// </summary>
  [DebuggerDisplay("Explicit: {Database}, {Username}")]
  public class ExplicitCredentials : INetCredentials, IUserCredentials
  {

#if SECURECRED
    /// <summary>
    /// An <see cref="System.Net.ICredentials"/> instance with the same user name and password
    /// </summary>
    public System.Net.ICredentials Credentials 
    { 
      get
      { 
        return new NetworkCredential(Username, Password); 
      } 
    }
#else
    /// <summary>
    /// An <see cref="System.Net.ICredentials"/> instance with the same user name and password
    /// </summary>
    public System.Net.ICredentials Credentials
    {
      get
      {
        return new NetworkCredential(Username, Password.UseString((ref string s) => new string(s.ToCharArray())));
      }
    }
#endif
    /// <summary>
    /// The database to connect to
    /// </summary>
    public string Database { get; }
    /// <summary>
    /// The password to use
    /// </summary>
    public SecureToken Password { get; }
    /// <summary>
    /// The user name to use
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Instantiate an <see cref="ExplicitCredentials"/> instance
    /// </summary>
    /// <param name="database">Name of the database</param>
    /// <param name="username">User name</param>
    /// <param name="password">Password</param>
    public ExplicitCredentials(string database, string username, SecureToken password)
    {
      Database = database;
      Username = username;
      Password = password;
    }
  }
}
