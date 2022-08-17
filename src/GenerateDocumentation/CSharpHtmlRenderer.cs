using System.Xml;

namespace GenerateDocumentation
{
  internal class CSharpHtmlRenderer : CSharpRenderer
  {
    private readonly XmlWriter _writer;
    private readonly HashSet<string> _iomTypes = new HashSet<string>();
    private readonly string _namespace;

    public CSharpHtmlRenderer(XmlWriter writer, HashSet<string> iomTypes, string ns)
    {
      _writer = writer;
      _iomTypes = iomTypes;
      _namespace = ns;
    }

    protected override void WriteComment(string comment)
    {
      _writer.WriteElementString("i", comment);
    }

    protected override void WriteMemberName(string value)
    {
      _writer.WriteElementString("b", value);
    }

    protected override void WriteParameterName(string value)
    {
      _writer.WriteElementString("b", value);
    }

    protected override void WriteNewLine()
    {
      _writer.WriteRaw("<br>");
      for (var i = 0; i < Indent; i++)
        _writer.WriteRaw("&nbsp;");
    }

    protected override void WriteRaw(string value)
    {
      _writer.WriteString(value);
    }

    protected override void WriteTypeName(string name, Type type, bool isKeyword)
    {
      if (isKeyword)
      {
        WriteKeyword(name);
      }
      else if (type.FullName?.StartsWith("System.") == true
        || type.FullName?.StartsWith("Microsoft.") == true)
      {
        var linkName = type.FullName.Replace('`', '-').Split('[')[0];
        _writer.WriteStartElement("a");
        _writer.WriteAttributeString("title", type.FullName);
        _writer.WriteAttributeString("href", "https://docs.microsoft.com/en-us/dotnet/api/" + linkName);
        _writer.WriteString(name);
        _writer.WriteEndElement();
      }
      else if (_iomTypes.Contains(type.FullName ?? ""))
      {
        _writer.WriteStartElement("a");
        _writer.WriteAttributeString("title", type.FullName);
        _writer.WriteAttributeString("href", "https://myinnovator.com/Client/WebHelp/APIReferenceDotNet/Html/html/T_" + type.FullName.Replace('.', '_') + ".htm");
        _writer.WriteString(name);
        _writer.WriteEndElement();
      }
      else if (type.FullName?.StartsWith("Aras.") == true
        || type.FullName?.StartsWith("Enums.") == true)
      {
        var title = TypeTitle(type).Replace("<", "").Replace(">", "").Replace(",", "");
        _writer.WriteStartElement("a");
        _writer.WriteAttributeString("title", type.FullName);
        if (_namespace == type.Namespace)
          _writer.WriteAttributeString("href", "#" + title.ToLowerInvariant() + "-" + TypeKeyword(type).ToLowerInvariant());
        else
          _writer.WriteAttributeString("href", "Server-APIs-–-" + type.Namespace + "#" + title.ToLowerInvariant() + "-" + TypeKeyword(type).ToLowerInvariant());
        _writer.WriteString(name);
        _writer.WriteEndElement();
      }
      else
      {
        _writer.WriteStartElement("b");
        _writer.WriteAttributeString("title", type.FullName);
        _writer.WriteString(name);
        _writer.WriteEndElement();
      }
    }

    internal static string TypeKeyword(Type type)
    {
      if (type.IsInterface)
        return "Interface";
      else if (type.IsEnum)
        return "Enum";
      else
        return "Class";
    }

    internal static string TypeTitle(Type type)
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
  }
}
