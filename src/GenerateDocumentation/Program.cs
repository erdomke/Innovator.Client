using System.Reflection;
using System.Text;
using System.Xml;

namespace GenerateDocumentation
{
  class Program
  {
    private static HashSet<string> _iomTypes = new HashSet<string>();

    static void Main(string[] args)
    {
      const string basePath = @"C:\Program Files (x86)\Aras\Innovator21\Innovator\Server\bin\";
      var paths = Directory.GetFiles(basePath, "*.dll", SearchOption.AllDirectories)
        .GroupBy(p => Path.GetFileName(p), StringComparer.OrdinalIgnoreCase)
        .Select(g => g.OrderBy(p => p.Length).First())
        .ToList();
      var resolver = new PathAssemblyResolver(paths);
      using var context = new MetadataLoadContext(resolver);

      _iomTypes.UnionWith(context.LoadFromAssemblyPath(Path.Combine(basePath, "IOM.dll"))
        .GetTypes()
        .Select(t => t.FullName));

      var types = new string[]
      {
        @"Aras.Server.Core.dll",
        @"Aras.TDF.Base.dll",
        @"Aras.TreeGridView.dll",
        @"Aras.Web.Configuration.dll",
        @"Conversion.Base.dll"
      }.Select(a => context.LoadFromAssemblyPath(Path.Combine(basePath, a)))
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
          var html = new CSharpHtmlRenderer(writer, _iomTypes, group.Key);
          foreach (var type in group)
          {
            writer.WriteStartElement("li");
            html.TypeName(type);
            writer.WriteEndElement();
          }
          writer.WriteEndElement();

          foreach (var type in group)
          {
            WriteHtml(type, writer, types, html);
          }
          writer.WriteEndElement();
        }
      }
    }

    static void WriteHtml(Type type, XmlWriter writer, IEnumerable<Type> allTypes, CSharpHtmlRenderer html)
    {
      writer.WriteElementString("h1", CSharpHtmlRenderer.TypeTitle(type) + " " + CSharpHtmlRenderer.TypeKeyword(type));

      var attributes = type.CustomAttributes
        .Where(a => a.AttributeType != typeof(DefaultMemberAttribute)
          && a.AttributeType != typeof(System.Diagnostics.CodeAnalysis.SuppressMessageAttribute))
        .OrderBy(a => a.AttributeType.Name)
        .ToList();
      var obsolete = attributes.FirstOrDefault(a => a.AttributeType.FullName == "System.ObsoleteAttribute");
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
          html.TypeName(baseType);
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
          html.TypeName(subType);
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
          html.TypeName(attribute.AttributeType);
        }
        writer.WriteEndElement();
      }

      var implements = CSharpRenderer.GetTopLevelInterfaces(type)
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
          html.TypeName(inter);
        }
        writer.WriteEndElement();
      }

      writer.WriteRaw("\r\n");
      writer.WriteStartElement("pre");
      html.TypeInterface(type, attributes, implements);
      writer.WriteEndElement();
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
