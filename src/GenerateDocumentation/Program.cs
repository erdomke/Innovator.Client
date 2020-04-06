using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GenerateDocumentation
{
  class Program
  {
    private static HashSet<string> _iomTypes = new HashSet<string>();

    static void Main(string[] args)
    {
      _iomTypes.UnionWith(Assembly.LoadFrom(@"D:\ANSYSDev\Reference\12.0 SP04 CD Image\Utilities\IOM\IOM.dll")
        .GetTypes()
        .Select(t => t.FullName));

      var types = new string[]
      {
        @"C:\Program Files (x86)\Aras\Innovator12sp4\Innovator\Server\bin\Aras.Server.Core.dll",
        @"C:\Program Files (x86)\Aras\Innovator12sp4\Innovator\Server\bin\Aras.TDF.Base.dll",
        @"C:\Program Files (x86)\Aras\Innovator12sp4\Innovator\Server\bin\Aras.TreeGridView.dll",
        @"C:\Program Files (x86)\Aras\Innovator12sp4\Innovator\Server\bin\Aras.Web.Configuration.dll",
        @"C:\Program Files (x86)\Aras\Innovator12sp4\Innovator\Server\bin\Conversion.Base.dll"
      }.Select(a => Assembly.LoadFrom(a))
      .SelectMany(a => a.GetTypes())
      .Where(t => t.IsPublic)
      .OrderBy(t => t.FullName, StringComparer.OrdinalIgnoreCase);

      foreach (var group in types
        .GroupBy(t => t.Namespace))
      {
        using (var writer = XmlWriter.Create("API_" + group.Key + ".html", new XmlWriterSettings()
        {
          OmitXmlDeclaration = true
        }))
        {
          writer.WriteStartElement("div");
          var assemblyName = group.First().Assembly.GetName();
          writer.WriteElementString("p", assemblyName.Name + " v" + assemblyName.Version.ToString());
          writer.WriteStartElement("ul");
          foreach (var type in group)
          {
            writer.WriteStartElement("li");
            WriteTypeRef(type, writer);
            writer.WriteEndElement();
          }
          writer.WriteEndElement();

          foreach (var type in group)
          {
            WriteHtml(type, writer, types);
          }
          writer.WriteEndElement();
        }
      }
    }

    static void WriteHtml(Type type, XmlWriter writer, IEnumerable<Type> allTypes)
    {
      writer.WriteElementString("h1", TypeTitle(type) + " " + TypeKeyword(type));

      var attributes = type.CustomAttributes
        .Where(a => a.AttributeType != typeof(DefaultMemberAttribute)
          && a.AttributeType != typeof(System.Diagnostics.CodeAnalysis.SuppressMessageAttribute))
        .OrderBy(a => a.AttributeType.Name)
        .ToList();
      var obsolete = attributes.FirstOrDefault(a => a.AttributeType == typeof(ObsoleteAttribute));
      if (obsolete?.ConstructorArguments.Count > 0)
      {
        writer.WriteStartElement("blockquote");
        writer.WriteString("⚠ ");
        writer.WriteElementString("b", "Obsolete");
        writer.WriteString(": ");
        writer.WriteString(obsolete.ConstructorArguments[0].Value?.ToString());
        writer.WriteEndElement();
      }

      var inherited = Inheritance(type);
      var first = true;
      if (inherited.Count > 1)
      {
        writer.WriteStartElement("p");
        writer.WriteString("Inheritance ");
        first = true;
        foreach (var baseType in inherited)
        {
          if (first)
            first = false;
          else
            writer.WriteString(" → ");
          WriteTypeRef(baseType, writer, baseType != type);
        }
        writer.WriteEndElement();
      }

      var derived = allTypes
        .Where(t => t.BaseType == type)
        .OrderBy(t => t.Name)
        .ToList();
      if (derived.Count > 0)
      {
        writer.WriteElementString("p", "Derived");
        writer.WriteStartElement("ul");
        foreach (var subType in derived)
        {
          writer.WriteStartElement("li");
          WriteTypeRef(subType, writer);
          writer.WriteEndElement();
        }
        writer.WriteEndElement();
      }

      if (attributes.Count > 0)
      {
        writer.WriteStartElement("p");
        writer.WriteString("Attributes ");
        first = true;
        foreach (var attribute in attributes)
        {
          if (first)
            first = false;
          else
            writer.WriteString(", ");
          WriteTypeRef(attribute.AttributeType, writer);
        }
        writer.WriteEndElement();
      }

      var implements = GetTopLevelInterfaces(type)
        .Where(i => i.IsPublic)
        .OrderBy(t => t.Name)
        .ToList();
      if (implements.Count > 0)
      {
        writer.WriteStartElement("p");
        writer.WriteString("Implements ");
        first = true;
        foreach (var inter in implements)
        {
          if (first)
            first = false;
          else
            writer.WriteString(", ");
          WriteTypeRef(inter, writer);
        }
        writer.WriteEndElement();
      }

      writer.WriteRaw("\r\n");
      writer.WriteStartElement("pre");
      WriteClassCode(type, writer, 0, attributes, implements);
      writer.WriteEndElement();
    }

    static void WriteClassCode(Type type, XmlWriter writer
      , int indent = 0
      , List<CustomAttributeData> attributes = null
      , List<Type> implements = null)
    {
      var first = true;
      WriteAttributes(attributes ?? type.CustomAttributes, writer, indent);

      var name = type.Name.Split('`')[0];
      if (type.CustomAttributes.Any() || indent > 0)
        WriteNewLine(indent, writer);
      writer.WriteString("public ");
      if (type.IsClass)
      {
        if (type.IsAbstract)
        {
          if (type.IsSealed)
            writer.WriteString("static ");
          else
            writer.WriteString("abstract ");
        }
        else if (type.IsSealed)
        {
          writer.WriteString("sealed ");
        }
      }
      writer.WriteString(TypeKeyword(type).ToLowerInvariant() + " ");
      writer.WriteElementString("b", name);
      if (type.GetGenericArguments().Length > 0)
      {
        writer.WriteString("<");
        first = true;
        foreach (var argument in type.GetGenericArguments())
        {
          if (first)
            first = false;
          else
            writer.WriteString(", ");
          writer.WriteElementString("b", argument.Name);
        }
        writer.WriteString(">");
      }

      var baseTypes = new List<Type>();
      if (!type.IsEnum)
      {
        if (type.BaseType != null && type.BaseType != typeof(object) && type.BaseType.IsPublic)
          baseTypes.Add(type.BaseType);
        implements = implements ?? GetTopLevelInterfaces(type)
          .Where(i => i.IsPublic)
          .OrderBy(t => t.Name)
          .ToList();
        baseTypes.AddRange(implements);
        if (baseTypes.Count > 0)
        {
          writer.WriteString(" : ");
          first = true;
          foreach (var baseType in baseTypes)
          {
            if (first)
              first = false;
            else
              writer.WriteString(", ");
            WriteTypeRef(baseType, writer);
          }
        }
      }

      WriteNewLine(indent, writer);
      writer.WriteString("{");

      if (type.IsEnum)
      {
        foreach (var field in type.GetFields())
        {
          if (field.FieldType.IsEnum && field.IsPublic)
          {
            WriteNewLine(indent + 2, writer);
            writer.WriteElementString("b", field.Name);
            writer.WriteString(" = ");
            WriteConstant(field.GetRawConstantValue(), writer);
            writer.WriteString(",");
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
          WriteNewLine(indent + 2, writer);
          writer.WriteElementString("i", "// Constructors");
          foreach (var constructor in constructors)
          {
            WriteAttributes(constructor.CustomAttributes, writer, indent + 2);
            WriteNewLine(indent + 2, writer);
            writer.WriteString("public ");
            writer.WriteElementString("b", name);
            writer.WriteString("(");
            WriteParameters(constructor.GetParameters(), writer);
            writer.WriteString(");");
          }
        }

        var fields = type.GetFields()
          .Where(f => f.IsPublic)
          .OrderBy(f => f.Name)
          .ToList();
        if (fields.Count > 0)
        {
          writer.WriteRaw("<br>");
          WriteNewLine(indent + 2, writer);
          writer.WriteElementString("i", "// Fields");
          foreach (var field in fields)
          {
            WriteAttributes(field.CustomAttributes, writer, indent + 2);
            WriteNewLine(indent + 2, writer);
            writer.WriteString("public ");
            if (field.IsStatic)
            {
              if (field.IsLiteral && !field.IsInitOnly)
              {
                writer.WriteString("const ");
              }
              else
              {
                writer.WriteString("static ");
                if (field.IsInitOnly)
                  writer.WriteString("readonly ");
              }
            }
            else if (field.IsInitOnly)
            {
              writer.WriteString("readonly ");
            }

            WriteTypeRef(field.FieldType, writer);
            writer.WriteString(" ");
            writer.WriteElementString("b", field.Name);

            if (field.IsLiteral)
            {
              writer.WriteString(" = ");
              WriteConstant(field.GetRawConstantValue(), writer);
            }
          }
        }

        var properties = type.GetProperties()
          .Where(p => (p.GetGetMethod().IsPublic || p.GetSetMethod().IsPublic)
            && p.DeclaringType == type)
          .OrderBy(p => p.GetIndexParameters().Length)
          .ThenBy(p => p.Name)
          .ToList();
        if (properties.Count > 0)
        {
          writer.WriteRaw("<br>");
          WriteNewLine(indent + 2, writer);
          writer.WriteElementString("i", "// Properties");
          foreach (var prop in properties)
          {
            var getter = prop.GetGetMethod();
            var setter = prop.GetSetMethod();

            WriteAttributes(prop.CustomAttributes, writer, indent + 2);
            WriteNewLine(indent + 2, writer);
            writer.WriteString("public ");
            if ((getter ?? setter).IsStatic)
            {
              writer.WriteString("static ");
            }
            if ((getter ?? setter).IsAbstract && !type.IsInterface)
            {
              writer.WriteString("abstract ");
            }
            WriteTypeRef(prop.PropertyType, writer);
            writer.WriteString(" ");
            if (prop.GetIndexParameters().Length > 0)
            {
              writer.WriteElementString("b", "this");
              writer.WriteString("[");
              WriteParameters(prop.GetIndexParameters(), writer);
              writer.WriteString("]");
            }
            else
            {
              writer.WriteElementString("b", prop.Name);
            }
            writer.WriteString(" {");
            if (getter != null)
              writer.WriteString(" get;");
            if (setter != null)
              writer.WriteString(" set;");
            writer.WriteString(" }");
          }
        }

        var methods = type.GetMethods()
          .Where(m => m.IsPublic && m.DeclaringType == type && !m.IsSpecialName)
          .OrderBy(m => m.Name)
          .ThenBy(m => m.GetParameters().Length)
          .ToList();
        if (methods.Count > 0)
        {
          writer.WriteRaw("<br>");
          WriteNewLine(indent + 2, writer);
          writer.WriteElementString("i", "// Methods");
          foreach (var method in methods)
          {
            WriteAttributes(method.CustomAttributes, writer, indent + 2);
            WriteNewLine(indent + 2, writer);
            writer.WriteString("public ");
            if (method.IsStatic)
            {
              writer.WriteString("static ");
            }
            if (method.IsAbstract && !type.IsInterface)
            {
              writer.WriteString("abstract ");
            }
            WriteTypeRef(method.ReturnType, writer);
            writer.WriteString(" ");
            writer.WriteElementString("b", method.Name);
            writer.WriteString("(");
            WriteParameters(method.GetParameters(), writer);
            writer.WriteString(");");
          }
        }

        var classes = type.GetNestedTypes()
          .Where(t => t.IsNestedPublic)
          .ToList();
        if (classes.Count > 0)
        {
          writer.WriteRaw("<br>");
          WriteNewLine(indent + 2, writer);
          writer.WriteElementString("i", "// Nested Classes");
          foreach (var c in classes)
          {
            writer.WriteRaw("<br>");
            WriteClassCode(c, writer, indent + 2);
          }
        }
      }

      WriteNewLine(indent, writer);
      writer.WriteString("}");
    }

    static void WriteNewLine(int indent, XmlWriter writer)
    {
      writer.WriteRaw("<br>");
      for (var i = 0; i < indent; i++)
        writer.WriteRaw("&nbsp;");
    }

    static void WriteAttributes(IEnumerable<CustomAttributeData> attributes, XmlWriter writer, int indent)
    {
      var filtered = attributes
        .Where(a => a.AttributeType != typeof(DefaultMemberAttribute)
          && a.AttributeType != typeof(System.Diagnostics.CodeAnalysis.SuppressMessageAttribute)
          && a.AttributeType.IsPublic)
        .OrderBy(a => a.AttributeType.Name)
        .ToList();

      foreach (var attribute in filtered)
      {
        WriteNewLine(indent, writer);
        writer.WriteString("[");
        WriteTypeRef(attribute.AttributeType, writer);
        writer.WriteString("(");
        var first = true;
        foreach (var arg in attribute.ConstructorArguments)
        {
          if (first)
            first = false;
          else
            writer.WriteString(", ");
          WriteConstant(arg.Value, writer);
        }
        foreach (var arg in attribute.NamedArguments)
        {
          if (first)
            first = false;
          else
            writer.WriteString(", ");
          writer.WriteString(arg.MemberName);
          writer.WriteString(": ");
          WriteConstant(arg.TypedValue.Value, writer);
        }
        writer.WriteString(")]");
      }
    }

    static void WriteConstant(object value, XmlWriter writer)
    {
      if (value is string str)
        writer.WriteString("@\"" + str.Replace("\"", "\"\"") + "\"");
      else if (value is bool)
        writer.WriteString(value.ToString().ToLowerInvariant());
      else
        writer.WriteString(value?.ToString());
    }

    static void WriteTypeRef(Type type, XmlWriter writer, bool generateLink = true)
    {
      if (type.IsByRef)
        type = type.GetElementType();

      var nullable = false;
      if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
      {
        type = type.GetGenericArguments()[0];
        nullable = true;
      }
      else if (type.IsGenericType && !type.IsGenericTypeDefinition)
      {
        WriteTypeRef(type.GetGenericTypeDefinition(), writer, generateLink);
        writer.WriteString("<");
        var first = true;
        foreach (var argument in type.GetGenericArguments())
        {
          if (first)
            first = false;
          else
            writer.WriteString(", ");
          WriteTypeRef(argument, writer, generateLink);
        }
        writer.WriteString(">");
        return;
      }

      var name = type.Name.Split('`')[0];
      var simpleName = new CSharpCodeProvider().GetTypeOutput(new CodeTypeReference(type));
      if (simpleName != type.FullName && !type.IsGenericTypeDefinition && !type.IsNested)
        name = simpleName;

      if (type.BaseType == typeof(Attribute) && name.EndsWith("Attribute"))
        name = name.Substring(0, name.Length - 9);

      if (nullable)
        name += "?";

      if (type.IsArray)
        type = type.GetElementType();

      if (type == typeof(void))
      {
        writer.WriteString(name);
      }
      else if (generateLink && (type.FullName?.StartsWith("System.") == true
        || type.FullName?.StartsWith("Microsoft.") == true))
      {
        var linkName = type.FullName.Replace('`', '-');
        writer.WriteStartElement("a");
        writer.WriteAttributeString("title", type.FullName);
        writer.WriteAttributeString("href", "https://docs.microsoft.com/en-us/dotnet/api/" + linkName);
        writer.WriteString(name);
        writer.WriteEndElement();
      }
      else if (generateLink && _iomTypes.Contains(type.FullName ?? ""))
      {
        writer.WriteStartElement("a");
        writer.WriteAttributeString("title", type.FullName);
        writer.WriteAttributeString("href", "https://myinnovator.com/Client/WebHelp/APIReferenceDotNet/Html/html/T_" + type.FullName.Replace('.', '_') + ".htm");
        writer.WriteString(name);
        writer.WriteEndElement();
      }
      else if (generateLink && (type.FullName?.StartsWith("Aras.") == true
        || type.FullName?.StartsWith("Enums.") == true))
      {
        var title = TypeTitle(type).Replace("<", "").Replace(">", "").Replace(",", "");
        writer.WriteStartElement("a");
        writer.WriteAttributeString("title", type.FullName);
        writer.WriteAttributeString("href", "Server-APIs-–-" + type.Namespace + "#" + title.ToLowerInvariant() + "-" + TypeKeyword(type).ToLowerInvariant());
        writer.WriteString(name);
        writer.WriteEndElement();
      }
      else
      {
        writer.WriteStartElement("b");
        writer.WriteAttributeString("title", type.FullName);
        writer.WriteString(name);
        writer.WriteEndElement();
      }
    }

    static string TypeTitle(Type type)
    {
      var name = type.Name.Split('`')[0];
      if (type.GetGenericArguments().Length > 0)
      {
        name += "<";
        var first = true;
        foreach (var argument in type.GetGenericArguments())
        {
          if (first)
            first = false;
          else
            name += ",";
          name += argument.Name;
        }
        name += ">";
      }
      return name;
    }

    static string TypeKeyword(Type type)
    {
      if (type.IsInterface)
        return "Interface";
      else if (type.IsEnum)
        return "Enum";
      else
        return "Class";
    }

    static IEnumerable<Type> GetTopLevelInterfaces(Type t)
    {
      var allInterfaces = t.GetInterfaces();
      var selection = allInterfaces
          .Where(x => !allInterfaces.Any(y => y.GetInterfaces().Contains(x)))
          .Except(t.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>());
      return selection;
    }

    static void WriteParameters(IEnumerable<ParameterInfo> parameters, XmlWriter writer)
    {
      var first = true;
      foreach (var p in parameters)
      {
        if (first)
          first = false;
        else
          writer.WriteString(", ");
        if (p.ParameterType.IsByRef)
        {
          if (p.IsOut)
            writer.WriteString("out ");
          else
            writer.WriteString("ref ");
        }
        WriteTypeRef(p.ParameterType, writer);
        writer.WriteString(" ");
        writer.WriteElementString("b", p.Name);
        if (p.HasDefaultValue)
        {
          writer.WriteString(" = ");
          WriteConstant(p.RawDefaultValue, writer);
        }
      }
    }

    static List<Type> Inheritance(Type type)
    {
      var result = new List<Type>();
      var curr = type;
      while (curr != null)
      {
        result.Add(curr);
        curr = curr.BaseType;
      }
      result.Reverse();
      return result;
    }
  }
}
