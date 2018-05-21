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
  /// A default implementation of IQueryable for use with QueryProvider
  /// </summary>
  public class InnovatorQuery<T> : IQueryable<T>, IInnovatorQuery, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
  {
    private Expression _expression;
    private IQueryProvider _provider;
    private IQueryResult<T> _result;

    public Expression Expression { get { return this._expression; } }
    public Type ElementType { get { return typeof(T); } }
    public IQueryProvider Provider { get { return this._provider; } }
    public string ItemType { get; }

    private IQueryResult<T> Result
    {
      get
      {
        if (_result == null)
          _result = (IQueryResult<T>)this._provider.Execute(this._expression);
        return _result;
      }
    }

    public InnovatorQuery(IQueryProvider provider, string itemType) : this(provider, default(Type))
    {
      this.ItemType = itemType;
    }

    internal InnovatorQuery(IQueryProvider provider, Type staticType)
    {
      if (provider == null)
      {
        throw new ArgumentNullException("Provider");
      }
      this._provider = provider;
      this._expression = staticType != null ? Expression.Constant(this, staticType) : Expression.Constant(this);
    }

    internal InnovatorQuery(QueryProvider provider, Expression expression)
    {
      if (provider == null)
      {
        throw new ArgumentNullException("Provider");
      }
      if (expression == null)
      {
        throw new ArgumentNullException("expression");
      }
      if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
      {
        throw new ArgumentOutOfRangeException("expression");
      }
      this._provider = provider;
      this._expression = expression;
    }

    public IQueryResult<T> Apply()
    {
      return Result;
    }

    public IPromise<IQueryResult<T>> ApplyAsync()
    {
      var innProvider = _provider as InnovatorQueryProvider;
      if (_result != null || innProvider == null)
        return Promises.Resolved(Result);

      return innProvider.ExecuteAsync(this._expression)
        .Convert(o =>
        {
          _result = _result ?? (IQueryResult<T>)o;
          return _result;
        });
    }

    public InnovatorQuery<T> Include(string path)
    {
      var innProvider = _provider as InnovatorQueryProvider;
      if (innProvider != null)
        innProvider.Include(path);
      return this;
    }

    public IEnumerator<T> GetEnumerator()
    {
      return Result.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public override string ToString()
    {
      return this.ToAml();
    }

    public void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      var innProvider = (InnovatorQueryProvider)_provider;
      var query = innProvider.Translate(this._expression);
      query.ToAml(writer, settings);
    }

    public QueryItem ToQueryItem()
    {
      var innProvider = (InnovatorQueryProvider)_provider;
      var query = innProvider.Translate(this._expression);
      return query.QueryItem;
    }

    IQueryResult IInnovatorQuery.Apply()
    {
      return Apply();
    }

    IPromise<IQueryResult> IInnovatorQuery.ApplyAsync()
    {
      return ApplyAsync().Convert(r => (IQueryResult)r);
    }

    IInnovatorQuery IInnovatorQuery.Include(string path)
    {
      return Include(path);
    }
  }
}
#endif
