using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Tests
{
  public class AssertExtensions
  {
    public static void AreEqual<T>(string expected, T actual) where T : IReadOnlyItem
    {
      Assert.AreEqual(ElementFactory.Local.FromXml(expected).AssertItem().ToAml(), actual.ToAml());
    }
  }
}

namespace System.Runtime.CompilerServices
{
  /// <summary>
  /// A factory type used by compilers to create instances of the type <see cref="FormattableString"/>.
  /// </summary>
  public static class FormattableStringFactory
  {
    /// <summary>
    /// Create a <see cref="FormattableString"/> from a composite format string and object
    /// array containing zero or more objects to format.
    /// </summary>
    public static FormattableString Create(string format, params object[] arguments)
    {
      if (format == null)
      {
        throw new ArgumentNullException(nameof(format));
      }

      if (arguments == null)
      {
        throw new ArgumentNullException(nameof(arguments));
      }

      return new ConcreteFormattableString(format, arguments);
    }

    private sealed class ConcreteFormattableString : FormattableString
    {
      private readonly string _format;
      private readonly object[] _arguments;

      internal ConcreteFormattableString(string format, object[] arguments)
      {
        _format = format;
        _arguments = arguments;
      }

      public override string Format { get { return _format; } }
      public override object[] GetArguments() { return _arguments; }
      public override int ArgumentCount { get { return _arguments.Length; } }
      public override object GetArgument(int index) { return _arguments[index]; }
      public override string ToString(IFormatProvider formatProvider) { return string.Format(formatProvider, _format, _arguments); }
    }
  }
}

namespace System
{
  /// <summary>
  /// A composite format string along with the arguments to be formatted. An instance of this
  /// type may result from the use of the C# or VB language primitive "interpolated string".
  /// </summary>
  public abstract class FormattableString : IFormattable
  {
    /// <summary>
    /// The composite format string.
    /// </summary>
    public abstract string Format { get; }

    /// <summary>
    /// Returns an object array that contains zero or more objects to format. Clients should not
    /// mutate the contents of the array.
    /// </summary>
    public abstract object[] GetArguments();

    /// <summary>
    /// The number of arguments to be formatted.
    /// </summary>
    public abstract int ArgumentCount { get; }

    /// <summary>
    /// Returns one argument to be formatted from argument position <paramref name="index"/>.
    /// </summary>
    public abstract object GetArgument(int index);

    /// <summary>
    /// Format to a string using the given culture.
    /// </summary>
    public abstract string ToString(IFormatProvider formatProvider);

    string IFormattable.ToString(string ignored, IFormatProvider formatProvider)
    {
      return ToString(formatProvider);
    }

    /// <summary>
    /// Format the given object in the invariant culture. This static method may be
    /// imported in C# by
    /// <code>
    /// using static System.FormattableString;
    /// </code>.
    /// Within the scope
    /// of that import directive an interpolated string may be formatted in the
    /// invariant culture by writing, for example,
    /// <code>
    /// Invariant($"{{ lat = {latitude}; lon = {longitude} }}")
    /// </code>
    /// </summary>
    public static string Invariant(FormattableString formattable)
    {
      if (formattable == null)
      {
        throw new ArgumentNullException(nameof(formattable));
      }

      return formattable.ToString(Globalization.CultureInfo.InvariantCulture);
    }

    public override string ToString()
    {
      return ToString(Globalization.CultureInfo.CurrentCulture);
    }
  }
}
