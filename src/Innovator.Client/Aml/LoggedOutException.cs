using System;
#if SERIALIZATION
using System.Runtime.Serialization;
#endif

namespace Innovator.Client
{
  /// <summary>
  /// Indicates that the current connection is not logged in with valid credentials
  /// </summary>
#if SERIALIZATION
  [Serializable]
#endif
  public class LoggedOutException : Exception
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LoggedOutException"/> class.
    /// </summary>
    public LoggedOutException() : base() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="LoggedOutException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public LoggedOutException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="LoggedOutException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public LoggedOutException(string message, Exception innerException) : base(message, innerException) { }
#if SERIALIZATION
    public LoggedOutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
  }
}
