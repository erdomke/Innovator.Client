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
  /// A default implementation of IQueryable for use with QueryProvider
  /// </summary>
  internal class InnovatorQuery<T> : IQueryable<T>, IInnovatorQuery, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable, IAmlNode
  {
    Expression _expression;
    IQueryProvider _provider;
    IReadOnlyResult<T> _result;

    public Expression Expression { get { return this._expression; } }
    public Type ElementType { get { return typeof(T); } }
    public IQueryProvider Provider { get { return this._provider; } }
    public IReadOnlyResult<T> Result
    {
      get
      {
        if (_result == null)
          _result = (IReadOnlyResult<T>)this._provider.Execute(this._expression);
        return _result;
      }
    }
    public string Type { get; set; }

    public InnovatorQuery(IQueryProvider provider) : this(provider, null) { }

    public InnovatorQuery(IQueryProvider provider, Type staticType)
    {
      if (provider == null)
      {
        throw new ArgumentNullException("Provider");
      }
      this._provider = provider;
      this._expression = staticType != null ? Expression.Constant(this, staticType) : Expression.Constant(this);
    }

    public InnovatorQuery(QueryProvider provider, Expression expression)
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

    public IPromise<IReadOnlyResult<T>> GetResultAsync()
    {
      var innProvider = _provider as InnovatorQueryProvider;
      if (_result != null || innProvider == null)
        return Promises.Resolved(Result);

      return innProvider.ExecuteAsync(this._expression)
        .Convert(o =>
        {
          _result = _result ?? (IReadOnlyResult<T>)o;
          return _result;
        });
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
      query.Aml.ToAml(writer, settings);
    }
  }
}
#endif