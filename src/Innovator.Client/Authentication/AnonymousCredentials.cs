using System.Diagnostics;

namespace Innovator.Client
{
  /// <summary>
  /// Credentials for a user that is not authenticated
  /// </summary>
  [DebuggerDisplay("Anonymous: {Database}")]
  public class AnonymousCredentials : ICredentials
  {
    /// <summary>
    /// The database to connect to
    /// </summary>
    public string Database { get; }

    /// <summary>
    /// Instantiate an <c>AnonymousCredentials</c> instance
    /// </summary>
    /// <param name="database">The database to connect to</param>
    public AnonymousCredentials(string database)
    {
      Database = database;
    }
  }
}
