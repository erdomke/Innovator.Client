#if REFLECTION
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using Innovator.Client.Model;

namespace Innovator.Client.Queryable
{
  internal class AmlVisitor : ExpressionVisitor
  {
    protected bool VisitAmlMethod(MethodCallExpression m)
    {
      if ((m.Method.DeclaringType == typeof(IReadOnlyItem) && m.Method.Name == "Property"))
      {
        Visit(m.Object);
        var arg = _paramStack.TrySimplify(m.Arguments[0]);
        var propName = ((ConstantExpression)arg).Value.ToString().ToLowerInvariant();
        VisitProperty(propName);
        return true;
      }
      else if (m.Method.DeclaringType == typeof(IReadOnlyElement) && m.Method.Name == "Attribute")
      {
        Visit(m.Object);
        var arg = _paramStack.TrySimplify(m.Arguments[0]);
        var propName = ((ConstantExpression)arg).Value.ToString().ToLowerInvariant();
        if (propName != "id")
          throw new NotSupportedException(string.Format("Unsupported attribute '{0}'", propName));
        VisitProperty("id");
        return true;
      }
      else if (m.Method.DeclaringType == typeof(IItemRef) && m.Method.Name == "Id")
      {
        Visit(m.Object);
        VisitProperty("id");
        return true;
      }
      else if (m.Method.DeclaringType.IsGenericType && m.Method.DeclaringType.GetGenericTypeDefinition() == typeof(IReadOnlyProperty_Item<>)
        && (m.Method.Name == "AsItem" || m.Method.Name == "AsModel"))
      {
        Visit(m.Object);
        VisitItem();
        return true;
      }
      else if (m.Method.DeclaringType == typeof(Core))
      {
        Visit(m.Arguments.First());
        switch (m.Method.Name)
        {
          case "Classification":
            VisitProperty("classification");
            break;
          case "ConfigId":
            VisitProperty("config_id");
            break;
          case "CreatedById":
            VisitProperty("created_by_id");
            break;
          case "CreatedOn":
            VisitProperty("created_on");
            break;
          case "Css":
            VisitProperty("css");
            break;
          case "CurrentState":
            VisitProperty("current_state");
            break;
          case "Generation":
            VisitProperty("generation");
            break;
          case "IdProp":
            VisitProperty("id");
            break;
          case "IsCurrent":
            VisitProperty("is_current");
            break;
          case "IsReleased":
            VisitProperty("is_released");
            break;
          case "KeyedName":
            VisitProperty("keyed_name");
            break;
          case "LockedById":
            VisitProperty("locked_by_id");
            break;
          case "MajorRev":
            VisitProperty("major_rev");
            break;
          case "ManagedById":
            VisitProperty("managed_by_id");
            break;
          case "MinorRev":
            VisitProperty("minor_rev");
            break;
          case "ModifiedById":
            VisitProperty("modified_by_id");
            break;
          case "ModifiedOn":
            VisitProperty("modified_on");
            break;
          case "NewVersion":
            VisitProperty("new_version");
            break;
          case "NotLockable":
            VisitProperty("not_lockable");
            break;
          case "OwnedById":
            VisitProperty("owned_by_id");
            break;
          case "PermissionId":
            VisitProperty("permission_id");
            break;
          case "RelatedId":
            VisitProperty("related_id");
            break;
          case "RelatedItem":
            VisitProperty("related_id");
            VisitItem();
            break;
          case "State":
            VisitProperty("state");
            break;
          case "SourceId":
            VisitProperty("source_id");
            break;
          case "SourceItem":
            VisitProperty("source_id");
            VisitItem();
            break;
          case "TeamId":
            VisitProperty("team_id");
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
          VisitProperty(nameAttr.Name);
          return true;
        }
      }

      return false;
    }

    protected virtual void VisitProperty(string name) { }
    protected virtual void VisitItem() { }
  }
}
#endif
