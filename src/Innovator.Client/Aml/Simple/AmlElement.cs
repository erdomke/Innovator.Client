namespace Innovator.Client
{
  internal class AmlElement : Element
  {
    private ElementFactory _amlContext;
    private string _name;
    private ILinkedElement _next;
    private IElement _parent;
    private string _prefix;

    public override ElementFactory AmlContext { get { return _amlContext; } }
    public override bool Exists { get { return Next != null || _parent == _nullElem; } }
    public override string Name { get { return _name; } }
    public override ILinkedElement Next
    {
      get { return _next; }
      set { _next = value; }
    }
    public override IElement Parent
    {
      get { return _parent ?? NullElem; }
      set { _parent = value; }
    }
    public override string Prefix { get { return _prefix; } }

    internal AmlElement() { }
    public AmlElement(ElementFactory amlContext, string name, params object[] content)
    {
      var kvp = XmlUtils.GetXmlNamePrefix(name);
      _prefix = kvp.Key;
      _name = kvp.Value;
      _amlContext = amlContext;
      _parent = NullElem;
      Add(content);
    }
    public AmlElement(IElement parent, string name)
    {
      var kvp = XmlUtils.GetXmlNamePrefix(name);
      _prefix = kvp.Key;
      _name = kvp.Value;
      _amlContext = parent.AmlContext;
      _parent = parent;
    }
    internal AmlElement(IElement newParent, IReadOnlyElement elem) : base()
    {
      _amlContext = newParent.AmlContext;
      _name = elem.Name;
      _prefix = elem.Prefix;
      _parent = newParent;
      CopyData(elem);
    }

    private static AmlElement _nullElem = new AmlElement();
    public static AmlElement NullElem { get { return _nullElem; } }
  }
}
