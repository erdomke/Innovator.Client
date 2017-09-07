using System;
using System.IO;
using System.Threading;

namespace Innovator.Client
{
  /// <summary>
  /// A pool of multiple connections using the same credentials and the same database
  /// </summary>
  /// <remarks>Because Aras uses Session State on the server, requests sent in 
  /// parallel are still processed serially.  The only way to overcome this is to
  /// have separate connections.  This class allows you to support that transparently
  /// by wrapping any connection with a pool</remarks>
  /// <seealso cref="Innovator.Client.IAsyncConnection" />
  public class ConnectionPool : IAsyncConnection, IDisposable
  {
    private readonly PooledConnection[] _pool;
    private readonly IRemoteConnection _ref;
    private readonly Promise<bool> _available;

    private ConnectionPool(IRemoteConnection conn, int size)
    {
      _available = new Promise<bool>();
      _ref = conn;
      _pool = new PooledConnection[size];
      for (var i = 0; i < size; i++)
      {
        var idx = i;
        conn.Clone(true)
          .Done(c =>
          {
            _pool[idx] = new PooledConnection(c);
            _available.Resolve(true);
          });
      }
    }

    /// <summary>
    /// Creates a connection pool with <paramref name="size"/> connections.
    /// </summary>
    /// <param name="conn">The connection to use as the basis.</param>
    /// <param name="size">The number of connections.</param>
    /// <returns>A promise to return a pool logged-in of <paramref name="size"/> 
    /// logged in connections</returns>
    public static IPromise<ConnectionPool> Create(IRemoteConnection conn, int size)
    {
      var result = new ConnectionPool(conn, size);
      return result._available
        .Convert(a => result);
    }

    /// <summary>
    /// AML context used for creating AML objects and formatting AML statements
    /// </summary>
    public ElementFactory AmlContext { get { return _ref.AmlContext; } }

    /// <summary>
    /// Name of the connected database
    /// </summary>
    public string Database { get { return _ref.Database; } }

    /// <summary>
    /// ID of the authenticated user
    /// </summary>
    public string UserId { get { return _ref.UserId; } }

    /// <summary>
    /// Creates an upload request used for uploading files to the server
    /// </summary>
    /// <returns>
    /// A new upload request used for uploading files to the server
    /// </returns>
    public UploadCommand CreateUploadCommand()
    {
      return _ref.CreateUploadCommand();
    }

    /// <summary>
    /// Logs out of each connection
    /// </summary>
    public void Dispose()
    {
      for (var i = 0; i < _pool.Length; i++)
      {
        var conn = _pool[i];
        _pool[i] = null;
        conn.Dispose();
      }
    }

    /// <summary>
    /// Expands a relative URL to a full URL
    /// </summary>
    /// <param name="relativeUrl">The relative URL</param>
    /// <returns>
    /// A full URL relative to the connection
    /// </returns>
    public string MapClientUrl(string relativeUrl)
    {
      return _ref.MapClientUrl(relativeUrl);
    }

    /// <summary>
    /// Calls a SOAP action asynchronously
    /// </summary>
    /// <param name="request">Request AML and possibly files <see cref="UploadCommand" /></param>
    /// <returns>
    /// An XML SOAP response as a string
    /// </returns>
    public Stream Process(Command request)
    {
      var conn = GetConnection();
      return conn.Process(request, false).Value;
    }

    /// <summary>
    /// Calls a SOAP action asynchronously
    /// </summary>
    /// <param name="request">Request AML and possibly files <see cref="UploadCommand" /></param>
    /// <param name="async">Whether to perform this action asynchronously</param>
    /// <returns>
    /// A promise to return an XML SOAP response as a string
    /// </returns>
    public IPromise<Stream> Process(Command request, bool async)
    {
      var conn = GetConnection();
      return conn.Process(request, async);
    }

    private PooledConnection GetConnection()
    {
      PooledConnection result = null;
      for (var i = 0; i < _pool.Length; i++)
      {
        var curr = _pool[i];
        if (curr != null)
        {
          if (result == null)
          {
            result = curr;
          }
          else if (result.ConcurrentQueries > curr.ConcurrentQueries)
          {
            result = curr;
          }
        }
      }
      return result;
    }

    /// <summary>
    /// Individual connection which tracks the number of pending queries
    /// </summary>
    private class PooledConnection : IDisposable
    {
      private int _concurrentQueries;
      private IRemoteConnection _conn;
      private ConnectionState _state = ConnectionState.Normal;

      public int ConcurrentQueries { get { return _concurrentQueries; } }

      public PooledConnection(IRemoteConnection conn)
      {
        _conn = conn;
      }

      public IPromise<Stream> Process(Command cmd, bool async)
      {
        if (_state != ConnectionState.Normal)
          return Promises.Rejected<Stream>(new ObjectDisposedException("Cannot execute a query because the connection is being disposed (i.e. logged out)."));
        Interlocked.Increment(ref _concurrentQueries);
        return _conn.Process(cmd, async)
          .Always(() =>
          {
            var newCount = Interlocked.Decrement(ref _concurrentQueries);
            if (newCount < 1 && _state == ConnectionState.Disposing)
              ExecuteDispose();
          });
      }

      public void Dispose()
      {
        if (_state == ConnectionState.Normal)
        {
          _state = ConnectionState.Disposing;
          if (_concurrentQueries < 1)
            ExecuteDispose();
        }
      }

      private void ExecuteDispose()
      {
        var conn = _conn;
        _conn = null;
        conn.Dispose();
        _state = ConnectionState.Disposed;
      }

      private enum ConnectionState
      {
        Normal,
        Disposing,
        Disposed
      }
    }
  }
}
