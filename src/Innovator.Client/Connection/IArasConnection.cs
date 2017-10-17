﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    /// Gets the major version of the Aras installation
    /// </summary>
    /// <value>
    /// The major version of the Aras installation.
    /// </value>
    int Version { get; }
    /// <summary>
    /// Sets the default headers for an AML request. (e.g. AUTHUSER, AUTHPASSWORD,
    /// DATABASE, LOCALE, TIMEZONE_NAME);
    /// </summary>
    /// <param name="writer">The method to call with each header.</param>
    void SetDefaultHeaders(Action<string, string> writer);
  }
}