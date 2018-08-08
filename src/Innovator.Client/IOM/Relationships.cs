using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Innovator.Client.IOM
{
  internal class Relationships : Item, IRelationships, IReadOnlyRelationships
  {
    public override string Name => "Relationships";

    private XmlElement _relElment;
    private string _itemTypeName;

    internal Relationships(Innovator innovator, Element parent, XmlElement relationships, string itemTypeName = null) : base(innovator, parent)
    {
      _relElment = relationships;
      _itemTypeName = itemTypeName;
      Parent = parent;
      dom = relationships?.OwnerDocument ?? parent.Xml.OwnerDocument;
      InitNodes();
    }

    public override IElement Add(object content)
    {
      if (_relElment == null && Parent != null)
      {
        var parentNode = ((Element)Parent).Xml;
        _relElment = parentNode.OwnerDocument.CreateElement(Name);
        parentNode.AppendChild(_relElment);
      }

      if (content is Element elem && elem.Xml != null)
      {
        var imported = _relElment.OwnerDocument.ImportNode(elem.Xml, true);
        _relElment.AppendChild(imported);
        InitNodes();
        return this;
      }

      if (content is Item item && item.nodeList != null)
      {
        foreach (var node in item.nodeList.OfType<XmlNode>())
          _relElment.AppendChild(dom.ImportNode(node, true));
        InitNodes();
        return this;
      }

      if (content is IAmlNode aml)
      {
        using (var writer = _relElment.CreateNavigator().AppendChild())
          aml.ToAml(writer);
        InitNodes();
        return this;
      }

      if (content is string str)
        throw new NotSupportedException();

      var enumerable = content as IEnumerable;
      if (enumerable != null)
      {
        foreach (var curr in enumerable)
        {
          Add(curr);
        }
        return this;
      }

      throw new NotSupportedException();
    }

    public override void ToAml(XmlWriter writer, AmlWriterSettings settings)
    {
      _relElment.WriteTo(writer);
    }

    public IEnumerator<IItem> GetEnumerator()
    {
      if (nodeList == null)
        return Enumerable.Empty<IItem>().GetEnumerator();
      return nodeList.OfType<XmlElement>().Select(e => (IItem)new Item((Innovator)AmlContext, e) { Parent = this }).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator<IReadOnlyItem> IEnumerable<IReadOnlyItem>.GetEnumerator()
    {
      if (nodeList == null)
        return Enumerable.Empty<IReadOnlyItem>().GetEnumerator();
      return nodeList.OfType<XmlElement>().Select(e => (IReadOnlyItem)new Item((Innovator)AmlContext, e) { Parent = this }).GetEnumerator();
    }

    private void InitNodes()
    {
      if (_relElment == null)
      {
        node = null;
        nodeList = null;
        return;
      }

      if (string.IsNullOrEmpty(_itemTypeName))
        nodeList = _relElment.SelectNodes("./Item");
      else
        nodeList = _relElment.SelectNodes("Item[@type='" + _itemTypeName + "']");
    }
  }
}
