namespace Innovator.Server
{
  /// <summary>
  /// A cache of key/value pairs stored in server memory
  /// </summary>
  public interface IServerCache
  {
    /// <summary>
    /// Gets or sets the <see cref="System.Object"/> with the specified key.
    /// </summary>
    /// <value>
    /// The <see cref="System.Object"/>.
    /// </value>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    object this[string key] { get; set; }
    /// <summary>
    /// Gets the object with the specified key coerced to the type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type to cast the object as</typeparam>
    /// <param name="key">The key.</param>
    /// <returns>The casted object</returns>
    T Get<T>(string key);
  }
}
