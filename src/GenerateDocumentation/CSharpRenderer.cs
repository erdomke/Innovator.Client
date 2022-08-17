using System.Reflection;

namespace GenerateDocumentation
{
  internal abstract class CSharpRenderer
  {
    private int _indent;

    public int Indent => _indent;

    protected virtual void WriteComment(string comment)
    {
      WriteRaw(comment);
    }
    protected virtual void WriteConstant(object value)
    {
      if (value == null)
        WriteKeyword("null");
      else if (value is bool boolean)
        WriteKeyword(boolean ? "true" : "false");
      else if (value is string str)
        WriteStringLiteral(str);
      else
        WriteRaw(value.ToString());
    }
    protected virtual void WriteKeyword(string value)
    {
      WriteRaw(value);
    }
    protected virtual void WriteMemberName(string value)
    {
      WriteRaw(value);
    }
    protected virtual void WriteNewLine()
    {
      WriteRaw("\r\n");
    }
    protected virtual void WriteOperator(string value)
    {
      WriteRaw(value);
    }
    protected virtual void WriteParameterName(string value)
    {
      WriteRaw(value);
    }
    protected abstract void WriteRaw(string value);
    protected virtual void WriteRaw(char value)
    {
      WriteRaw(value.ToString());
    }
    protected virtual void WriteStringLiteral(string value)
    {
      WriteRaw('"');
      for (var i = 0; i < value.Length; i++)
      {
        switch (value[i])
        {
          case '\\':
            WriteRaw(@"\\");
            break;
          case '\a':
            WriteRaw(@"\a");
            break;
          case '\b':
            WriteRaw(@"\b");
            break;
          case '\f':
            WriteRaw(@"\f");
            break;
          case '\n':
            WriteRaw(@"\n");
            break;
          case '\r':
            WriteRaw(@"\r");
            break;
          case '\t':
            WriteRaw(@"\t");
            break;
          case '\v':
            WriteRaw(@"\v");
            break;
          default:
            if (value[i] == '"')
              WriteRaw('\\');
            WriteRaw(value[i]);
            break;
        }
      }
      WriteRaw('"');
    }
    protected virtual void WriteTypeName(string name, Type type, bool isKeyword)
    {
      if (isKeyword)
        WriteKeyword(name);
      else
        WriteRaw(name);
    }

    public CSharpRenderer TypeInterface(Type type, IEnumerable<CustomAttributeData> attributes = null, IEnumerable<Type> implements = null)
    {
      WriteAttributes(attributes ?? type.CustomAttributes);
      if (type.IsPublic)
        WriteKeyword("public ");
      else
        WriteKeyword("private ");
      if (type.IsClass)
      {
        if (type.IsAbstract)
        {
          if (type.IsSealed)
            WriteKeyword("static ");
          else
            WriteKeyword("abstract ");
        }
        else if (type.IsSealed)
        {
          WriteKeyword("sealed ");
        }
      }

      if (type.IsInterface)
        WriteKeyword("interface ");
      else if (type.IsEnum)
        WriteKeyword("enum ");
      else
        WriteKeyword("class ");
      TypeName(type);

      var baseTypes = new List<Type>();
      if (!type.IsEnum)
      {
        if (type.BaseType != null
          && type.BaseType.FullName != "System.Object"
          && type.BaseType.IsPublic)
          baseTypes.Add(type.BaseType);
        implements = implements ?? GetTopLevelInterfaces(type)
          .Where(i => i.IsPublic)
          .OrderBy(t => t.Name)
          .ToList();
        baseTypes.AddRange(implements);
        if (baseTypes.Count > 0)
        {
          WriteOperator(" : ");
          var first = true;
          foreach (var baseType in baseTypes)
          {
            if (first)
              first = false;
            else
              WriteOperator(", ");
            TypeName(baseType);
          }
        }
      }

      WriteNewLine();
      WriteOperator("{");
      _indent += 2;

      if (type.IsEnum)
      {
        foreach (var field in type.GetFields())
        {
          if (field.FieldType.IsEnum && field.IsPublic)
          {
            WriteNewLine();
            WriteMemberName(field.Name);
            WriteOperator(" = ");
            WriteConstant(field.GetRawConstantValue());
            WriteOperator(",");
          }
        }
      }
      else
      {
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
          .OrderBy(c => c.GetParameters().Length)
          .ToList();
        if (constructors.Count > 0)
        {
          WriteNewLine();
          WriteComment("// Constructors");
          foreach (var constructor in constructors)
          {
            WriteNewLine();
            MethodDefinition(constructor);
          }
        }

        var fields = type.GetFields()
          .Where(f => f.IsPublic)
          .OrderBy(f => f.Name)
          .ToList();
        if (fields.Count > 0)
        {
          WriteNewLine();
          WriteComment("// Fields");
          foreach (var field in fields)
          {
            WriteNewLine();
            FieldDefinition(field);
          }
        }

        var properties = type.GetProperties()
          .Where(p => (p.GetGetMethod()?.IsPublic == true || p.GetSetMethod()?.IsPublic == true)
            && p.DeclaringType == type)
          .OrderBy(p => p.GetIndexParameters().Length)
          .ThenBy(p => p.Name)
          .ToList();
        if (properties.Count > 0)
        {
          WriteNewLine();
          WriteComment("// Properties");
          foreach (var property in properties)
          {
            WriteNewLine();
            PropertyDefinition(property);
          }
        }

        var methods = type.GetMethods()
          .Where(m => m.IsPublic && m.DeclaringType == type && !m.IsSpecialName)
          .OrderBy(m => m.Name)
          .ThenBy(m => m.GetParameters().Length)
          .ToList();
        if (methods.Count > 0)
        {
          WriteNewLine();
          WriteComment("// Methods");
          foreach (var method in methods)
          {
            WriteNewLine();
            MethodDefinition(method);
          }
        }

        var classes = type.GetNestedTypes()
          .Where(t => t.IsNestedPublic)
          .ToList();
        if (classes.Count > 0)
        {
          WriteNewLine();
          WriteComment("// Nested Classes");
          foreach (var c in classes)
          {
            WriteNewLine();
            TypeInterface(c);
          }
        }
      }

      _indent -= 2;
      WriteNewLine();
      WriteOperator("}");

      return this;
    }

    internal static IEnumerable<Type> GetTopLevelInterfaces(Type t)
    {
      var allInterfaces = t.GetInterfaces();
      var selection = allInterfaces
          .Where(x => !allInterfaces.Any(y => y.GetInterfaces().Contains(x)))
          .Except(t.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>());
      return selection;
    }

    public CSharpRenderer FieldDefinition(FieldInfo field)
    {
      WriteAttributes(field.CustomAttributes);
      if (field.IsPublic)
        WriteKeyword("public ");
      else if (field.IsAssembly)
        WriteKeyword("internal ");
      else if (field.IsFamily)
        WriteKeyword("protected ");
      else if (field.IsPrivate)
        WriteKeyword("private ");
      if (field.IsStatic)
      {
        if (field.IsLiteral && !field.IsInitOnly)
        {
          WriteKeyword("const ");
        }
        else
        {
          WriteKeyword("static ");
          if (field.IsInitOnly)
            WriteKeyword("readonly ");
        }
      }
      else if (field.IsInitOnly)
      {
        WriteKeyword("readonly ");
      }
      TypeName(field.FieldType);
      WriteRaw(" ");
      WriteMemberName(field.Name);

      try
      {
        if (field.IsLiteral)
        {
          WriteOperator(" = ");
          WriteConstant(field.GetRawConstantValue());
        }
      }
      catch (Exception) { } // Do nothing
      WriteOperator(";");
      return this;
    }

    public CSharpRenderer MethodDefinition(MethodBase method)
    {
      WriteAttributes(method.CustomAttributes);
      WriteMethodAccess(method);
      Signature(method, method.GetParameters());
      WriteOperator(";");
      return this;
    }

    private void WriteMethodAccess(MethodBase method)
    {
      if (method.IsPublic)
        WriteKeyword("public ");
      else if (method.IsAssembly)
        WriteKeyword("internal ");
      else if (method.IsFamily)
        WriteKeyword("protected ");
      else if (method.IsPrivate)
        WriteKeyword("private ");

      if (method is ConstructorInfo)
        return;

      if (method.IsStatic)
        WriteKeyword("static ");
      if (method.IsAbstract && !method.DeclaringType.IsInterface)
        WriteKeyword("abstract ");
      else if (method.IsVirtual && !method.IsFinal && !method.DeclaringType.IsInterface)
        WriteKeyword("virtual ");
    }

    public CSharpRenderer PropertyDefinition(PropertyInfo property)
    {
      var getter = property.GetGetMethod();
      var setter = property.GetSetMethod();
      WriteAttributes(property.CustomAttributes);
      WriteMethodAccess(getter ?? setter);
      Signature(property, property.GetIndexParameters());
      WriteOperator(" {");
      if (getter != null)
      {
        WriteKeyword(" get");
        WriteOperator(";");
      }
      if (setter != null)
      {
        WriteKeyword(" set");
        WriteOperator(";");
      }
      WriteOperator(" }");
      return this;
    }

    public CSharpRenderer Signature(System.Reflection.MemberInfo member, ParameterInfo[] parameters)
    {
      var open = "(";
      var close = ")";
      var genericArgs = default(Type[]);
      if (member is System.Reflection.MethodInfo methodInfo)
      {
        TypeName(methodInfo.ReturnType);
        if (methodInfo.IsGenericMethod)
          genericArgs = methodInfo.GetGenericArguments();
        WriteRaw(' ');
        WriteMemberName(member.Name);
      }
      else if (member is PropertyInfo propertyInfo)
      {
        open = "[";
        close = "]";
        TypeName(propertyInfo.PropertyType);
        WriteRaw(' ');
        if (parameters?.Length > 0)
        {
          WriteKeyword("this");
        }
        else
        {
          WriteMemberName(member.Name);
          parameters = null;
        }
      }
      else if (member is ConstructorInfo constructorInfo)
      {
        WriteMemberName(member.DeclaringType.Name);
      }

      if (parameters != null)
      {
        WriteOperator(open);
        WriteParameters(parameters);
        WriteOperator(close);
      }

      return this;
    }

    private void WriteParameters(IEnumerable<ParameterInfo> parameters)
    {
      var first = true;
      foreach (var p in parameters.Where(p => !(p.ParameterType.Name == "Closure" && p.ParameterType.Namespace == "System.Runtime.CompilerServices")))
      {
        if (first)
          first = false;
        else
          WriteRaw(", ");
        if (p.ParameterType.IsByRef)
        {
          if (p.IsOut)
          {
            WriteKeyword("out");
            WriteRaw(" ");
          }
          else
          {
            WriteKeyword("ref");
            WriteRaw(" ");
          }
        }
        TypeName(p.ParameterType);
        WriteRaw(" ");
        WriteParameterName(p.Name ?? "?");
        try
        {
          if (p.HasDefaultValue)
          {
            WriteOperator(" = ");
            WriteConstant(p.RawDefaultValue);
          }
        }
        catch (Exception) { } // Do nothing
      }
    }

    private void WriteAttributes(IEnumerable<CustomAttributeData> attributes)
    {
      var filtered = attributes
        .Where(a => a.AttributeType != typeof(DefaultMemberAttribute)
          && a.AttributeType != typeof(System.Diagnostics.CodeAnalysis.SuppressMessageAttribute)
          && a.AttributeType.IsPublic)
        .OrderBy(a => a.AttributeType.Name)
        .ToList();

      foreach (var attribute in filtered)
      {
        WriteOperator("[");
        var type = attribute.AttributeType;
        if (type.BaseType?.FullName == "System.Attribute" && type.Name.EndsWith("Attribute"))
          WriteTypeName(type.Name.Substring(0, type.Name.Length - 9), type, false);
        else
          TypeName(type);
        WriteOperator("(");
        var first = true;
        foreach (var arg in attribute.ConstructorArguments)
        {
          if (first)
            first = false;
          else
            WriteOperator(", ");
          WriteConstant(arg.Value);
        }
        foreach (var arg in attribute.NamedArguments)
        {
          if (first)
            first = false;
          else
            WriteOperator(", ");
          WriteParameterName(arg.MemberName);
          WriteOperator(": ");
          WriteConstant(arg.TypedValue.Value);
        }
        WriteOperator(")]");
        WriteNewLine();
      }
    }

    public CSharpRenderer TypeName(Type type)
    {
      if (type.IsByRef)
      {
        TypeName(type.GetElementType());
      }
      else if (type.IsGenericType)
      {
        if (type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
          && TryGetTypeKeword(type, out var shortName))
        {
          WriteTypeName(shortName + "?", type, true);
        }
        else
        {
          WriteTypeName(type.Name.Split('`')[0], type.GetGenericTypeDefinition(), false);
          WriteOperator("<");
          var first = true;
          foreach (var arg in type.GetGenericArguments())
          {
            if (first)
              first = false;
            else
              WriteRaw(", ");
            TypeName(arg);
          }
          WriteOperator(">");
        }
      }
      else if (type.IsArray)
      {
        TypeName(type.GetElementType());
        WriteOperator("[]");
      }
      else
      {
        if (TryGetTypeKeword(type, out var shortName))
          WriteTypeName(shortName, type, true);
        else
          WriteTypeName(type.Name, type, false);
      }

      return this;
    }

    private static bool TryGetTypeKeword(Type type, out string shortName)
    {
      switch (type.FullName)
      {
        case "System.Boolean": shortName = "bool"; return true;
        case "System.Byte": shortName = "byte"; return true;
        case "System.SByte": shortName = "sbyte"; return true;
        case "System.Char": shortName = "char"; return true;
        case "System.Decimal": shortName = "decimal"; return true;
        case "System.Double": shortName = "double"; return true;
        case "System.Single": shortName = "float"; return true;
        case "System.Int32": shortName = "int"; return true;
        case "System.UInt32": shortName = "uint"; return true;
        case "System.IntPtr": shortName = "nint"; return true;
        case "System.UIntPtr": shortName = "nuint"; return true;
        case "System.Int64": shortName = "long"; return true;
        case "System.UInt64": shortName = "ulong"; return true;
        case "System.Int16": shortName = "short"; return true;
        case "System.UInt16": shortName = "ushort"; return true;
        case "System.Object": shortName = "object"; return true;
        case "System.String": shortName = "string"; return true;
        case "System.Void": shortName = "void"; return true;
        default: shortName = null; return false;
      }
    }
  }
}
