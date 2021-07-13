using Innovator.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
#if TASKS
using System.Threading;
#endif
using System.Xml;
using System.Xml.Linq;

namespace Innovator.Client
{
  /// <summary>
  /// Various extension methods pertaining to various AML interfaces
  /// </summary>
  public static class ItemExtensions
  {
    /// <summary>Value converted to a <see cref="bool"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="prop">The property to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="bool"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="bool"/></exception>
    public static bool AsBoolean(this IReadOnlyProperty_Boolean prop, bool defaultValue)
    {
      return prop.AsBoolean() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="bool"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="attr">The attribute to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="bool"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="bool"/></exception>
    public static bool AsBoolean(this ICoercible attr, bool defaultValue)
    {
      return attr.AsBoolean() ?? defaultValue;
    }

    /// <summary>
    /// Return the value of a property parsed into the appropriate CLR type
    /// </summary>
    public static object AsClrValue(this IReadOnlyProperty_Base prop, Model.IPropertyDefinition meta)
    {
      meta = meta.Metadata();
      if (!prop.HasValue())
        return null;

      var range = prop.Attribute(ParameterSubstitution.DateRangeAttribute).Value;
      if (!string.IsNullOrEmpty(range)
        && ParameterSubstitution.TryDeserializeDateRange(range, out DateOffset? offsetStart, out DateOffset? offsetEnd))
      {
        if (offsetStart.HasValue && offsetEnd.HasValue)
          return new Range<DateOffset>(offsetStart.Value, offsetEnd.Value);
        else if (offsetStart.HasValue)
          return offsetStart.Value;
        else
          return offsetEnd.Value;
      }

      switch (prop.Condition().Value)
      {
        case "in":
          switch (meta.DataType().Value)
          {
            case "boolean":
              throw new NotSupportedException();
            case "integer":
              return AsList(prop, (v, c) => c.AsInt(v).Value);
            case "decimal":
              if (meta.Prec().HasValue()
                && meta.Scale().HasValue()
                && meta.Prec().Value == meta.Scale().Value)
              {
                if (meta.Scale().AsInt(int.MaxValue) <= 9)
                  return AsList(prop, (v, c) => c.AsInt(v).Value);
                return AsList(prop, (v, c) => c.AsLong(v).Value);
              }
              return AsList(prop, (v, c) => c.AsDouble(v).Value);
            case "float":
              return AsList(prop, (v, c) => c.AsDouble(v).Value);
            case "date":
              return AsList(prop, (v, c) => c.AsDateTime(v).Value);
            default:
              return AsList(prop, (v, c) => v);
          }
        case "between":
          switch (meta.DataType().Value)
          {
            case "boolean":
              throw new NotSupportedException();
            case "integer":
              return AsRange(prop, (v, c) => c.AsInt(v).Value);
            case "decimal":
              if (meta.Prec().HasValue()
                && meta.Scale().HasValue()
                && meta.Prec().Value == meta.Scale().Value)
              {
                if (meta.Scale().AsInt(int.MaxValue) <= 9)
                  return AsRange(prop, (v, c) => c.AsInt(v).Value);
                return AsRange(prop, (v, c) => c.AsLong(v).Value);
              }
              return AsRange(prop, (v, c) => c.AsDouble(v).Value);
            case "float":
              return AsRange(prop, (v, c) => c.AsDouble(v).Value);
            case "date":
              return AsRange(prop, (v, c) => c.AsDateTime(v).Value);
            default:
              return AsRange(prop, (v, c) => v);
          }
        default:
          switch (meta.DataType().Value)
          {
            case "boolean":
              return ((IReadOnlyProperty_Boolean)prop).AsBoolean();
            case "integer":
              return ((IReadOnlyProperty_Number)prop).AsInt();
            case "decimal":
              if (meta.Prec().HasValue()
                && meta.Scale().HasValue()
                && meta.Prec().Value == meta.Scale().Value)
              {
                if (meta.Scale().AsInt(int.MaxValue) <= 9)
                  return ((IReadOnlyProperty_Number)prop).AsInt();
                return ((IReadOnlyProperty_Number)prop).AsLong();
              }
              return ((IReadOnlyProperty_Number)prop).AsDouble();
            case "float":
              return ((IReadOnlyProperty_Number)prop).AsDouble();
            case "date":
              return ((IReadOnlyProperty_Date)prop).AsDateTime();
            case "mv_list":
              return prop.Value.Split(',');
            case "item":
              if (prop.Name == "id" || prop.Name == "config_id")
                return prop.Value;

              var aml = prop.AmlContext;
              var propItem = ((IReadOnlyProperty_Item<IReadOnlyItem>)prop).AsItem();
              if (!propItem.Exists && meta.DataSource().HasValue())
                propItem = aml.Item(aml.TypeId(meta.DataSource().Value), aml.Id(prop.Value), aml.KeyedName(prop.KeyedName().Value));

              return propItem;
            case "image":
              if (prop.Value.StartsWith(Utils.VaultPicturePrefix, StringComparison.OrdinalIgnoreCase))
                return ((IReadOnlyProperty_Image)prop).AsItem();
              return new Uri(prop.Value, UriKind.RelativeOrAbsolute);
            case "federated":
              DateTime dateValue;
              return DateTime.TryParse(prop.Value, out dateValue) ? (object)dateValue : prop.Value;
            default:
              return prop.Value;
          }
      }
    }

    internal static Model.IPropertyDefinition Metadata(this Model.IPropertyDefinition prop)
    {
      var meta = prop;
      while (meta.DataType().Value == "foreign" && meta.Property("foreign_property").HasValue() == true)
      {
        meta = (Model.IPropertyDefinition)meta.Property("foreign_property").AsItem();
      }
      return meta;
    }

    private static T[] AsList<T>(IReadOnlyProperty_Base prop, Func<string, IServerContext, T> convert)
    {
      var list = new List<T>();
      var inQuote = false;
      var start = 0;
      var chars = prop.Value;
      if (chars.TrimStart()[0] == '(')
        throw new NotSupportedException("SQL in clause parameters are not supported");
      var context = prop.AmlContext.LocalizationContext;

      for (var i = 0; i < chars.Length; i++)
      {
        if (inQuote)
        {
          if (chars[i] == '\'')
          {
            if (i + 1 < chars.Length && chars[i + 1] == '\'')
              i++;
            else
              inQuote = false;
          }
        }
        else
        {
          if (chars[i] == '\'')
          {
            inQuote = true;
          }
          else if (chars[i] == ',')
          {
            var sub = chars.Substring(start, i - start).Trim();
            if (sub.Length > 0 && sub[0] == '\'')
              sub = sub.Trim('\'').Replace("''", "'");
            list.Add(convert(sub, context));
            start = i + 1;
          }

        }

        if (chars[i] == '\'')
        {
          if (inQuote && i + 1 < chars.Length && chars[i + 1] == '\'')
            i++;
          else
            inQuote = !inQuote;
        }
      }

      return list.ToArray();
    }

    private static Range<T> AsRange<T>(IReadOnlyProperty_Base prop, Func<string, IServerContext, T> convert) where T : IComparable
    {
      var parts = prop.Value.Split(new string[] { " and " }, StringSplitOptions.None);
      if (parts.Length != 2)
        throw new InvalidOperationException();
      var context = prop.AmlContext.LocalizationContext;
      return new Range<T>(convert(parts[0], context), convert(parts[1], context));
    }

    /// <summary>Value converted to a <see cref="DateTime"/> in the local timezone using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="prop">The property to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="DateTime"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTime"/></exception>
    /// <example>
    /// <code lang="C#">
    /// // If the part was created after 2016-01-01, put the name of the creator in the description
    /// if (comp.CreatedOn().AsDateTime(DateTime.MaxValue) &gt; new DateTime(2016, 1, 1))
    /// {
    ///     edits.Property("description").Set("Created by: " + comp.CreatedById().KeyedName().Value);
    /// }
    /// </code>
    /// </example>
    public static DateTime AsDateTime(this IReadOnlyProperty_Date prop, DateTime defaultValue)
    {
      return prop.AsDateTime() ?? defaultValue;
    }

    public static DateTime? AsDateTime(this IServerContext context, object value)
    {
      return context.AsZonedDateTime(value)?.LocalDateTime;
    }

    public static DateTime? AsDateTimeUtc(this IServerContext context, object value)
    {
      return context.AsZonedDateTime(value)?.UtcDateTime;
    }

    /// <summary>Value converted to a <see cref="DateTime"/> in the local timezone using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="attr">The attribute to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="DateTime"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTime"/></exception>
    public static DateTime AsDateTime(this ICoercible attr, DateTime defaultValue)
    {
      return attr.AsDateTime() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="DateTimeOffset"/> in the local timezone using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="prop">The property to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="DateTimeOffset"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTimeOffset"/></exception>
    public static DateTimeOffset AsDateTimeOffset(this IReadOnlyProperty_Date prop, DateTimeOffset defaultValue)
    {
      return prop.AsDateTimeOffset() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="DateTime"/> in the UTC timezone using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="DateTime"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <param name="prop">The property to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTime"/></exception>
    public static DateTime AsDateTimeUtc(this IReadOnlyProperty_Date prop, DateTime defaultValue)
    {
      return prop.AsDateTimeUtc() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="DateTime"/> in the UTC timezone using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="DateTime"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <param name="attr">The attribute to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTime"/></exception>
    public static DateTime AsDateTimeUtc(this ICoercible attr, DateTime defaultValue)
    {
      return attr.AsDateTimeUtc() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="double"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="prop">The property to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="double"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="double"/></exception>
    public static double AsDouble(this IReadOnlyProperty_Number prop, double defaultValue)
    {
      return prop.AsDouble() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="double"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="attr">The attribute to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="double"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="double"/></exception>
    public static double AsDouble(this ICoercible attr, double defaultValue)
    {
      return attr.AsDouble() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="int"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="prop">The property to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="int"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="int"/></exception>
    public static int AsInt(this IReadOnlyProperty_Number prop, int defaultValue)
    {
      return prop.AsInt() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="int"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="attr">The attribute to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="int"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="int"/></exception>
    public static int AsInt(this ICoercible attr, int defaultValue)
    {
      return attr.AsInt() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="long"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="prop">The property to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="long"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="long"/></exception>
    public static long AsLong(this IReadOnlyProperty_Number prop, long defaultValue)
    {
      return prop.AsLong() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="long"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="attr">The attribute to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="long"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="long"/></exception>
    public static long AsLong(this ICoercible attr, long defaultValue)
    {
      return attr.AsLong() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="Guid"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="prop">The property to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="Guid"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="Guid"/></exception>
    public static Guid AsGuid<T>(this IReadOnlyProperty_Item<T> prop, Guid defaultValue)
    {
      return prop.AsGuid() ?? defaultValue;
    }

    /// <summary>Value converted to a <see cref="Guid"/> using the <paramref name="defaultValue"/> if null.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <param name="attr">The attribute to convert</param>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="Guid"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="Guid"/></exception>
    public static Guid AsGuid(this ICoercible attr, Guid defaultValue)
    {
      return attr.AsGuid() ?? defaultValue;
    }

    /// <summary>
    /// Convert an item to a result object so that it can be more easily
    /// returned from a server method
    /// </summary>
    public static IReadOnlyResult AsResult(this IReadOnlyItem item)
    {
      var result = new Result(item.AmlContext);
      result.AddReadOnly(item);
      return result;
    }

    /// <summary>Return a single item cast as the specified type.  If that
    /// is not possible, throw an appropriate exception (e.g. the exception
    /// returned by the server where possible)</summary>
    public static T AssertItem<T>(this IReadOnlyResult result) where T : IReadOnlyItem
    {
      return CastModel<T>(result.AssertItem());
    }

    /// <summary>Return an item cast as the specified type if that type of
    /// item can be derived from the property value.  Otherwise, an exception
    /// is thrown</summary>
    public static T AsModel<T>(this IReadOnlyProperty_Item<T> prop) where T : IReadOnlyItem
    {
      return CastModel<T>(prop.AsItem());
    }

    /// <summary>Return an item cast as the specified type if that type of
    /// item can be derived from the property value.  Otherwise, an exception
    /// is thrown</summary>
    public static T RelatedModel<T>(this IRelationship<T> item) where T : IReadOnlyItem
    {
      return CastModel<T>(item.RelatedItem());
    }

    /// <summary>Return an item cast as the specified type if that type of
    /// item can be derived from the property value.  Otherwise, an exception
    /// is thrown</summary>
    public static T SourceModel<T>(this INullRelationship<T> item) where T : IReadOnlyItem
    {
      return CastModel<T>(item.SourceItem());
    }

    private static T CastModel<T>(IReadOnlyItem item) where T : IReadOnlyItem
    {
      if (item is T)
        return (T)item;

      if (!item.Exists)
        return Item.GetNullItem<T>();

      var typeName = item.Type().HasValue() ? item.Type().Value : item.GetType().Name;
      throw new InvalidOperationException(string.Format("An item of type '{0}' was found while an item of type '{1}' was expected.", typeName, typeof(T).Name));
    }

    /// <summary>
    /// Converts an AML node into an AML string
    /// </summary>
    public static string ToAml(this IAmlNode node, AmlWriterSettings settings = null)
    {
      using (var writer = new StringWriter())
      using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings() { OmitXmlDeclaration = true }))
      {
        node.ToAml(xmlWriter, settings ?? new AmlWriterSettings());
        xmlWriter.Flush();
        return writer.ToString();
      }
    }

    /// <summary>
    /// Converts an AML node into an AML string
    /// </summary>
    public static void ToAml(this IAmlNode node, XmlWriter writer)
    {
      node.ToAml(writer, new AmlWriterSettings());
    }

    /// <summary>
    /// Converts an AML node into a query which can be converted to other forms (e.g. SQL, OData, ...)
    /// </summary>
    /// <param name="node">The node.</param>
    public static QueryModel.QueryItem ToQueryItem(this IAmlNode node, IServerContext context = null)
    {
      return QueryModel.QueryItem.FromXml(node, context = context
        ?? (node as IReadOnlyElement)?.AmlContext.LocalizationContext
        ?? ElementFactory.Local.LocalizationContext);
    }

    /// <summary>
    /// Apply this item in the database
    /// </summary>
    public static IReadOnlyResult Apply(this IReadOnlyItem item, IConnection conn)
    {
      var aml = conn.AmlContext;
      var query = item.ToAml();
      return aml.FromXml(conn.Process(query), query, conn);
    }

    /// <summary>
    /// Get the result of executing the specified AML query
    /// </summary>
    /// <param name="item">AML query to apply</param>
    /// <param name="conn">Connection to execute the query on</param>
    /// <returns>A read-only result</returns>
    public static IPromise<IReadOnlyResult> ApplyAsync(this IReadOnlyItem item, IAsyncConnection conn)
    {
      return conn.ApplyAsync(new Command(item.ToAml()), true, false);
    }

#if TASKS
    /// <summary>
    /// Get the result of executing the specified AML query
    /// </summary>
    /// <param name="item">AML query to apply</param>
    /// <param name="conn">Connection to execute the query on</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the asynchronous operation</param>
    /// <returns>A read-only result</returns>
    public static IPromise<IReadOnlyResult> ApplyAsync(this IReadOnlyItem item, IAsyncConnection conn, CancellationToken ct)
    {
      return conn.ApplyAsync(new Command(item.ToAml()), ct);
    }
#endif

    /// <summary>Download the file represented by the property </summary>
    /// <returns>This will return the file contents for item properties of type 'File' and
    /// image properties that point to vault files</returns>
    public static Stream AsFile(this IReadOnlyProperty prop, IConnection conn)
    {
      return prop.AsFile(conn, false).Value;
    }

    /// <summary>Asynchronously download the file represented by the property</summary>
    /// <returns>This will return the file contents for item properties of type 'File' and
    /// image properties that point to vault files</returns>
    public static IPromise<Stream> AsFile(this IReadOnlyProperty prop, IConnection conn, bool async)
    {
      if (prop == null)
        throw new ArgumentNullException("prop");

      var file = prop.AsItem();
      if (file.Exists && file.Type().Value == "File")
      {
        var cmd = new Command(file.ToString()).WithAction(CommandAction.DownloadFile);
        return conn.ProcessAsync(cmd, async);
      }

      var url = prop.AsString("");
      if (url.StartsWith(Utils.VaultPicturePrefix, StringComparison.OrdinalIgnoreCase))
      {
        var id = url.Substring(Utils.VaultPicturePrefix.Length);
        var cmd = new Command("<Item type='File' action='get' id='@0' />", id).WithAction(CommandAction.DownloadFile);
        return conn.ProcessAsync(cmd, async);
      }

      // Random url used for testing the relative path
      var baseUri = new Uri("http://www.test.com/a/b/c/d/e/f");
      Uri testResult;
      if (Uri.TryCreate(baseUri, url, out testResult))
      {
        url = conn.MapClientUrl(url);

        byte[] data;
        if (Factory.ImageCache.TryGetValue(url, out data))
        {
          return Promises.Resolved((Stream)new MemoryTributary(data));
        }

        var trace = new LogData(4, "Innovator: Get picture from client server", Factory.LogListener)
        {
          { "aras_url", conn.MapClientUrl("../../Server") },
          { "database", conn.Database },
          { "property", prop.ToAml() },
          { "url", url },
          { "user_id", conn.UserId }
        };
        var service = (conn as Connection.ArasHttpConnection)?.Service
          ?? ConnectionPreferences.GetService();
        return service.GetPromise(new Uri(url), async, trace).Convert(r =>
        {
          var download = r.AsStream;
          var buffer = download as MemoryTributary;
          if (buffer == null)
          {
            using (var stream = r.AsStream)
            {
              buffer = new MemoryTributary();
              stream.CopyTo(buffer);
            }
          }
          Factory.ImageCache.TryAdd(url, buffer.ToArray());
          buffer.Position = 0;
          return (Stream)buffer;
        }).Always(trace.Dispose);
      }

      return Promises.Rejected<Stream>(
        new ArgumentException(string.Format("Property '{0}' does not reference a file to download", prop.Name), "prop"));
    }

    /// <summary>Determine if the <c>classification</c> of the <paramref name="item"/> starts with one of the specified root paths</summary>
    public static bool ClassStartsWith(this IReadOnlyItem item, params string[] roots)
    {
      if (roots == null) return false;
      var path = item.Classification().Value;

      foreach (var root in roots)
      {
        if (string.IsNullOrEmpty(root)) return true;
        if (!string.IsNullOrEmpty(path)
          && (path.Equals(root.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(root.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))) return true;
      }
      return false;
    }

    /// <summary>
    /// Removes system properties from an item
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="doRemoveProperty">The property callback.  Return <c>true</c> to remove the property</param>
    public static void RemoveSystemProperties(this IItem item, Func<string, bool> doRemoveProperty = null)
    {
      if (doRemoveProperty?.Invoke("is_released") != false)
        item.IsReleased().Remove();
      if (doRemoveProperty?.Invoke("not_lockable") != false)
        item.NotLockable().Remove();

      if (doRemoveProperty?.Invoke("state") != false)
      {
        item.State().Remove();
        item.CurrentState().Remove();
      }

      if (doRemoveProperty?.Invoke("release_date") != false)
        item.Property("release_date").Remove();
      if (doRemoveProperty?.Invoke("effective_date") != false)
        item.Property("effective_date").Remove();
      if (doRemoveProperty?.Invoke("superseded_date") != false)
        item.Property("superseded_date").Remove();

      if (item.TypeName() != "RelationshipType" && doRemoveProperty?.Invoke("behavior") != false)
        item.Property("behavior").Remove();

      if (doRemoveProperty?.Invoke("permission_id") != false)
        item.PermissionId().Remove();

      if (doRemoveProperty?.Invoke("created_by_id") != false)
        item.CreatedById().Remove();
      if (doRemoveProperty?.Invoke("created_on") != false)
        item.CreatedOn().Remove();
      if (doRemoveProperty?.Invoke("modified_by_id") != false)
        item.ModifiedById().Remove();
      if (doRemoveProperty?.Invoke("modified_on") != false)
        item.ModifiedOn().Remove();

      if (doRemoveProperty?.Invoke("major_rev") != false)
        item.MajorRev().Remove();
      if (doRemoveProperty?.Invoke("minor_rev") != false)
        item.MinorRev().Remove();

      if (doRemoveProperty?.Invoke("cache_query") != false)
        item.Property("cache_query").Remove();
      if (doRemoveProperty?.Invoke("config_id") != false)
        item.ConfigId().Remove();
      if (doRemoveProperty?.Invoke("core") != false)
        item.Property("core").Remove();
      if (doRemoveProperty?.Invoke("generation") != false)
        item.Generation().Remove();
      if (doRemoveProperty?.Invoke("history_id") != false)
        item.Property("history_id").Remove();
      if (doRemoveProperty?.Invoke("is_cached") != false)
        item.Property("is_cached").Remove();

      if (doRemoveProperty?.Invoke("is_current") != false)
        item.IsCurrent().Remove();
      if (doRemoveProperty?.Invoke("keyed_name") != false)
        item.KeyedName().Remove();
      if (doRemoveProperty?.Invoke("locked_by_id") != false)
        item.LockedById().Remove();
      if (doRemoveProperty?.Invoke("new_version") != false)
        item.NewVersion().Remove();
      if (doRemoveProperty?.Invoke("itemtype") != false)
        item.Property("itemtype").Remove();

      if (item.TypeName() != "Field" && item.TypeName() != "Body" && doRemoveProperty?.Invoke("css") != false)
        item.Property("css").Remove();

      if (item.Parent.Name == "Relationships")
        item.SourceId().Remove();
    }

    /// <summary>
    /// Creates a duplicate of the item object, removing system properties, and setting a new ID.
    /// </summary>
    /// <param name="item">The item to duplicate</param>
    /// <param name="cloneSettings">The clone settings.</param>
    /// <returns>The cloned/duplicated item</returns>
    public static IItem CloneAsNew(this IReadOnlyItem item, CloneSettings cloneSettings = null)
    {
      var newItem = item.Clone();
      ProcessClone(newItem, cloneSettings, item.TypeName());
      return newItem;
    }

    private static void ProcessClone(IItem item, CloneSettings cloneSettings, string path)
    {
      item.Where().Remove();
      item.IdList().Remove();
      item.Action().Set("add");
      item.RemoveSystemProperties(cloneSettings?.DoRemoveSystemProperty);
      var newId = item.AmlContext.NewId();
      item.Attribute("id").Set(newId);
      item.Property("id").Remove();

      foreach (var nullProp in item.Elements().OfType<IProperty_Base>().Where(p => p.IsNull().Exists && !p.HasValue()))
      {
        nullProp.Remove();
      }

      foreach (var child in item.Elements().SelectMany(e => e.Elements().OfType<IItem>()))
      {
        path += "/" + child.Parent.Name + "/" + child.TypeName();
        if (cloneSettings?.DoCloneItem?.Invoke(path, child) == false)
        {
          ((IProperty_Base)child.Parent).Set(child.Id());
        }
        else
        {
          child.Parent.Attribute("keyed_name").Remove();
          child.Parent.Attribute("type").Remove();
          ProcessClone(child, cloneSettings, path);
        }
      }
    }

    /// <summary>
    /// Send an AML edit query to the database with the body of the <c>&lt;Item /&gt;</c> tag being the contents specified
    /// </summary>
    /// <param name="item">Item to edit</param>
    /// <param name="conn">Connection to use for applying the edit</param>
    /// <param name="contents">Body of the <c>&lt;Item action='edit' /&gt;</c> tag</param>
    public static IReadOnlyResult Edit(this IItemRef item, IConnection conn, params object[] contents)
    {
      var aml = conn.AmlContext;
      var editItem = aml.Item(aml.Action("edit"), aml.Type(item.TypeName()), aml.Id(item.Id()));
      foreach (var content in contents)
        editItem.Add(content);
      var query = editItem.ToString();
      return aml.FromXml(conn.Process(query), query, conn);
    }

    /// <summary>
    /// Retrieve the lock status from the database
    /// </summary>
    /// <param name="item">Item to check the lock status of</param>
    /// <param name="conn">Connection to use for executing this query</param>
    /// <remarks>If the item is editable, the <c>locked_by_id</c> property will be updated</remarks>
    public static LockStatusType FetchLockStatus(this IItemRef item, IConnection conn)
    {
      var aml = conn.AmlContext;
      return aml.Item(aml.Action("get"),
        aml.Type(item.TypeName()),
        aml.Id(item.Id()),
        aml.Select("locked_by_id")
      ).Apply(conn).AssertItem().LockStatus(conn);
    }

    /// <summary>
    /// Returns either first element from the parent with a local name of <paramref name="name"/>
    /// or a 'null' element (where <see cref="IReadOnlyElement.Exists"/> is <c>false</c>)
    /// if there are no elements
    /// </summary>
    /// <param name="element">Parent element to search through</param>
    /// <param name="name">Name of the element to find</param>
    public static IReadOnlyElement Element(this IReadOnlyElement element, string name)
    {
      var elem = element as Element;
      if (elem != null)
        return elem.ElementByName(name);
      return element.Elements().FirstOrDefault(e => e.Name == name) ?? AmlElement.NullElem;
    }

    /// <summary>
    /// Returns either the first element from the enumerable or a 
    /// 'null' element (where <see cref="IReadOnlyElement.Exists"/> is <c>false</c>)
    /// if there are no elements
    /// </summary>
    /// <param name="elements">Elements to search through</param>
    public static IElement FirstOrNullElement(this IEnumerable<IElement> elements)
    {
      if (elements == null)
        throw new ArgumentNullException("elements");

      return elements.FirstOrDefault() ?? AmlElement.NullElem;
    }

    /// <summary>
    /// Returns either the first item from the enumerable or a 'null' item (where <c>Exists</c> is <c>false</c>)
    /// if there are no items
    /// </summary>
    /// <param name="items">Items to search through</param>
    public static T FirstOrNullItem<T>(this IEnumerable<T> items) where T : IReadOnlyItem
    {
      if (items == null)
        throw new ArgumentNullException("items");

      var list = items as IList<T>;
      if (list != null)
      {
        if (list.Count > 0)
        {
          return list[0];
        }
      }
      else
      {
        using (var enumerator = items.GetEnumerator())
        {
          if (enumerator.MoveNext())
          {
            return enumerator.Current;
          }
        }
      }
      return Item.GetNullItem<T>();
    }

    /// <summary>
    /// Returns either the first matching item from the enumerable or a 'null' item (where <c>Exists</c> is <c>false</c>)
    /// if there are no items which match the predicate
    /// </summary>
    /// <param name="items">Items to search through</param>
    /// <param name="predicate">Criteria to match</param>
    public static T FirstOrNullItem<T>(this IEnumerable<T> items, Func<T, bool> predicate) where T : IReadOnlyItem
    {
      if (items == null)
        throw new ArgumentNullException("items");
      if (predicate == null)
        throw new ArgumentNullException("items");

      foreach (var item in items)
      {
        if (predicate(item))
          return item;
      }
      return Item.GetNullItem<T>();
    }

    /// <summary>
    /// Indicates that the property is neither null nor empty
    /// </summary>
    public static bool HasValue(this IReadOnlyProperty_Base prop)
    {
      return prop.Exists
        && (!string.IsNullOrEmpty(prop.Value)
          || prop.Elements().Any());
    }

    /// <summary>
    /// Indicates that the attribute is neither null nor empty
    /// </summary>
    public static bool HasValue(this IReadOnlyAttribute attr)
    {
      return attr.Exists && !string.IsNullOrEmpty(attr.Value);
    }

    /// <summary>
    /// Returns the total number of items founds including those
    /// returned on other pages
    /// </summary>
    public static int ItemMax(this IReadOnlyResult result)
    {
      var cntMessage = result.Message.Elements()
          .FirstOrDefault(e => e.Name == "event"
            && e.Attribute("name").Value == "itemmax"
            && e.Attribute("value").HasValue());
      if (cntMessage != null)
      {
        int cnt;
        if (int.TryParse(cntMessage.Attribute("value").Value, out cnt))
          return cnt;
      }

      return result.Items().FirstOrNullItem().Attribute("itemmax").AsInt()
        ?? result.Items().Count();
    }

    /// <summary>
    /// Returns the number of matching items which the user does
    /// not have access to see
    /// </summary>
    public static int? ItemsWithNoAccessCount(this IReadOnlyResult result)
    {
      var ex = result.Exception as NoItemsFoundException;
      if (ex != null)
      {
        return ex.ItemsWithNoAccessCount();
      }
      else
      {
        var cntMessage = result.Message.Elements()
          .FirstOrDefault(e => e.Name == "event"
            && e.Attribute("name").Value == "items_with_no_access_count"
            && e.Attribute("value").HasValue());
        if (cntMessage != null)
        {
          int count;
          if (int.TryParse(cntMessage.Attribute("value").Value, out count))
            return count;
        }
      }
      return null;
    }

    /// <summary>
    /// Returns the number of matching items which the user does
    /// not have access to see
    /// </summary>
    public static int? ItemsWithNoAccessCount(this NoItemsFoundException ex)
    {
      if (ex == null)
        return null;

      var cntMessage = ex.Fault.Element("detail").Elements()
          .FirstOrDefault(e => e.Name == "message"
            && e.Attribute("key").Value == "items_with_no_access_count"
            && e.Attribute("value").HasValue());
      if (cntMessage != null)
      {
        int count;
        if (int.TryParse(cntMessage.Attribute("value").Value, out count))
          return count;
      }
      return null;
    }

    /// <summary>
    /// <inheritdoc cref="ConnectionExtensions.Lock(IConnection, string, string, IAttribute[])"/>
    /// </summary>
    /// <param name="item">The item to unlock</param>
    /// <param name="conn">The Aras connection</param>
    /// <param name="attributes">Extra parameters to pass to the aml call.</param>
    public static IReadOnlyResult Lock(this IItemRef item, IConnection conn, params IAttribute[] attributes)
    {
      var result = conn.Lock(item.TypeName(), item.Id(), attributes);

      if (item is Element elem && elem.ReadOnly)
        return result;

      if (item is IItem editable)
      {
        var serverResponse = result.Items().FirstOrNullItem();
        if (serverResponse.Exists)
          editable.LockedById().Set(serverResponse.LockedById().Value);
      }

      return result;
    }

    /// <summary>
    /// Determine the lock status of an item based on the data loaded into
    /// memory.
    /// </summary>
    /// <param name="item">Item to check the status of</param>
    /// <param name="conn">Connection containing the ID of the current user</param>
    /// <remarks>A call will not be made to the database</remarks>
    public static LockStatusType LockStatus(this IReadOnlyItem item, IConnection conn)
    {
      if (!item.LockedById().HasValue() || item.Attribute("isNew").AsBoolean(false))
        return LockStatusType.NotLocked;
      if (item.LockedById().Value == conn.UserId) return LockStatusType.LockedByUser;
      return LockStatusType.LockedByOther;
    }

    /// <summary>
    /// Add multiple elements to an <see cref="IElement"/> at the same time
    /// </summary>
    public static IElement Add(this IElement elem, params object[] content)
    {
      return elem.Add((object)content);
    }

    /// <summary>
    /// <inheritdoc cref="ConnectionExtensions.Promote(IConnection, string, string, string, string, IAttribute[])"/>
    /// </summary>
    /// <param name="item">Item to promote</param>
    /// <param name="conn">Connection to execute the promotion on</param>
    /// <param name="state">New state of the item</param>
    /// <param name="comments">Comments to include with the promotion</param>
    /// <param name="attributes">Extra parameters to pass to the aml call.</param>
    /// <example>
    /// <code lang="C#">
    /// // Promote the item. Throw an exception if an error occurs.
    /// comp.Promote(conn, "Released").AssertNoError();
    /// </code>
    /// </example>
    public static IReadOnlyResult Promote(this IItemRef item, IConnection conn, string state, string comments = null, params IAttribute[] attributes)
    {
      return conn.Promote(item.TypeName(), item.Id(), state, comments, attributes);
    }

    /// <summary>
    /// Renders an AML node to XML
    /// </summary>
    public static XElement ToXml(this IAmlNode node)
    {
      var doc = new XDocument();
      using (var writer = doc.CreateWriter())
      {
        node.ToAml(writer);
      }
      return doc.Root;
    }

    /// <summary>
    /// <inheritdoc cref="ConnectionExtensions.Unlock(IConnection, string, string, IAttribute[])"/>
    /// </summary>
    /// <param name="item">The item to unlock</param>
    /// <param name="conn">The Aras connection</param>
    /// <param name="attributes">Extra parameters to pass to the aml call.</param>
    public static IReadOnlyResult Unlock(this IItemRef item, IConnection conn, params IAttribute[] attributes)
    {
      var result = conn.Unlock(item.TypeName(), item.Id(), attributes);

      if (item is Element elem && elem.ReadOnly)
        return result;

      if (item is IItem editable)
        editable.LockedById().Remove();

      return result;
    }

    /// <summary>
    /// Maps an item to a new object.  If there are properties which couldn't be found during the
    /// initial mapping, the method will query the database and run the mapper again with the
    /// database results
    /// </summary>
    /// <param name="item">Item to map</param>
    /// <param name="conn">Connection used for querying the database when property values are not available</param>
    /// <param name="mapper">Function which creates a new object by referencing values from the item</param>
    /// <example>
    /// <code lang="C#">
    /// var result = item.LazyMap(conn, i => new
    /// {
    ///   FirstName = i.CreatedById().AsItem().Property("first_name").Value,
    ///   PermName = i.PermissionId().AsItem().Property("name").Value,
    ///   KeyedName = i.Property("id").KeyedName().Value,
    ///   Empty = i.OwnedById().Value
    /// });
    /// </code>
    /// </example>
    public static T LazyMap<T>(this IReadOnlyItem item, IConnection conn, Func<IReadOnlyItem, T> mapper)
    {
      return LazyMap(new IReadOnlyItem[] { item }, conn, mapper).Single();
    }

    /// <summary>
    /// Maps an set of items to new objects.  If there are properties which couldn't be found 
    /// during the initial mapping, the method will query the database and run the mapper again 
    /// with the database results
    /// </summary>
    /// <param name="items">The set of items to map</param>
    /// <param name="conn">Connection used for querying the database when property values are not available</param>
    /// <param name="mapper">Function which creates a new object by referencing values from the item</param>
    /// <example>
    /// <code lang="C#">
    /// var results = items.LazyMap(conn, i => new
    /// {
    ///   FirstName = i.CreatedById().AsItem().Property("first_name").Value,
    ///   PermName = i.PermissionId().AsItem().Property("name").Value,
    ///   KeyedName = i.Property("id").KeyedName().Value,
    ///   Empty = i.OwnedById().Value
    /// });
    /// </code>
    /// </example>
    public static IEnumerable<T> LazyMap<T>(this IEnumerable<IReadOnlyItem> items, IConnection conn, Func<IReadOnlyItem, T> mapper)
    {
      var results = new List<T>();
      var select = new SelectNode();
      var missingPropsItems = new List<KeyValuePair<int, IItem>>();

      foreach (var item in items)
      {
        var missingProps = false;
        var watched = new ItemWatcher(item, "", (path, exists) =>
        {
          select.EnsurePath(path.Split('/'));
          missingProps = missingProps || !exists;
        });

        results.Add(mapper.Invoke(watched));
        if (missingProps)
        {
          if (string.IsNullOrEmpty(item.Id()))
            throw new ArgumentException(string.Format("No id specified for the item '{0}'", item.ToAml()));
          missingPropsItems.Add(new KeyValuePair<int, IItem>(results.Count - 1, item.Clone()));
        }
      }

      foreach (var group in missingPropsItems.GroupBy(t => t.Value.TypeName()))
      {
        var aml = conn.AmlContext;
        var query = aml.Item(aml.Action("get"), aml.Type(group.Key), aml.Select(select));
        var ids = group.Select(t => t.Value.Id()).GroupConcat(",");
        if (ids.IndexOf(',') > 0)
          query.IdList().Set(ids);
        else
          query.Attribute("id").Set(ids);
        var databaseItemDict = query.Apply(conn).Items().ToDictionary(i => i.Id());
        IEnumerable<Model.Property> itemTypeProperties = null;

        foreach (var tuple in group)
        {
          var incomingItem = tuple.Value;
          if (databaseItemDict.TryGetValue(incomingItem.Id(), out IReadOnlyItem databaseItem))
          {
            // Take the database item attributes and add to the incoming item any missing attributes
            foreach (var databaseAttribute in databaseItem.Attributes())
            {
              var incomingAttribute = incomingItem.Attribute(databaseAttribute.Name);
              if (!incomingAttribute.Exists)
              {
                incomingAttribute.Remove();
                incomingItem.Add(databaseAttribute);
              }
            }
            // Take the database item properties and add to the incoming item any missing properties
            foreach (var databaseElement in databaseItem.Elements())
            {
              var incomingProp = incomingItem.Property(databaseElement.Name);
              // We want to take the database property if:
              // The property does not exist on the incoming item
              // Or the database value is the same as the incoming
              //  - In this case we may be getting additional props from the database that we don't already have
              if (!incomingProp.Exists || incomingProp.Value == databaseElement.Value)
              {
                incomingProp.Remove();
                incomingItem.Add(databaseElement);
              }
              // Retrieve the data for the new value
              else if (incomingProp.HasValue() && incomingProp.Value != databaseElement.Value)
              {
                var propertySelect = select.SelectMany(x => x).FirstOrDefault(x => x.Name == databaseElement.Name);
                // Count > 0 means we need to get nested properties like created_by_id(first_name,last_name)
                if (propertySelect?.Count > 0)
                {
                  // We need extra data about this property
                  var propertyItem = incomingProp.AsItem();

                  // This property is an item link but without a type attribute
                  if (!propertyItem.Exists && incomingProp.Value.IsGuid())
                  {
                    // Only get the itemType Properties once
                    if (itemTypeProperties == null)
                    {
                      // Get the Properties for this ItemType
                      itemTypeProperties = aml.Item(aml.Action("get"),
                        aml.Type("Property"),
                        aml.Select("data_source,data_type"),
                        aml.Property("data_type", aml.Condition(Condition.In), new[] { "item", "foreign" }),
                        aml.SourceId(aml.Item(
                          aml.Action("get"),
                          aml.Type("ItemType"),
                          aml.Property("name", group.Key))))
                        .Apply(conn).Items().OfType<Model.Property>();
                    }
                    var itemTypeProperty = itemTypeProperties
                      .FirstOrNullItem(x => string.Equals(x.NameProp().Value, databaseElement.Name, StringComparison.OrdinalIgnoreCase));
                    if (itemTypeProperty.Exists)
                    {
                      if (string.Equals(itemTypeProperty.DataType().Value, "foreign", StringComparison.OrdinalIgnoreCase))
                      {
                        // TODO: Implement Foreign property support
                        throw new NotImplementedException("LazyMap does not support Foreign properties at this time.");
                      }
                      // Only do the lookup for properties with the 'item' data type
                      else if (string.Equals(itemTypeProperty.DataType().Value, "item", StringComparison.OrdinalIgnoreCase))
                      {
                        propertyItem = aml.Item(aml.Type(itemTypeProperty.DataSource().Attribute("name").Value),
                          aml.Id(incomingProp.Value));
                      }
                    }
                  }

                  if (propertyItem.Exists)
                  {
                    // Do the item lookup
                    propertyItem.Action().Set("get");
                    propertyItem.Select().Set(propertySelect);
                    var propertyDatabaseItem = propertyItem.Apply(conn).Items().FirstOrNullItem();
                    if (propertyDatabaseItem.Exists)
                    {
                      incomingProp.Set(propertyDatabaseItem);
                    }
                  }
                }
              }
            }
            results[tuple.Key] = mapper.Invoke(incomingItem);
          }
          // So the top item couldn't be found (e.g. perhaps this is during an onBeforeAdd).  Now, let's try filling
          // in any multi-level selects
          else if (select[0].Any(s => s.Count > 0))
          {
            foreach (var multiSelect in select[0].Where(s => s.Count > 0 && s.Name != "id" && s.Name != "config_id"))
            {
              if (incomingItem.Property(multiSelect.Name).HasValue() && incomingItem.Property(multiSelect.Name).Type().HasValue())
              {
                query = aml.Item(aml.Action("get")
                  , aml.Type(incomingItem.Property(multiSelect.Name).Type().Value)
                  , aml.Select(multiSelect)
                  , aml.Id(incomingItem.Property(multiSelect.Name).Value));
                var res = query.Apply(conn);
                if (res.Items().Any())
                {
                  incomingItem.Property(multiSelect.Name).Set(res.AssertItem());
                }
              }
            }
            results[tuple.Key] = mapper.Invoke(incomingItem);
          }
        }
      }

      return results;
    }

    /// <summary>
    /// Maps an item to a new object.  If there are properties which couldn't be found during the
    /// initial mapping, the method will query the database and run the mapper again with the
    /// database results
    /// </summary>
    /// <param name="context">Context containing the item to map and the connection used for 
    /// querying the database when property values are not available</param>
    /// <param name="mapper">Function which creates a new object by referencing values from the item</param>
    /// <example>
    /// <code lang="C#">
    /// var result = arg.LazyMap(i => new
    /// {
    ///   FirstName = i.CreatedById().AsItem().Property("first_name").Value,
    ///   PermName = i.PermissionId().AsItem().Property("name").Value,
    ///   KeyedName = i.Property("id").KeyedName().Value,
    ///   Empty = i.OwnedById().Value
    /// });
    /// </code>
    /// </example>
    public static T LazyMap<T>(this ISingleItemContext context, Func<IReadOnlyItem, T> mapper)
    {
      return context.Item.LazyMap<T>(context.Conn, mapper);
    }

    private class ItemWatcher : ItemWrapper
    {
      private readonly string _path;
      private readonly Action<string, bool> _listener;

      public ItemWatcher(IReadOnlyItem item, string path, Action<string, bool> listener) : base(item)
      {
        _path = path ?? "";
        _listener = listener;
      }

      public override IReadOnlyProperty Property(string name)
      {
        return new PropertyWatcher(base.Property(name), _path + "/" + name, _listener);
      }

      public override IReadOnlyProperty Property(string name, string lang)
      {
        return new PropertyWatcher(base.Property(name, lang), _path + "/" + name, _listener);
      }
    }

    private class PropertyWatcher : IReadOnlyProperty
    {
      private readonly IReadOnlyProperty _prop;
      private readonly string _path;
      private readonly Action<string, bool> _listener;

      public PropertyWatcher(IReadOnlyProperty prop, string path, Action<string, bool> listener)
      {
        _prop = prop;
        _path = path ?? "";
        _listener = listener;
        _listener(path, _prop.Exists);
      }

      public bool? AsBoolean()
      {
        return _prop.AsBoolean();
      }

      public DateTime? AsDateTime()
      {
        return _prop.AsDateTime();
      }

      public DateTimeOffset? AsDateTimeOffset()
      {
        return _prop.AsDateTimeOffset();
      }

      public DateTime? AsDateTimeUtc()
      {
        return _prop.AsDateTimeUtc();
      }

      public double? AsDouble()
      {
        return _prop.AsDouble();
      }

      public Guid? AsGuid()
      {
        return _prop.AsGuid();
      }

      public int? AsInt()
      {
        return _prop.AsInt();
      }

      public IReadOnlyItem AsItem()
      {
        return new ItemWatcher(_prop.AsItem(), _path, _listener);
      }

      public string AsString(string defaultValue)
      {
        return _prop.AsString(defaultValue);
      }

      public IReadOnlyAttribute Attribute(string name)
      {
        return _prop.Attribute(name);
      }

      public IEnumerable<IReadOnlyAttribute> Attributes()
      {
        return _prop.Attributes();
      }

      public IEnumerable<IReadOnlyElement> Elements()
      {
        return _prop.Elements();
      }

      public ElementFactory AmlContext
      {
        get { return _prop.AmlContext; }
      }

      public bool Exists
      {
        get { return _prop.Exists; }
      }

      public string Name
      {
        get { return _prop.Name; }
      }

      public IReadOnlyElement Parent
      {
        get { return _prop.Parent; }
      }

      public string Prefix
      {
        get { return _prop.Prefix; }
      }

      public string Value
      {
        get { return _prop.Value; }
      }

      public void ToAml(XmlWriter writer, AmlWriterSettings settings)
      {
        _prop.ToAml(writer, settings);
      }

      public long? AsLong()
      {
        return _prop.AsLong();
      }

      public long AsLong(long defaultValue)
      {
        return _prop.AsLong(defaultValue);
      }
    }

    private static SelectNode MissingProperties(IReadOnlyItem item, string propName, IEnumerable<SelectNode> properties)
    {
      var result = new SelectNode(propName);
      IReadOnlyProperty itemProp;
      foreach (var prop in properties)
      {
        itemProp = item.Property(prop.Name);
        if (itemProp.Exists)
        {
          if (prop.Any())
          {
            var child = itemProp.AsItem();
            if (child == null)
            {
              result.Add(prop);
            }
            else
            {
              var missing = MissingProperties(child, prop.Name, prop);
              if (missing.Any()) result.Add(missing);
            }
          }
        }
        else
        {
          result.Add(prop);
        }
      }
      return result;
    }


    /// <summary>
    /// Retrieve the Workflow Process Path by name
    /// </summary>
    public static IReadOnlyItem Path(this Model.Activity act, IConnection conn, string name)
    {
      var path = act.Relationships("Workflow Process Path").FirstOrDefault(i => i.Property("name").Value == name);
      if (path != null) return path;
      return conn.ItemByQuery(new Command(@"<Item type='Workflow Process Path' action='get'>
                                              <source_id>@0</source_id>
                                              <name>@1</name>
                                            </Item>", act.Id(), name));
    }

    /// <summary>
    /// Perform a vote for a specified assignment and path
    /// </summary>
    public static IReadOnlyResult PerformVote(this Model.Activity act, IConnection conn, string assignmentId, string pathName,
      string comment = null)
    {
      var path = act.Path(conn, pathName);
      return conn.Apply(new Command(@"<AML>
                                        <Item type='Activity' action='EvaluateActivityEx'>
                                          <Activity>@0</Activity>
                                          <ActivityAssignment>@1</ActivityAssignment>
                                          <Paths>
                                            <Path id='@2'>@3</Path>
                                          </Paths>
                                          <DelegateTo>0</DelegateTo>
                                          <Tasks/>
                                          <Variables/>
                                          <Authentication mode=''/>
                                          <Comments>@4</Comments>
                                          <Complete>1</Complete>
                                        </Item>
                                      </AML>", act.Id(), assignmentId, path.Id(), pathName,
                                             comment));
    }

    /// <summary>
    /// Set the duration of a workflow activity <paramref name="act"/> so that it will be due on <paramref name="dueDate"/>
    /// </summary>
    public static void SetDurationByDate(this Model.Activity act, IConnection conn, DateTime dueDate, int minDuration = 1,
                                  int maxDuration = int.MaxValue)
    {
      var props = act.LazyMap(conn, i => new
      {
        ActiveDate = i.Property("active_date").AsDateTime(DateTime.Now)
      });
      var duration = Math.Min(Math.Max((dueDate.Date - props.ActiveDate.Date).Days,
                                      minDuration), maxDuration);
      act.Edit(conn, conn.AmlContext.Property("expected_duration", duration)).AssertNoError();
    }

    /// <summary>
    /// Set the activity <paramref name="act"/> to be an automatic activity
    /// </summary>
    public static void SetIsAuto(this Model.Activity act, IConnection conn, bool isAuto)
    {
      act.Edit(conn, conn.AmlContext.Property("is_auto", isAuto)).AssertNoError();
    }

    /// <summary>
    /// Create an <see cref="XmlReader"/> for reading the XML contents of <paramref name="elem"/>
    /// </summary>
    /// <example>
    /// <code lang="C#">
    /// var xml = XElement.Load(item.CreateReader());
    /// </code>
    /// <para>Note, this approach should be more performant than first generating a string
    /// (e.g. with <see cref="ToAml(IAmlNode, AmlWriterSettings)"/> and then parsing the string)</para>
    /// </example>
    public static XmlReader CreateReader(this IReadOnlyElement elem)
    {
      return new AmlReader(elem);
    }

    /// <summary>
    /// Create an <see cref="XmlReader"/> for reading the XML contents of <paramref name="elem"/>
    /// </summary>
    /// <example>
    /// <code lang="C#">
    /// var xml = XElement.Load(result.CreateReader());
    /// </code>
    /// <para>Note, this approach should be more performant than first generating a string
    /// (e.g. with <see cref="ToAml(IAmlNode, AmlWriterSettings)"/> and then parsing the string)</para>
    /// </example>
    public static XmlReader CreateReader(this IReadOnlyResult elem)
    {
      return new AmlReader(elem);
    }

    /// <summary>
    /// Returns the total number of pages founds including those
    /// returned on other pages
    /// </summary>
    public static int PageMax(this IReadOnlyResult result)
    {
      var cntMessage = result.Message.Elements()
          .FirstOrDefault(e => e.Name == "event"
            && e.Attribute("name").Value == "pagemax"
            && e.Attribute("value").HasValue());
      if (cntMessage != null)
      {
        int cnt;
        if (int.TryParse(cntMessage.Attribute("value").Value, out cnt))
          return cnt;
      }

      return result.Items().FirstOrNullItem().Attribute("pagemax").AsInt() ?? 1;
    }

    /// <summary>
    /// Return the list of all parents of the element, <paramref name="elem"/>
    /// </summary>
    /// <returns>The first element is the direct parent of <paramref name="elem"/></returns>
    public static IEnumerable<IReadOnlyElement> Parents(this IReadOnlyElement elem)
    {
      if (!elem.Exists)
        yield break;
      var curr = elem.Parent;
      while (curr.Exists)
      {
        yield return curr;
        curr = curr.Parent;
      }
    }

    /// <summary>
    /// Return a list consisting of <paramref name="elem"/> and all of its parents
    /// </summary>
    /// <param name="elem">Element to start the search with</param>
    /// <returns>The list starts with the element (if it exists), 
    /// followed by its parent, and so on</returns>
    public static IEnumerable<IReadOnlyElement> ParentsAndSelf(this IReadOnlyElement elem)
    {
      if (!elem.Exists)
        yield break;

      yield return elem;

      var curr = elem.Parent;
      while (curr.Exists)
      {
        yield return curr;
        curr = curr.Parent;
      }
    }

    public static IEnumerable<IReadOnlyElement> DescendantsAndSelf(this IReadOnlyElement elem)
    {
      yield return elem;
      foreach (var child in Descendants<IReadOnlyElement>(elem))
        yield return child;
    }

    public static IEnumerable<IElement> DescendantsAndSelf(this IElement elem)
    {
      yield return elem;
      foreach (var child in Descendants<IElement>(elem))
        yield return child;
    }

    public static IEnumerable<IReadOnlyElement> Descendants(this IReadOnlyElement elem)
    {
      return Descendants<IReadOnlyElement>(elem);
    }

    public static IEnumerable<IElement> Descendants(this IElement elem)
    {
      return Descendants<IElement>(elem);
    }

    private static IEnumerable<T> Descendants<T>(T elem) where T : IReadOnlyElement
    {
      foreach (var child in elem.Elements().Cast<T>())
      {
        yield return child;
        foreach (var desc in Descendants(child))
        {
          yield return desc;
        }
      }
    }

#if XMLLEGACY
    /// <summary>
    /// Get an XPath navigator for executing XPath against an <see cref="IReadOnlyElement"/>
    /// </summary>
    /// <param name="elem">The <see cref="IReadOnlyElement"/> to query</param>
    /// <returns>An XPath navigator</returns>
    public static IAmlXPath XPath(this IReadOnlyElement elem)
    {
      return new AmlNavigator(elem);
    }
    /// <summary>
    /// Get an XPath navigator for executing XPath against an <see cref="IReadOnlyResult"/>
    /// </summary>
    /// <param name="elem">The <see cref="IReadOnlyResult"/> to query</param>
    /// <returns>An XPath navigator</returns>
    public static IAmlXPath XPath(this IReadOnlyResult elem)
    {
      return new AmlNavigator(elem);
    }
#endif

    /// <summary>
    /// Given a range of dynamic date offsets (e.g. 3 days ago to today) and a specific date for 
    /// "today", calculate the corresponding static date range
    /// </summary>
    public static Range<DateTime> AsDateRange(this Range<DateOffset> range, ZonedDateTime todaysDate)
    {
      return new Range<DateTime>(range.Minimum.AsDate(todaysDate), range.Maximum.AsDate(todaysDate, true));
    }

    /// <summary>
    /// Given a range of dynamic date offsets (e.g. 3 days ago to today) and a sever context giving
    /// a current date, calculate the corresponding static date range
    /// </summary>
    public static Range<DateTime> AsDateRange(this Range<DateOffset> range, IServerContext context)
    {
      return new Range<DateTime>(range.Minimum.AsDate(context.Now()), range.Maximum.AsDate(context.Now(), true));
    }

    /// <summary>
    /// Return the system time expressed in the timezone of the server
    /// </summary>
    public static ZonedDateTime Now(this IServerContext context)
    {
      return ServerContext._clock().WithZone(context.GetTimeZone());
    }

    /// <summary>
    /// Sets both the id attribute and (if necessary) the id property of the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="id">The ID.</param>
    public static void Id(this IItem item, string id)
    {
      item.Attribute("id").Set(id);
      var prop = item.IdProp();
      if (prop.Exists)
        prop.Set(id);
    }

    /// <summary>
    /// Sets both the id attribute and (if necessary) the id property of the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="id">The ID.</param>
    public static void Id(this IItem item, Guid id)
    {
      item.Attribute("id").Set(id);
      var prop = item.IdProp();
      if (prop.Exists)
        prop.Set(id);
    }

    /// <summary>
    /// Perform parameter substitutions and return the resulting AML
    /// </summary>
    public static string ToNormalizedAml(this Command cmd, ElementFactory factory)
    {
      return cmd.ToNormalizedAml(factory.LocalizationContext);
    }

    /// <summary>Returns a reference to the property with the specified name and language</summary>
    /// <param name="logical">Logical element to return a property for</param>
    /// <param name="name">Name of the property</param>
    /// <param name="lang">Language of the (multilingual) property</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>If the property exists, a valid <see cref="IReadOnlyProperty"/> will be returned</description></item>
    ///   <item><description>If the property does not exists, a "null" <see cref="IReadOnlyProperty"/> will be returned where <see cref="IReadOnlyElement.Exists"/> = <c>false</c></description></item>
    /// </list></returns>
    public static IReadOnlyProperty Property(this IReadOnlyLogical logical, string name, string lang = null)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException(name);

      if (logical.Exists)
      {
        var prop = logical.Elements()
          .OfType<IReadOnlyProperty>()
          .FirstOrDefault(p => !p.Attribute("xml:lang").Exists
            || string.IsNullOrEmpty(lang)
            || p.Attribute("xml:lang").Value == lang);

        if (prop != null)
          return prop;

        var mutable = logical as ILogical;
        if (mutable != null)
        {
          var result = new Property(mutable, name);
          if (!string.IsNullOrEmpty(lang))
            result.Add(new Attribute("xml:lang", lang));
          return result;
        }
      }

      return Innovator.Client.Property.NullProp;
    }

    /// <summary>Returns a reference to the property with the specified name and language</summary>
    /// <param name="logical">Logical element to return a property for</param>
    /// <param name="name">Name of the property</param>
    /// <param name="lang">Language of the (multilingual) property</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>If the property exists, a valid <see cref="IReadOnlyProperty"/> will be returned</description></item>
    ///   <item><description>If the property does not exists, a "null" <see cref="IReadOnlyProperty"/> will be returned where <see cref="IReadOnlyElement.Exists"/> = <c>false</c></description></item>
    /// </list></returns>
    public static IProperty Property(this ILogical logical, string name, string lang = null)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException(name);

      if (logical.Exists)
      {
        var prop = logical.Elements()
          .OfType<IProperty>()
          .FirstOrDefault(p => !p.Attribute("xml:lang").Exists
            || string.IsNullOrEmpty(lang)
            || p.Attribute("xml:lang").Value == lang);

        if (prop != null)
          return prop;

        var result = new Property(logical, name);
        if (!string.IsNullOrEmpty(lang))
          result.Add(new Attribute("xml:lang", lang));
        return result;
      }

      return Innovator.Client.Property.NullProp;
    }

    /// <summary>
    /// Gets the XML inner text of the element
    /// </summary>
    /// <param name="elem">The element to retrieve text for.</param>
    public static string InnerText(this IReadOnlyElement elem)
    {
      var val = elem.Value;
      if (val != null)
        return val;

      var builder = new StringBuilder();
      AppendInnerText(elem, builder);
      return builder.ToString();
    }

    private static void AppendInnerText(IReadOnlyElement elem, StringBuilder builder)
    {
      var val = elem.Value;
      if (val != null)
      {
        builder.Append(val);
      }
      else
      {
        foreach (var child in elem.Elements())
        {
          AppendInnerText(child, builder);
        }
      }
    }

    public static bool TryParseDateTime(this IServerContext context, object value, out ZonedDateTime? parsed)
    {
      parsed = null;
      if (value == null)
        return true;

      var zone = context.GetTimeZone();
      if (value is DateTime date)
      {
        parsed = new ZonedDateTime(date, zone);
        return true;
      }
      else if (value is DateTimeOffset dateOffset)
      {
        parsed = new ZonedDateTime(dateOffset.UtcDateTime, zone);
        return true;
      }
      else if (value is ZonedDateTime zoned)
      {
        parsed = zoned.WithZone(zone);
        return true;
      }
      else
      {
        if (!(value is string str))
          return false;
        if (str?.Length == 0)
          return true;

        if (str == "__now()")
        {
          parsed = context.Now();
          return true;
        }

        if (!ZonedDateTime.TryParse(str, zone, out var result))
          return false;

        parsed = result;
        return true;
      }
    }

    public static DateTimeZone GetTimeZone(this IServerContext context)
    {
      return (context as ServerContext)?.Zone ?? DateTimeZone.ById(context.TimeZone);
    }

    /// <summary>
    /// Sorts the specified list of objects in place.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="comparison">The comparison.</param>
    public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
    {
      if (list is List<T> generic)
      {
        generic.Sort(comparison);
      }
      else
      {
        var array = new T[list.Count];
        list.CopyTo(array, 0);
        Array.Sort(array, comparison);
        for (var i = 0; i < list.Count; i++)
          list[i] = array[i];
      }
    }
  }
}
