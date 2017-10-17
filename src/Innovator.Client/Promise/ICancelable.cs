namespace Innovator.Client
{
  /// <summary>
  /// Indicates that an operation can be canceled
  /// </summary>
  public interface ICancelable
  {
    /// <summary>
    /// Cancel the operation
    /// </summary>
    void Cancel();
  }
}
