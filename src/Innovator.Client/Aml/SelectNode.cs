using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  /// <summary>
  /// Class for parsing and storing AML select attributes
  /// </summary>
  /// <remarks>
  /// Use <see cref="SelectNode.FromString(string)"/> to parse an AML select attribute.
  /// </remarks>
  public class SelectNode : ICollection<SelectNode>
  {
    private List<SelectNode> _children;

    /// <summary>
    /// Number of child columns
    /// </summary>
    public int Count
    {
      get
      {
        if (_children == null) return 0;
        return _children.Count;
      }
    }

    /// <summary>
    /// Name of the current column
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Name of the function
    /// </summary>
    public string Function { get; set; }

    bool ICollection<SelectNode>.IsReadOnly { get { return false; } }

    /// <summary>
    /// Access a child sub-select by index
    /// </summary>
    public SelectNode this[int index]
    {
      get
      {
        if (_children == null) throw new IndexOutOfRangeException();
        return _children[index];
      }
    }

    /// <summary>
    /// Initializes a new <see cref="SelectNode"/> instance for storing an AML select path
    /// </summary>
    public SelectNode() { }

    /// <summary>
    /// Initializes a new <see cref="SelectNode"/> instance with a property name.
    /// </summary>
    /// <param name="name">The name of the property to store in the node</param>
    public SelectNode(string name)
    {
      Name = name;
    }

    /// <summary>
    /// Initializes a new <see cref="SelectNode"/> instance with a property name.
    /// </summary>
    /// <param name="name">The name of the property to store in the node</param>
    /// <param name="function">The name of the property function</param>
    public SelectNode(string name, string function)
    {
      Name = name;
      Function = function;
    }

    internal SelectNode(string name, IEnumerable<SelectNode> children)
    {
      Name = name;
      _children = children.ToList();
    }

    internal SelectNode(IEnumerable<SelectNode> children)
    {
      _children = children.ToList();
    }

    /// <summary>
    /// Add a child <see cref="SelectNode"/> to this instance
    /// </summary>
    /// <param name="item"></param>
    public void Add(SelectNode item)
    {
      if (item == null) return;
      if (_children == null) _children = new List<SelectNode>();

      var existing = _children.Find(c => c.Name == item.Name);
      if (existing == null)
      {
        _children.Add(item);
      }
      else
      {
        foreach (var child in item)
        {
          existing.Add(child);
        }
      }
    }

    /// <summary>
    /// Ensure that the path of properties exists in the select statement
    /// </summary>
    /// <example>Sending in <c>"created_by_id, first_name"</c> will result in the select statement <c>created_by_id(first_name)</c></example>
    public void EnsurePath(params string[] path)
    {
      EnsurePath((IEnumerable<string>)path);
    }

    /// <summary>
    /// Ensure that the path of properties exists in the select statement
    /// </summary>
    /// <example>Sending in <c>new string[] {"created_by_id, first_name"}</c> will result in the select statement <c>created_by_id(first_name)</c></example>
    public void EnsurePath(IEnumerable<string> path)
    {
      if (!path.Any()) return;
      if (_children == null) _children = new List<SelectNode>();
      var name = path.First();
      var match = _children.Find(c => c.Name == name);
      if (match == null)
      {
        match = new SelectNode(name);
        _children.Add(match);
      }
      match.EnsurePath(path.Skip(1));
    }

    /// <summary>
    /// Union the children of two select nodes
    /// </summary>
    public void UnionWith(SelectNode item)
    {
      foreach (var child in item)
        Add(child);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the child nodes.
    /// </summary>
    /// <returns>
    /// An enumerator that can be used to iterate through the child nodes.
    /// </returns>
    public IEnumerator<SelectNode> GetEnumerator()
    {
      if (_children == null) return Enumerable.Empty<SelectNode>().GetEnumerator();
      return _children.GetEnumerator();
    }

    /// <summary>
    /// Sort the properties by name
    /// </summary>
    public void Sort()
    {
      if (_children != null)
      {
        _children.Sort((x, y) => (x.Name ?? "").CompareTo(y.Name ?? ""));
        foreach (var child in _children)
        {
          child.Sort();
        }
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="SelectNode"/>.
    /// </summary>
    public static implicit operator SelectNode(string select)
    {
      return FromString(select);
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> in the format of an AML select attribute
    /// </summary>
    public override string ToString()
    {
      return Write(new StringBuilder()).ToString();
    }

    private StringBuilder Write(StringBuilder builder)
    {
      if (string.IsNullOrEmpty(Name))
      {
        builder.EnsureCapacity(_children.Count * 5);
        Write(builder, _children);
      }
      else
      {
        builder.Append(Name);
        if (_children != null)
        {
          builder.Append('(');
          builder.EnsureCapacity(_children.Count * 5);
          Write(builder, _children);
          builder.Append(')');
        }
      }
      return builder;
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> in the format of an AML select attribute
    /// </summary>
    public static string ToString(IEnumerable<SelectNode> items)
    {
      return Write(new StringBuilder(), items).ToString();
    }

    /// <summary>
    /// Parse an AML select statement into a <see cref="SelectNode"/> structure
    /// </summary>
    public static SelectNode FromString(string select)
    {
      var result = new SelectNode();
      if (string.IsNullOrEmpty(select))
        return result;

      var path = new Stack<SelectNode>();
      var curr = default(SelectNode);
      path.Push(result);
      var start = 0;
      for (var i = 0; i < select.Length; i++)
      {
        switch (select[i])
        {
          case ',':
            if (i - start > 0)
              path.Peek().Add(new SelectNode(select.Substring(start, i - start).Trim()));
            start = i + 1;
            break;
          case '|':
            if (i - start > 0)
              path.Peek().Add(new SelectNode(select.Substring(start, i - start).Trim()));
            curr = path.Pop();
            if (path.Count < 1)
            {
              path.Push(result = new SelectNode()
              {
                curr
              });
            }
            curr = new SelectNode();
            path.Peek()._children.Add(curr);
            path.Push(curr);
            start = i + 1;
            break;
          case '[':
            var idx = select.IndexOf(']', i);
            if (idx < i) idx = select.Length;
            var func = select.Substring(i + 1, idx - i - 1);

            if (i - start > 0)
            {
              curr = new SelectNode(select.Substring(start, i - start).Trim(), func);
              path.Peek().Add(curr);
            }
            else if (string.IsNullOrEmpty(path.Peek().Last().Name))
            {
              curr = path.Peek().Last();
              path.Peek()._children.RemoveAt(path.Peek().Count - 1);
              foreach (var child in curr)
              {
                child.Function = func;
                path.Peek().Add(child);
              }
            }

            if (idx > i)
              i = idx;
            start = i + 1;
            break;
          case '(':
            curr = new SelectNode(i - start > 0 ? select.Substring(start, i - start).Trim() : null);
            path.Peek().Add(curr);
            path.Push(curr);
            start = i + 1;
            break;
          case ')':
            if (i - start > 0)
              path.Peek().Add(new SelectNode(select.Substring(start, i - start).Trim()));
            path.Pop();
            start = i + 1;
            break;
        }
      }

      if (start < select.Length)
      {
        path.Peek().Add(new SelectNode(select.Substring(start, select.Length - start).Trim()));
      }
      return result;
    }

    private static StringBuilder Write(StringBuilder builder, IEnumerable<SelectNode> items)
    {
      var first = true;
      var delim = ',';
      foreach (var group in items.GroupBy(i => i.Function ?? ""))
      {
        var renderParentheses = group.Key != "" && group.Skip(1).Any();
        if (renderParentheses)
        {
          if (!first)
          {
            builder.Append(delim);
            first = true;
          }
          builder.Append('(');
        }

        foreach (var item in group)
        {
          if (first)
          {
            if (string.IsNullOrEmpty(item.Name)) delim = '|';
          }
          else
          {
            builder.Append(delim);
          }
          item.Write(builder);
          first = false;
        }

        if (renderParentheses)
          builder.Append(')');

        if (group.Key != "")
          builder.Append('[').Append(group.Key).Append(']');
      }
      return builder;
    }

    /// <summary>
    /// Removes all child items from the <see cref="SelectNode"/>
    /// </summary>
    public void Clear()
    {
      _children.Clear();
    }

    /// <summary>
    /// Determines whether the <see cref="SelectNode" /> contains a specific value.
    /// </summary>
    /// <param name="item">The child <see cref="SelectNode"/> to locate in the <see cref="SelectNode" />.</param>
    /// <returns>
    /// true if <paramref name="item" /> is found in the <see cref="SelectNode" />; otherwise, false.
    /// </returns>
    public bool Contains(SelectNode item)
    {
      return _children.Contains(item);
    }

    /// <summary>
    /// Copies the elements of the <see cref="SelectNode" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="SelectNode" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    public void CopyTo(SelectNode[] array, int arrayIndex)
    {
      _children.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the first occurrence of the child <see cref="SelectNode"/> from the <see cref="SelectNode" />.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="SelectNode" />.</param>
    /// <returns>
    /// true if <paramref name="item" /> was successfully removed from the <see cref="SelectNode" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="SelectNode" />.
    /// </returns>
    public bool Remove(SelectNode item)
    {
      return _children.Remove(item);
    }
  }
}
