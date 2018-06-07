using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  /// <summary>
  /// Interface representing an element with a value that can be represented as one of several types
  /// </summary>
  public interface ICoercible
  {
    /// <summary>Value converted to a <see cref="Nullable{Boolean}"/>.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="bool"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="bool"/></exception>
    bool? AsBoolean();

    /// <summary>Value converted to a <see cref="Nullable{DateTime}"/> in the local timezone.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="DateTime"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTime"/></exception>
    DateTime? AsDateTime();

    /// <summary>Value converted to a <see cref="Nullable{DateTime}"/> in the UTC timezone.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="DateTime"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="DateTime"/></exception>
    DateTime? AsDateTimeUtc();

    /// <summary>Value converted to a <see cref="Nullable{Double}"/>.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="double"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="double"/></exception>
    double? AsDouble();

    /// <summary>Value converted to a <see cref="Nullable{Guid}"/>.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="Guid"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="Guid"/></exception>
    Guid? AsGuid();

    /// <summary>Value converted to a <see cref="Nullable{Int32}"/>.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="int"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="int"/></exception>
    int? AsInt();

    /// <summary>Value converted to a <see cref="Nullable{Int64}"/>.
    /// If the value cannot be converted, an exception is thrown</summary>
    /// <returns>A <see cref="long"/> or <c>null</c> if the value is empty</returns>
    /// <exception cref="InvalidCastException">If the non-empty value cannot be converted to a <see cref="long"/></exception>
    long? AsLong();

    /// <summary>Value converted to a <see cref="string"/> using the <paramref name="defaultValue"/> if null.</summary>
    /// <param name="defaultValue">The default value to return if the value is empty</param>
    /// <returns>A <see cref="string"/> or <paramref name="defaultValue"/> if the value is empty</returns>
    string AsString(string defaultValue);
  }
}
