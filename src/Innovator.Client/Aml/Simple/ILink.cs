namespace Innovator.Client
{
  /// <summary>
  /// Represents an element of a linked list with a <see cref="string"/> key.
  /// </summary>
  public interface ILink<T> where T : ILink<T>
  {
    /// <summary>
    /// Gets the <see cref="string"/> key of the list item.
    /// </summary>
    /// <value>
    /// The <see cref="string"/> key of the list item.
    /// </value>
    string Name { get; }
    /// <summary>
    /// Gets or sets the next <see cref="ILink{T}"/> in the linked list.
    /// </summary>
    /// <value>
    /// The next <see cref="ILink{T}"/> in the linked list.
    /// </value>
    T Next { get; set; }
  }
}
