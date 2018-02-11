using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client
{
  /// <summary>
  /// Represents a tree data structure used to hold the class structure
  /// of an ItemType or an xClassificationTree
  /// </summary>
  /// <seealso cref="Innovator.Client.ClassNode" />
  public class ClassStructure : ClassNode
  {
    private readonly Dictionary<Guid, ClassNode> _byId = new Dictionary<Guid, ClassNode>();

    /// <summary>
    /// Gets the <see cref="ClassNode"/> with the specified identifier
    /// (either the GUID id or the path of the node).
    /// </summary>
    /// <value>
    /// The <see cref="ClassNode"/>.
    /// </value>
    /// <param name="id">The identifier (GUID or path).</param>
    /// <returns>The <see cref="ClassNode"/> if found. Otherwise, <c>null</c>.</returns>
    public ClassNode this[string id]
    {
      get
      {
        if (id.IsGuid())
          return this[new Guid(id)];
        var norm = id.Trim().Trim('/');
        if (string.IsNullOrEmpty(norm) || norm == "*")
          return this;
        var path = norm.Split('/');
        return FindChild(path, 0);
      }
    }

    /// <summary>
    /// Gets the <see cref="ClassNode"/> with the specified ID.
    /// </summary>
    /// <value>
    /// The <see cref="ClassNode"/>.
    /// </value>
    /// <param name="id">The ID.</param>
    /// <returns>The <see cref="ClassNode"/> if found. Otherwise, <c>null</c>.</returns>
    public ClassNode this[Guid id]
    {
      get
      {
        ClassNode result;
        if (_byId.TryGetValue(id, out result))
          return result;
        return default(ClassNode);
      }
    }

    /// <summary>
    /// Gets a value indicating whether items should be assigned to
    /// leaf classes only.
    /// </summary>
    /// <value>
    ///   <c>true</c> if items are assigned to leaf class only;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool LeafClassOnly { get; private set; } = true;
    /// <summary>
    /// Gets a value indicating whether items should be assigned to a
    /// single class only.
    /// </summary>
    /// <value>
    ///   <c>true</c> if items are assigned to a single class
    /// only; otherwise, <c>false</c>.
    /// </value>
    public bool SingleClassOnly { get; private set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassStructure"/> class.
    /// </summary>
    /// <param name="aml">The AML.</param>
    public ClassStructure(string aml)
    {
      using (var xml = XmlReader.Create(new StringReader(aml)))
      {
        Init(xml);
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassStructure"/> class.
    /// </summary>
    /// <param name="stream">The stream containing AML.</param>
    public ClassStructure(Stream stream)
    {
      using (var xml = XmlReader.Create(stream))
      {
        Init(xml);
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassStructure"/> class.
    /// </summary>
    /// <param name="reader">The reader containing AML.</param>
    public ClassStructure(TextReader reader)
    {
      using (var xml = XmlReader.Create(reader))
      {
        Init(xml);
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassStructure"/> class.
    /// </summary>
    /// <param name="reader">The reader containing AML.</param>
    public ClassStructure(XmlReader reader)
    {
      Init(reader);
    }

    /// <summary>
    /// Gets all the descendant nodes of the current node.
    /// </summary>
    public override IEnumerable<ClassNode> Descendants()
    {
      return _byId
        .Where(k => k.Key == k.Value.Id && k.Value.Id != Id)
        .Select(k => k.Value);
    }

    /// <summary>
    /// Gets all the descendant nodes of the current node as
    /// well as the current node.
    /// </summary>
    public override IEnumerable<ClassNode> DescendantsAndSelf()
    {
      return new ClassNode[] { this }
        .Concat(Descendants());
    }

    private void Init(XmlReader reader)
    {
      while (reader.Read())
      {
        if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Item")
        {
          switch (reader.GetAttribute("type"))
          {
            case "ItemType":
              InitItemType(reader);
              return;
            case "xClassificationTree":
              InitXClass(reader);
              return;
            default:
              throw new InvalidOperationException("Cannot get classification from Item of type `" + reader.GetAttribute("type") + "`");
          }
        }
        else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "class")
        {
          InitClassStruct(reader);
          return;
        }
        else if (reader.NodeType == XmlNodeType.Element
          && reader.LocalName == "Fault"
          && string.Equals(reader.NamespaceURI, "http://schemas.xmlsoap.org/soap/envelope/", StringComparison.OrdinalIgnoreCase))
        {
          var writer = new ResultWriter(ElementFactory.Local, null, null);
          writer.WriteStartElement("SOAP-ENV", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
          writer.WriteAttributeString("xmlns", "SOAP-ENV", null, "http://schemas.xmlsoap.org/soap/envelope/");
          writer.WriteStartElement("SOAP-ENV", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
          writer.WriteNode(reader, false);
          writer.WriteEndElement();
          writer.WriteEndElement();
          throw writer.Result.Exception;
        }
      }
      throw new InvalidOperationException("Unable to get classification from XML");
    }

    private void InitItemType(XmlReader reader)
    {
      var lastElem = default(string);
      while (reader.Read())
      {
        switch (reader.NodeType)
        {
          case XmlNodeType.Element:
            lastElem = reader.LocalName;
            break;
          case XmlNodeType.CDATA:
          case XmlNodeType.Text:
            if (lastElem == "class_structure")
            {
              using (var r = new StringReader(reader.Value))
              using (var xml = XmlReader.Create(r))
              {
                InitClassStruct(xml);
              }
            }
            break;
        }
      }
    }

    private void InitClassStruct(XmlReader reader)
    {
      if (reader.NodeType != XmlNodeType.Element)
        reader.MoveToContent();

      Id = new Guid(reader.GetAttribute("id"));
      _byId[Id] = this;
      var stack = new Stack<ClassNode>();
      stack.Push(this);
      while (reader.Read())
      {
        switch (reader.NodeType)
        {
          case XmlNodeType.Element:
            var newClass = new ClassNode()
            {
              Name = reader.GetAttribute("name"),
              Label = reader.GetAttribute("name"),
              Id = new Guid(reader.GetAttribute("id"))
            };
            stack.Peek().Add(newClass);
            _byId[newClass.Id] = newClass;
            if (!reader.IsEmptyElement)
              stack.Push(newClass);
            break;
          case XmlNodeType.EndElement:
            stack.Pop();
            break;
        }
      }
    }

    private void InitXClass(XmlReader reader)
    {
      var lastElem = default(string);
      var hierarchyJson = default(LightJson.JsonArray);
      var currNode = default(ClassNode);

      while (reader.Read())
      {
        switch (reader.NodeType)
        {
          case XmlNodeType.Element:
            if (reader.LocalName == "Item" && reader.GetAttribute("type") == "xClass")
            {
              currNode = new ClassNode()
              {
                Id = new Guid(reader.GetAttribute("id"))
              };
              _byId[currNode.Id] = currNode;
            }
            else
            {
              lastElem = reader.LocalName;
            }
            break;
          case XmlNodeType.CDATA:
          case XmlNodeType.Text:
            if (currNode == null)
            {
              switch (lastElem)
              {
                case "classification_hierarchy":
                  hierarchyJson = LightJson.JsonValue.Parse(reader.Value).AsJsonArray;
                  break;
                case "id":
                  Id = new Guid(reader.Value);
                  break;
                case "name":
                  Name = reader.Value;
                  break;
                case "select_only_leaf_class":
                  LeafClassOnly = reader.Value == "1";
                  break;
                case "select_only_single_class":
                  SingleClassOnly = reader.Value == "1";
                  break;
              }
            }
            else
            {
              switch (lastElem)
              {
                case "name":
                  currNode.Name = reader.Value;
                  break;
                case "label":
                  currNode.Label = reader.Value;
                  break;
                case "ref_id":
                  var refId = new Guid(reader.Value);
                  if (refId == this.Id)
                  {
                    this.Name = currNode.Name;
                    this.Label = currNode.Label;
                    _byId[currNode.Id] = this;
                    _byId[this.Id] = this;
                  }
                  else
                  {
                    _byId[refId] = currNode;
                  }
                  break;
                case "xproperties_sort_order":
                  currNode.PropertySortOrder = reader.Value;
                  break;
              }
            }
            break;
        }
      }

      foreach (var elem in hierarchyJson)
      {
        if (elem["fromRefId"] != LightJson.JsonValue.Null)
        {
          _byId[new Guid(elem["fromRefId"].AsString)].Add(_byId[new Guid(elem["toRefId"].AsString)]);
        }
      }
    }
  }
}
