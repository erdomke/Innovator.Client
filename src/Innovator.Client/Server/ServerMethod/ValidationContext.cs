using Innovator.Client;
using System;

namespace Innovator.Server
{
  /// <inheritdoc cref="IValidationContext"/>
  public class ValidationContext : ChangingContext, IValidationContext
  {
    private readonly IResult _result;

    /// <inheritdoc cref="IValidationContext.ErrorBuilder"/>
    public IErrorBuilder ErrorBuilder
    {
      get { return _result; }
    }

    /// <inheritdoc cref="IValidationContext.Exception"/>
    public Exception Exception
    {
      get { return _result.Exception; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationContext"/> class.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="changes">The changes.</param>
    public ValidationContext(IServerConnection conn, IItem changes) : base(conn, changes)
    {
      _result = Conn.AmlContext.Result();
      _result.ErrorContext(Item);
    }
  }
}
