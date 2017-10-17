namespace Innovator.Client
{
  /// <summary>
  /// Represents an asynchronous script
  /// </summary>
  public interface IAsyncScript
  {
    /// <summary>
    /// Executes the specified script.
    /// </summary>
    /// <param name="conn">The connection.</param>
    IPromise<string> Execute(IAsyncConnection conn);
  }
}
