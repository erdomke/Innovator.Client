using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  /// <summary>
  /// Represents a string literal used in a query
  /// </summary>
  /// <seealso cref="Innovator.Client.QueryModel.ILiteral" />
  /// <seealso cref="System.IEquatable{Innovator.Client.QueryModel.StringLiteral}" />
  /// <seealso cref="System.IEquatable{System.String}" />
  public class StringLiteral : ILiteral, IEquatable<StringLiteral>, IEquatable<string>
  {
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public string Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLiteral"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    public StringLiteral(string value)
    {
      Value = value;
    }

    public void Visit(IExpressionVisitor visitor)
    {
      visitor.Visit(this);
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (obj is StringLiteral other)
        return Equals(other);
      else if (obj is string str)
        return Equals(str);
      return false;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(StringLiteral other)
    {
      return other?.Value == Value;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(string other)
    {
      return string.Equals(Value, other);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      return Value?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return Value;
    }

    public bool? AsBoolean()
    {
      return ElementFactory.Local.LocalizationContext.AsBoolean(Value);
    }

    public DateTime? AsDateTime()
    {
      return ElementFactory.Local.LocalizationContext.AsDateTime(Value);
    }

    public DateTime? AsDateTimeUtc()
    {
      return ElementFactory.Local.LocalizationContext.AsDateTimeUtc(Value);
    }

    public double? AsDouble()
    {
      return ElementFactory.Local.LocalizationContext.AsDouble(Value);
    }

    public Guid? AsGuid()
    {
      if (Value == null) return null;
      return new Guid(Value);
    }

    public int? AsInt()
    {
      return ElementFactory.Local.LocalizationContext.AsInt(Value);
    }

    public long? AsLong()
    {
      return ElementFactory.Local.LocalizationContext.AsLong(Value);
    }

    public string AsString(string defaultValue)
    {
      return ElementFactory.Local.LocalizationContext.Format(Value) ?? defaultValue;
    }

    public object AsClrValue()
    {
      return Value;
    }
  }
}
