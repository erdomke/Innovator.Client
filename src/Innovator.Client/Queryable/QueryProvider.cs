#if REFLECTION
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Innovator.Client.Queryable
{
  /// <summary>
  /// A basic abstract LINQ query provider
  /// </summary>
  internal abstract class QueryProvider : IQueryProvider
  {
    private static Dictionary<Type, Func<QueryProvider, Expression, IQueryable>> _constructorCache
      = new Dictionary<Type, Func<QueryProvider, Expression, IQueryable>>();

    protected QueryProvider()
    {
    }

    IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
    {
      return new InnovatorQuery<S>(this, expression);
    }

    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
      Type elementType = TypeHelper.GetElementType(expression.Type);
      try
      {
        var lambda = default(Func<QueryProvider, Expression, IQueryable>);
        if (!_constructorCache.TryGetValue(elementType, out lambda))
        {
          var constructor = typeof(InnovatorQuery<>).MakeGenericType(elementType)
            .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(QueryProvider), typeof(Expression) }, null);
          var paramProvider = Expression.Parameter(typeof(QueryProvider), "p");
          var exprProvider = Expression.Parameter(typeof(Expression), "e");
          lambda = Expression.Lambda<Func<QueryProvider, Expression, IQueryable>>(
            Expression.New(constructor, paramProvider, exprProvider), paramProvider, exprProvider
          ).Compile();
          _constructorCache[elementType] = lambda;
        }

        return lambda(this, expression);
      }
      catch (TargetInvocationException tie)
      {
        throw tie.InnerException;
      }
    }

    S IQueryProvider.Execute<S>(Expression expression)
    {
      return (S)this.Execute(expression);
    }

    object IQueryProvider.Execute(Expression expression)
    {
      return this.Execute(expression);
    }

    public abstract object Execute(Expression expression);
  }
}
#endif
