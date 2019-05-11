using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client
{
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public class TocNode : IItemRef
  {
    private List<IItemRef> _references = new List<IItemRef>();
    private List<TocNode> _children = new List<TocNode>();
    private string _id;
    private string _type;

    public string Name { get; private set; }
    public string Label { get; private set; }
    public string Image { get; private set; }
    public int SortOrder { get; private set; }
    public IEnumerable<TocNode> Children { get { return _children; } }
    public IEnumerable<IItemRef> References { get { return _references; } }
    public Dictionary<string, object> AdditionalData { get; } = new Dictionary<string, object>();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
      get { return _children.Count > 0 ? string.Format("{0} [{1}]", Label ?? Name, _children.Count) : (Label ?? Name); }
    }

    public string Id()
    {
      return _id;
    }

    public string TypeName()
    {
      return _type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TocNode"/> class.
    /// </summary>
    /// <param name="aml">The AML.</param>
    public static TocNode FromXml(string aml)
    {
      using (var xml = XmlReader.Create(new StringReader(aml)))
      {
        return FromXml(xml);
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TocNode"/> class.
    /// </summary>
    /// <param name="stream">The stream containing AML.</param>
    public static TocNode FromXml(Stream stream)
    {
      using (var xml = XmlReader.Create(stream))
      {
        return FromXml(xml);
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TocNode"/> class.
    /// </summary>
    /// <param name="reader">The reader containing AML.</param>
    public static TocNode FromXml(TextReader reader)
    {
      using (var xml = XmlReader.Create(reader))
      {
        return FromXml(xml);
      }
    }

    public static TocNode FromXml(XmlReader reader)
    {
      while (reader.Read())
      {
        if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Item")
        {
          switch (reader.GetAttribute("type").ToUpperInvariant())
          {
            case "TREE":
              return InitMainItems(reader);
            case "COMMANDBARMENU":
            case "COMMANDBARBUTTON":
              return InitCui(reader);
            default:
              throw new InvalidOperationException("Cannot get TOC data from Item of type `" + reader.GetAttribute("type") + "`");
          }
        }
      }

      throw new InvalidOperationException("Cannot get TOC data from this structure");
    }

    private static TocNode InitMainItems(XmlReader reader)
    {
      var stack = new Stack<TocNode>();
      stack.Push(new TocNode());
      var elementNames = new Stack<string>();
      elementNames.Push(reader.GetAttribute("type"));

      while (reader.Read())
      {
        switch (reader.NodeType)
        {
          case XmlNodeType.Element:
            if (!reader.IsEmptyElement)
            {
              if (reader.LocalName == "Item")
                elementNames.Push(reader.GetAttribute("type"));
              else
                elementNames.Push(reader.LocalName);
            }
            if (string.Equals(reader.GetAttribute("type"), "Tree Node", StringComparison.OrdinalIgnoreCase))
            {
              var newNode = new TocNode()
              {
                _type = reader.GetAttribute("type"),
                SortOrder = (stack.Peek()._children.Count + 1) * 10
              };
              stack.Peek()._children.Add(newNode);
              stack.Push(newNode);
            }
            break;
          case XmlNodeType.EndElement:
            if (elementNames.Count > 0)
            {
              var lastElem = elementNames.Pop();
              if (string.Equals(lastElem, "Tree Node", StringComparison.OrdinalIgnoreCase))
              {
                if (stack.Count == 1)
                  return stack.Pop();
                else
                  stack.Pop();
              }
            }
            break;
          case XmlNodeType.Text:
          case XmlNodeType.CDATA:
            if (!string.IsNullOrEmpty(reader.Value))
            {
              switch (elementNames.Peek())
              {
                case "itemtype_id":
                  stack.Peek()._references.Add(new ItemRef("ItemType", reader.Value));
                  break;
                case "saved_search_id":
                  stack.Peek()._references.Add(new ItemRef("SavedSearch", reader.Value));
                  break;
                case "name":
                  stack.Peek().Name = reader.Value;
                  break;
                case "label":
                  stack.Peek().Label = reader.Value;
                  break;
                case "open_icon":
                  stack.Peek().Image = reader.Value;
                  break;
                default:
                  stack.Peek().AdditionalData[elementNames.Peek()] = reader.Value;
                  break;
              }
            }
            break;
        }
      }

      if (stack.Count == 1)
        return stack.Pop();
      else
        return null;
    }

    private static TocNode InitCui(XmlReader reader)
    {
      var elementNames = new Stack<string>();
      elementNames.Push(reader.GetAttribute("type"));
      var curr = new TocNode()
      {
        _type = reader.GetAttribute("type"),
        _id = reader.GetAttribute("id")
      };
      var allNodes = new Dictionary<string, TocNode>();
      allNodes[curr._id] = curr;

      while (reader.Read())
      {
        switch (reader.NodeType)
        {
          case XmlNodeType.Element:
            if (reader.LocalName == "Item")
              elementNames.Push(reader.GetAttribute("type"));
            else
              elementNames.Push(reader.LocalName);
            if (reader.LocalName == "Item")
            {
              curr = new TocNode()
              {
                _type = reader.GetAttribute("type"),
                _id = reader.GetAttribute("id")
              };
              allNodes[curr._id] = curr;
            }
            break;
          case XmlNodeType.EndElement:
            if (elementNames.Count > 0)
              elementNames.Pop();
            break;
          case XmlNodeType.Text:
          case XmlNodeType.CDATA:
            if (!string.IsNullOrEmpty(reader.Value))
            {
              switch (elementNames.Peek())
              {
                case "id":
                case "itemtype":
                case "section":
                  break; // Do nothing
                case "image":
                  curr.Image = reader.Value;
                  break;
                case "name":
                  curr.Name = reader.Value;
                  break;
                case "label":
                  curr.Label = reader.Value;
                  break;
                case "sort_order":
                  curr.SortOrder = int.Parse(reader.Value);
                  break;
                case "additional_data":
                  using (var json = new Json.Embed.JsonTextReader(reader.Value))
                  {
                    foreach (var kvp in json.Flatten())
                    {
                      switch (kvp.Key)
                      {
                        case "$.itemTypeId":
                          curr._references.Add(new ItemRef("ItemType", kvp.Value?.ToString()));
                          break;
                        case "$.tocViewId":
                          curr._references.Add(new ItemRef("TOC View", kvp.Value?.ToString()));
                          break;
                        case "$.tocAccessId":
                        case "$.relatedTocAccessId":
                          curr._references.Add(new ItemRef("TOC Access", kvp.Value?.ToString()));
                          break;
                        case "$.formId":
                          curr._references.Add(new ItemRef("Form", kvp.Value?.ToString()));
                          break;
                        default:
                          if (kvp.Value != null)
                            curr.AdditionalData[kvp.Key.Substring(2)] = kvp.Value;
                          break;
                      }
                    }
                  }
                  break;
                default:
                  curr.AdditionalData[elementNames.Peek()] = reader.Value;
                  break;
              }
            }
            break;
        }
      }

      var root = new TocNode();
      foreach (var node in allNodes.Values)
      {
        if (node.AdditionalData.TryGetValue("parent_menu", out var parentId)
          && !string.IsNullOrEmpty(parentId?.ToString())
          && allNodes.TryGetValue(parentId?.ToString(), out var parent))
        {
          parent._children.Add(node);
          node.AdditionalData.Remove("parent_menu");
        }
        else
        {
          root._children.Add(node);
        }
      }

      return root;
    }
  }
}
