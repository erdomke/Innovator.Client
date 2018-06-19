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
          case "Classification":
            VisitProperty(m.Arguments[0], "classification");
            break;
          case "ConfigId":
            VisitProperty(m.Arguments[0], "config_id");
            break;
          case "CreatedById":
            VisitProperty(m.Arguments[0], "created_by_id");
            break;
          case "CreatedOn":
            VisitProperty(m.Arguments[0], "created_on");
            break;
          case "Css":
            VisitProperty(m.Arguments[0], "css");
            break;
          case "CurrentState":
            VisitProperty(m.Arguments[0], "current_state");
            break;
          case "Generation":
            VisitProperty(m.Arguments[0], "generation");
            break;
          case "IdProp":
            VisitProperty(m.Arguments[0], "id");
            break;
          case "IsCurrent":
            VisitProperty(m.Arguments[0], "is_current");
            break;
          case "IsReleased":
            VisitProperty(m.Arguments[0], "is_released");
            break;
          case "KeyedName":
            VisitProperty(m.Arguments[0], "keyed_name");
            break;
          case "LockedById":
            VisitProperty(m.Arguments[0], "locked_by_id");
            break;
          case "MajorRev":
            VisitProperty(m.Arguments[0], "major_rev");
            break;
          case "ManagedById":
            VisitProperty(m.Arguments[0], "managed_by_id");
            break;
          case "MinorRev":
            VisitProperty(m.Arguments[0], "minor_rev");
            break;
          case "ModifiedById":
            VisitProperty(m.Arguments[0], "modified_by_id");
            break;
          case "ModifiedOn":
            VisitProperty(m.Arguments[0], "modified_on");
            break;
          case "NewVersion":
            VisitProperty(m.Arguments[0], "new_version");
            break;
          case "NotLockable":
            VisitProperty(m.Arguments[0], "not_lockable");
            break;
          case "OwnedById":
            VisitProperty(m.Arguments[0], "owned_by_id");
            break;
          case "PermissionId":
            VisitProperty(m.Arguments[0], "permission_id");
            break;
          case "RelatedId":
            VisitProperty(m.Arguments[0], "related_id");
            break;
          case "RelatedItem":
            VisitItem(VisitProperty(m.Arguments[0], "related_id"));
            break;
          case "State":
            VisitProperty(m.Arguments[0], "state");
            break;
          case "SourceId":
            VisitProperty(m.Arguments[0], "source_id");
            break;
          case "SourceItem":
            VisitItem(VisitProperty(m.Arguments[0], "source_id"));
            break;
          case "TeamId":
            VisitProperty(m.Arguments[0], "team_id");
            break;
          default:
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
