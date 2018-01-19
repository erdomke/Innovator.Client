using System;
using System.Collections.Generic;

namespace Innovator.Client.Connection
{
  /// <summary>
  /// Interface implemented by Aras connection objects which is used
  /// for coordination with the <see cref="ArasVaultConnection"/>.
  /// </summary>
  /// <seealso cref="Innovator.Client.IAsyncConnection" />
  public interface IArasConnection : IAsyncConnection
  {
    /// <summary>
    /// Gets the default settings for configuring the HTTP request.
    /// </summary>
    /// <value>
    /// The default settings for configuring the HTTP request.
    /// </value>
    List<Action<IHttpRequest>> DefaultSettings { get; }
    /// <summary>
    /// Gets the compression setting to use when sending requests to the server.
    /// </summary>
    /// <value>
    /// The compression setting to use when sending requests to the server.
    /// </value>
    CompressionType Compression { get; }
    /// <summary>
    /// Gets the version of the Aras installation
    /// </summary>
    /// <value>
    /// The version of the Aras installation.
    /// </value>
    Version Version { get; }
    /// <summary>
    /// Sets the default headers for an AML request. (e.g. AUTHUSER, AUTHPASSWORD,
    /// DATABASE, LOCALE, TIMEZONE_NAME);
    /// </summary>
    /// <param name="writer">The method to call with each header.</param>
    void SetDefaultHeaders(Action<string, string> writer);
    /// <summary>
    /// Fetches the version from the database if it is not already known.
    /// </summary>
    /// <param name="async">Whether to fetch the version asynchronously</param>
    /// <returns>A promise to return the version of the Aras installation.</returns>
    IPromise<Version> FetchVersion(bool async);
  }
}
