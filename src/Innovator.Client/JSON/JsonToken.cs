namespace Json.Embed
{
  /// <summary>
  /// Specifies the type of JSON token.
  /// </summary>
  internal enum JsonToken
  {
    /// <summary>
    /// This is returned by the reader if a read method has not been called.
    /// </summary>
    None = 0,
    /// <summary>
    /// An object start token.
    /// </summary>
    StartObject = 1,
    /// <summary>
    /// An array start token.
    /// </summary>
    StartArray = 2,
    /// <summary>
    /// An object property name.
    /// </summary>
    PropertyName = 4,
    /// <summary>
    /// A number.
    /// </summary>
    Number = 8,
    /// <summary>
    /// A string.
    /// </summary>
    String = 9,
    /// <summary>
    /// A boolean.
    /// </summary>
    Boolean = 10,
    /// <summary>
    /// A null token.
    /// </summary>
    Null = 11,
    /// <summary>
    /// An object end token.
    /// </summary>
    EndObject = 13,
    /// <summary>
    /// An array end token.
    /// </summary>
    EndArray = 14
  }
}
