using System.Diagnostics;

namespace Innovator.Client
{
  /// <summary>
  /// Credentials for a user that is not authenticated
  /// </summary>
  [DebuggerDisplay("Anonymous: {Database}")]
  public class AnonymousCredentials : ICredentials
  {
    private string _database;

    /// <summary>
    /// The database to connect to
    /// </summary>
    public string Database { get { return _database; } }

    /// <summary>
    /// Instantiate an <c>AnonymousCredentials</c> instance
    /// </summary>
    public AnonymousCredentials(string database)
    {
      _database = database;
    }
  }
}
