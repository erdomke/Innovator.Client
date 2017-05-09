#if REFLECTION
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
    private IConnection _conn;

    public InnovatorQueryProvider(IConnection conn)
    {
      _conn = conn;
    }

    public override object Execute(Expression expression)
    {
      var query = Translate(expression);
      var result = _conn.Apply(query.Aml.ToAml());
      return ConvertResult(result, expression, query);
    }

    public IPromise<object> ExecuteAsync(Expression expression)
    {
      var asyncConn = _conn as IAsyncConnection;
      if (asyncConn == null)
        return Promises.Resolved(Execute(expression));

      var query = Translate(expression);
      return asyncConn.ApplyAsync(query.Aml.ToAml(), true, false)
        .Convert(result => (object)ConvertResult(result, expression, query));
    }

    private object ConvertResult(IReadOnlyResult result, Expression expression, AmlQuery query)
    {
      var elementType = TypeHelper.GetElementType(expression.Type);
      if (query.Projection != null)
      {
        var func = query.Projection.Compile();
        return Activator.CreateInstance(typeof(Projector<>).MakeGenericType(elementType), result, func);
      }
      else if (typeof(IIndexedItem).IsAssignableFrom(elementType))
      {
        return new Projector<IIndexedItem>(result, i => new QueryableItem(i));
      }
      return new Projector<IReadOnlyItem>(result, null);
    }

    internal AmlQuery Translate(Expression expression)
    {
      expression = Evaluator.PartialEval(expression);
      return new QueryTranslator(_conn.AmlContext).Translate(expression);
    }

    private class Projector<T> : IReadOnlyResult<T>
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