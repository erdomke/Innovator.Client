namespace Innovator.Client
{
  internal class Logical : AmlElement, ILogical
  {
    public Logical(ElementFactory amlContext, string name, params object[] content) : base(amlContext, name, content) { }
    public Logical(IElement parent, string name) : base(parent, name) { }
    public Logical(IElement parent, IReadOnlyElement elem) : base(parent, elem) { }

    protected override Element Clone(IElement newParent)
    {
      return new Logical(newParent, this);
    }
  }
}
