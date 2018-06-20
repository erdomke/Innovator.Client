#if REFLECTION
using Innovator.Client.Model;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Innovator.Client.Queryable
{
  internal class AmlVisitor : ExpressionVisitor
  {
    protected bool VisitAmlMethod(MethodCallExpression m)
    {
      if (m.Method.DeclaringType == typeof(IReadOnlyItem) && m.Method.Name == "Property")
      {
        var arg = _paramStack.TrySimplify(m.Arguments[0]);
        var propName = ((ConstantExpression)arg).Value.ToString().ToLowerInvariant();
        VisitProperty(m.Object, propName);
        return true;
      }
      else if (m.Method.DeclaringType == typeof(IReadOnlyElement) && m.Method.Name == "Attribute")
      {
        var arg = _paramStack.TrySimplify(m.Arguments[0]);
        var propName = ((ConstantExpression)arg).Value.ToString().ToLowerInvariant();
        if (propName != "id")
          throw new NotSupportedException(string.Format("Unsupported attribute '{0}'", propName));
        VisitProperty(m.Object, "id");
        return true;
      }
      else if (m.Method.DeclaringType == typeof(IItemRef) && m.Method.Name == "Id")
      {
        VisitProperty(m.Object, "id");
        return true;
      }
      else if (m.Method.DeclaringType.IsGenericType && m.Method.DeclaringType.GetGenericTypeDefinition() == typeof(IReadOnlyProperty_Item<>)
        && (m.Method.Name == "AsItem" || m.Method.Name == "AsModel"))
      {
        VisitItem(m.Object);
        return true;
      }
      else if (m.Method.DeclaringType == typeof(Core))
      {
        switch (m.Method.Name)
        {
          case "RelatedItem":
            VisitItem(VisitProperty(m.Arguments[0], "related_id"));
            break;
          case "SourceItem":
            VisitItem(VisitProperty(m.Arguments[0], "source_id"));
            break;
          default:
            var nameAttr = m.Method.GetCustomAttributes(false).OfType<ArasNameAttribute>().FirstOrDefault();
            if (nameAttr != null)
            {
              VisitProperty(m.Object ?? m.Arguments[0], nameAttr.Name);
              return true;
            }
            throw new NotSupportedException();
        }
        return true;
      }
      else
      {
        var nameAttr = m.Method.GetCustomAttributes(false).OfType<ArasNameAttribute>().FirstOrDefault();
        if (nameAttr != null)
        {
          VisitProperty(m.Object ?? m.Arguments[0], nameAttr.Name);
          return true;
        }
      }

      return false;
    }

    protected virtual object VisitProperty(Expression table, string name)
    {
      Visit(table);
      return null;
    }

    protected virtual void VisitItem(object property)
    {
      if (property is Expression expr)
        Visit(expr);
    }
  }
}
#endif
