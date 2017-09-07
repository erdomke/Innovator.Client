namespace Innovator.Client
{
  /// <summary>
  /// Method of authentication
  /// </summary>
  public enum Authentication
  {
    /// <summary>
    /// User name and password and given explicitly
    /// </summary>
    Explicit,
    /// <summary>
    /// The windows credentials are passed to the server directly
    /// </summary>
    Windows,
    /// <summary>
    /// No credentials are given
    /// </summary>
    Anonymous
  }
}
