#if REFLECTION
using Innovator.Client.QueryModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;

namespace Innovator.Client.Queryable
{
  /// <summary>
  /// Adapted from https://blogs.msdn.microsoft.com/mattwar/2008/11/18/linq-building-an-iqueryable-provider-series/
  /// </summary>
  internal class InnovatorQueryProvider : QueryProvider
  {
    private readonly IConnection _conn;
    private readonly ElementFactory _factory;
    private readonly QuerySettings _settings;
    private readonly List<string> _includePaths = new List<string>();
    private Action<AmlQuery> _translateCallback;

    internal InnovatorQueryProvider(ElementFactory factory, Action<AmlQuery> translateCallback)
    {
      _factory = factory;
      _translateCallback = translateCallback;
    }

    public InnovatorQueryProvider(IConnection conn, QuerySettings settings)
    {
      _conn = conn;
      _factory = conn.AmlContext;
      _settings = settings;
    }

    public override object Execute(Expression expression)
    {
      var query = Translate(expression);
      if (_conn == null)
        return Utils.Default(expression.Type);
      else
        return ConvertResult(_conn.Apply(query.ToAml()), expression, query);
    }

    public IPromise<object> ExecuteAsync(Expression expression)
    {
      var asyncConn = _conn as IAsyncConnection;
      if (asyncConn == null)
        return Promises.Resolved(Execute(expression));

      var query = Translate(expression);
      return asyncConn.ApplyAsync(query.ToAml(), true, false)
        .Convert(result => (object)ConvertResult(result, expression, query));
    }

    private object ConvertResult(IReadOnlyResult result, Expression expression, AmlQuery query)
    {
      if (query.ResultAggregator != null)
        return query.ResultAggregator(result);

      var elementType = TypeHelper.GetElementType(expression.Type);
      var source = default(IEnumerable);
      if (query.Projection != null)
      {
        var func = query.Projection.Compile();
        source = (IEnumerable)Activator.CreateInstance(typeof(Projector<>).MakeGenericType(elementType), result, func);
      }
      else if (elementType == typeof(IReadOnlyItem))
      {
        source = new Projector<IReadOnlyItem>(result, null);
      }
      else if (elementType == typeof(IItem))
      {
        source = new Projector<IItem>(result, null);
      }
      else
      {
        source = (IEnumerable)Activator.CreateInstance(typeof(Projector<>).MakeGenericType(elementType), result, null);
      }

      if (query.SetAggregator != null)
        return query.SetAggregator(source, elementType);

      return source;
    }

    public void Include(string path)
    {
      _includePaths.Add(path);
    }

    internal AmlQuery Translate(Expression expression)
    {
      expression = Evaluator.PartialEval(expression);
      var result = new QueryTranslator(_factory).Translate(expression);
      result.Settings = _settings;

      // Add AML for force propery expansion
      if (_includePaths.Count > 0)
      {
        foreach (var path in _includePaths)
        {
          var parts = (path + "/id").Split(new char[] { '.', '/' });
          var table = result.QueryItem.GetProperty(parts).Table;
          table.Select.Add(new SelectExpression()
          {
            Expression = new AllProperties(table) { XProperties = false },
            OnlyReturnNonNull = true
          });
        }
      }

      _translateCallback?.Invoke(result);
      return result;
    }

    private class Projector<T> : IQueryResult<T>
    {
      private IReadOnlyResult _result;
      private Delegate _projection;

      public ServerException Exception { get { return _result.Exception; } }
      public IReadOnlyElement Message { get { return _result.Message; } }
      public string Value { get { return _result.Value; } }

      public Projector(IReadOnlyResult result, Func<IReadOnlyItem, T> projection)
      {
        _result = result;
        _projection = projection;
      }
      public Projector(IReadOnlyResult result, Delegate projection)
      {
        _result = result;
        _projection = projection;
      }

      public IEnumerator<T> GetEnumerator()
      {
        foreach (var item in _result.Items())
        {
          if (_projection == null)
            yield return (T)item;
          else
            yield return (T)_projection.DynamicInvoke(item);
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      public IReadOnlyItem AssertItem(string type = null)
      {
        return _result.AssertItem(type);
      }

      public IEnumerable<IReadOnlyItem> AssertItems()
      {
        return _result.AssertItems();
      }

      public IReadOnlyResult AssertNoError()
      {
        return _result.AssertNoError();
      }

      public IReadOnlyResult AssertNoError(bool ignoreNoItemsFound)
      {
        return _result.AssertNoError(ignoreNoItemsFound);
      }

      public IEnumerable<IReadOnlyItem> Items()
      {
        return _result.Items();
      }

      public void ToAml(XmlWriter writer, AmlWriterSettings settings)
      {
        _result.ToAml(writer, settings);
      }
    }
  }
}
#endif
