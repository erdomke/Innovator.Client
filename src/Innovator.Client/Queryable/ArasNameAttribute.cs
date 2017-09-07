using System;

namespace Innovator.Client.Model
{
  /// <summary>
  /// Attribute for specifying the Aras database name of a type or class member
  /// </summary>
  [AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
  public class ArasNameAttribute : System.Attribute
  {
    /// <summary>
    /// Gets the database name of the type or class member.
    /// </summary>
    /// <value>
    /// The database name of the type or class member.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArasNameAttribute"/> class.
    /// </summary>
    /// <param name="name">The database name.</param>
    public ArasNameAttribute(string name)
    {
      Name = name;
    }
  }
}
