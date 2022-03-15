using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Innovator.Client
{
  class Relationships : Element, IRelationships, IReadOnlyRelationships
  {
    public override string Name { get { return "Relationships"; } }
    public override ILinkedElement Next { get; set; }
    public override IElement Parent { get; set; }
    public override string Prefix { get { return string.Empty; } }

    public Relationships() { }
    public Relationships(IElement parent)
    {
      this.Parent = parent;
    }
    public Relationships(params object[] content) : this()
    {
      for (var i = 0; i < content.Length; i++)
      {
        Add(content[i]);
      }
    }

    public override IElement Add(object content)
    {
      if (!Exists && this.Parent != null)
        Parent.Add(this);
      return base.Add(content);
    }

    public IEnumerable<IReadOnlyItem> ByType(string type)
    {
      return Elements().OfType<IReadOnlyItem>().Where(i => i.TypeName() == type);
    }

    IEnumerator<IItem> IEnumerable<IItem>.GetEnumerator()
    {
      return Elements().OfType<IItem>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return Elements().OfType<IReadOnlyItem>().GetEnumerator();
    }

    IEnumerator<IReadOnlyItem> IEnumerable<IReadOnlyItem>.GetEnumerator()
    {
      return Elements().OfType<IReadOnlyItem>().GetEnumerator();
    }

    protected override Element Clone(IElement newParent)
    {
      var result = new Relationships(newParent);
      result.CopyData(this);
      return result;
    }
  }
}
