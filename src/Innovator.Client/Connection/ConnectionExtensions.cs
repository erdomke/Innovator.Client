using System;
using System.Linq;
using System.Threading;

namespace Innovator.Client
{
  /// <summary>
  /// Extension methods related to <see cref="IConnection"/>.
  /// </summary>
  public static class ConnectionExtensions
  {
    /// <summary>
    /// Get the result of executing the specified AML query
    /// </summary>
    /// <param name="conn">Connection to execute the query on</param>
    /// <param name="query">Query to be performed.  If parameters are specified, they will be substituted into the query</param>
    /// <param name="parameters">Parameters to be injected into the query</param>
    /// <returns>A read-only result</returns>
    /// <example>
    /// <code lang="C#">
    /// // Get preliminary parts which have existed for a little bit of time
    /// var components = conn.Apply(@"<Item type='Part' action='get'>
    ///   <classification>@0</classification>
    ///   <created_on condition='lt'>@1</created_on>
    ///   <state>Preliminary</state>
    /// </Item>", classification, DateTime.Now.AddMinutes(-20)).Items();
    /// </code>
    /// </example>
    public static IReadOnlyResult Apply(this IConnection conn, Command query, params object[] parameters)
    {
      query.WithAml(query.Aml, parameters);
      if (query.Action == CommandAction.ApplySQL)
        return ElementFactory.Utc.FromXml(conn.Process(query), query, conn);
      return conn.AmlContext.FromXml(conn.Process(query), query, conn);
    }

    internal static IReadOnlyResult ApplyMutable(this IConnection conn, Command query, params object[] parameters)
    {
      query.WithAml(query.Aml, parameters);
      var factory = conn.AmlContext;
      if (query.Action == CommandAction.ApplySQL)
        factory = ElementFactory.Utc;

      var xml = conn.Process(query);
      var xmlStream = xml as IXmlStream;
      using (var xmlReader = (xmlStream == null ? System.Xml.XmlReader.Create(xml) : xmlStream.CreateReader()))
      {
        return factory.FromXml(xmlReader, query, conn?.Database, false);
      }
    }

    /// <summary>
    /// Get the result of executing the specified AML query
    /// </summary>
    /// <param name="conn">Connection to execute the query on</param>
    /// <param name="query">Query to be performed.  If parameters are specified, they will be substituted into the query</param>
    /// <param name="async">Whether to perform the query asynchronously</param>
    /// <param name="noItemsIsError">Whether a 'No items found' exception should be signaled to the <see cref="IPromise"/> as an exception</param>
    /// <param name="parameters">Parameters to be injected into the query</param>
    /// <returns>A read-only result</returns>
    public static IPromise<IReadOnlyResult> ApplyAsync(this IAsyncConnection conn, Command query, bool async, bool noItemsIsError, params object[] parameters)
    {
      if (async)
        return ApplyAsyncInt(conn, query, default(CancellationToken), parameters);
      return Promises.Resolved(Apply(conn, query, parameters));
    }

#if TASKS
    /// <summary>
    /// Get the result of executing the specified AML query
    /// </summary>
    /// <param name="conn">Connection to execute the query on</param>
    /// <param name="query">Query to be performed.  If parameters are specified, they will be substituted into the query</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the asynchronous operation</param>
    /// <param name="parameters">Parameters to be injected into the query</param>
    /// <returns>A read-only result</returns>
    public static IPromise<IReadOnlyResult> ApplyAsync(this IAsyncConnection conn, Command query, CancellationToken ct, params object[] parameters)
    {
      return ApplyAsyncInt(conn, query, ct, parameters);
    }
#endif

    private static IPromise<IReadOnlyResult> ApplyAsyncInt(this IAsyncConnection conn, Command query, CancellationToken ct, params object[] parameters)
    {
      var result = new Promise<IReadOnlyResult>();
      query.WithAml(query.Aml, parameters);

      ct.Register(() => result.Cancel());

      result.CancelTarget(
        conn.Process(query, true)
        .Progress((p, m) => result.Notify(p, m))
        .Done(r =>
          {
            try
            {
              if (query.Action == CommandAction.ApplySQL)
              {
                var res = ElementFactory.Utc.FromXml(r, query, conn);
                result.Resolve(res);
              }
              else
              {
                var res = conn.AmlContext.FromXml(r, query, conn);
                result.Resolve(res);
              }
            }
            catch (Exception ex)
            {
              result.Reject(ex);
            }
          }).Fail(ex =>
          {
            result.Reject(ex);
          }));
      return result;
    }

    /// <summary>
    /// Get the result of executing the specified SQL query
    /// </summary>
    /// <param name="conn">Connection to execute the query on</param>
    /// <param name="sql">SQL query to be performed.  If parameters are specified, they will be substituted into the query</param>
    /// <param name="parameters">Parameters to be injected into the query</param>
    /// <returns>A read-only result</returns>
    public static IReadOnlyResult ApplySql(this IConnection conn, Command sql, params object[] parameters)
    {
      var aml = sql.Aml;
      if (!aml.TrimStart().StartsWith("<"))
        aml = "<sql>" + ServerContext.XmlEscape(aml) + "</sql>";
      return conn.Apply(sql.WithAml(aml, parameters).WithAction(CommandAction.ApplySQL));
    }
    /// <summary>
    /// Get the result of executing the specified SQL query
    /// </summary>
    /// <param name="conn">Connection to execute the query on</param>
    /// <param name="sql">SQL query to be performed.  If parameters are specified, they will be substituted into the query</param>
    /// <param name="async">Whether to perform the query asynchronously</param>
    /// <returns>A read-only result</returns>
    public static IPromise<IReadOnlyResult> ApplySql(this IAsyncConnection conn, Command sql, bool async)
    {
      if (!sql.Aml.TrimStart().StartsWith("<"))
        sql.Aml = "<sql>" + ServerContext.XmlEscape(sql.Aml) + "</sql>";
      return ApplyAsyncInt(conn, sql.WithAction(CommandAction.ApplySQL), default(CancellationToken));
    }

#if TASKS
    /// <summary>
    /// Get the result of executing the specified SQL query
    /// </summary>
    /// <param name="conn">Connection to execute the query on</param>
    /// <param name="sql">SQL query to be performed.  If parameters are specified, they will be substituted into the query</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the asynchronous operation</param>
    /// <returns>A read-only result</returns>
    public static IPromise<IReadOnlyResult> ApplySql(this IAsyncConnection conn, Command sql, CancellationToken ct)
    {
      if (!sql.Aml.TrimStart().StartsWith("<"))
      {
        sql.Aml = "<sql>" + ServerContext.XmlEscape(sql.Aml) + "</sql>";
      }
      return ApplyAsyncInt(conn, sql.WithAction(CommandAction.ApplySQL), ct);
    }
#endif

    /// <summary>
    /// Fetches the version from the database if it is not already known.
    /// </summary>
    /// <param name="conn">The connection to fetch the version for</param>
    /// <param name="async">Whether to fetch the version asynchronously</param>
    /// <returns>A promise to return the version of the Aras installation.</returns>
    public static IPromise<Version> FetchVersion(this IAsyncConnection conn, bool async)
    {
      var version = (conn as Connection.IArasConnection)?.Version;
      if (version != default(Version) && version.Major > 0)
        return Promises.Resolved(version);

      return conn.ApplyAsync(@"<Item type='Variable' action='get' select='name,value'>
        <name condition='like'>Version*</name>
      </Item>", async, false)
        .Convert(res =>
        {
          var dict = res.Items()
            .GroupBy(i => i.Property("name").AsString(""))
            .ToDictionary(g => g.Key, g => g.First().Property("value").Value);

          string majorStr;
          int major;
          string minorStr;
          int minor;
          string servicePackStr;
          int servicePack;
          string buildStr;
          int build;
          if (dict.TryGetValue("VersionMajor", out majorStr) && int.TryParse(majorStr, out major)
            && dict.TryGetValue("VersionMinor", out minorStr) && int.TryParse(minorStr, out minor)
            && dict.TryGetValue("VersionServicePack", out servicePackStr))
          {
            if (!dict.TryGetValue("VersionBuild", out buildStr) || !int.TryParse(buildStr, out build))
              build = 0;

            if (!int.TryParse(servicePackStr.TrimStart('S', 'P'), out servicePack))
              servicePack = 0;

            return new Version(major, minor, servicePack, build);
          }
          return default(Version);
        });
    }

    /// <summary>
    /// Retrieve an item based on its type and ID
    /// </summary>
    /// <param name="conn">Connection to query the item on</param>
    /// <param name="itemTypeName">Name of the item type</param>
    /// <param name="id">ID of the item</param>
    /// <param name="attributes">Extra parameters to pass to the aml call.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="itemTypeName"/> is not specified
    /// - or -
    /// <paramref name="id"/> is not specified
    /// </exception>
    public static IReadOnlyItem ItemById(this IConnection conn, string itemTypeName, string id, params IAttribute[] attributes)
    {
      if (itemTypeName.IsNullOrWhiteSpace())
        throw new ArgumentException("Item type must be specified", nameof(itemTypeName));
      if (id.IsNullOrWhiteSpace())
        throw new ArgumentException("ID must be specified", nameof(id));

      var aml = conn.AmlContext;
      return aml.Item(aml.Action("get"),
        aml.Type(itemTypeName),
        aml.Id(id),
        attributes)
        .Apply(conn)
        .AssertItem();
    }

    /// <summary>
    /// Retrieve an item based on its type and ID and map it to an object
    /// </summary>
    /// <param name="conn">Connection to query the item on</param>
    /// <param name="itemTypeName">Name of the item type</param>
    /// <param name="id">ID of the item</param>
    /// <param name="mapper">Mapping function used to get an object from the item data</param>
    public static T ItemById<T>(this IConnection conn, string itemTypeName, string id, Func<IReadOnlyItem, T> mapper)
    {
      if (itemTypeName.IsNullOrWhiteSpace())
        throw new ArgumentException("Item type must be specified", nameof(itemTypeName));
      if (id.IsNullOrWhiteSpace())
        throw new ArgumentException("ID must be specified", nameof(id));

      var aml = conn.AmlContext;
      var itemQuery = aml.Item(aml.Type(itemTypeName), aml.Id(id));
      return itemQuery.LazyMap(conn, mapper);
    }

    /// <summary>
    /// Retrieve an item based on its type and keyed name
    /// </summary>
    /// <param name="conn">Connection to query the item on</param>
    /// <param name="itemTypeName">Name of the item type</param>
    /// <param name="keyedName">Keyed name of the item</param>
    public static IReadOnlyItem ItemByKeyedName(this IConnection conn, string itemTypeName, string keyedName)
    {
      if (itemTypeName.IsNullOrWhiteSpace())
        throw new ArgumentException("Item type must be specified", nameof(itemTypeName));
      if (keyedName.IsNullOrWhiteSpace())
        throw new ArgumentException("Keyed name must be specified", nameof(keyedName));

      return conn.Apply(new Command("<Item type='@0 action=\"get\"><keyed_name>@1</keyed_name></Item>", itemTypeName, keyedName)
                          .WithAction(CommandAction.ApplyItem)).AssertItem();
    }

    /// <summary>
    /// Get a single item from the database using the specified query.  If the result is not a single item, an exception will be thrown
    /// </summary>
    /// <param name="conn">Server connection</param>
    /// <param name="request">Query/command which should return a single item</param>
    /// <returns>A single readonly item</returns>
    public static IReadOnlyItem ItemByQuery(this IConnection conn, Command request)
    {
      return ItemByQuery(conn, request, false).Value;
    }
    /// <summary>
    /// Get a single item from the database using the specified query asynchronously.  If the result is not a single item, an exception will be thrown
    /// </summary>
    /// <param name="conn">Server connection</param>
    /// <param name="request">Query/command which should return a single item</param>
    /// <param name="async">Whether to perform this request asynchronously</param>
    /// <returns>A promise to return a single readonly item</returns>
    public static IPromise<IReadOnlyItem> ItemByQuery(this IConnection conn, Command request, bool async)
    {
      var result = new Promise<IReadOnlyItem>();
      result.CancelTarget(conn.ProcessAsync(request, async)
        .Progress((p, m) => result.Notify(p, m))
        .Done(r =>
        {
          if (string.IsNullOrEmpty(conn.UserId))
          {
            result.Reject(new LoggedOutException());
          }
          else
          {
            var res = conn.AmlContext.FromXml(r, request, conn);
            var ex = res.Exception;
            if (ex == null)
            {

              try
              {
                result.Resolve(res.AssertItem());
              }
              catch (Exception exc)
              {
                result.Reject(exc);
              }
            }
            else
            {
              result.Reject(ex);
            }
          }
        }).Fail(ex => result.Reject(ex)));
      return result;
    }

    /// <summary>
    /// Locks the specified item.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="itemTypeName">Name of the item type.</param>
    /// <param name="id">The ID.</param>
    /// <param name="attributes">Extra parameters to pass to the aml call.</param>
    /// <returns>The lock result</returns>
    public static IReadOnlyResult Lock(this IConnection conn, string itemTypeName, string id, params IAttribute[] attributes)
    {
      var aml = conn.AmlContext;
      return aml.Item(aml.Action("lock"),
        aml.Type(itemTypeName),
        aml.Id(id),
        attributes
      ).Apply(conn)
      .AssertNoError();
    }

    /// <summary>
    /// Returns the next number in the sequence.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="sequenceName">Name of the sequence.</param>
    /// <returns>The next number in the sequence.</returns>
    /// <exception cref="ArgumentException">Sequence name must be specified - sequenceName</exception>
    public static string NextSequence(this IConnection conn, string sequenceName)
    {
      if (sequenceName.IsNullOrWhiteSpace())
        throw new ArgumentException("Sequence name must be specified", "sequenceName");

      var aml = conn.AmlContext;
      var query = new Command("<Item><name>@0</name></Item>", sequenceName)
                              .WithAction(CommandAction.GetNextSequence);
      return aml.FromXml(conn.Process(query), query, conn).Value;
    }

    internal static IPromise<System.IO.Stream> ProcessAsync(this IConnection conn, Command cmd, bool async)
    {
      var remote = conn as IAsyncConnection;
      if (remote != null)
      {
        return remote.Process(cmd, async);
      }

      var result = new Promise<System.IO.Stream>();
      try
      {
        result.Resolve(conn.Process(cmd));
      }
      catch (Exception ex)
      {
        result.Reject(ex);
      }
      return result;
    }

    /// <summary>
    /// Promotes the specified item.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="itemTypeName">Name of the item type.</param>
    /// <param name="id">The Aras ID.</param>
    /// <param name="newState">The new state.</param>
    /// <param name="comments">The comments.</param>
    /// <param name="attributes">Extra parameters to pass to the aml call.</param>
    /// <returns>The result returned by the server</returns>
    /// <exception cref="ArgumentException">State must be a non-empty string to run a promotion. - newState</exception>
    public static IReadOnlyResult Promote(this IConnection conn, string itemTypeName, string id, string newState, string comments = null, params IAttribute[] attributes)
    {
      if (newState.IsNullOrWhiteSpace()) throw new ArgumentException("State must be a non-empty string to run a promotion.", nameof(newState));
      var aml = conn.AmlContext;
      var promoteItem = aml.Item(aml.Action("promoteItem"),
        aml.Type(itemTypeName),
        aml.Id(id),
        aml.State(newState),
        attributes
      );
      if (!string.IsNullOrEmpty(comments)) promoteItem.Add(aml.Property("comments", comments));
      return promoteItem.Apply(conn);
    }

    /// <summary>
    /// Unlocks the specified item.
    /// </summary>
    /// <param name="conn">The connection.</param>
    /// <param name="itemTypeName">Name of the item type.</param>
    /// <param name="id">The Aras ID.</param>
    /// <param name="attributes">Extra parameters to pass to the aml call.</param>
    /// <returns>The unlock result</returns>
    public static IReadOnlyResult Unlock(this IConnection conn, string itemTypeName, string id, params IAttribute[] attributes)
    {
      var aml = conn.AmlContext;
      return aml.Item(aml.Action("unlock"),
        aml.Type(itemTypeName),
        aml.Id(id),
        attributes
      ).Apply(conn)
      .AssertNoError();
    }
  }
}
