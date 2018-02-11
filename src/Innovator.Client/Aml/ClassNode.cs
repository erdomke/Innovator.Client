using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  /// <summary>
  /// A particular node in a classification structure tree
  /// </summary>
  public class ClassNode
  {
    private IList<ClassNode> _children;
    private string _path;

    /// <summary>
    /// Gets the direct children of the current node.
    /// </summary>
    public IEnumerable<ClassNode> Children { get { return _children ?? Enumerable.Empty<ClassNode>(); } }
    /// <summary>
    /// Gets the ID f the current node.
    /// </summary>
    public Guid Id { get; internal set; }
    /// <summary>
    /// Gets a value indicating whether this instance is a leaf node.
    /// </summary>
    public bool IsLeaf { get { return _children == null; } }
    /// <summary>
    /// Gets the label of the node.
    /// </summary>
    public string Label { get; internal set; }
    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    public string Name { get; internal set; }
    /// <summary>
    /// Gets the parent node.
    /// </summary>
    public ClassNode Parent { get; private set; }
    /// <summary>
    /// Gets a <see cref="string"/> representation of the node's the path in the tree.
    /// </summary>
    public string Path
    {
      get
      {
        _path = _path ?? ParentsAndSelf()
          .Reverse()
          .Where(n => !string.IsNullOrEmpty(n.Name) && !(n is ClassStructure))
          .GroupConcat("/", n => n.Name);
        return _path;
      }
    }
    /// <summary>
    /// Gets the property sort order.
    /// </summary>
    public string PropertySortOrder { get; internal set; }

    internal ClassNode() { }

    /// <summary>
    /// Gets all the descendant nodes of the current node.
    /// </summary>
    public virtual IEnumerable<ClassNode> Descendants()
    {
      var list = new List<ClassNode>();
      BuildDescendantList(list);
      return list;
    }

    /// <summary>
    /// Gets all the descendant nodes of the current node as
    /// well as the current node.
    /// </summary>
    public virtual IEnumerable<ClassNode> DescendantsAndSelf()
    {
      var list = new List<ClassNode> { this };
      BuildDescendantList(list);
      return list;
    }

    /// <summary>
    /// Gets all of the parents of the current node.
    /// </summary>
    public IEnumerable<ClassNode> Parents()
    {
      var curr = Parent;
      while (curr != null)
      {
        yield return curr;
        curr = curr.Parent;
      }
    }

    /// <summary>
    /// Gets all of the parents of the current node as well as the
    /// current node.
    /// </summary>
    public IEnumerable<ClassNode> ParentsAndSelf()
    {
      var curr = this;
      while (curr != null)
      {
        yield return curr;
        curr = curr.Parent;
      }
    }

    internal void Add(ClassNode node)
    {
      if (_children == null)
        _children = new List<ClassNode>();
      _children.Add(node);
      node.Parent = this;
    }

    private void BuildDescendantList(List<ClassNode> nodes)
    {
      foreach (var child in Children)
      {
        nodes.Add(child);
        child.BuildDescendantList(nodes);
      }
    }

    /// <summary>
    /// Finds the child whose name matches the string at position
    /// <paramref name="index"/> in the <paramref name="path"/> array.
    /// </summary>
    /// <param name="path">The path segments.</param>
    /// <param name="index">The index in the path to match.</param>
    /// <returns>The matching <see cref="ClassNode"/> if found.  Otherwise,
    /// <c>null</c>.</returns>
    protected ClassNode FindChild(string[] path, int index)
    {
      var child = Children.FirstOrDefault(n => string.Equals(n.Name, path[index], StringComparison.OrdinalIgnoreCase));
      if (child != null && (index + 1) < path.Length)
        return child.FindChild(path, index + 1);
      return child;
    }
  }
}
